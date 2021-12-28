using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    public class HWSensor
    {
        public HWThermometer thermometer { get; set; } = new HWThermometer();
        public HWAccelerometer accelerometer { get; set; } = new HWAccelerometer();
    }
}
