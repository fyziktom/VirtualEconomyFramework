using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
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
    }
}
