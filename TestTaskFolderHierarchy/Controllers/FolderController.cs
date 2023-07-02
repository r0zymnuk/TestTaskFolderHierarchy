using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;
using TestTaskFolderHierarchy.Services.FolderService;

namespace TestTaskFolderHierarchy.Controllers;

public class FolderController : Controller
{
    private readonly string ProjectFolderPath = System.AppDomain.CurrentDomain.BaseDirectory;
    private readonly ILogger<FolderController> _logger;
    private readonly IFolderService _folderService;

    public FolderController(ILogger<FolderController> logger, IFolderService folderService)
    {
        _logger = logger;
        _folderService = folderService;
    }

    [HttpGet("{*path}")]
    public IActionResult Index(string path)
    {
        Console.WriteLine(path);
        var response = _folderService.GetFolderByPath(path);
        return View(response);
    }
    
    [HttpPost]
    public IActionResult PostFolder(string path, string name)
    {
        Console.WriteLine(path);
        Console.WriteLine(name);
        _folderService.CreateFolder(name, path);
        return Redirect("/" + path + "/"+ name);
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
            var folderResponse = _folderService.GetFolderModelByPath(path);

            if (folderResponse.Success)
            {
                // Create the folder structure in the destination folder
                _folderService.CreateFolderStructure(destinationFolderPath, folderResponse.Data!);

                // Get the name of the folder being downloaded
                string folderName = folderResponse.Data!.Name;

                // Generate the zip file
                byte[] zipFileBytes = _folderService.ZipFolder(Path.Combine(destinationFolderPath, folderName));

                // Delete the temporary folder structure
                Directory.Delete(destinationFolderPath, true);

                // Return the zip file to the user with the folder name as the file name
                string zipFileName = $"{folderName}.zip";
                return File(zipFileBytes, "application/zip", zipFileName);
            }
            else
            {
                // Handle the case when the folder retrieval failed
                return Content("Error: " + folderResponse.Message);
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occurred during the process
            // You can customize the error handling based on your requirements
            return Content("An error occurred: " + ex.Message);
        }
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}