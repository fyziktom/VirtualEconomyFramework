using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;
using Xunit;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class EntitiesHandlerTests
    {
        private EntitiesBlocksTestHelpers egth = new EntitiesBlocksTestHelpers();

        [Fact]
        public void AddEntityToHandlerTest()
        {
            var eGrid = new BaseEntitiesHandler();
            var name = "testPVE";
            var entity = new BaseEntity() { Type = EntityType.Source, Name = name, ParentId = "test" };
            var res = eGrid.AddEntity(entity, name, "test");
            var id = string.Empty;
            if (res.Item1)
                id = res.Item2.Item2;
            if (eGrid.Entities.TryGetValue(id, out var ent))
            {
                Assert.Equal(name, ent.Name);
            }
        }

        [Fact]
        public void RemoveEntityFromHandler()
        {
            var eGrid = new BaseEntitiesHandler();
            var name = "testPVE";
            var entity = new BaseEntity() { Type = EntityType.Source, Name = name, ParentId = "test" };
            var res = eGrid.AddEntity(entity, name, "test");
            var id = string.Empty;
            if (res.Item1)
                id = res.Item2.Item2;
            if (eGrid.Entities.TryGetValue(id, out var ent))
            {
                Assert.Equal(name, ent.Name);

                eGrid.RemoveEntity(id);
                var check = eGrid.Entities.ContainsKey(id);
                Assert.False(check);
            }
        }


        [Fact]
        public void GetConsumptionOfGroup()
        {
            // create energy handler with fake data
            var eGrid = egth.GetTestEnergyGridHandler();
            var Block = new BaseBlock();

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 5, 0, 0);
            var timeframe = new TimeSpan(160, 0, 0);
            var endtime = starttime + timeframe;

            //device which consume 1kW and run 160 hours, started on 3rd of January in 5am, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     egth.sourceName,
                                     egth.device2Id));

            //device which consume 1kW and run 160 hours, started on 3rd of January in 5am, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     egth.sourceName,
                                     egth.device2Id));

            //device which consume 1kW and run 160 hours, started on 3rd of January in 5am, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     egth.sourceName,
                                     egth.device1Id));

            // add three blocks into device2 (network/devicegroup/device2) 
            eGrid.AddBlocksToEntity(egth.device2Id, blockstoadd);

            var device = eGrid.GetEntity(egth.devicegroup, EntityType.Consumer);
            if (device != null)
            {
                var consumption = eGrid.GetConsumptionOfEntity(egth.devicegroup, BlockTimeframe.Hour, starttime, endtime);
                var total = 0.0;
                if (consumption != null)
                {
                    foreach (var frame in consumption)
                        total += frame.Amount;
                }

                Assert.Equal(160 * 3, total);
            }

            var network = eGrid.GetEntity(egth.networkId, EntityType.Consumer);
            if (network != null)
            {
                var consumption = eGrid.GetConsumptionOfEntity(egth.networkId, BlockTimeframe.Hour, starttime, endtime);
                var total = 0.0;
                if (consumption != null)
                {
                    foreach (var frame in consumption)
                        total += frame.Amount;
                }
                Assert.Equal(160 * 3, total);
            }

            // keep just one block in the list to add
            blockstoadd.Clear();

            //device which consume 1kW and run 160 hours, started on 3rd of January in 5am, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     egth.sourceName,
                                     egth.device4Id));

            // add one block into device4 (network/devicegroup2/device4) 
            eGrid.AddBlocksToEntity(egth.device4Id, blockstoadd);

            // keep just one block in the list to add
            blockstoadd.Clear();

            //device which consume 1kW and run 160 hours, started on 3rd of January in 5am, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     egth.sourceName,
                                     egth.device5Id));
            // add one block into device4 (network/devicegroup2/device5) 
            eGrid.AddBlocksToEntity(egth.device5Id, blockstoadd);

            if (network != null)
            {
                var consumption = eGrid.GetConsumptionOfEntity(egth.networkId, BlockTimeframe.Hour, starttime, endtime);
                var total = 0.0;
                if (consumption != null)
                {
                    foreach (var frame in consumption)
                        total += frame.Amount;
                }
                // three blocks in devicegroup and then two blocks in devicegroup2, both are under network
                Assert.Equal(160 * 3 + 160 * 2, total);
            }


            // add some custom blocks of simulated consumption
            var sourceblockstoadd = new List<IBlock>();

            // source with power 10kWh and run for specified timeframe from starttime
            sourceblockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Created,
                                     starttime,
                                     timeframe,
                                     10,
                                     egth.sourceName,
                                     egth.sourceId));

            // add one blocks into source (network/source) 
            eGrid.AddBlocksToEntity(egth.sourceId, sourceblockstoadd);

            if (network != null)
            {
                var consumption = eGrid.GetConsumptionOfEntity(egth.networkId, BlockTimeframe.Hour, starttime, endtime, true, true);
                var total = 0.0;
                if (consumption != null)
                {
                    foreach (var frame in consumption)
                        total += frame.Amount;
                }
                // three blocks in devicegroup and then two blocks in devicegroup2 and one source, all are in same network
                Assert.Equal(-1 * (160 * 3 + 160 * 2) + 1600, total);
            }

        }

        [Fact]
        public void GetPowerConsumptionOfEntityWithWindow()
        {
            var eGrid = egth.GetTestEnergyGridHandler();
            var Block = new BaseBlock();
            // add some custom blocks of simulated consumption
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);
            var timeframe = new TimeSpan(168, 0, 0);
            var endtime = starttime + timeframe;

            var timeframe2 = new TimeSpan(336, 0, 0);
            var endtime2 = starttime + timeframe2;


            //device which consume 1kW and run 168 hours, started on 3rd of January in 0:00, for example PC
            eGrid.AddBlockToEntity(egth.device2Id, Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     starttime,
                                     timeframe,
                                     1,
                                     egth.sourceName,
                                     "PC",
                                     null,
                                     egth.device2Id));

            ////////////////////////////////
            // device which runs each day from 3rd of January since 9am for 5 hours with consumption 1kW.
            // it will create consumption 50kWh in total
            var st = new DateTime(2022, 1, 3, 9, 0, 0);
            var tf = new TimeSpan(5, 0, 0);
            var blta = new List<IBlock>();
            for (var i = 0; i < 10; i++)
            {
                blta.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     st,
                                     tf,
                                     1,
                                     egth.sourceName,
                                     "airconidioning",
                                     null,
                                     egth.device2Id));
                st = st.AddDays(1);
            }
            eGrid.AddBlocksToEntity(egth.device2Id, blta);
            ////////////////////////////////////////////////

            ////////////////////////////////
            // device which runs each day from 3rd of January since 22am for 2 hours with consumption 1kW.
            // it will create consumption 20kWh in total
            var st1 = new DateTime(2022, 1, 3, 22, 0, 0);
            var tf1 = new TimeSpan(2, 0, 0);
            var blta1 = new List<IBlock>();
            for (var i = 0; i < 10; i++)
            {
                blta1.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     st1,
                                     tf1,
                                     1,
                                     egth.sourceName,
                                     "airconditioning-room1-night",
                                     null,
                                     egth.device2Id));
                st1 = st1.AddDays(1);
            }
            eGrid.AddBlocksToEntity(egth.device2Id, blta1);
            ////////////////////////////////////////////////

            ////////////////////////////////
            // device 1 which runs each day from 3rd of January since 22am for 2 hours with consumption 1kW.
            // it will create consumption 20kWh in total
            var st2 = new DateTime(2022, 1, 3, 22, 0, 0);
            var tf2 = new TimeSpan(2, 0, 0);
            var blta2 = new List<IBlock>();
            for (var i = 0; i < 10; i++)
            {
                blta2.Add(Block.GetBlockByPower(BlockType.Simulated,
                                     BlockDirection.Consumed,
                                     st2,
                                     tf2,
                                     1,
                                     egth.sourceName,
                                     "airconditioning-room2-night",
                                     null,
                                     egth.device1Id));
                st2 = st2.AddDays(1);
            }
            eGrid.AddBlocksToEntity(egth.device1Id, blta2);
            ////////////////////////////////////////////////

            // whole consumption
            var devicegroup = eGrid.GetEntity(egth.devicegroup, EntityType.Both);
            if (devicegroup != null)
            {
                var res = eGrid.GetConsumptionOfEntity(devicegroup.Id, BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 7 + 50 + 20 + 20, total);
            }

            int windowHourStart = 8;
            int windowHourEnd = 20;

            // day consumption only
            if (devicegroup != null)
            {
                var res = eGrid.GetConsumptionOfEntityWithWindow(devicegroup.Id,
                                                                 BlockTimeframe.Day,
                                                                 starttime,
                                                                 endtime2,
                                                                 DateTime.MinValue.AddHours(windowHourStart),
                                                                 DateTime.MinValue.AddHours(windowHourEnd));
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 12 * 7 + 50, total);
            }

            // night consumption only
            if (devicegroup != null)
            {
                var res = eGrid.GetConsumptionOfEntityWithWindow(devicegroup.Id,
                                                                 BlockTimeframe.Day,
                                                                 starttime,
                                                                 endtime2,
                                                                 DateTime.MinValue.AddHours(windowHourStart),
                                                                 DateTime.MinValue.AddHours(windowHourEnd),
                                                                 true);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 12 * 7 + 20 + 20, total);
            }
        }
    }
}
