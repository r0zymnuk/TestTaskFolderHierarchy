namespace TestTaskFolderHierarchy.Models;

public class FolderViewModel
{
    public string Name { get; set; }
    ICollection<FolderViewModel> SubFolders { get; set; } = new List<FolderViewModel>();
}