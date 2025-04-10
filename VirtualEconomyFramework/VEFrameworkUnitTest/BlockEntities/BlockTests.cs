using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using Xunit;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class BlockTests
    {
        private EntitiesBlocksTestHelpers ebth = new EntitiesBlocksTestHelpers();


        [Fact]
        public void CreateRepetitiveBlockMinuteDuration()
        {
            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 0:00 for 30 minutes and 15 minutes off with avg hour consumption 1kW.
            // it will create consumption 10kWh in total
            var st = new DateTime(2022, 1, 3, 0, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveBlock(st,
                                                          st.AddDays(10),
                                                          st,
                                                          st.AddMinutes(30),
                                                          new TimeSpan(0, 15, 0),
                                                          1,
                                                          ebth.sourceId,
                                                          ebth.device2Id,
                                                          BlockDirection.Consumed,
                                                          BlockType.Simulated);

            Assert.Equal(320, blta.Count);
            var total = 0.0;
            foreach (var b in blta)
                total += b.Amount;

            Assert.Equal(160, total);
        }

        [Fact]
        public void CreateRepetitiveBlockHourDuration()
        {
            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 0:00 for 1 hour with avg hour consumption 1kW.
            // it will create consumption 10kWh in total
            var st = new DateTime(2022, 1, 3, 0, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveBlock(st,
                                                          st.AddDays(10),
                                                          st,
                                                          st.AddHours(1),
                                                          new TimeSpan(1, 0, 0),
                                                          1,
                                                          ebth.sourceId,
                                                          ebth.device2Id,
                                                          BlockDirection.Consumed,
                                                          BlockType.Simulated);

            Assert.Equal(120, blta.Count);
            var total = 0.0;
            foreach (var b in blta)
                total += b.Amount;

            Assert.Equal(120, total);
        }

        [Fact]
        public void CreateRepetitiveDayBlockMinutesDuration()
        {
            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 0:00 for 1 minute with avg hour consumption 1kW.
            // it will create consumption 0,16666666kWh in total
            var st = new DateTime(2022, 1, 3, 0, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveDayBlock(st,
                                                             st.AddDays(10),
                                                             st,
                                                             st.AddMinutes(1),
                                                             1,
                                                             ebth.sourceId,
                                                             ebth.device2Id,
                                                             BlockDirection.Consumed,
                                                             BlockType.Simulated);

            Assert.Equal(10, blta.Count);
            var total = 0.0;
            foreach (var b in blta)
                total += b.Amount;

            Assert.Equal(((double)10 / 60), total);
        }

        [Fact]
        public void CreateRepetitiveDayBlockHourDuration()
        {
            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 0:00 for 1 hours with avg hour consumption 1kW.
            // it will create consumption 10kWh in total
            var st = new DateTime(2022, 1, 3, 0, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveDayBlock(st,
                                                             st.AddDays(10),
                                                             st,
                                                             st.AddHours(1),
                                                             1,
                                                             ebth.sourceId,
                                                             ebth.device2Id,
                                                             BlockDirection.Consumed,
                                                             BlockType.Simulated);

            Assert.Equal(10, blta.Count);
            var total = 0.0;
            foreach (var b in blta)
                total += b.Amount;

            Assert.Equal(10, total);
        }

        [Fact]
        public void GetBlockConfigDto()
        {
            var block = new BaseBlock();
            var Id = "1";
            var parentId = "123";
            var sourceId = "1234";
            var repetitiveSourceId = "12345";
            var starttime = new DateTime(2022, 1, 1);
            var timeframe = starttime.AddYears(1) - starttime;
            var repetitivefirstRun = new DateTime(2023, 1, 1, 8, 0, 0);
            var repetitiveendRun = new DateTime(2023, 1, 1, 18, 0, 0);
            var offperiod = new TimeSpan(0, 8, 0, 0);
            var isindayonly = true;
            var isoffperiodrepetitive = true;
            var justinweek = true;
            var justinweekends = true;

            block.Id = Id;
            block.ParentId = parentId;
            block.SourceId = sourceId;
            block.RepetitiveSourceBlockId = repetitiveSourceId;
            block.StartTime = starttime;
            block.Timeframe = timeframe;
            block.RepetitiveFirstRun = repetitivefirstRun;
            block.RepetitiveEndRun = repetitiveendRun;
            block.OffPeriod = offperiod;
            block.IsInDayOnly = isindayonly;
            block.IsOffPeriodRepetitive = isoffperiodrepetitive;
            block.JustInWeek = justinweek;
            block.JustInWeekends = justinweekends;

            var dto = new BaseBlockConfigDto();
            dto.Fill(block);

            Assert.Equal(Id, dto.Id);
            Assert.Equal(parentId, dto.ParentId);
            Assert.Equal(sourceId, dto.SourceId);
            Assert.Equal(repetitiveSourceId, dto.RepetitiveSourceBlockId);
            Assert.Equal(starttime, dto.StartTime);
            Assert.Equal(timeframe, dto.Timeframe);
            Assert.Equal(repetitivefirstRun, dto.RepetitiveFirstRun);
            Assert.Equal(repetitiveendRun, dto.RepetitiveEndRun);
            Assert.Equal(offperiod, dto.OffPeriod);
            Assert.True(dto.IsInDayOnly);
            Assert.True(dto.IsOffPeriodRepetitive);
            Assert.True(dto.JustInWeek);
            Assert.True(dto.JustInWeekends);

            var rblock = dto.GetBlockFromDto();

            Assert.Equal(Id, rblock.Id);
            Assert.Equal(parentId, rblock.ParentId);
            Assert.Equal(sourceId, rblock.SourceId);
            Assert.Equal(repetitiveSourceId, rblock.RepetitiveSourceBlockId);
            Assert.Equal(starttime, rblock.StartTime);
            Assert.Equal(timeframe, rblock.Timeframe);
            Assert.Equal(repetitivefirstRun, rblock.RepetitiveFirstRun);
            Assert.Equal(repetitiveendRun, rblock.RepetitiveEndRun);
            Assert.Equal(offperiod, rblock.OffPeriod);
            Assert.True(rblock.IsInDayOnly);
            Assert.True(rblock.IsOffPeriodRepetitive);
            Assert.True(rblock.JustInWeek);
            Assert.True(rblock.JustInWeekends);

        }

        [Fact]
        public void GetFilteredBlocksByTimeRanges()
        {
            var blocks = new List<IBlock>();
            var energy = 1;

            var starttime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var endtime = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            blocks = BlockHelpers.CreateEmptyBlocks(BlockTimeframe.Hour, 
                                                    starttime, 
                                                    endtime, 
                                                    "test", 
                                                    energy, 
                                                    BlockDirection.Consumed, 
                                                    BlockType.Simulated);

            var range1start = new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var range1end = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            var ranges = new List<(DateTime start, DateTime end)>();
            ranges.Add((range1start, range1end));

            var filteredBlocks = BlockHelpers.GetFilteredBlocksByTimeRanges(blocks, ranges);

            Assert.NotNull(filteredBlocks);
            Assert.Equal(62 * 24, filteredBlocks.Count);

            var range2start = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var range2end = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc);

            ranges.Add((range2start, range2end));
            filteredBlocks = BlockHelpers.GetFilteredBlocksByTimeRanges(blocks, ranges);
            Assert.NotNull(filteredBlocks);
            Assert.Equal(62 * 24 + 31 * 24, filteredBlocks.Count);
        }
    }
}
