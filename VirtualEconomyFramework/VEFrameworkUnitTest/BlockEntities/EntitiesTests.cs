using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Entities;
using Xunit;

namespace VEFrameworkUnitTest.BlockEntities
{
    public class EntitiesTests
    {
        private EntitiesBlocksTestHelpers ebth = new EntitiesBlocksTestHelpers();

        private IEntity getTestEntityWithPVEYearDayBlocks()
        {
            var start = new DateTime(2022, 1, 1, 8, 0, 0);
            var entity = new BaseEntity();

            // basic month profile for modelate common consumption in the year. 
            // for this case there is one 1kW/day so amount in list is ActualMonth.Days * 1
            var profile = new List<double> { 31.0, 28.0, 31.0, 30.0, 31.0, 30.0, 31.0, 31.0, 30.0, 31.0, 30.0, 31.0 };

            var blocks = BlockHelpers.PVECreateYearDaysBlocks(2022,
                                                                    2023,
                                                                    start,
                                                                    start.AddHours(10),
                                                                    "123", 1,
                                                                    profile);

            entity.AddBlocks(blocks);
            return entity;
        }

        private IEntity getTestEntityWithRepetitiveBlock()
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

            entity.AddBlocks(blocks);
            return entity;
        }

        [Fact]
        public void AddBlocksToEntityTest()
        {
            var entity = getTestEntityWithPVEYearDayBlocks();
            Assert.Equal(365, entity.BlocksOrderByTime.Count);

            var total = 0.0;
            foreach (var block in entity.BlocksOrderByTime)
                total += block.Amount;
            Assert.Equal(365 * 1, total);
        }

        [Fact]
        public void RemoveBlockFromEntityTest()
        {
            var entity = getTestEntityWithPVEYearDayBlocks();

            Assert.Equal(365, entity.BlocksOrderByTime.Count);

            var total = 0.0;
            foreach (var block in entity.BlocksOrderByTime)
                total += block.Amount;
            Assert.Equal(365 * 1, total);

            var blockstoremove = entity.BlocksOrderByTime.Where(b => b.StartTime > new DateTime(2022, 12, 1)).ToList();
            entity.RemoveBlocks(blockstoremove.Select(b => b.Id).ToList());
            Assert.Equal(334, entity.BlocksOrderByTime.Count);

            total = 0.0;
            foreach (var block in entity.BlocksOrderByTime)
                total += block.Amount;
            Assert.Equal(334 * 1, total);

        }

        [Fact]
        public void EntityTotalSummedValueTest()
        {
            var entity = getTestEntityWithPVEYearDayBlocks();

            Assert.Equal(365, entity.GetTotalSummedValue());

            var blockstoremove = entity.BlocksOrderByTime.Where(b => b.StartTime > new DateTime(2022, 12, 1)).ToList();
            entity.RemoveBlocks(blockstoremove.Select(b => b.Id).ToList());
            Assert.Equal(334, entity.BlocksOrderByTime.Count);

            Assert.Equal(334, entity.GetTotalSummedValue());

        }

        [Fact]
        public void EntitySummedValueTest()
        {
            var entity = getTestEntityWithPVEYearDayBlocks();

            var blocks = entity.GetSummedValues( BlockTimeframe.Day, new DateTime(2022,1,1), new DateTime(2022,2,1));
            var total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal(31 * 1, total);

            blocks = entity.GetSummedValues(BlockTimeframe.Day, new DateTime(2022, 12, 1), new DateTime(2023, 1, 1));
            total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal(31 * 1, total);


            entity.AddBlocks(new List<IBlock>()
            {
                new BaseBlock()
                {
                    Amount = 50,
                    Direction = BlockDirection.Consumed,
                    Type = BlockType.Simulated,
                    StartTime = new DateTime(2022, 12, 2),
                    Timeframe = new TimeSpan(1, 0, 0, 0)
                }
            });

            blocks = entity.GetSummedValues(BlockTimeframe.Day, new DateTime(2022, 12, 1), new DateTime(2023, 1, 1), true);
            total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal((31 - 50) * 1, total);
        }

        [Fact]
        public void EntitySummedValueOptimizedTest()
        {
            var entity = getTestEntityWithPVEYearDayBlocks();

            var blocks = entity.GetSummedValuesOptimized(BlockTimeframe.Day, new DateTime(2022, 1, 1), new DateTime(2022, 2, 1));
            var total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal(31 * 1, total);

            blocks = entity.GetSummedValues(BlockTimeframe.Day, new DateTime(2022, 12, 1), new DateTime(2023, 1, 1));
            total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal(31 * 1, total);
        }

        [Fact]
        public void EntitySummedValueWithWindowTest()
        {
            var entity = getTestEntityWithPVEYearDayBlocks();

            var blocks = entity.GetSummedValuesWithHourWindow(BlockTimeframe.Day, 
                                                              new DateTime(2022, 1, 1), 
                                                              new DateTime(2022, 2, 1),
                                                              new DateTime(2022, 1, 1, 8, 0 , 0),
                                                              new DateTime(2022, 1, 1, 13, 0, 0));

            var total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal(15.5, total);

        }

        [Fact]
        public void EntityRepetitiveBlockSummedValueTest()
        {
            var entity = getTestEntityWithRepetitiveBlock();

            var blocks = entity.GetSummedValuesOfRepetitiveBlocks(BlockTimeframe.Day,
                                                              new DateTime(2022, 1, 1),
                                                              new DateTime(2022, 2, 1));

            var total = 0.0;
            foreach (var block in blocks)
                total += block.Amount;
            Assert.Equal(31, total);

        }


        [Fact]
        public void GetPowerConsumptionOfRepetitiveblocksSpeedTest()
        {
            var entity = new BaseEntity();

            long gcms = 0;
            long gcmso = 0;
            long gcrpms = 0;

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);
            var timeframe = new TimeSpan(365, 0, 0, 0);
            var endtime = starttime + timeframe;

            var timeframe2 = new TimeSpan(335, 0, 0, 0);
            var endtime2 = starttime + timeframe2;

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 9am for 5 hours with consumption 1kW.
            // it will create consumption 50kWh in total
            var st = new DateTime(2022, 1, 3, 9, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveDayBlock(st,
                                                                   st.AddDays(10),
                                                                   st,
                                                                   st.AddHours(5),
                                                                   1,
                                                                   ebth.sourceId,
                                                                   ebth.device2Id,
                                                                   BlockDirection.Consumed,
                                                                   BlockType.Simulated);

            entity.AddBlocks(blta);
            ////////////////////////////////////////////////

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 22am for 2 hours with consumption 1kW.
            // it will create consumption 20kWh in total
            var st1 = new DateTime(2022, 1, 3, 22, 0, 0);
            var blta1 = BlockHelpers.CreateRepetitiveDayBlock(st1,
                                                                    st1.AddDays(10),
                                                                    st1,
                                                                    st1.AddHours(2),
                                                                    1,
                                                                    ebth.sourceId,
                                                                    ebth.device2Id,
                                                                    BlockDirection.Consumed,
                                                                    BlockType.Simulated);
            entity.AddBlocks(blta1);
            ////////////////////////////////////////////////


            if (entity != null)
            {
                var stp = new Stopwatch();
                stp.Start();
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime2);
                stp.Stop();
                gcms = stp.ElapsedMilliseconds;
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(50 + 20, total);
            }

            if (entity != null)
            {
                var stp = new Stopwatch();
                stp.Start();
                var res = entity.GetSummedValuesOfRepetitiveBlocks(BlockTimeframe.Day, starttime, endtime2);
                stp.Stop();
                gcrpms = stp.ElapsedMilliseconds;
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(50 + 20, total);
            }

            Console.WriteLine($"GetConsumption: total {gcms} ms");
            Console.WriteLine($"GetConsumptionOfRepetitiveBlocks: total {gcrpms} ms");

            //increase this parameters to test speed
            var numOfDevices = 2;
            //hour frame will cause more items in final test good to change for optimization test
            // day is set just to do not block tests when this test is not necessary
            BlockTimeframe tf = BlockTimeframe.Day;

            // create 10 devices
            for (var i = 0; i < numOfDevices; i++)
            {
                ////////////////////////////////////////////////
                // devices which runs each day from 3rd of January for 10 days for 1 hour and 1 hour off and again with consumption 1kW.
                // it will create consumption 120kWh in total (12kWh/day)
                var bltai = BlockHelpers.CreateRepetitiveBlock(st,
                                                               st.AddDays(100),
                                                               st,
                                                               st.AddHours(1),
                                                               new TimeSpan(0, 1, 0, 0, 0),
                                                               1,
                                                               ebth.sourceId,
                                                               ebth.device2Id,
                                                               BlockDirection.Consumed,
                                                               BlockType.Simulated);

                entity.AddBlocks(bltai);
                ////////////////////////////////////////////////
            }

            if (entity != null)
            {
                var stp = new Stopwatch();
                stp.Start();
                var res = entity.GetSummedValues(tf, starttime, endtime2);
                stp.Stop();
                gcms = stp.ElapsedMilliseconds;
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1200 * numOfDevices + 50 + 20, total);
            }

            if (entity != null)
            {
                var stp = new Stopwatch();
                stp.Start();
                var res = entity.GetSummedValuesOptimized(tf, starttime, endtime2);
                stp.Stop();
                gcmso = stp.ElapsedMilliseconds;
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1200 * numOfDevices + 50 + 20, total);
            }

            if (entity != null)
            {
                var stp = new Stopwatch();
                stp.Start();
                var res = entity.GetSummedValuesOfRepetitiveBlocks(tf, starttime, endtime2);
                stp.Stop();
                gcrpms = stp.ElapsedMilliseconds;
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1200 * numOfDevices + 50 + 20, total);
            }

            Console.WriteLine($"GetConsumption: total {gcms} ms");
            Console.WriteLine($"GetConsumption Optimized: total {gcmso} ms");

            Console.WriteLine($"GetConsumptionOfRepetitiveBlocks: total {gcrpms} ms");
        }


        [Fact]
        public void GetPowerConsumptionOfRepetitiveblocksClassicRepetitiveBlock()
        {
            var entity = new BaseEntity();

            // add some custom blocks of simulated consumption
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);

            var timeframe2 = new TimeSpan(335, 0, 0, 0);
            var endtime2 = starttime + timeframe2;

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 9am for 5 hours with consumption 1kW.
            // it will create consumption 50kWh in total
            var st = new DateTime(2022, 1, 3, 9, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveDayBlock(st,
                                                             st.AddDays(10),
                                                             st,
                                                             st.AddHours(5),
                                                             1,
                                                             ebth.sourceId,
                                                             ebth.device2Id,
                                                             BlockDirection.Consumed,
                                                             BlockType.Simulated);

            entity.AddBlocks(blta);
            ////////////////////////////////////////////////

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 22am for 2 hours with consumption 1kW.
            // it will create consumption 20kWh in total
            var st1 = new DateTime(2022, 1, 3, 22, 0, 0);
            var blta1 = BlockHelpers.CreateRepetitiveDayBlock(st1,
                                                              st1.AddDays(10),
                                                              st1,
                                                              st1.AddHours(2),
                                                              1,
                                                              ebth.sourceId,
                                                              ebth.device2Id,
                                                              BlockDirection.Consumed,
                                                              BlockType.Simulated);
            entity.AddBlocks(blta1);
            ////////////////////////////////////////////////

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(20 + 50, total);
            }

            if (entity != null)
            {
                var res = entity.GetSummedValuesOfRepetitiveBlocks(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(20 + 50, total);
            }

        }

        [Fact]
        public void GetPowerConsumptionOfRepetitiveblocksDayRepetitiveBlock()
        {
            var entity = new BaseEntity();

            // add some custom blocks of simulated consumption
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);

            var timeframe2 = new TimeSpan(335, 0, 0, 0);
            var endtime2 = starttime + timeframe2;

            var st = new DateTime(2022, 1, 3, 9, 0, 0);


            ////////////////////////////////
            // devices which runs each day from 3rd of January for 10 for 1 hours and 1 hour off and again with consumption 1kW.
            // it will create consumption 120kWh in total (12kWh/day)
            var blta2 = BlockHelpers.CreateRepetitiveBlock(st,
                                                           st.AddDays(100),
                                                           st,
                                                           st.AddHours(1),
                                                           new TimeSpan(0, 1, 0, 0, 0),
                                                           1,
                                                           ebth.sourceId,
                                                           ebth.device2Id,
                                                           BlockDirection.Consumed,
                                                           BlockType.Simulated);
            entity.AddBlocks(blta2);

            // devices which runs each day from 3rd of January for 10 for 1 hours and 1 hour off and again with consumption 1kW.
            // it will create consumption 120kWh in total (12kWh/day)
            var blta3 = BlockHelpers.CreateRepetitiveBlock(st,
                                                           st.AddDays(100),
                                                           st,
                                                           st.AddHours(1),
                                                           new TimeSpan(0, 1, 0, 0, 0),
                                                           1,
                                                           ebth.sourceId,
                                                           ebth.device2Id,
                                                           BlockDirection.Consumed,
                                                           BlockType.Simulated);
            entity.AddBlocks(blta3);
            ////////////////////////////////////////////////
            // devices which runs each day from 3rd of January for 10 days for 1 hour and 1 hour off and again with consumption 1kW.
            // it will create consumption 120kWh in total (12kWh/day)
            var blta4 = BlockHelpers.CreateRepetitiveBlock(st,
                                                           st.AddDays(100),
                                                           st,
                                                           st.AddHours(1),
                                                           new TimeSpan(0, 1, 0, 0, 0),
                                                           1,
                                                           ebth.sourceId,
                                                           ebth.device2Id,
                                                           BlockDirection.Consumed,
                                                           BlockType.Simulated);

            entity.AddBlocks(blta4);
            ////////////////////////////////////////////////

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1200 * 3, total);
            }

            if (entity != null)
            {
                var res = entity.GetSummedValuesOfRepetitiveBlocks(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1200 * 3, total);
            }

        }

        [Fact]
        public void GetPowerConsumptionOfRepetitiveblocks()
        {
            var entity = new BaseEntity();

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);
            var timeframe = new TimeSpan(168, 0, 0);
            var endtime = starttime + timeframe;

            var timeframe2 = new TimeSpan(336, 0, 0);
            var endtime2 = starttime + timeframe2;
            var Block = new BaseBlock();
            //device which consume 1kW and run 168 hours, started on 3rd of January in 0:00, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                            BlockDirection.Consumed,
                            starttime,
                            timeframe,
                            1,
                            ebth.sourceName,
                            ebth.device2Id));

            entity.AddBlocks(blockstoadd);

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 9am for 5 hours with consumption 1kW.
            // it will create consumption 50kWh in total
            var st = new DateTime(2022, 1, 3, 9, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveDayBlock(st,
                                                             st.AddDays(10),
                                                             st,
                                                             st.AddHours(5),
                                                             1,
                                                             ebth.sourceId,
                                                             ebth.device2Id,
                                                             BlockDirection.Consumed,
                                                             BlockType.Simulated);

            entity.AddBlocks(blta);
            ////////////////////////////////////////////////

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 22am for 2 hours with consumption 1kW.
            // it will create consumption 20kWh in total
            var st1 = new DateTime(2022, 1, 3, 22, 0, 0);
            var blta1 = BlockHelpers.CreateRepetitiveDayBlock(st1,
                                                              st1.AddDays(10),
                                                              st1,
                                                              st1.AddHours(2),
                                                              1,
                                                              ebth.sourceId,
                                                              ebth.device2Id,
                                                              BlockDirection.Consumed,
                                                              BlockType.Simulated);
            entity.AddBlocks(blta1);
            ////////////////////////////////////////////////

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 7 + 50 + 20, total);
            }

            if (entity != null)
            {
                var res = entity.GetSummedValuesOfRepetitiveBlocks(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(50 + 20, total);
            }

        }

        [Fact]
        public void GetPowerConsumptionOfEntityWithWindow()
        {
            var entity = new BaseEntity();

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);
            var timeframe = new TimeSpan(168, 0, 0);
            var endtime = starttime + timeframe;

            var timeframe2 = new TimeSpan(336, 0, 0);
            var endtime2 = starttime + timeframe2;
            var Block = new BaseBlock();
            //device which consume 1kW and run 168 hours, started on 3rd of January in 0:00, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                                  BlockDirection.Consumed,
                                                  starttime,
                                                  timeframe,
                                                  1,
                                                  ebth.sourceName,
                                                  ebth.device2Id));

            entity.AddBlocks(blockstoadd);

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 9am for 5 hours with consumption 1kW.
            // it will create consumption 50kWh in total
            var st = new DateTime(2022, 1, 3, 9, 0, 0);
            var blta = BlockHelpers.CreateRepetitiveDayBlock(st,
                                                             st.AddDays(10),
                                                             st,
                                                             st.AddHours(5),
                                                             1,
                                                             ebth.sourceId,
                                                             ebth.device2Id,
                                                             BlockDirection.Consumed,
                                                             BlockType.Simulated);

            entity.AddBlocks(blta);
            ////////////////////////////////////////////////

            ////////////////////////////////
            // device which runs each day from 3rd of January for 10 days since 22am for 2 hours with consumption 1kW.
            // it will create consumption 20kWh in total
            var st1 = new DateTime(2022, 1, 3, 22, 0, 0);
            var blta1 = BlockHelpers.CreateRepetitiveDayBlock(st1,
                                                              st1.AddDays(10),
                                                              st1,
                                                              st1.AddHours(2),
                                                              1,
                                                              ebth.sourceId,
                                                              ebth.device2Id,
                                                              BlockDirection.Consumed,
                                                              BlockType.Simulated);
            entity.AddBlocks(blta1);
            ////////////////////////////////////////////////

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime2);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 7 + 50 + 20, total);
            }

            if (entity != null)
            {
                var res = entity.GetSummedValuesWithHourWindow(BlockTimeframe.Day,
                                                              starttime,
                                                              endtime2,
                                                              DateTime.MinValue.AddHours(8),
                                                              DateTime.MinValue.AddHours(20));
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 12 * 7 + 50, total);
            }

            if (entity != null)
            {
                var res = entity.GetSummedValuesWithHourWindow(BlockTimeframe.Day,
                                                              starttime,
                                                              endtime2,
                                                              DateTime.MinValue.AddHours(8),
                                                              DateTime.MinValue.AddHours(20),
                                                              true);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 12 * 7 + 20, total);
            }

        }

        [Fact]
        public void GetPowerConsumptionOfEntity()
        {
            var entity = new BaseEntity();

            // add some custom blocks of simulated consumption
            var blockstoadd = new List<IBlock>();
            var starttime = new DateTime(2022, 1, 3, 0, 0, 0);
            var timeframe = new TimeSpan(168, 0, 0);
            var endtime = starttime + timeframe;

            var timeframe2 = new TimeSpan(336, 0, 0);
            var endtime2 = starttime + timeframe2;

            var starttime3 = new DateTime(2022, 1, 12, 0, 0, 0);
            var timeframe3 = new TimeSpan(24, 0, 0);
            var endtime3 = starttime3 + timeframe3;
            var Block = new BaseBlock();

            //device which consume 1kW and run 168 hours, started on 3rd of January in 0:00, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                                  BlockDirection.Consumed,
                                                  starttime,
                                                  timeframe,
                                                  1,
                                                  ebth.sourceName,
                                                  ebth.device2Id));

            //device which consume 1kW and run 336 hours, started on 3rd of January in 0:00, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                                  BlockDirection.Consumed,
                                                  starttime,
                                                  timeframe2,
                                                  1,
                                                  ebth.sourceName,
                                                  ebth.device2Id));

            //device which consume 1kW and run 24 hours, started on 12rd of January in 0:00, for example PC
            blockstoadd.Add(Block.GetBlockByPower(BlockType.Simulated,
                                                  BlockDirection.Consumed,
                                                  starttime3,
                                                  timeframe3,
                                                  1,
                                                  ebth.sourceName,
                                                  ebth.device2Id));

            entity.AddBlocks(blockstoadd);

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 7 * 2, total);
                Assert.Equal(7, res.Count());
            }

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime.AddDays(-1));
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 6 * 2, total);
                Assert.Equal(6, res.Count());
            }

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime.AddDays(1));
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 7 * 2 + 24, total);
                Assert.Equal(8, res.Count());
            }

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime.AddDays(5));
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(1 * 24 * 7 * 2 + 24 * 5 + 24, total);
                Assert.Equal(12, res.Count());
            }

            if (entity != null)
            {
                var res = entity.GetSummedValues(BlockTimeframe.Day, starttime, endtime.AddDays(5), true);
                var total = 0.0;
                foreach (var b in res)
                    total += b.Amount;
                Assert.Equal(-1 * (1 * 24 * 7 * 2 + 24 * 5 + 24), total);
                Assert.Equal(12, res.Count());
            }

        }
    }
}
