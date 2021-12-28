using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    public class HWAccelerometer
    {
        public Acceleration acceleration { get; set; } = new Acceleration();
        public double orientation { get; set; } = 0.0;
    }
}
