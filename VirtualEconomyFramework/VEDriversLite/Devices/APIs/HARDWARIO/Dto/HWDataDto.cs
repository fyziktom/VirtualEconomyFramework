using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    public class HWDataDto
    {
        public TrackingInfo tracking { get; set; } = new TrackingInfo();
        public HWSensor sensor { get; set; } = new HWSensor();
    }
}
