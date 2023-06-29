namespace TestTaskFolderHierarchy.Models;

public class Folder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
    public Guid? ParentId { get; set; }
    
}