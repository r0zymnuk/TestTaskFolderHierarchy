using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FileService;

public interface IFileService
{
    string CreateFolderStructure(string rootFolderPath, Folder folder);
    byte[] ZipFolder(string folderPath);
}