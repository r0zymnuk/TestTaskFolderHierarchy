using Microsoft.EntityFrameworkCore;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }
    
    public DbSet<Folder> Folders { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Folder>()
            .HasMany(f => f.SubFolders)
            .WithOne()
            .HasForeignKey(f => f.ParentId);
    }
}