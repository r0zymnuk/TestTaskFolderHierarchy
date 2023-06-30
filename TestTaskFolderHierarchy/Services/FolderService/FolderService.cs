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

    public string DeleteFolder(string path)
    {
        var pathList = new List<string>();
        
        //Get path to parent folder
        var parentPath = "";
        if (path.Contains('/'))
        {
            var pathListForParent = path.Split('/').ToList();
            pathListForParent.RemoveAt(pathListForParent.Count - 1);
            parentPath = string.Join('/', pathListForParent);
        }
        

        if (!string.IsNullOrEmpty(path))
        { 
            pathList = path.Split('/').ToList();
        }
        var folder = GetToTheLastFolder(pathList);
        if (folder is null)
        {
            return parentPath;
        }
        var parentFolder = _context.Folders
            .Include(f => f.SubFolders)
            .FirstOrDefault(f => f.Id == folder.ParentId);
        
        DeleteFolder(folder.Id);
        return parentPath;
    }

    public void DeleteFolder(Guid folderId)
    {
        var folder = _context.Folders.Include(f => f.SubFolders).FirstOrDefault(f => f.Id == folderId);

        if (folder != null)
        {
            DeleteSubfolders(folder.SubFolders);
            _context.Folders.Remove(folder);
            _context.SaveChanges();
        }
    }

    private void DeleteSubfolders(ICollection<Folder> subfolders)
    {
        foreach (var subfolder in subfolders.ToList())
        {
            DeleteFolder(subfolder.Id);
        }
    }
}