namespace TestTaskFolderHierarchy.Models;

public class FolderViewModel
{
    public string Name { get; set; } = string.Empty;
    public ICollection<FolderViewModel> SubFolders { get; set; } = new List<FolderViewModel>();
}