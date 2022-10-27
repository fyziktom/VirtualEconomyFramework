using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.PVECalculations;

namespace VEDriversLite.EntitiesBlocks.Consumers
{
    public class ConsumerTDDSimulator : CommonSimulator
    {
        public List<DataProfile> TDDs { get; set; } = new List<DataProfile>();

        public string Name { get; set; } = string.Empty;

        public override (bool, string) ExportConfig()
        {
            var res = JsonConvert.SerializeObject(TDDs);
            return (true, res);
        }
        public override (bool, string) ImportConfig(string config)
        {
            if (!string.IsNullOrEmpty(config))
            {
                try
                {
                    var tds = JsonConvert.DeserializeObject<List<DataProfile>>(config);
                    if (tds != null)
                    {
                        TDDs = tds;
                        return (true, string.Empty);
                    }
                }
                catch
                {
                    Console.WriteLine("Not standard config, trying to parse the TDD raw data...");
                }

                var tdds = ConsumersHelpers.LoadTDDs(config);
                if (tdds != null)
                {
                    TDDs = tdds;
                    return (true, string.Empty);
                }
            }
            return (false, string.Empty);
        }

        public override IEnumerable<IBlock> GetBlocks(BlockTimeframe timeframe, DateTime start, DateTime end, Dictionary<string, DataProfile> inputProfiles, Dictionary<string, List<IBlock>> inputBlocks, Dictionary<string, object> options)
        {
            if (TDDs == null || (TDDs != null && TDDs.Count == 0))
                throw new Exception($"Please import TDD data before use of simulator Id: {Id}.");
            if (end < start)
                throw new Exception($"Wrong input of the start and end in simulator Id: {Id}. End cannot be earlier than start.");

            var block = new BaseBlock();

            var sourceId = Id;
            if (options.TryGetValue("sourceId", out var sid))
                sourceId = sid as string;
            var parentId = ParentId;
            if (options.TryGetValue("parentId", out var pid))
                parentId = pid as string;
            var name = $"Block-{Name}";
            if (options.TryGetValue("name", out var n))
                name = n as string;
            var tddIndex = -1;
            if (options.TryGetValue("tddIndex", out var tix))
                tddIndex = (int)tix;

            start = start.AddHours(-start.Hour).AddMinutes(-start.Minute).AddSeconds(-start.Second);
            end = end.AddHours(-end.Hour).AddMinutes(-end.Minute).AddSeconds(-end.Second);
            var tmp = start;
            var firstBlockId = string.Empty;

            var ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, start);

            while (tmp < end)
            {
                var amount = 0.0;

                var htmp = tmp;
                var hend = htmp.Add(ts);
                while (htmp < hend)
                {
                    var tot = 0.0;
                    var tddmoment = htmp.AddMinutes(-htmp.Minute).AddSeconds(-htmp.Second).AddMilliseconds(-htmp.Millisecond);

                    if (TDDs != null && tddIndex >= 0 && TDDs.Count > tddIndex)
                    {
                        if (TDDs[tddIndex].ProfileData.TryGetValue(tddmoment, out var value))
                            tot = value;
                    }
                    else if (TDDs != null && tddIndex < 0 && TDDs.Count > 0)
                    {
                        foreach(var tdd in TDDs)
                        {
                            if (tdd.ProfileData.TryGetValue(tddmoment, out var value))
                                tot += value;
                        }
                    }

                    if (htmp.AddHours(1) < hend)
                        amount += tot;
                    else
                        amount += tot * (hend - htmp).TotalHours;

                    htmp = htmp.AddHours(1);
                }

                if (string.IsNullOrEmpty(sourceId))
                    sourceId = Id;
                if (string.IsNullOrEmpty(parentId))
                    parentId = Id;
                if (string.IsNullOrEmpty(name))
                    name = Name;

                var rblock = block.GetBlock(BlockType.Simulated,
                                            BlockDirection.Created,
                                            tmp,
                                            ts,
                                            amount,
                                            sourceId,
                                            name,
                                            null,
                                            parentId);

                rblock.IsInDayOnly = true;
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = rblock.Id;
                else
                    rblock.RepetitiveSourceBlockId = firstBlockId;

                yield return rblock;

                tmp = tmp.Add(ts);
                ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, tmp);
            }
        }
        
    }
}
