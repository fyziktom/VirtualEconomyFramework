using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using Xunit;

namespace VEFrameworkUnitTest.Energy.PVESimulator
{
    public class PVESimulation
    {
        [Fact]
        public void GetSunSetSunRiseTest()
        {
            var coord = new Coordinates(49.194103, 16.608998);

            SunMoonCalcs.SunCalc.GetTimes(new DateTime(2022, 10, 08, 0, 0, 0),
                              coord.Latitude,
                              coord.Longitude,
                              out var rise,
                              out var set);

            Assert.Equal(5, rise.Hour);
            Assert.Equal(04, rise.Minute);
            Assert.Equal(17, rise.Second);
            Assert.Equal(16, set.Hour);
            Assert.Equal(20, set.Minute);
            Assert.Equal(23, set.Second);
        }
        [Fact]
        public void SunBeamAngleTest()
        {
            var start = new DateTime(2022, 1, 1, 0, 0, 0);
            var coord = new Coordinates(49.194103, 16.608998);

            var pos = SunMoonCalcs.SunCalc.GetPosition(start, coord.Latitude, coord.Longitude);

            var result = SunMoonCalcs.SunCalc.GetSunBeamAngle(pos,
                                                              MathHelpers.DegreeToRadians(20),
                                                              MathHelpers.DegreeToRadians(10), true, true);

            Assert.Equal(-78, result);

            start = new DateTime(2022, 1, 1, 12, 0, 0);
            pos = SunMoonCalcs.SunCalc.GetPosition(start, coord.Latitude, coord.Longitude);

            result = SunMoonCalcs.SunCalc.GetSunBeamAngle(pos,
                                                          MathHelpers.DegreeToRadians(20),
                                                          MathHelpers.DegreeToRadians(10), true, true);

            Assert.Equal(36, result);

            start = new DateTime(2022, 1, 1, 20, 0, 0);
            pos = SunMoonCalcs.SunCalc.GetPosition(start, coord.Latitude, coord.Longitude);

            result = SunMoonCalcs.SunCalc.GetSunBeamAngle(pos,
                                                          MathHelpers.DegreeToRadians(20),
                                                          MathHelpers.DegreeToRadians(10), true, true);

            Assert.Equal(-46, result);
        }


        private PVPanelsGroupsHandler PVEGrid = new PVPanelsGroupsHandler();
        Coordinates coord = new Coordinates(49.194103, 16.608998);

        [Fact]
        public void GetYearPrediction()
        {
            string name = "mytest";
            int DurationInDays = 1;
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            IEnumerable<(string, string)> groupIds = new List<(string, string)>();

            var panelAzimuthE = MathHelpers.DegreeToRadians(-40);
            var panelAzimuthS = MathHelpers.DegreeToRadians(0);
            var panelAzimuthW = MathHelpers.DegreeToRadians(40);

            var eastPanelsId = PVEGrid.AddGroup("East");
            var southPanelsId = PVEGrid.AddGroup("South");
            var westPanelsId = PVEGrid.AddGroup("West");

            SetCommonPanel();
            // set template panel in this PVE
            AddPanelToGroup(eastPanelsId, panelAzimuthE, true, 5);
            AddPanelToGroup(southPanelsId, panelAzimuthS, true, 5);
            AddPanelToGroup(westPanelsId, panelAzimuthW, true, 5);

            groupIds = PVEGrid.PVPanelsGroups.Values.Where(p => p.Id != null).Select(p => (p.Id, p.Name));

            var final = new Dictionary<string, List<IBlock>>();
            foreach (var group in groupIds)
            {
                IEnumerable<IBlock> data = new List<IBlock>();
                data = PVEGrid.GetGroupPeakPowerInYearBlock(group.Item1, start.Year, coord, 1);
                final.TryAdd(group.Item1, data.ToList());
            }

            foreach(var blocks in final)
                foreach(var block in blocks.Value)
                    Console.WriteLine($"Block in the {blocks.Key}, Date {block.StartTime.ToString("dd.MM.yyyy")}, Amount {block.Amount} kWh.");
        }

        private void SetCommonPanel()
        {
            if (PVEGrid != null)
            {
                var panel = new PVPanel()
                {
                    Name = "test",
                    Azimuth = 0,
                    BaseAngle = MathHelpers.DegreeToRadians(23),
                    DirtRatio = 0.05 / 365,
                    Efficiency = 1,
                    Height = 2000,
                    Width = 1000,
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude,
                    PeakPower = 0.3,
                    PanelPeakAngle = MathHelpers.DegreeToRadians(90)
                };
                PVEGrid.SetCommonPanel(panel);
            }
        }

        private void AddPanelToGroup(string groupId, double azimuth = 0, bool setAzimuth = false, int count = 1)
        {
            SetCommonPanel();
            if (setAzimuth)
                PVEGrid.CommonPanel.Azimuth = azimuth;

            var addedPanelsId = PVEGrid.AddPanelToGroup(groupId, PVEGrid.CommonPanel, count).ToList();
        }


        [Fact]
        public void ExportImportSettings()
        {
            string name = "mytest";
            int DurationInDays = 1;
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            IEnumerable<(string, string)> groupIds = new List<(string, string)>();

            var panelAzimuthE = MathHelpers.DegreeToRadians(-40);
            var panelAzimuthS = MathHelpers.DegreeToRadians(0);
            var panelAzimuthW = MathHelpers.DegreeToRadians(60);

            var eastPanelsId = PVEGrid.AddGroup("East");
            var southPanelsId = PVEGrid.AddGroup("South");
            var westPanelsId = PVEGrid.AddGroup("West");

            SetCommonPanel();
            // set template panel in this PVE
            AddPanelToGroup(eastPanelsId, panelAzimuthE, true, 3);
            AddPanelToGroup(southPanelsId, panelAzimuthS, true, 6);
            AddPanelToGroup(westPanelsId, panelAzimuthW, true, 8);

            var config = PVEGrid.ExportConfig();

            PVEGrid = new PVPanelsGroupsHandler();

            PVEGrid.ImportConfig(config.Item2);

            Assert.Equal(17, PVEGrid.PanelCount);
            Assert.Equal(49.194103, PVEGrid.MedianLatitude);
            Assert.Equal(16.608998, PVEGrid.MedianLongitude);
        }
    }
}
