using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Energy.Consumers.Dto;
using VEDriversLite.Energy.Consumers;
using VEDriversLite.Energy.Handlers.Dto;
using VEDriversLite.Energy.Sources.Dto;
using VEDriversLite.Energy.Sources;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Handlers;

namespace VEDriversLite.Energy.Handlers
{
    public class EnergyGridHandler : BaseEntitiesHandler
    {
        /// <summary>
        /// Load Sources and Consumers data from the config file
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public (bool, string) LoadFromConfig(string config)
        {
            try
            {
                var baseload = JsonConvert.DeserializeObject<CompleteConfigDto>(config);

                if (baseload != null)
                {
                    foreach (var item in baseload.Sources)
                    {
                        if (item != null)
                        {
                            var ent = item.Fill();


                            foreach (var rblock in ent.Blocks.Values.Where(b => b.IsRepetitiveSource))
                            {
                                if (rblock.IsOffPeriodRepetitive)
                                {
                                    var ents = BlockHelpers.CreateRepetitiveBlock(rblock.RepetitiveFirstRun.Value,
                                                                                  rblock.RepetitiveEndRun.Value,
                                                                                  rblock.StartTime,
                                                                                  rblock.EndTime,
                                                                                  rblock.OffPeriod.Value,
                                                                                  rblock.Amount,
                                                                                  rblock.SourceId,
                                                                                  rblock.ParentId,
                                                                                  rblock.Direction,
                                                                                  rblock.Type,
                                                                                  rblock.Name,
                                                                                  rblock.Id);
                                    ents.RemoveAt(0);
                                    ent.AddBlocks(ents);
                                }
                                else if (rblock.IsInDayOnly)
                                {
                                    var ents = BlockHelpers.CreateRepetitiveDayBlock(rblock.RepetitiveFirstRun.Value,
                                                                                     rblock.RepetitiveEndRun.Value,
                                                                                     rblock.StartTime,
                                                                                     rblock.EndTime,
                                                                                     rblock.Amount,
                                                                                     rblock.SourceId,
                                                                                     rblock.ParentId,
                                                                                     rblock.Direction,
                                                                                     rblock.Type,
                                                                                     rblock.JustInWeek,
                                                                                     rblock.JustInWeekends,
                                                                                     rblock.Name,
                                                                                     rblock.Id);
                                    ents.RemoveAt(0);
                                    ent.AddBlocks(ents);
                                }
                            }

                            if (!Entities.ContainsKey(ent.Id))
                                Entities.TryAdd(ent.Id, ent);
                        }
                    }
                    foreach (var item in baseload.Consumers)
                    {
                        if (item != null)
                        {
                            var ent = item.Fill();

                            foreach (var rblock in ent.Blocks.Values.Where(b => b.IsRepetitiveSource))
                            {
                                if (rblock.IsOffPeriodRepetitive)
                                {
                                    var ents = BlockHelpers.CreateRepetitiveBlock(rblock.RepetitiveFirstRun.Value,
                                                                                  rblock.RepetitiveEndRun.Value,
                                                                                  rblock.StartTime,
                                                                                  rblock.EndTime,
                                                                                  rblock.OffPeriod.Value,
                                                                                  rblock.Amount,
                                                                                  rblock.SourceId,
                                                                                  rblock.ParentId,
                                                                                  rblock.Direction,
                                                                                  rblock.Type,
                                                                                  rblock.Name,
                                                                                  rblock.Id);
                                    ents.RemoveAt(0);
                                    ent.AddBlocks(ents);
                                }
                                else if (rblock.IsInDayOnly)
                                {
                                    var ents = BlockHelpers.CreateRepetitiveDayBlock(rblock.RepetitiveFirstRun.Value,
                                                                                     rblock.RepetitiveEndRun.Value,
                                                                                     rblock.StartTime,
                                                                                     rblock.EndTime,
                                                                                     rblock.Amount,
                                                                                     rblock.SourceId,
                                                                                     rblock.ParentId,
                                                                                     rblock.Direction,
                                                                                     rblock.Type,
                                                                                     rblock.JustInWeek,
                                                                                     rblock.JustInWeekends,
                                                                                     rblock.Name,
                                                                                     rblock.Id);
                                    ents.RemoveAt(0);
                                    ent.AddBlocks(ents);
                                }
                            }

                            if (!Entities.ContainsKey(ent.Id))
                                Entities.TryAdd(ent.Id, ent);
                        }
                    }
                }
                return (true, "Loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load the config. " + ex.Message);
                return (false, "Cannot load the config. " + ex.Message);
            }
            return (false, "Cannot load the config.");
        }


        /// <summary>
        /// Export the config file as json string
        /// </summary>
        /// <returns></returns>
        public (bool, string) ExportToConfig()
        {
            try
            {
                var resultobj = new CompleteConfigDto();
                foreach (var src in Sources)
                {
                    var s = new SourceConfigDto();
                    var sc = src as ISource;
                    if (sc == null) continue;
                    s.Load(sc);
                    var repetitiveToRemove = src.Blocks.Values.Where(b => b.IsRepetitiveChild).Select(b => b.Id).ToList();
                    foreach (var block in repetitiveToRemove)
                    {
                        if (s.Blocks.ContainsKey(block))
                            s.Blocks.TryRemove(block, out var blk);
                    }
                    resultobj.Sources.Add(s);
                }
                var cnss = new List<ConsumerConfigDto>();
                foreach (var cns in Consumers)
                {
                    var c = new ConsumerConfigDto();
                    var cs = cns as IConsumer;
                    if (cs == null) continue;
                    c.Load(cs);
                    var repetitiveToRemove = cns.Blocks.Values.Where(b => b.IsRepetitiveChild).Select(b => b.Id).ToList();
                    foreach (var block in repetitiveToRemove)
                    {
                        if (c.Blocks.ContainsKey(block))
                            c.Blocks.TryRemove(block, out var blk);
                    }
                    resultobj.Consumers.Add(c);
                }

                var result = JsonConvert.SerializeObject(resultobj, Newtonsoft.Json.Formatting.Indented);

                return (true, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot create the config. " + ex.Message);
                return (false, "Cannot create the config. " + ex.Message);
            }
            return (false, "Cannot create the config.");
        }

    }
}
