using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public interface IFolderService
{
    ServiceResponse<FolderViewModel> GetFolderByPath(string path);
    Folder? GetToTheLastFolder(string path);
    string CreateFolder(string name, string path);
    string CreateFolderStructure(string rootFolderPath, Folder folder);
    string DeleteFolder(string path);
    byte[] ZipFolder(string folderPath);
    void ParseFolder(string folderPath, Folder? parentFolder = null);
}