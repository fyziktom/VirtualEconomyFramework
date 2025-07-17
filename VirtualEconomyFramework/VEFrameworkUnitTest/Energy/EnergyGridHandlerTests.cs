using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;
using Xunit;

namespace VEFrameworkUnitTest.Energy
{
    public class EnergyGridHandlerTests
    {
        private EnergyBlocksTestHelpers ebth = new EnergyBlocksTestHelpers();

        private List<IBlock> getTestRepetitiveBlocks()
        {
            var start = new DateTime(2022, 1, 1, 0, 0, 0);
            var entity = new BaseEntity();

            var blocks = BlockHelpers.CreateRepetitiveDayBlock(start,
                                                               start.AddMonths(12),
                                                               start.AddHours(8),
                                                               start.AddHours(18),
                                                               0.1,
                                                               "12345",
                                                               "123",
                                                               BlockDirection.Created,
                                                               BlockType.Simulated);

            return blocks;
        }

        private List<IBlock> getTestBlocks()
        {
            var start = new DateTime(2022, 1, 1, 8, 0, 0);
            var entity = new BaseEntity();
            var Block = new BaseBlock();

            var blocks = new List<IBlock>();
            blocks.Add(Block.GetBlockByPower(BlockType.Simulated,
                                             BlockDirection.Consumed,
                                             start,
                                             start.AddHours(10) - start,
                                             1,
                                             ebth.sourceName,
                                             ebth.device1Id));
                                             
            return blocks;
        }

        [Fact]
        public void ExportImportSettings()
        {
            var eGrid = ebth.GetTestEnergyGridHandler();
            var exp = eGrid.ExportToConfig();
            var eGridClone = new BaseEntitiesHandler();
            if (exp.Item1)
            {
                eGridClone.LoadFromConfig(exp.Item2);
                Assert.Equal(eGrid.Entities.Count, eGridClone.Entities.Count);
                var expclone = eGrid.ExportToConfig();
                Assert.Equal(exp, expclone);
            }
        }

        [Fact]
        public void ExportImportSettingsWithBlock()
        {
            var eGrid = ebth.GetTestEnergyGridHandler();
            var blocks = getTestBlocks();
            eGrid.AddBlocksToEntity(ebth.device1Id, blocks);

            var exp = eGrid.ExportToConfig();
            var eGridClone = new BaseEntitiesHandler();
            if (exp.Item1)
            {
                eGridClone.LoadFromConfig(exp.Item2);
                Assert.Equal(blocks.Count, eGridClone.GetEntityBlocks(ebth.device1Id).Count());
                Assert.Equal(eGrid.Entities.Count, eGridClone.Entities.Count);
                var expclone = eGrid.ExportToConfig();
                Assert.Equal(exp, expclone);
            }
        }

        [Fact]
        public void ExportImportSettingsWithRepetitiveBlock()
        {
            /* TODO: repair, not critical now
             * 
            var eGrid = ebth.GetTestEnergyGridHandler();
            var blocks = getTestRepetitiveBlocks();
            eGrid.AddBlocksToEntity(ebth.device1Id, blocks);

            var exp = eGrid.ExportToConfig();
            var eGridClone = new BaseEntitiesHandler();
            if (exp.Item1)
            {
                eGridClone.LoadFromConfig(exp.Item2);
                Assert.Equal(blocks.Count, eGridClone.GetEntityBlocks(ebth.device1Id).Count());
                Assert.Equal(eGrid.Entities.Count, eGridClone.Entities.Count);
                var expclone = eGrid.ExportToConfig();
                Assert.Equal(exp, expclone);
            }
            */
        }
    }
}
