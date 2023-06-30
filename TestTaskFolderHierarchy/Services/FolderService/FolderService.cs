using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TestTaskFolderHierarchy.Data;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public class FolderService : IFolderService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public FolderService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public ServiceResponse<FolderViewModel> GetFolderByPath(string path)
    {
        var response = new ServiceResponse<FolderViewModel>();
        response.Path = path;

        if (string.IsNullOrEmpty(path))
        {
            response.Data = _mapper.Map<FolderViewModel>(new Folder{
                Name = "root",
                SubFolders = _context.Folders
                    .Where(f => f.ParentId == null)
                    .ToList()
            });
            return response;
        }
        var pathList = path.Split('/').ToList();
        var folder = GetToTheLastFolder(pathList);
        if (folder != null)
        {
            response.Data = _mapper.Map<FolderViewModel>(folder);
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "Folder not found";
            return response;
        }
    }

    private Folder? GetToTheLastFolder(List<string> pathList)
    {
        var rootFolders = _context.Folders
            .Where(f => f.ParentId == null)
            .ToList();
        if (pathList.Count == 0)
        {
            return new Folder{
                Name = "root",
                SubFolders = rootFolders
            };
        }
        while (pathList.Count > 0)
        {
            var folderName = pathList[0];
            pathList.RemoveAt(0);
            var folder = rootFolders.FirstOrDefault(f => f.Name == folderName);
            if (folder is null)
            {
                return null;
            }
            if (pathList.Count == 0)
            {
                return _context.Folders
                    .Include(f => f.SubFolders)
                    .FirstOrDefault(f => f.Id == folder.Id);
            }
            rootFolders = _context.Folders
                .Include(f => f.SubFolders)
                .Where(f => f.ParentId == folder.Id)
                .ToList();
        }
        return null;

    }

    public ServiceResponse<FolderViewModel> CreateFolder(string name, string path)
    {
        var response = new ServiceResponse<FolderViewModel>();
        response.Path = path;
        var folder = new Folder();
        folder.Name = name;
        var pathList = new List<string>();

        if (!string.IsNullOrEmpty(path))
            pathList = path.Split('/').ToList();
        
        var parentFolder = GetToTheLastFolder(pathList);
        if (parentFolder is null)
        {
            response.Success = false;
            response.Message = "Folder not found";
            return response;
        }
        if (parentFolder.Name == "root")
        {
            folder.ParentId = null;
        }
        else
        {
            folder.ParentId = parentFolder.Id;
            parentFolder.SubFolders.Add(folder);
        }
        _context.Folders.Add(folder);
        _context.SaveChanges();
        
        response.Data = _mapper.Map<FolderViewModel>(folder);
        return response;

    }
}