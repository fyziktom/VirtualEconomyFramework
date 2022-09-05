using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    public class StreamResponseDto
    {
        public string? Filename { get; set; }
        public Stream? Data { get; set; }
    }
}
