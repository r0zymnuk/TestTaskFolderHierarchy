using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestTaskFolderHierarchy.Models
{
    public class ServiceResponse<T> where T : class
    {
        public T? Data { get; set; }
        public string Path { get; set; } = "";
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        
    }
}