using NBitcoin;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;
using Xunit;
using static NBitcoin.RPC.SignRawTransactionRequest;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class AllocationSchemeTests
    {
        private BaseEntitiesHandler  eGrid = new BaseEntitiesHandler();

        private string House1Id { get; set; } = "0812cf0c-a82c-4506-9585-c75139897e4c";

        private List<string> House1FlatsIds = new List<string>()
            {
                "0e8e9c3f-2a9b-4a6c-b432-8e2c3a9b4e61",
                "9fbb90e1-cf87-4994-bb36-2d97c9938e5d",
                "c67e9b52-f3ea-46e1-8c5e-913fd54653a8",
                "e0d5b1d1-d2a0-4e09-83a6-3fc8f1e037fc",
                "1a3b4f8c-df6e-423f-9f23-bc82cb7d72e9",
                "a4272f85-11f2-420d-a490-fae0d1870325",
                "99f1ed55-5d88-45b2-abe8-b9d41a47bb38",
                "54e2d7a7-981e-4d12-b9f2-c1f34e90dcf5",
                "ed4df6a0-0a5c-4f5c-a4f0-45a71b8b5301",
                "373a9f8f-b8fa-4269-b123-178fc88ea61e"
            };

        [Fact]
        public void TestAllocationScheme()
        {
            var day = new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);

            var alocationScheme = new AlocationScheme();

            var networkId = "";
            var name = "House1";
            var owner = "simulator";
            var network = new GroupNetwork() { Id = House1Id, Type = EntityType.Source, Name = name, ParentId = owner };
            var res = eGrid.AddEntity(network, name, owner, House1Id, forceId: true);
            if (res.Item1)
                networkId = res.Item2.Item2;

            var sourceId = string.Empty;
            var source = new GroupNetwork() { Type = EntityType.Source, Name = "source", ParentId = network.Id };
            var src = eGrid.AddEntity(source, "source", network.Id);
            if (src.Item1)
                sourceId = src.Item2.Item2;

            var Block = new BaseBlock();
            var production = 5.0;
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2025, 1, 1, 0, 0, 0);
            var timeframe = new TimeSpan(52 * 168, 0, 0);
            var endtime = starttime + timeframe;
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Real,
                                                  BlockDirection.Created,
                                                  starttime,
                                                  timeframe,
                                                  production,
                                                  source.Name,
                                                  source.Id));

            eGrid.AddBlocksToEntity(source.Id, blockstoadd);

            // create alocation scheme
            alocationScheme = new AlocationScheme()
            {
                Id = System.Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Main Scheme",
                MainDepositPeerId = source.Id
            };


            var counter = 1;
            foreach (var fid in House1FlatsIds)
            {
                name = $"Flat {counter}";
                var flat = new GroupNetwork() { Id = fid, Type = EntityType.Consumer, Name = name, ParentId = networkId };
                res = eGrid.AddEntity(flat, name, networkId, fid, forceId: true);

                eGrid.AddSubEntityToEntity(networkId, fid);

                AddFlatGroupOneFullSimulators(fid);

                // just for testing to see that flat 2 will be skipped
                // all other flats will have 10% from the production block
                if (counter != 2)
                {
                    alocationScheme.DepositPeers.TryAdd(flat.Id, new DepositPeer()
                    {
                        PeerId = flat.Id,
                        Name = flat.Name,
                        Percentage = 10
                    });
                }

                counter++;
            }


            eGrid.AlocationSchemes.TryAdd(alocationScheme.Id, alocationScheme);

            var NetworkFlatBlocksAfterAlocation = DataProfileHelpers.GetEntityBalanceBlocksAfterAlocationOfProductionBlocks(source, eGrid, day);
            BlockHelpers.DrawBlocks("Network: Rest of production to forward to flats", NetworkFlatBlocksAfterAlocation.Item1, day, day.AddDays(1));
            BlockHelpers.DrawBlocks("Network: Not Covered In network", NetworkFlatBlocksAfterAlocation.Item2, day, day.AddDays(1));

            // this is moment when the allocation scheme is applied
            // it will take source blok and based on percentage it will split the amount and load it into entities that are in the scheme
            // then you have to get their consumption to see the result of bilance
            eGrid.AddBlocksToEntity(source.Id,
                                    NetworkFlatBlocksAfterAlocation.Item1,
                                    alocationScheme.Id);

            Assert.Equal(production * 24, NetworkFlatBlocksAfterAlocation.Item1.Select(b => b.Amount).Sum());
            Assert.Equal(0, NetworkFlatBlocksAfterAlocation.Item2.Select(b => b.Amount).Sum());

            for (var i = 0; i < House1FlatsIds.Count; i++)
            {
                var fid = House1FlatsIds[i];

                var flat = eGrid.GetEntity(fid, EntityType.Both);
                Console.WriteLine("------------------------------------");
                Console.WriteLine($"------------{flat.Name}---------------");
                Console.WriteLine("------------------------------------");

                var flatblocksAfterAlocation = DataProfileHelpers.GetEntityBalanceBlocksAfterAlocationOfProductionBlocks(flat, eGrid, day);
                BlockHelpers.DrawBlocks("Rest of production to forward to another flat", flatblocksAfterAlocation.Item1, day, day.AddDays(1));
                BlockHelpers.DrawBlocks("Not Covered In flat", flatblocksAfterAlocation.Item2, day, day.AddDays(1));

                if (i != 1) // because now loop starts from 0, so flat2 is index 1
                {
                    Assert.Equal(0, flatblocksAfterAlocation.Item1.Select(b => b.Amount).Sum());
                    // each flat has stable consumption 1kW, each flat with allocation has 10% of production and it is for one day in hour timeframe
                    Assert.Equal((1 - production * 0.1) * 24, flatblocksAfterAlocation.Item2.Select(b => b.Amount).Sum());
                }
                else
                {
                    Assert.Equal(0, flatblocksAfterAlocation.Item1.Select(b => b.Amount).Sum());
                    // flat 2 has no allocation so it must has all consumption as before
                    Assert.Equal(1 * 24, flatblocksAfterAlocation.Item2.Select(b => b.Amount).Sum());
                }
            }
        }

        private void AddFlatGroupOneFullSimulators(string id)
        {
            var entity = eGrid.GetEntity(id, EntityType.Both);
            if (entity == null)
                return;

            var power = 1.0;   // [kW] slightly larger split‑unit

            double[] fridgeRun = new double[24]
            {
                1,1,1,1,1,1,1,1,1,1,1,1,
                1,1,1,1,1,1,1,1,1,1,1,1
            };

            _ = entity.AddSimulator(new DeviceSimulator(fridgeRun, power));
        }
    }
}
