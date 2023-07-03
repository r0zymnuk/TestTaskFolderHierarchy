using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace TestTaskFolderHierarchy.Services.FileService;

public class FileService : IFileService
{
    public void ExtractFolder(Stream inputStream, string destinationFolderPath)
    {
        destinationFolderPath = Path.Combine(destinationFolderPath, Guid.NewGuid().ToString());
        Directory.CreateDirectory(destinationFolderPath);

        using (var zipArchive = new ZipArchive(inputStream, ZipArchiveMode.Read))
        {
            // Extract each entry in the zip archive
            foreach (var entry in zipArchive.Entries)
            {
                // Skip directories (if any)
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                // Create the entry's full destination path
                string entryDestinationPath = Path.Combine(destinationFolderPath, entry.FullName);

                // Ensure the parent directory of the entry exists
                Directory.CreateDirectory(Path.GetDirectoryName(entryDestinationPath)!);

                // Extract the entry to the destination path
                entry.ExtractToFile(entryDestinationPath, overwrite: true);
            }
        }
    }
}