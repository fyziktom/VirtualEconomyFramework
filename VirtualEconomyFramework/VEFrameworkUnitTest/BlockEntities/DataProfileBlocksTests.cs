using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using Xunit;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class DataProfileBlocksTests
    {
        private DataProfile CreateDataProfileTestData(double value = 3, 
                                                    int count = 10,
                                                    string name = "test",
                                                    BlockTimeframe timeframe = BlockTimeframe.Day,
                                                    DataProfileType type = DataProfileType.MultiplyCoeficient)
        {
            var start = new DateTime(2022, 1, 1);
            var end = start;

            var data = new List<double>();
            for (var i = 0; i < count; i++)
            {
                end = end.AddSeconds(count * BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, end).TotalSeconds);
                data.Add(value);
            }

            var profile = DataProfileHelpers.LoadDataProfileFromListOfValuesAndTimeframe(name,
                                                                                       data,
                                                                                       start,
                                                                                       end,
                                                                                       type,
                                                                                       timeframe);

            return profile;
        }

        [Fact]
        public void CreateProfileByDateTimeTest()
        {
            var value = 3;
            var days = 10;
            var name = "test";
            var start = new DateTime(2022,1,1);
            var end = start.AddDays(days);
            var data = new List<double>();
            for(var i = 0; i < days; i ++)
                data.Add(value);

            var profile = DataProfileHelpers.LoadDataProfileFromListOfValuesAndTimeframe(name, 
                                                                                       data, 
                                                                                       start, 
                                                                                       end, 
                                                                                       DataProfileType.MultiplyCoeficient,
                                                                                       BlockTimeframe.Day);
            Assert.Equal(name, profile.Name);
            Assert.Equal(days, profile.ProfileData.Count);
            Assert.Equal(start, profile.FirstDate);
        }

        [Fact]
        public void ProfileWithMonthFrame()
        {
            var timeframe = BlockTimeframe.Month;
            var profile = CreateDataProfileTestData(3, 12, "test", timeframe, DataProfileType.MultiplyCoeficient);
            
            Assert.Equal(12, profile.ProfileData.Count);
            var counter = 1;
            foreach (var data in profile.ProfileData)
            {
                Assert.Equal(3, data.Value);
                var month = counter < 10 ? "0" + counter.ToString() : counter.ToString();
                if (DateTime.TryParse($"2022-{month}-01T00:00:00", out var date))
                    Assert.Equal(date, data.Key);
                counter++;
            }
        }

        [Fact]
        public void ProfileWithYearFrame()
        {
            var timeframe = BlockTimeframe.Year;
            var profile = CreateDataProfileTestData(3, 5, "test", timeframe, DataProfileType.MultiplyCoeficient);

            Assert.Equal(5, profile.ProfileData.Count);
            var counter = 2022;
            foreach (var data in profile.ProfileData)
            {
                Assert.Equal(3, data.Value);
                var year = counter < 10 ? "0" + counter.ToString() : counter.ToString();
                if (DateTime.TryParse($"{year}-01-01T00:00:00", out var date))
                    Assert.Equal(date, data.Key);
                counter++;
            }
        }

        [Fact]
        public void ProfileWithHourFrame()
        {
            var timeframe = BlockTimeframe.Hour;
            var profile = CreateDataProfileTestData(3, 24, "test", timeframe, DataProfileType.MultiplyCoeficient);

            Assert.Equal(24, profile.ProfileData.Count);
            var counter = 0;
            foreach (var data in profile.ProfileData)
            {
                Assert.Equal(3, data.Value);
                var hour = counter < 10 ? "0" + counter.ToString() : counter.ToString();
                if (DateTime.TryParse($"2022-01-01T{hour}:00:00", out var date))
                    Assert.Equal(date, data.Key);
                counter++;
            }
        }

        [Fact]
        public void MultiplyBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDataProfileTestData(3, 10, "test", timeframe, DataProfileType.MultiplyCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 1;

            var mblocks = DataProfileHelpers.ActionBlockWithDataProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(3, mb.Amount);
        }

        [Fact]
        public void DivideBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDataProfileTestData(3, 10, "test", timeframe, DataProfileType.DivideCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 3;

            var mblocks = DataProfileHelpers.ActionBlockWithDataProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(1, mb.Amount);
        }

        [Fact]
        public void AddBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDataProfileTestData(3, 10, "test", timeframe, DataProfileType.AddCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 3;

            var mblocks = DataProfileHelpers.ActionBlockWithDataProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(6, mb.Amount);
        }

        [Fact]
        public void AddBlocksWithProfileMultipleDayBlock()
        {
            var datavalue = 3;
            var blockvalue = 3;
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDataProfileTestData(datavalue, 10, "test", timeframe, DataProfileType.AddCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = blockvalue;

            var blockdays = 3;
            var blocktimespan = new TimeSpan(blockdays, 0, 0);
            blocks.Add(new BaseBlock()
            {
                Id = Guid.NewGuid().ToString(),
                StartTime = profile.LastDate,
                Timeframe = blocktimespan,
                Amount = blockvalue * blockdays,
                Direction = BlockDirection.Consumed,
                Type = BlockType.Simulated
            });

            var mblocks = DataProfileHelpers.ActionBlockWithDataProfile(blocks, profile);
            foreach (var mb in mblocks)
            {
                if (mb.Timeframe == blocktimespan)
                    Assert.Equal((blockvalue + datavalue) * blockdays, mb.Amount);
                else
                    Assert.Equal(datavalue + blockvalue, mb.Amount);
            }
        }

        [Fact]
        public void SubtractBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDataProfileTestData(3, 10, "test", timeframe, DataProfileType.SubtractCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 3;

            var mblocks = DataProfileHelpers.ActionBlockWithDataProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(0, mb.Amount);
        }

        [Fact]
        public void LoadDataProfileDataFromRawWithDates()
        {
            var data = "2022-01-01T00:00:00\t3.0" +
                       "2022-01-02T00:00:00\t3.0" +
                       "2022-01-03T00:00:00\t3.0" +
                       "2022-01-04T00:00:00\t3.0" +
                       "2022-01-05T00:00:00\t3.0" +
                       "2022-01-06T00:00:00\t3.0" +
                       "2022-01-07T00:00:00\t3.0" +
                       "2022-01-08T00:00:00\t3.0" +
                       "2022-01-09T00:00:00\t3.0" +
                       "2022-01-10T00:00:00\t3.0";
            var profile = DataProfileHelpers.LoadDataProfileFromRawData(data, "\t", "test", DataProfileType.MultiplyCoeficient);

            var counter = 1;
            foreach (var pr in profile.ProfileData)
            {
                Assert.Equal(3, pr.Value);
                var day = counter < 10 ? "0" + counter.ToString() : counter.ToString();
                if(DateTime.TryParse($"2022-01-{day}T00:00:00", out var date))
                    Assert.Equal(date, pr.Key);
            }
        }

        [Fact]
        public void LoadDataProfileDataFromRaw()
        {
            var value = 3;
            var data = new List<double>();
            var start = new DateTime(2022, 1, 1);
            for (var i = 0; i < 10; i++)
                data.Add(value);

            var profile = DataProfileHelpers.LoadDataProfileFromListOfValuesAndTimeframe("test", 
                                                                                         data, 
                                                                                         start,
                                                                                         start.AddDays(10),
                                                                                         DataProfileType.MultiplyCoeficient,
                                                                                         BlockTimeframe.Day);

            var counter = 1;
            foreach (var pr in profile.ProfileData)
            {
                Assert.Equal(value, pr.Value);
                var day = counter < 10 ? "0" + counter.ToString() : counter.ToString();
                if (DateTime.TryParse($"2022-01-{day}T00:00:00", out var date))
                    Assert.Equal(date, pr.Key);
                counter++;
            }
        }

        [Fact]
        public void ImportExportProfile()
        {
            var value = 3;
            var data = new List<double>();
            var start = new DateTime(2022, 1, 1);
            for (var i = 0; i < 10; i++)
                data.Add(value);

            var profile = DataProfileHelpers.LoadDataProfileFromListOfValuesAndTimeframe("test",
                                                                                         data,
                                                                                         start,
                                                                                         start.AddDays(10),
                                                                                         DataProfileType.MultiplyCoeficient,
                                                                                         BlockTimeframe.Day);

            var export = DataProfileHelpers.ExportDataProfileToJson(profile);

            var inprofile = DataProfileHelpers.ImportDataProfileFromJson(export);

            Assert.Equal(profile.Name, inprofile.Name);
            Assert.Equal(profile.Type, inprofile.Type);
            Assert.Equal(profile.FirstDate, inprofile.FirstDate);
            Assert.Equal(profile.ProfileData.Count, inprofile.ProfileData.Count);

            foreach(var val in profile.ProfileData)
            {
                if (inprofile.ProfileData.TryGetValue(val.Key, out var inval))
                    Assert.Equal(val.Value, inval);
            }
        }
    }
}
