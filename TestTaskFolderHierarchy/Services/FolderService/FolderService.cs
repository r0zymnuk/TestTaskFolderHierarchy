using System.IO.Compression;
using System.Transactions;
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
        Folder? folder = GetToTheLastFolder(path);

        response.Data = _mapper.Map<FolderViewModel>(folder);
        response.Success = folder is null ? false : true;
        response.Message = folder is null ? "Folder not found" : $"Folder {folder.Name} found";

        return response;
    }

    public Folder? GetToTheLastFolder(string path)
    {
        var pathList = new List<string>();
        if (!string.IsNullOrEmpty(path))
        {
            pathList = path.Split('/').ToList();
        }
        var rootFolders = _context.Folders
            .Where(f => f.ParentId == null)
            .ToList();
        if (pathList.Count == 0)
        {
            return new Folder{
                Id = Guid.Empty,
                Name = "Root Folder",
                SubFolders = rootFolders
            };
        }
        while (pathList.Count > 0)
        {
            string folderName = pathList[0];
            pathList.RemoveAt(0);
            Folder? folder = rootFolders.FirstOrDefault(f => f.Name == folderName);
            if (folder is null)
                return null;
            
            if (pathList.Count == 0)
                return _context.Folders
                    .Include(f => f.SubFolders)
                    .FirstOrDefault(f => f.Id == folder.Id);
            
            rootFolders = _context.Folders
                .Include(f => f.SubFolders)
                .Where(f => f.ParentId == folder.Id)
                .ToList();
        }
        return null;
    }

    public string CreateFolder(string name, string path)
    {
        Folder folder = new Folder();
        folder.Name = name;

        var parentFolder = GetToTheLastFolder(path);

        if (parentFolder is null)
            return path;

        if (parentFolder.Name == "Root Folder")
        {
            folder.ParentId = null;
        }
        else
        {
            folder.ParentId = parentFolder.Id;
            parentFolder.SubFolders.Add(folder);
        }

        using (var transactionScope = new TransactionScope())
        {
            _context.Folders.Add(folder);
            _context.SaveChanges();

            transactionScope.Complete();
        }
        
        return Path.Combine(path == null ? "" : path, name);
    }


    public string DeleteFolder(string path)
    {        
        string parentPath = Path.GetDirectoryName(path) ?? string.Empty;

        Folder? folder = GetToTheLastFolder(path);
        if (folder is null)
        {
            return parentPath;
        }

        DeleteSubfolders(folder.SubFolders.ToList());
        _context.Folders.Remove(folder);
        _context.SaveChanges();

        return parentPath;
    }

    private void DeleteSubfolders(List<Folder> subfolders)
    {
        Folder folder = new Folder();
        foreach (Folder subfolder in subfolders)
        {   
            folder = _context.Folders
                .Include(f => f.SubFolders)
                .First(f => f.Id == subfolder.Id);
            if (folder.SubFolders.Count > 0)
                DeleteSubfolders(folder.SubFolders.ToList());
                
            _context.Folders.Remove(folder);
            _context.SaveChanges();

        }
    }

    public void ParseFolder(string folderPath, Folder? parentFolder)
    {
        string[] subdirectories = Directory.GetDirectories(folderPath);

        foreach (string subdirectory in subdirectories)
        {
            Folder folder = new Folder
            {
                Name = Path.GetFileName(subdirectory),
                ParentId = Guid.Equals(parentFolder?.Id, Guid.Empty) ? null : parentFolder?.Id
            };

            if (parentFolder is not null)
                parentFolder.SubFolders.Add(folder);

            _context.Folders.Add(folder);
            _context.SaveChanges();
            ParseFolder(subdirectory, _context.Folders.Include(f => f.SubFolders).First(f => f.Id == folder.Id));
        }
        
    }
}