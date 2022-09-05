using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    public class ReadFileRequestDto
    {
        public string Hash { get; set; }
        public bool GetFromLocalIfAvailable { get; set; }
        public string? StorageId { get; set; }
        public StorageDriverType DriverType { get; set; }
    }
}
