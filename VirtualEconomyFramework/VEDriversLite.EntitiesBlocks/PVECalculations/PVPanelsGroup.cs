using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.Calendar;

namespace VEDriversLite.EntitiesBlocks.PVECalculations
{
    /// <summary>
    /// Panels group is for example set of the panels on some specific side of the roof
    /// </summary>
    public class PVPanelsGroup
    {
        /// <summary>
        /// Id of panels group
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Name of the panel Group
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Total count of Panels in group
        /// </summary>
        public int PanelCount { get => PVPanels.Count; }
        /// <summary>
        /// Total Peak power of all panels in the group
        /// </summary>
        public double TotalPeakPower { get => PVPanels.Values.Where(p => p.PeakPower > 0).Select(p => p.PeakPower).Sum(); }
        /// <summary>
        /// Total width of all panels if imagine in one row
        /// </summary>
        public double TotalWidth { get => PVPanels.Values.Where(p => p.Width > 0).Select(p => p.Width).Sum(); }
        /// <summary>
        /// total width of all panels if imagine in one column
        /// </summary>
        public double TotalHeight { get => PVPanels.Values.Where(p => p.Height > 0).Select(p => p.Height).Sum(); }
        /// <summary>
        /// Median Sun Beam To Panel Peak Angle (where Peak Power was measured) across all panels in this group
        /// </summary>
        public double MedianPanelPeakAngle { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.PanelPeakAngle > 0).Select(p => p.PanelPeakAngle)); }
        /// <summary>
        /// Median Azimuth across all panels in this group
        /// </summary>
        public double MedianAzimuth { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.Azimuth > 0).Select(p => p.Azimuth)); }
        /// <summary>
        /// Median Efficiency across all panels in this group
        /// </summary>
        public double MedianEfficiency { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.Efficiency > 0).Select(p => p.Efficiency)); }
        /// <summary>
        /// Median Dirt ratio across all panels in this group
        /// </summary>
        public double MedianDirtRatio { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.DirtRatio > 0).Select(p => p.DirtRatio)); }
        /// <summary>
        /// Median Latitude across all panels in this group
        /// </summary>
        public double MedianLatitude { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.Latitude > 0).Select(p => p.Latitude)); }
        /// <summary>
        /// Median Longitude across all panels in this group
        /// </summary>
        public double MedianLongitude { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.Longitude > 0).Select(p => p.Longitude)); }
        /// <summary>
        /// Median Base angle (against the earth plane) across all panels in this group
        /// </summary>
        public double MedianBaseAngle { get => MathHelpers.Median<double>(PVPanels.Values.Where(p => p.BaseAngle > 0).Select(p => p.BaseAngle)); }
        /// <summary>
        /// Dictionary of all PV Panels in this group
        /// </summary>
        public ConcurrentDictionary<string, PVPanel> PVPanels { get; set; } = new ConcurrentDictionary<string, PVPanel>();

        /// <summary>
        /// Add PVPanel to the group
        /// </summary>
        /// <param name="panel"></param>
        /// <returns>Id of new added panel</returns>
        public string AddPanel(PVPanel panel)
        {
            if (panel == null || string.IsNullOrEmpty(panel.Id))
                return null;
            if (!PVPanels.ContainsKey(panel.Id))
                PVPanels.TryAdd(panel.Id, panel);

            return panel.Id;
        }
        /// <summary>
        /// Remove panel from the group based on Id
        /// </summary>
        /// <param name="panelId">Panel Id</param>
        /// <returns>true if success</returns>
        public bool RemovePanel(string panelId)
        {
            if (string.IsNullOrEmpty(panelId))
                return false;
            if (PVPanels.TryRemove(panelId, out var panel))
                    return true;

            return false;
        }
        /// <summary>
        /// Get panel which contains median values of all panels in the group
        /// </summary>
        /// <returns></returns>
        public PVPanel GetMedianPanel()
        {
            var result = new PVPanel()
            {
                Azimuth = MedianAzimuth,
                Efficiency = MedianEfficiency,
                DirtRatio = MedianDirtRatio,
                Latitude = MedianLatitude,
                Longitude = MedianLongitude,
                BaseAngle = MedianBaseAngle,
                PeakPower = TotalPeakPower,
                GroupId = Id,
                PanelPeakAngle = MedianPanelPeakAngle
            };
            return result;
        }

        /// <summary>
        /// Get group peak power across all panels in the group based on the DateTime
        /// It affects sun position so it will change the real expected power in the specific date and time
        /// </summary>
        /// <param name="start">Date time of the moment</param>
        /// <param name="coord">Coordinates on planet</param>
        /// <param name="weatherFactor">Weather efficiency factor</param>
        /// <returns></returns>
        public double GetGroupPeakPowerInDateTime(DateTime start, Coordinates coord, double weatherFactor)
        {
            var pos = SunMoonCalcs.SunCalc.GetPosition(start,
                                                       coord.Latitude,
                                                       coord.Longitude);
            var panel = PVPanels.Values.FirstOrDefault();
            if (panel != null)
            {
                var sunbeamangle = SunMoonCalcs.SunCalc.GetSunBeamAngle(pos, panel.BaseAngle, panel.Azimuth, true, true);

                var result = PVECalcs.GetTotalPeakPowerOfPanel(TotalPeakPower,
                                                               MedianEfficiency,
                                                               weatherFactor,
                                                               panel.PanelPeakAngleInDegrees,
                                                               sunbeamangle,
                                                               true,
                                                               true);

                // expect clean of the panel per year
                result = result * (1 - MedianDirtRatio * start.DayOfYear);

                return result;
            }
            else
            {
                return 0;
            }
        }
    }
}
