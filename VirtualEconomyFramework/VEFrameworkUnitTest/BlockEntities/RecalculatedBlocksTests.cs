using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Entities;
using Xunit;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class RecalculatedBlocksTests
    {
        private EntitiesBlocksTestHelpers ebth = new EntitiesBlocksTestHelpers();

        [Fact]
        public void GetRecalculatedBlocksToHoursFromClassicBlocksTest()
        {
            // create energy handler with fake data
            var eGrid = ebth.GetTestEnergyGridHandler();
            var Block = new BaseBlock();

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 1, 0, 0, 0);
            var timeframe = new TimeSpan(160, 0, 0);
            var endtime = starttime + timeframe;

            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     ebth.sourceName,
                                     ebth.device2Id));

            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime.AddDays(1),
                                     timeframe,
                                     1,
                                     ebth.sourceName,
                                     ebth.device2Id));

            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime.AddDays(2),
                                     timeframe,
                                     1,
                                     ebth.sourceName,
                                     ebth.device2Id));

            // add three blocks into device2 (network/devicegroup/device2) 
            eGrid.AddBlocksToEntity(ebth.device2Id, blockstoadd);

            var blocks = eGrid.GetRecalculatedBlocks(new List<string>() { ebth.device2Id }, 
                                                     BlockTimeframe.Hour, 
                                                     starttime, 
                                                     starttime.AddDays(1));
            var total = 0.0;
            if (blocks != null)
            {
                foreach (var block in blocks)
                    total += block.Amount;
            }

            Assert.Equal(24, total);

            blocks = eGrid.GetRecalculatedBlocks(new List<string>() { ebth.device2Id },
                                                 BlockTimeframe.Hour,
                                                 starttime,
                                                 starttime.AddDays(2));
            total = 0.0;
            if (blocks != null)
            {
                foreach (var block in blocks)
                    total += block.Amount;
            }

            Assert.Equal(24 + 24 * 2, total);

            blocks = eGrid.GetRecalculatedBlocks(new List<string>() { ebth.device2Id },
                                                 BlockTimeframe.Hour,
                                                 starttime,
                                                 starttime.AddDays(3));
            total = 0.0;
            if (blocks != null)
            {
                foreach (var block in blocks)
                    total += block.Amount;
            }

            Assert.Equal(24 + 24 * 2 + 24 * 3, total);
        }

        [Fact]
        public void GetRecalculatedBlocksToSecondsFromClassicBlocksTest()
        {
            // create energy handler with fake data
            var eGrid = ebth.GetTestEnergyGridHandler();
            var Block = new BaseBlock();

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 1, 0, 0, 0);
            var timeframe = new TimeSpan(160, 0, 0);
            var endtime = starttime + timeframe;

            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     3600,
                                     ebth.sourceName,
                                     ebth.device2Id));

            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime.AddDays(1),
                                     timeframe,
                                     3600,
                                     ebth.sourceName,
                                     ebth.device2Id));

            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime.AddDays(2),
                                     timeframe,
                                     3600,
                                     ebth.sourceName,
                                     ebth.device2Id));

            // add three blocks into device2 (network/devicegroup/device2) 
            eGrid.AddBlocksToEntity(ebth.device2Id, blockstoadd);

            var blocks = eGrid.GetRecalculatedBlocks(new List<string>() { ebth.device2Id },
                                                     BlockTimeframe.Second,
                                                     starttime,
                                                     starttime.AddHours(1));
            var total = 0.0;
            if (blocks != null)
            {
                foreach (var block in blocks)
                    total += block.Amount;
            }

            Assert.Equal(3600, total);

            blocks = eGrid.GetRecalculatedBlocks(new List<string>() { ebth.device2Id },
                                                 BlockTimeframe.Second,
                                                 starttime.AddDays(1),
                                                 starttime.AddDays(1).AddHours(1));
            total = 0.0;
            if (blocks != null)
            {
                foreach (var block in blocks)
                    total += block.Amount;
            }

            Assert.Equal(3600 * 2, total);

            blocks = eGrid.GetRecalculatedBlocks(new List<string>() { ebth.device2Id },
                                                 BlockTimeframe.Second,
                                                 starttime.AddDays(2),
                                                 starttime.AddDays(2).AddHours(1));
            total = 0.0;
            if (blocks != null)
            {
                foreach (var block in blocks)
                    total += block.Amount;
            }

            Assert.Equal(3600 * 3, total);
        }
    }
}
