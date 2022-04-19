using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Imaging.Xray.Dto
{
    /// <summary>
    /// Exposure parameters of the captured image
    /// </summary>
    public class XrayExposureParameters
    {
        /// <summary>
        /// Anode voltage on the X-ray tube in the kilo Volts - kV
        /// Usuall range is between 20 - 150 kV. 
        /// For casting or welding it can be even 225kV, 450kV or more.
        /// </summary>
        public double Voltage { get; set; } = 0.0;
        /// <summary>
        /// Current through the X-ray tube in the micro Ampers - uA
        /// Usuall range starts from 50uA and can go up to 1-10 mili Ampers (mA) or more for casting/welding
        /// </summary>
        public double Current { get; set; } = 0.0;
        /// <summary>
        /// List of the primary filters. These filters are real materials placed directly on the outpu window of the Xray tube
        /// </summary>
        public List<XrayPrimaryFilter> Filters { get; set; } = new List<XrayPrimaryFilter>();
        /// <summary>
        /// Exposure time in ms - how long X-ray goes to the detector per one captured image/frame
        /// Usuall range starts from 10ms and can goes up to 10 seconds. More in special cases or casting/welding.
        /// </summary>
        public double ExposureTime { get; set; } = 0.0;
        /// <summary>
        /// Returns true if the all values inside class are in default values
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return (Voltage == 0.0 && Current == 0.0 && Filters.Count == 0);
        }
    }
}
