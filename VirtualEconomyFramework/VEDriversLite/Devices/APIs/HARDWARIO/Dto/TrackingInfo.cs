using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    public class TrackingInfo
    {
        public long time { get; set; } = 0;
        public double latitude { get; set; } = 0.0;
        public double longitude { get; set; } = 0.0;
    }
}
