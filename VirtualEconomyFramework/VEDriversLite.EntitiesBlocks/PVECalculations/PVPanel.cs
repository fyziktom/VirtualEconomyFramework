using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;

namespace VEDriversLite.EntitiesBlocks.PVECalculations
{
    /// <summary>
    /// Basic object of PVE Panel
    /// </summary>
    public class PVPanel
    {
        /// <summary>
        /// Id of the solar panel
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Name of the panel
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Name of the type of the panel - from producer
        /// </summary>
        public string TypeName { get; set; } = string.Empty;
        /// <summary>
        /// Peak power output of this panel
        /// </summary>
        public double PeakPower { get; set; }
        /// <summary>
        /// Angle of sun against panel where to achieve peak power 
        /// In radians
        /// </summary>
        public double PanelPeakAngle { get; set; } = Math.PI / 4;
        /// <summary>
        /// Angle against base earth plane
        /// In radians
        /// </summary>
        public double BaseAngle { get; set; }
        /// <summary>
        /// Angle of the panel against south
        /// In radians
        /// </summary>
        public double Azimuth { get; set; }
        /// <summary>
        /// Id of the group of the panels where this panel belongs
        /// </summary>
        public string GroupId { get; set; } = string.Empty;
        /// <summary>
        /// Width of the panel in mm
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// Height of the panel in mm
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// Center width of the panel in mm
        /// </summary>
        public double CenterWidth { get => Width / 2; }
        /// <summary>
        /// Center height of the panel in mm
        /// </summary>
        public double CenterHeight { get => Height / 2; }
        /// <summary>
        /// Efficiency coeficient of the panel
        /// 1 - means 100% efficiency from sun to output
        /// </summary>
        public double Efficiency { get; set; }
        /// <summary>
        /// Dirt ratio coeficient of the panel
        /// 1 - means 100% celan panel without any dirt
        /// </summary>
        public double DirtRatio { get; set; }
        /// <summary>
        /// Latitude of the panel position 
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Longitude of the panel position
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Azimut of the panel in the degrees
        /// </summary>
        public double AzimutInDegrees { get => MathHelpers.RadiansToDegrees(Azimuth); }
        /// <summary>
        /// Base angle of the panel in the degrees
        /// </summary>
        public double BaseAngleInDegrees { get => MathHelpers.RadiansToDegrees(BaseAngle); }
        /// <summary>
        /// Panel peak angle in the degrees
        /// </summary>
        public double PanelPeakAngleInDegrees { get => MathHelpers.RadiansToDegrees(PanelPeakAngle); }

    }
}
