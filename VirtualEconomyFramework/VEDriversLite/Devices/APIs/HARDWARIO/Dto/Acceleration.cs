using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    /// <summary>
    /// Dto for HARDWARIO CHESTER data about acceleration
    /// </summary>
    public class Acceleration
    {
        /// <summary>
        /// X Axis Data
        /// </summary>
        public double axis_x { get; set; } = 0.0;
        /// <summary>
        /// Y Axis Data
        /// </summary>
        public double axis_y { get; set; } = 0.0;
        /// <summary>
        /// Z Axis data
        /// </summary>
        public double axis_z { get; set; } = 0.0;
    }
}
