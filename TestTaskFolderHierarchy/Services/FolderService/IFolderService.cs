using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public interface IFolderService
{
    List<Folder> GetFolders();
    List<Folder> GetFolderById(Guid id);
    List<Folder> GetFolderByName(string name);
    List<Folder> CreateFolder(Folder folder);
    // List<Folder> UpdateFolder(Folder folder);
    // void DeleteFolder(Guid id);
    // List<Folder> UploadFile(Guid id, IFormFile file);
    // FileResult DownloadFile(Guid id);

}