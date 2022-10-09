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
    public class DayProfileBlocksTests
    {
        private DayProfile CreateDayProfileTestData(double value = 3, 
                                                    int count = 10,
                                                    string name = "test",
                                                    BlockTimeframe timeframe = BlockTimeframe.Day,
                                                    DayProfileType type = DayProfileType.MultiplyCoeficient)
        {
            var start = new DateTime(2022, 1, 1);
            var end = start;

            var data = new List<double>();
            for (var i = 0; i < count; i++)
            {
                end = end.AddSeconds(count * BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, end).TotalSeconds);
                data.Add(value);
            }

            var profile = DayProfileHelpers.LoadDayProfileFromListOfValuesAndTimeframe(name,
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

            var profile = DayProfileHelpers.LoadDayProfileFromListOfValuesAndTimeframe(name, 
                                                                                       data, 
                                                                                       start, 
                                                                                       end, 
                                                                                       DayProfileType.MultiplyCoeficient,
                                                                                       BlockTimeframe.Day);
            Assert.Equal(name, profile.Name);
            Assert.Equal(days, profile.ProfileData.Count);
            Assert.Equal(start, profile.FirstDate);
        }

        [Fact]
        public void ProfileWithMonthFrame()
        {
            var timeframe = BlockTimeframe.Month;
            var profile = CreateDayProfileTestData(3, 12, "test", timeframe, DayProfileType.MultiplyCoeficient);
            
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
            var profile = CreateDayProfileTestData(3, 5, "test", timeframe, DayProfileType.MultiplyCoeficient);

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
            var profile = CreateDayProfileTestData(3, 24, "test", timeframe, DayProfileType.MultiplyCoeficient);

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
            var profile = CreateDayProfileTestData(3, 10, "test", timeframe, DayProfileType.MultiplyCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 1;

            var mblocks = DayProfileHelpers.ActionBlockWithDayProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(3, mb.Amount);
        }

        [Fact]
        public void DivideBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDayProfileTestData(3, 10, "test", timeframe, DayProfileType.DivideCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 3;

            var mblocks = DayProfileHelpers.ActionBlockWithDayProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(1, mb.Amount);
        }

        [Fact]
        public void AddBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDayProfileTestData(3, 10, "test", timeframe, DayProfileType.AddCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 3;

            var mblocks = DayProfileHelpers.ActionBlockWithDayProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(6, mb.Amount);
        }

        [Fact]
        public void SubtractBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDayProfileTestData(3, 10, "test", timeframe, DayProfileType.SubtractCoeficient);
            var blocks = BlockHelpers.GetResultBlocks(timeframe, profile.FirstDate, profile.LastDate, profile.Name);
            foreach (var block in blocks)
                block.Amount = 3;

            var mblocks = DayProfileHelpers.ActionBlockWithDayProfile(blocks, profile);
            foreach (var mb in mblocks)
                Assert.Equal(0, mb.Amount);
        }

        [Fact]
        public void LoadDayProfileDataFromRawWithDates()
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
            var profile = DayProfileHelpers.LoadDayProfileFromRawData(data, "\t", "test", DayProfileType.MultiplyCoeficient);

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
        public void LoadDayProfileDataFromRaw()
        {
            var value = 3;
            var data = new List<double>();
            var start = new DateTime(2022, 1, 1);
            for (var i = 0; i < 10; i++)
                data.Add(value);

            var profile = DayProfileHelpers.LoadDayProfileFromListOfValuesAndTimeframe("test", 
                                                                                        data, 
                                                                                        start,
                                                                                        start.AddDays(10),
                                                                                        DayProfileType.MultiplyCoeficient,
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

            var profile = DayProfileHelpers.LoadDayProfileFromListOfValuesAndTimeframe("test",
                                                                                        data,
                                                                                        start,
                                                                                        start.AddDays(10),
                                                                                        DayProfileType.MultiplyCoeficient,
                                                                                        BlockTimeframe.Day);

            var export = DayProfileHelpers.ExportDayProfileToJson(profile);

            var inprofile = DayProfileHelpers.ImportDayProfileFromJson(export);

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
