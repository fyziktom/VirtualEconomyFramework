using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Consumers;

namespace TestVEDriversLite
{
    public static class EntitiesBlocksTests
    {
        [TestEntry]
        public static void EB_LoadTDD(string param)
        {
            EB_LoadTDDAsync(param);
        }
        public static async Task EB_LoadTDDAsync(string param)
        {
            var content = FileHelpers.ReadTextFromFile(param);
            if (!string.IsNullOrEmpty(content))
            {
                var tdds = ConsumersHelpers.LoadTDDs(content);
                var start = new DateTime(2022, 1, 1);

                var first = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[0], 
                                                                      start, start.AddMonths(1), 
                                                                      BlockTimeframe.Day);
                var second = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[0],
                                                                       start, start.AddYears(1),
                                                                       BlockTimeframe.Month);

                var firstblocks = DataProfileHelpers.ConvertDataProfileToBlocks(first, 
                                                                                BlockDirection.Consumed, 
                                                                                BlockType.Simulated, 
                                                                                "1234").ToList();

                var secondblocks = DataProfileHelpers.ConvertDataProfileToBlocks(second,
                                                                                 BlockDirection.Consumed,
                                                                                 BlockType.Simulated,
                                                                                 "1234").ToList();
            }
        }
    }
}
