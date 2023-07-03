using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public interface IFolderService
{
    ServiceResponse<FolderViewModel> GetFolderByPath(string path);
    Folder? GetToTheLastFolder(string path);
    string CreateFolder(string name, string path);
    string DeleteFolder(string path);
    void ParseFolder(string folderPath, Folder? parentFolder = null);
}