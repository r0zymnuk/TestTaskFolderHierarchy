using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;
using TestTaskFolderHierarchy.Services.FileService;
using TestTaskFolderHierarchy.Services.FolderService;
using System.IO.Compression;

namespace TestTaskFolderHierarchy.Controllers;

public class FolderController : Controller
{
    private string ProjectFolderPath = System.AppDomain.CurrentDomain.BaseDirectory;
    private readonly ILogger<FolderController> _logger;
    private readonly IFolderService _folderService;
    private readonly IFileService _fileService;

    public FolderController(ILogger<FolderController> logger, IFolderService folderService, IFileService fileService)
    {
        _logger = logger;
        _folderService = folderService;
        _fileService = fileService;
    }

    [HttpGet("{*path}")]
    public IActionResult Index(string path)
    {
        return View(_folderService.GetFolderByPath(path));
    }
    
    [HttpPost]
    public IActionResult PostFolder(string path, string name)
    {
        var newFolderPath = _folderService.CreateFolder(name, path); 
        return Redirect("/" + newFolderPath);
    }

    [HttpPost]
    public IActionResult DeleteFolder(string path)
    {
        var parentPath = _folderService.DeleteFolder(path);
        return Redirect("/" + parentPath);
    }

    [HttpPost]
    public ActionResult DownloadFolder(string path)
    {
        string destinationFolderPath = Path.Combine(ProjectFolderPath, "TemporaryFolder");

        try
        {
            // Get the folder entity with its subfolders by the folder path
            var folder = _folderService.GetToTheLastFolder(path);

            if (folder is not null)
            {
                // Create the folder structure in the destination folder
                _fileService.CreateFolderStructure(destinationFolderPath, folder);

                // Get the name of the folder being downloaded
                string folderName = folder.Name;

                // Generate the zip file
                byte[] zipFileBytes = _fileService.ZipFolder(Path.Combine(destinationFolderPath, folderName));

                // Delete the temporary folder structure
                Directory.Delete(destinationFolderPath, true);

                // Return the zip file to the user with the folder name as the file name
                string zipFileName = $"{folderName}.zip";
                return File(zipFileBytes, "application/zip", zipFileName);
            }
            else
            {
                // Handle the case when the folder retrieval failed
                return Content($"Error: Cannot find the folder at path {path}");
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occurred during the process
            // You can customize the error handling based on your requirements
            return Content("An error occurred: " + ex.Message);
        }
    }

    [HttpPost]
    public ActionResult UploadFolder(IFormFile file, string path)
    {
        if (file != null && file.Length > 0)
        {
            try
            {
                // Create the destination folder path
                string destinationFolderPath = Path.Combine(ProjectFolderPath, "TemporaryFolder", Guid.NewGuid().ToString());
                Directory.CreateDirectory(destinationFolderPath);
                var pathToZip = Path.Combine(destinationFolderPath, file.FileName);

                // Extract the uploaded folder to the destination folder
                using (var fileStream = new FileStream(pathToZip, FileMode.Create) )
                {
                    file.CopyTo(fileStream);
                }
                ZipFile.ExtractToDirectory(pathToZip, destinationFolderPath);

                var parent = _folderService.GetToTheLastFolder(path);

                // Parse the folder structure into folder entities
                _folderService.ParseFolder(destinationFolderPath, parent);

                // Delete the temporary folder structure
                Directory.Delete(Directory.GetParent(destinationFolderPath)!.ToString(), true);

                // Get the root folder entity
                return Redirect("/" + path);
            }
            catch (Exception ex)
            {
                return Content("An error occurred: " + ex.Message);
            }
        }

        // If no file was uploaded, return an error response or redirect to an error page
        return Content("No file uploaded!");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}