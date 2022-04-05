using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    /// <summary>
    /// Dto for HARDWARIO CHESTER data about acceleration
    /// </summary>
    public class HWAccelerometer
    {
        /// <summary>
        /// Data about axceleration
        /// </summary>
        public Acceleration acceleration { get; set; } = new Acceleration();
        /// <summary>
        /// Orientation of the accelerometer
        /// </summary>
        public double orientation { get; set; } = 0.0;
    }
}
