using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    /// <summary>
    /// Dto for HARDWARIO CHESTER data about thermometer
    /// </summary>
    public class HWThermometer
    {
        /// <summary>
        /// Temperature on the sensor
        /// </summary>
        public double temperature { get; set; } = 0.0;
    }
}
