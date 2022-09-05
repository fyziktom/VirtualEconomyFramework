using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    public class WriteStreamRequestDto
    {
        public string? Filename { get; set; }
        public Stream? Data { get; set; }
        public bool BackupInLocal { get; set; }
        public string? StorageId { get; set; }
        public StorageDriverType DriverType { get; set; }
    }
}
