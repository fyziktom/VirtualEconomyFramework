using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.StorageCalculations;
using Xunit;

namespace VEFrameworkUnitTest.Energy.StorageSimulator
{
    public class BatteryStorageSimulationTests
    {
        private DataProfile CreatekWhDataProfile(BlockTimeframe timeframe, double amountperhour, DateTime start, DateTime end)
        {
            var result = new DataProfile() { Type = DataProfileType.AddCoeficient };
            var step = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, start);
            var tmp = start;
            while (tmp < end)
            {
                result.ProfileData.Add(tmp, amountperhour * 1000 * step.TotalHours); // input in kWh
                tmp += step;
            }

            return result;
        }

        private BatteryBlockHandler CreateStorage()
        {
            return new BatteryBlockHandler(null,
                                           "mainbattery",
                                           BatteryBlockHandler.DefaultChargingFunction,
                                           BatteryBlockHandler.DefaultDischargingFunction);
        }

        [Fact]
        public void AddBatteryToStorage()
        {
            var storage = CreateStorage();

            Assert.Equal("mainbattery", storage.Name);

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 500,
                MaximumDischargePower = 2000
            });

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 500,
                MaximumDischargePower = 2000
            });

            Assert.Equal(2, storage.BatteryBlocks.Count);
            Assert.Equal(20000, storage.TotalCapacity);
            Assert.Equal(500, storage.AverageMaxChargePower);
            Assert.Equal(2000, storage.AverageMaxDischargePower);
        }

        [Fact]
        public void RemoveBatteryFromStorage()
        {
            var storage = CreateStorage();

            Assert.Equal("mainbattery", storage.Name);

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 500,
                MaximumDischargePower = 2000
            });

            var batId = storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 500,
                MaximumDischargePower = 2000
            });

            Assert.Equal(2, storage.BatteryBlocks.Count);
            Assert.Equal(20000, storage.TotalCapacity);
            Assert.Equal(500, storage.AverageMaxChargePower);
            Assert.Equal(2000, storage.AverageMaxDischargePower);

            if (storage.RemoveBatteryBlock(batId))
            {
                Assert.Equal(1, storage.BatteryBlocks.Count);
                Assert.Equal(10000, storage.TotalCapacity);
                Assert.Equal(500, storage.AverageMaxChargePower);
                Assert.Equal(2000, storage.AverageMaxDischargePower);
            }
        }

        [Fact]
        public void LoadProfileToBatteryStorage()
        {
            var start = new DateTime(2022, 1, 1);
            var storage = CreateStorage();

            Assert.Equal("mainbattery", storage.Name);

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            });

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            });

            var source = CreatekWhDataProfile(BlockTimeframe.Minute, 1, start, start.AddDays(1));

            var result = storage.GetChargingProfileData(source).ToList();
            var res = source.ProfileData.FirstOrDefault();
            Assert.NotNull(res);
            Assert.Equal(1440, result.Count);
            Assert.Equal(res.Value * source.ProfileData.Count, Math.Round(storage.TotalActualFilledCapacity, 0));
        }

        [Fact]
        public void DischargeProfileOfBatteryStorage()
        {
            var start = new DateTime(2022, 1, 1);
            var storage = CreateStorage();

            Assert.Equal("mainbattery", storage.Name);

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            });

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            });

            var source = CreatekWhDataProfile(BlockTimeframe.Minute, 1, start, start.AddDays(1));

            var chargingresult = storage.GetChargingProfileData(source).ToList();
            var res = source.ProfileData.FirstOrDefault();
            Assert.Equal(1440, chargingresult.Count);
            Assert.Equal(res.Value * source.ProfileData.Count, Math.Round(storage.TotalActualFilledCapacity, 0));

            var result = storage.GetGetDischargingProfileData(source).ToList();
            Assert.NotNull(res);
            Assert.Equal(1440, result.Count);
            Assert.Equal(0, Math.Round(storage.TotalActualFilledCapacity, 0));
        }
    }
}
