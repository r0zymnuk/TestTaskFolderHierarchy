using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public interface IFolderService
{
    ServiceResponse<FolderViewModel> GetFolderByPath(string path);
    ServiceResponse<Folder> GetFolderModelByPath(string path);
    ServiceResponse<FolderViewModel> CreateFolder(string name, string path);
    string CreateFolderStructure(string rootFolderPath, Folder folder);
    string DeleteFolder(string path);
    byte[] ZipFolder(string folderPath);
    // List<Folder> UpdateFolder(Folder folder);
    // void DeleteFolder(Guid id);
    // List<Folder> UploadFile(Guid id, IFormFile file);
    // FileResult DownloadFile(Guid id);

}