using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class StorageSettings
    {
        public string? ConnectionString { get; set; }
        public string? ContainerName { get; set; }
        public Dictionary<string, string> ContainerEnvironmentNames { get; set; }
    }
}
