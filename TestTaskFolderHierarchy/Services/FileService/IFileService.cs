using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestTaskFolderHierarchy.Services.FileService;

public interface IFileService
{
    void ExtractFolder(Stream inputStream, string destinationFolderPath);
}