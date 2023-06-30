using AutoMapper;
using TestTaskFolderHierarchy.Models;

namespace TestTaskFolderHierarchy;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Folder, FolderViewModel>();
    }
    
}