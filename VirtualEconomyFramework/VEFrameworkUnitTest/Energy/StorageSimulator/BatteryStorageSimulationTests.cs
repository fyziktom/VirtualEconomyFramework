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
            }).ToList();

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 500,
                MaximumDischargePower = 2000
            }).ToList();

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
            }).ToList();

            var batIds = storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 500,
                MaximumDischargePower = 2000
            }).ToList();

            var batId = batIds.ToList().FirstOrDefault();

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
            }).ToList();

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            }).ToList();

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
            }).ToList();

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            }).ToList();

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

        [Fact]
        public void ImportExportBatteryStorageConfigTet()
        {
            var start = new DateTime(2022, 1, 1);
            var storage = CreateStorage();

            Assert.Equal("mainbattery", storage.Name);

            storage.SetCommonBattery(new BatteryBlock()
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
            }).ToList();

            storage.AddBatteryBlock(new BatteryBlock()
            {
                Id = Guid.NewGuid().ToString(),
                Capacity = 10000,
                InternalResistance = 0.1,
                MaximumChargePower = 2000,
                MaximumDischargePower = 2000
            }).ToList();

            var export = storage.ExportSettingsToJSON();

            var storage1 = CreateStorage();
            storage1.Id = "";
            storage1.Name = "";

            if (storage1.ImportConfigFromJson(export))
            {
                Assert.Equal("mainbattery", storage1.Name);
                Assert.Equal(20000, storage1.TotalCapacity);
                Assert.Equal(2, storage1.BatteryBlocks.Count);
                Assert.Equal(storage.CommonBattery.Id, storage1.CommonBattery.Id);
                Assert.Equal(storage.CommonBattery.Capacity, storage1.CommonBattery.Capacity);
                Assert.Equal(storage.CommonBattery.MaximumChargePower, storage1.CommonBattery.MaximumChargePower);
                Assert.Equal(storage.CommonBattery.MaximumDischargePower, storage1.CommonBattery.MaximumDischargePower);

            }
        }
    }
}
