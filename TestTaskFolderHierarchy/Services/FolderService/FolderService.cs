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
        var folderResponse = GetFolderModelByPath(path);

        response.Data = _mapper.Map<FolderViewModel>(folderResponse.Data);
        response.Success = folderResponse.Success;
        response.Message = folderResponse.Message;

        return response;
    }

    public ServiceResponse<Folder> GetFolderModelByPath(string path)
    {
        var response = new ServiceResponse<Folder>();
        response.Path = path;

        var folder = GetToTheLastFolder(path);
        response.Data = folder;
        if (folder is null)
        {
            response.Success = false;
            response.Message = "Folder not found";
        }

        return response;
    }

    private Folder? GetToTheLastFolder(string path)
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
                Name = "Root Folder",
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

    public string CreateFolder(string name, string path)
    {
        var response = new ServiceResponse<FolderViewModel>();
        response.Path = path;
        
        var folder = new Folder();
        folder.Name = name;

        var parentFolder = GetToTheLastFolder(path);

        if (parentFolder is null)
        {
            return path;
        }

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
        var pathList = new List<string>();
        
        //Get path to parent folder
        var parentPath = Path.GetDirectoryName(path) ?? string.Empty;

        var folder = GetToTheLastFolder(path);
        if (folder is null)
        {
            return parentPath;
        }

        DeleteSubfolders(folder.SubFolders);
        _context.Folders.Remove(folder);
        _context.SaveChanges();

        return parentPath;
    }

    private void DeleteSubfolders(ICollection<Folder> subfolders)
    {
        foreach (var subfolder in subfolders.ToList())
        {
            using (var transactionScope = new TransactionScope())
            {
                var subfolderEntity = _context.Folders
                    .Include(f => f.SubFolders)
                    .First(f => f.Id == subfolder.Id);

                if (subfolder.SubFolders.Count > 0)
                    DeleteSubfolders(subfolder.SubFolders);
                
                _context.Folders.Remove(subfolderEntity);
                _context.SaveChanges();

                transactionScope.Complete();
            }
        }
    }

    public string CreateFolderStructure(string rootFolderPath, Folder folder)
    {
        string folderPath = Path.Combine(rootFolderPath, folder.Name);

        // Create the main folder
        Directory.CreateDirectory(folderPath);

        // Create subfolders recursively
        if (folder.SubFolders != null)
        {
            foreach (Folder subFolder in folder.SubFolders)
            {
                var subFolderEntity = _context.Folders
                    .Include(f => f.SubFolders)
                    .First(f => f.Id == subFolder.Id);
                CreateFolderStructure(folderPath, subFolderEntity);
            }
        }

        return folderPath;
    }

    public byte[] ZipFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            using (MemoryStream zipMemoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Create, true))
                {
                    AddFolderToZipArchive(zipArchive, folderPath, string.Empty);
                }

                return zipMemoryStream.ToArray();
            }
        }
        else
        {
            throw new DirectoryNotFoundException("Folder not found");
        }
    }

    private void AddFolderToZipArchive(ZipArchive zipArchive, string folderPath, string parentPath)
    {
        string entryPath = Path.Combine(parentPath, Path.GetFileName(folderPath));

        ZipArchiveEntry folderEntry = zipArchive.CreateEntry(entryPath + "/");

        foreach (string filePath in Directory.GetFiles(folderPath))
        {
            string fileEntryPath = Path.Combine(entryPath, Path.GetFileName(filePath));

            ZipArchiveEntry fileEntry = zipArchive.CreateEntryFromFile(filePath, fileEntryPath);
        }

        foreach (string subFolderPath in Directory.GetDirectories(folderPath))
        {
            AddFolderToZipArchive(zipArchive, subFolderPath, entryPath);
        }
    }
}