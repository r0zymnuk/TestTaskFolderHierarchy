using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestTaskFolderHierarchy.Data;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FileService;

public class FileService : IFileService
{
    private readonly DataContext _context;

    public FileService(DataContext context)
    {
        _context = context;
    }

    public string CreateFolderStructure(string rootFolderPath, Folder folder)
    {
        string folderPath = Path.Combine(rootFolderPath, folder.Name);

        Directory.CreateDirectory(folderPath);

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