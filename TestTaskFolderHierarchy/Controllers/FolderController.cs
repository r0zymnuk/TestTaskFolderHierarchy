using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Controllers;

public class FolderController : Controller
{
    private readonly ILogger<FolderController> _logger;

    public FolderController(ILogger<FolderController> logger)
    {
        _logger = logger;
        
    }
    // GET
    [HttpGet("folder/{*path}")]
    public IActionResult Index(string path)
    {
        Console.WriteLine(path);
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}