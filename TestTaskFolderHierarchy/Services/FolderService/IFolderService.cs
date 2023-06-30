using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public interface IFolderService
{
    ServiceResponse<FolderViewModel> GetFolderByPath(string path);
    ServiceResponse<FolderViewModel> CreateFolder(string name, string path);
    string DeleteFolder(string path);
    // List<Folder> UpdateFolder(Folder folder);
    // void DeleteFolder(Guid id);
    // List<Folder> UploadFile(Guid id, IFormFile file);
    // FileResult DownloadFile(Guid id);

}