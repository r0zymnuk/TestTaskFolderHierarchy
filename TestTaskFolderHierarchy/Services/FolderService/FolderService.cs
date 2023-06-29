using Microsoft.EntityFrameworkCore;
using TestTaskFolderHierarchy.Data;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Services.FolderService;

public class FolderService : IFolderService
{
    private readonly DataContext _context;

    public FolderService(DataContext context)
    {
        _context = context;
    }
    
    public List<Folder> GetFolders()
    {
        return _context.Folders.ToList();
    }

    public List<Folder> GetFolderById(Guid id)
    {
        throw new NotImplementedException();
    }
    
    public List<Folder> GetFolderByName(string name)
    {
        throw new NotImplementedException();
    }

    public List<Folder> CreateFolder(Folder folder)
    {
        throw new NotImplementedException();
    }
}