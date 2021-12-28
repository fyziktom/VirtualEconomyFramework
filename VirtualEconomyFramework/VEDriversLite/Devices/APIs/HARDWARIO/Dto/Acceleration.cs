using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    public class Acceleration
    {
        public double axis_x { get; set; } = 0.0;
        public double axis_y { get; set; } = 0.0;
        public double axis_z { get; set; } = 0.0;
    }
}
