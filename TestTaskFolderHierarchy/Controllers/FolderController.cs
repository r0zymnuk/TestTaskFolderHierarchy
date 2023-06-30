using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;
using TestTaskFolderHierarchy.Services.FolderService;

namespace TestTaskFolderHierarchy.Controllers;

public class FolderController : Controller
{
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
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}