using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using Xunit;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class EntitiesSimulators
    {
        private PVPanelsGroupsHandler PVEGrid = new PVPanelsGroupsHandler();
        private Coordinates coord = new Coordinates(49.194103, 16.608998);

        private DataProfile CreateDummyYearTDD(double amount = 1.0)
        {
            var result = new DataProfile();

            var start = new DateTime(2022, 1, 1, 0, 0, 0);
            var end = start.AddYears(1);

            var tmp = start;
            while(tmp < end)
            {
                result.ProfileData.TryAdd(tmp, amount);
                tmp = tmp.AddHours(1);
            }

            return result;
        }

        private PVPanel GetCommonPanel()
        {
            return new PVPanel()
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
                PeakPower = 1,
                PanelPeakAngle = MathHelpers.DegreeToRadians(90)
            };
        }

        [Fact]
        public void PVESimulatorInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var firstJuly2022PVE1kWhProduction = 0.975166397260274;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var PVESim = new PVPanelsGroupsHandler();
            PVESim.SetCommonPanel(GetCommonPanel());
            var southPanelsId = PVESim.AddGroup("South");
            PVESim.AddPanelToGroup(southPanelsId, PVESim.CommonPanel, 1).ToList();

            var addsimres = entity.AddSimulator(PVESim);
            if (addsimres.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach(var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(firstJuly2022PVE1kWhProduction, total);
            }
        }

        [Fact]
        public void TwoPVESimulatorsInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var firstJuly2022PVE1kWhProduction = 0.975166397260274;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var PVESim = new PVPanelsGroupsHandler();
            PVESim.SetCommonPanel(GetCommonPanel());
            var southPanelsId = PVESim.AddGroup("South");
            PVESim.AddPanelToGroup(southPanelsId, PVESim.CommonPanel, 1).ToList();


            var PVESim2 = new PVPanelsGroupsHandler();
            PVESim2.SetCommonPanel(GetCommonPanel());
            var southPanelsId2 = PVESim2.AddGroup("South");
            PVESim2.AddPanelToGroup(southPanelsId2, PVESim2.CommonPanel, 1).ToList();

            var addsimres = entity.AddSimulator(PVESim);
            var addsimres2 = entity.AddSimulator(PVESim2);
            if (addsimres.Item1 && addsimres2.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(2 * firstJuly2022PVE1kWhProduction, total);
            }
        }

        [Fact]
        public void TDDSimulatorInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var consumptionPerHour = 1;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var tddSim = new ConsumerTDDSimulator();
            tddSim.TDDs.Add(CreateDummyYearTDD(consumptionPerHour));

            var addsimres = entity.AddSimulator(tddSim);
            if (addsimres.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(-consumptionPerHour * 24 * days, total);
            }
        }

        [Fact]
        public void TwoTDDSimulatorInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var consumptionPerHour = 1;
            var consumptionPerHour2 = 2;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var tddSim = new ConsumerTDDSimulator();
            tddSim.TDDs.Add(CreateDummyYearTDD(consumptionPerHour));

            var tddSim2 = new ConsumerTDDSimulator();
            tddSim2.TDDs.Add(CreateDummyYearTDD(consumptionPerHour2));

            var addsimres = entity.AddSimulator(tddSim);
            var addsimres2 = entity.AddSimulator(tddSim2);
            if (addsimres.Item1 && addsimres2.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(-(consumptionPerHour + consumptionPerHour2) * 24 * days, total);
            }
        }

        [Fact]
        public void TDDandPVESimulatorInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var firstJuly2022PVE1kWhProduction = 0.975166397260274;
            var consumptionPerHour = 1;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var PVESim = new PVPanelsGroupsHandler();
            PVESim.SetCommonPanel(GetCommonPanel());
            var southPanelsId = PVESim.AddGroup("South");
            PVESim.AddPanelToGroup(southPanelsId, PVESim.CommonPanel, 1).ToList();

            var tddSim = new ConsumerTDDSimulator();
            tddSim.TDDs.Add(CreateDummyYearTDD(consumptionPerHour));

            var addsimres = entity.AddSimulator(tddSim);
            var addsimres1 = entity.AddSimulator(PVESim);

            if (addsimres.Item1 && addsimres1.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(firstJuly2022PVE1kWhProduction - consumptionPerHour * 24 * days, total);
            }
        }

        [Fact]
        public void TwoTDDandPVESimulatorInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var firstJuly2022PVE1kWhProduction = 0.975166397260274;
            var consumptionPerHour = 1;
            var consumptionPerHour1 = 2;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var PVESim = new PVPanelsGroupsHandler();
            PVESim.SetCommonPanel(GetCommonPanel());
            var southPanelsId = PVESim.AddGroup("South");
            PVESim.AddPanelToGroup(southPanelsId, PVESim.CommonPanel, 1).ToList();

            var tddSim = new ConsumerTDDSimulator();
            tddSim.TDDs.Add(CreateDummyYearTDD(consumptionPerHour));
            var tddSim1 = new ConsumerTDDSimulator();
            tddSim1.TDDs.Add(CreateDummyYearTDD(consumptionPerHour1));

            var addsimres = entity.AddSimulator(tddSim);
            var addsimres1 = entity.AddSimulator(tddSim1);
            var addsimres2 = entity.AddSimulator(PVESim);

            if (addsimres.Item1 && addsimres1.Item1 && addsimres2.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(firstJuly2022PVE1kWhProduction - (consumptionPerHour + consumptionPerHour1) * 24 * days, total);
            }
        }

        [Fact]
        public void TwoTDDandPVESimulatorInEntityDayFrameTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var PVE1kWh2022YearProduction = 577.9884758630136;
            var consumptionPerHour = 1;
            var consumptionPerHour1 = 2;
            var days = 365;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            var PVESim = new PVPanelsGroupsHandler();
            PVESim.SetCommonPanel(GetCommonPanel());
            var southPanelsId = PVESim.AddGroup("South");
            PVESim.AddPanelToGroup(southPanelsId, PVESim.CommonPanel, 1).ToList();

            var tddSim = new ConsumerTDDSimulator();
            tddSim.TDDs.Add(CreateDummyYearTDD(consumptionPerHour));
            var tddSim1 = new ConsumerTDDSimulator();
            tddSim1.TDDs.Add(CreateDummyYearTDD(consumptionPerHour1));

            var addsimres = entity.AddSimulator(tddSim);
            var addsimres1 = entity.AddSimulator(tddSim1);
            var addsimres2 = entity.AddSimulator(PVESim);

            if (addsimres.Item1 && addsimres1.Item1 && addsimres2.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Day, start, end, true, null, null, true);
                Assert.Equal(days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(PVE1kWh2022YearProduction - (consumptionPerHour + consumptionPerHour1) * 24 * days, total);
            }
        }

        [Fact]
        public void DeviceSimulatorInEntityTest()
        {
            DateTime start = new DateTime(2022, 1, 1, 0, 0, 0);
            var consumptionPerHour = 1;
            var days = 1;
            var end = start.AddDays(days);

            var entity = new BaseEntity();

            double[] acRun = new double[24]
            {
                0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.5, 0.5, 0.5, 0.5
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var deviceSim = new DeviceSimulator(acRun, 2);
                        
            var addsimres = entity.AddSimulator(deviceSim);
            if (addsimres.Item1)
            {
                var consumption = entity.GetSummedValues(BlockTimeframe.Hour, start, end, true, null, null, true);
                Assert.Equal(24 * days, consumption.Count);

                var total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(-consumptionPerHour * 36 * days, total);

                consumption = entity.GetSummedValues(BlockTimeframe.Day, start, end, true, null, null, true);
                Assert.Equal(days, consumption.Count);

                total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(-consumptionPerHour * 36 * days, total);

                consumption = entity.GetSummedValues(BlockTimeframe.QuaterHour, start, end, true, null, null, true);
                Assert.Equal(24 * 4 * days, consumption.Count);

                total = 0.0;
                foreach (var cons in consumption)
                    total += cons.Amount;

                Assert.Equal(-consumptionPerHour * 36 * days, total);
            }
        }

    }
}
