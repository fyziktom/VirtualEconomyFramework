using System;
using System.Collections.Generic;
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
                                                    int days = 10,
                                                    string name = "test",
                                                    DayProfileType type = DayProfileType.MultiplyCoeficient)
        {
            var start = new DateTime(2022, 1, 1);
            var end = start.AddDays(days);
            var data = new List<double>();
            for (var i = 0; i < days; i++)
                data.Add(value);

            var profile = DayProfileHelpers.LoadDayProfileFromListOfValuesAndTimeframe(name,
                                                                                       data,
                                                                                       start,
                                                                                       end,
                                                                                       type,
                                                                                       BlockTimeframe.Day);

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
        public void MultiplyBlocksWithProfile()
        {
            var timeframe = BlockTimeframe.Day;
            var profile = CreateDayProfileTestData(3, 10, "test", DayProfileType.MultiplyCoeficient);
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
            var profile = CreateDayProfileTestData(3, 10, "test", DayProfileType.DivideCoeficient);
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
            var profile = CreateDayProfileTestData(3, 10, "test", DayProfileType.AddCoeficient);
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
            var profile = CreateDayProfileTestData(3, 10, "test", DayProfileType.SubtractCoeficient);
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
                       "2022-02-01T00:00:00\t3.0" +
                       "2022-03-01T00:00:00\t3.0" +
                       "2022-04-01T00:00:00\t3.0" +
                       "2022-05-01T00:00:00\t3.0" +
                       "2022-06-01T00:00:00\t3.0" +
                       "2022-07-01T00:00:00\t3.0" +
                       "2022-08-01T00:00:00\t3.0" +
                       "2022-09-01T00:00:00\t3.0" +
                       "2022-10-01T00:00:00\t3.0";
            var profile = DayProfileHelpers.LoadDayProfileFromRawData(data, "\t", "test", DayProfileType.MultiplyCoeficient);

            var counter = 1;
            foreach (var pr in profile.ProfileData)
            {
                Assert.Equal(3, pr.Value);
                var day = counter < 10 ? "0" + counter.ToString() : counter.ToString();
                if(DateTime.TryParse($"2022-{day}-01T00:00:00", out var date))
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
                if (DateTime.TryParse($"2022-{day}-01T00:00:00", out var date))
                    Assert.Equal(date, pr.Key);
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
