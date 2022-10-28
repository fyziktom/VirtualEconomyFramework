using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Reflection.Metadata.BlobBuilder;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Tree;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.Consumers.Dto;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Sources.Dto;
using VEDriversLite.EntitiesBlocks.Sources;
using Newtonsoft.Json;
using VEDriversLite.EntitiesBlocks.Handlers.Dto;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Handlers
{
    public abstract class CommonEntitiesHandler : IEntitiesHandler
    {
        /// <summary>
        /// dictionary of all sources in the network, where the key is the unieque Id of the source
        /// </summary>
        public IEnumerable<IEntity> Sources { get => Entities.Values.Where(e => e.Type == EntityType.Source) ?? new List<IEntity>(); }
        /// <summary>
        /// dictionary of all consumers in the network, where the key is the uniue Id of the source
        /// </summary>
        public IEnumerable<IEntity> Consumers { get => Entities.Values.Where(e => e.Type == EntityType.Consumer) ?? new List<IEntity>(); }
        /// <summary>
        /// dictionary of all entities in the network, where the key is the uniue Id of the entity
        /// </summary>
        public ConcurrentDictionary<string, IEntity> Entities { get; set; } = new ConcurrentDictionary<string, IEntity>();
        /// <summary>
        /// Alocation schemes for split the amount of some block (for example automatic split shared PVE source between flats).
        /// </summary>
        public ConcurrentDictionary<string, AlocationScheme> AlocationSchemes { get; set; } = new ConcurrentDictionary<string, AlocationScheme>();
        /// <summary>
        /// Label of the unit of the Amount. For example "kWh" for energy application
        /// </summary>
        public string UnitLabel { get; set; } = "kWh";

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
                    if (baseload.AlocationSchemes != null)
                        foreach (var scheme in baseload.AlocationSchemes)
                            AlocationSchemes.TryAdd(scheme.Key, scheme.Value);

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

                foreach (var scheme in AlocationSchemes)
                    resultobj.AlocationSchemes.TryAdd(scheme.Key, scheme.Value);

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

        /// <summary>
        /// Create new Entity and add it to the entities list
        /// </summary>
        /// <param name="type"></param>
        /// <param name="entityName"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public virtual (bool, (string, string)) AddEntity(IEntity entity, string entityName, string parentId, string id = null, List<IBlock> blocks = null)
        {
            if (id == null)
            {
                id = Guid.NewGuid().ToString();
                entity.Name = entityName;
                entity.ParentId = parentId;
                entity.Id = id;
                if (blocks != null)
                    entity.AddBlocks(blocks);

                if (Entities.TryAdd(id, entity))
                {
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        if (Entities.TryGetValue(parentId, out var parent))
                            parent.Children.Add(id);

                        return (true, ($"New entity {entityName} - {id} added to the Entities list.", id));
                    }
                    else
                        return (false, ($"Cannot add entity {entityName} - {id} to the Entities list.", string.Empty));
                }
                else
                    return (false, ($"Cannot add entity {entityName} - {id} to the Parent. Parent no in Entities list.", string.Empty));
            }
            else
            {
                if (Entities.ContainsKey(id))
                    return (false, ($"Id {id} is already exists in the Entities dict.", string.Empty));
            }
            return (false, ($"Cannot add entity {entityName} to the list.", string.Empty));
        }


        /// <summary>
        /// remove the specific Entity
        /// </summary>
        /// <param name="id">consumer id</param>
        /// <returns></returns>
        public virtual (bool, string) RemoveEntity(string id)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot remove the entity. Entity id cannot be empty.");

            if (Entities.TryRemove(id, out var entity))
            {
                return (true, $"Consumer {entity.Name} - {id} removed.");
            }
            else
                return (false, $"Cannot remove the entity. Entity {id} is not in Entities list. Add the entity first please.");
        }

        /// <summary>
        /// Add simulator to specific entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="simulator"></param>
        /// <returns></returns>
        public virtual (bool, string) AddSimulatorToEntity(string id, ISimulator simulator)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot add simulator to the entity. Entity id cannot be empty.");
            if (simulator == null)
                return (false, $"Cannot add simulator to the entity {id}, block cannot be empty.");

            if (Entities.TryGetValue(id, out var entity))
            {
                var res = entity.AddSimulator(simulator);
                if (res.Item1)
                    return (true, $"Simulator {res.Item2} added to the entity {entity.Name} - {id}.");
                else
                    return (true, $"Cannot add simulator to the entity {entity.Name} - {id}. Error: {res.Item2}");
            }
            else
                return (false, $"Cannot add simulator to the entity. Entity {id} is not in Entities list. Add the entity first please.");

        }
        /// <summary>
        /// Remove simulators from entity. You can add multiple Ids in one command
        /// </summary>
        /// <param name="simulatorIds"></param>
        /// <returns></returns>
        public virtual (bool, string) RemoveSimulatorsFromEntity(string id, List<string> simulatorIds)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot remove simulator from the entity. Entity id cannot be empty.");
            if (simulatorIds == null)
                return (false, $"Cannot remove simulator from the entity {id}, simulators ids cannot be empty.");

            if (Entities.TryGetValue(id, out var entity))
            {
                var res = entity.RemoveSimulator(simulatorIds);
                if (res.Item1)
                    return (true, $"Simulators removed from the entity {entity.Name} - {id}.");
                else
                    return (true, $"Cannot remove simulators from the entity {entity.Name} - {id}.");
            }
            else
                return (false, $"Cannot remove simulators from the entity. Source {id} is not in Entities list. Add the consumer first please.");

        }

        /// <summary>
        /// Add block to the entity. 
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="block">Block</param>
        /// <returns></returns>
        public virtual (bool, string) AddBlockToEntity(string id, IBlock block)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot add block to the entity. Entity id cannot be empty.");
            if (block == null)
                return (false, $"Cannot add block to the entity {id}, block cannot be empty.");

            if (Entities.TryGetValue(id, out var entity))
            {
                if (entity.AddBlocks(new List<IBlock>() { block }))
                    return (true, $"Blocks added to the entity {entity.Name} - {id}.");
                else
                    return (true, $"Cannot add block to the entity {entity.Name} - {id}.");
            }
            else
                return (false, $"Cannot add block to the entity. Entity {id} is not in Entities list. Add the entity first please.");
        }

        /// <summary>
        /// Add block to the entity. 
        /// This param override of AddBlockToEntity will split block based on AlocationScheme to several entities
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="block">Block</param>
        /// <param name="alocationSchemeId">Alocation Scheme Id</param>
        /// <returns></returns>
        public virtual (bool, string) AddBlockToEntity(string id, IBlock block, string alocationSchemeId)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot add block to the entity. Entity id cannot be empty.");
            if (block == null)
                return (false, $"Cannot add block to the entity {id}, block cannot be empty.");


            if (AlocationSchemes.TryGetValue(alocationSchemeId, out var scheme))
            {
                var percentageRest = 0.0;
                var percentageSum = scheme.DepositPeers.Values.Where(p => p.Percentage > 0).Select(p => p.Percentage).Sum();
                if (percentageSum > 100)
                    return (false, $"Wrong deposit scheme {alocationSchemeId}. More than 100% of the alocation.");
                else if (percentageSum < 100)
                {
                    percentageRest = 100 - percentageSum;
                    if (Entities.TryGetValue(id, out var mainentity))
                    {
                        var b = new BaseBlock();
                        b.Fill(block);
                        b.Amount = block.Amount * (percentageRest / 100);

                        mainentity.AddBlocks(new List<IBlock>() { block });
                    }
                }

                foreach (var peer in scheme.DepositPeers)
                {
                    if (Entities.TryGetValue(peer.Value.PeerId, out var entity))
                    {
                        var b = new BaseBlock();
                        b.Fill(block);
                        b.Amount = block.Amount * (peer.Value.Percentage / 100);

                        if (entity.AddBlocks(new List<IBlock>() { block }))
                            return (true, $"Block added to the entity {entity.Name} - {id}.");
                        else
                            return (true, $"Cannot add block to the entity {entity.Name} - {id}.");
                    }
                }
            }
            else
            {
                return (true, $"Cannot add block to the entity {id}. Cannot find Alocation Scheme {alocationSchemeId}.");
            }
            return (true, $"Cannot add block {block.Id} to the entity {id}.");
        }

        /// <summary>
        /// Add blocks to the entity. 
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="blocks">list of the Blocks</param>
        /// <returns></returns>
        public virtual (bool, string) AddBlocksToEntity(string id, List<IBlock> blocks)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot add blocks to the entity. Entity id cannot be empty.");
            if (blocks == null)
                return (false, $"Cannot add blocks to the entity {id}, blocks cannot be empty.");

            if (Entities.TryGetValue(id, out var entity))
            {
                if (entity.AddBlocks(blocks))
                    return (true, $"Blocks added to the entity {entity.Name} - {id}.");
                else
                    return (true, $"Cannot add blocks to the entity {entity.Name} - {id}.");
            }
            else
                return (false, $"Cannot add blocks to the entity. Entity {id} is not in Entities list. Add the entity first please.");
        }

        /// <summary>
        /// Add blocks to the entity. 
        /// This param override of AddBlockToEntity will split block based on AlocationScheme to several entities
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="block">Block</param>
        /// <param name="alocationSchemeId">Alocation Scheme Id</param>
        /// <returns></returns>
        public virtual (bool, string) AddBlocksToEntity(string id, List<IBlock> blocks, string alocationSchemeId)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot add blocks to the entity. Entity id cannot be empty.");
            if (blocks == null)
                return (false, $"Cannot add blocks to the entity {id}, block cannot be empty.");

            if (AlocationSchemes.TryGetValue(alocationSchemeId, out var scheme))
            {
                var percentageRest = 0.0;
                var percentageSum = scheme.DepositPeers.Values.Where(p => p.Percentage > 0).Select(p => p.Percentage).Sum();
                if (percentageSum > 100)
                    return (false, $"Wrong deposit scheme {alocationSchemeId}. More than 100% of the alocation.");

                else if (percentageSum < 100)
                {
                    percentageRest = 100 - percentageSum;
                    if (Entities.TryGetValue(id, out var mainentity))
                    {
                        var blks = new List<IBlock>();

                        foreach (var block in blocks)
                        {
                            var b = new BaseBlock();
                            b.Fill(block);
                            b.Amount = block.Amount * (percentageRest / 100);
                            blks.Add(b);
                        }

                        mainentity.AddBlocks(blks);
                    }
                }

                foreach (var peer in scheme.DepositPeers)
                {
                    if (Entities.TryGetValue(peer.Value.PeerId, out var entity))
                    {
                        var blks = new List<IBlock>();

                        foreach (var block in blocks)
                        {
                            var b = new BaseBlock();
                            b.Fill(block);
                            b.Amount = block.Amount * (peer.Value.Percentage / 100);
                            blks.Add(b);
                        }

                        entity.AddBlocks(blks);
                    }
                }

                return (true, $"Blocks added to the entity - {id}.");
            }
            else
            {
                return (true, $"Cannot add block to the entity {id}. Cannot find Alocation Scheme {alocationSchemeId}.");
            }
            return (true, $"Cannot add block to the entity {id}.");
        }

        /// <summary>
        /// Change block parameters to the entity. 
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="type">type of Block</param>
        /// <returns></returns>
        public virtual (bool, string) ChangEntityBlockParameters(string id,
                                                                 string blockId,
                                                                 string name = null,
                                                                 string description = null,
                                                                 BlockType? type = null,
                                                                 double? amount = null,
                                                                 BlockDirection? direction = null,
                                                                 DateTime? startTime = null,
                                                                 TimeSpan? timeframe = null)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot change blocks to the entity. Entity id cannot be empty.");
            if (string.IsNullOrEmpty(blockId))
                return (false, "Cannot change blocks to the entity. Block id cannot be empty.");

            if (Entities.TryGetValue(id, out var entity))
            {
                if (entity.ChangeBlockParameters(blockId, name, description, type, amount, direction, startTime, timeframe).Item1)
                {
                    return (true, $"Block {blockId} in {entity.Name} - {id} paramters changed.");
                }
                else
                    return (true, $"Cannot add blocks to the entity {entity.Name} - {id}");
            }
            else
                return (false, $"Cannot add blocks to the consumer. Entity {id} is not in entities list. Add the consumer first please.");
        }


        /// <summary>
        /// Remove blocks from the entity. 
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="blocks">list of the Ids of the Blocks</param>
        /// <returns></returns>
        public virtual (bool, string) RemoveBlocksFromEntity(string id, List<string> blocks)
        {
            if (string.IsNullOrEmpty(id))
                return (false, "Cannot remove blocks from the entity. Entity id cannot be empty.");
            if (blocks == null)
                return (false, $"Cannot remove blocks from the entity {id}, blocks cannot be empty.");

            if (Entities.TryGetValue(id, out var entity))
            {
                if (entity.RemoveBlocks(blocks))
                    return (true, $"Blocks removed from the entity {entity.Name} - {id}.");
                else
                    return (true, $"Cannot remove blocks from the entity {entity.Name} - {id}.");
            }
            else
                return (false, $"Cannot remove blocks from the entity. Source {id} is not in Entities list. Add the consumer first please.");
        }

        /// <summary>
        /// Add symbolic connection between some two subsource in the network. 
        /// each entity has list of childerns, means relation to other entities
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="subentityId"></param>
        /// <returns></returns>
        public virtual (bool, string) AddSubEntityToEntity(string entityId, string subentityId)
        {
            if (!string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(subentityId))
            {
                if (Entities.TryGetValue(entityId, out var entity))
                {
                    if (Entities.TryGetValue(subentityId, out var subentity))
                    {
                        var scan = Entities.Values.Where(c => c.Children.Contains(subentityId)).ToList();
                        if (scan != null && scan.Count > 0)
                        {
                            var cns = scan.First();
                            if (cns != null)
                                return (false, $"subentity is already registered with entity {cns.Name} - {cns.Id}.");
                        }
                        if (!entity.Children.Contains(subentityId))
                        {
                            subentity.ParentId = entity.Id;
                            entity.Children.Add(subentityId);
                            return (true, $"subentity {subentity.Name} - {subentityId} added to entity {entity.Name} - {entity.Id} list.");
                        }
                    }
                    else
                        return (false, $"cannot find subentity {subentityId} in the Entities list. You must add the subentity to the Entities list first.");

                }
                else
                    return (false, $"cannot find consumer {entityId} in the Consumers list. You must add the consumer to the Consumers list first.");
            }
            else
            {
                return (false, $"Please fill the inspu Ids of both consumers and subconsumer.");
            }
            return (false, $"Cannot add subconsumer to the specific consumer.");
        }

        /// <summary>
        /// Remove symbolic connection between some two entities in the network. 
        /// each entity has list of subentities, means relation to other entities
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="subentityId"></param>
        /// <returns></returns>
        public virtual (bool, string) RemoveSubEntityFromEntity(string entityId, string subentityId)
        {
            if (!string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(subentityId))
            {
                if (Entities.TryGetValue(entityId, out var entity))
                {
                    if (entity.Children.Contains(subentityId))
                    {
                        if (Entities.TryGetValue(subentityId, out var subentity))
                            subentity.ParentId = "";
                        
                        entity.Children.Remove(subentityId);
                        return (true, $"Subentity {subentityId} removed from entity {entity.Name} - {entity.Id} list.");
                    }
                }
                else
                    return (false, $"cannot find entity {entityId} in the Entities list. You must add the entity to the Entities list first.");
            }
            else
            {
                return (false, $"Please fill the inspu Ids of both entity and subentity.");
            }
            return (false, $"Cannot add subentity to the specific entity.");
        }

        /// <summary>
        /// Get recalculated power consumption represented as list of Blocks split based on setted timegrame
        /// </summary>
        /// <param name="entityId">consumer Id</param>
        /// <param name="withSubConsumers">when this is set, the function will load the same consumption of all subconsumers of this consumer. Then it is summed together as one output profile of consumpion.</param>
        /// <param name="timeframesteps">enum of step of timeframe to recalculate the consumption into blocks, for example based on minutes</param>
        /// <param name="starttime">start datetime of the recalculation frame</param>
        /// <param name="endtime">end datetime of the recalculation frame</param>
        /// <returns></returns>
        public virtual List<IBlock> GetConsumptionOfEntity(string entityId, 
                                                                BlockTimeframe timeframesteps, 
                                                                DateTime starttime, 
                                                                DateTime endtime, 
                                                                bool withSubConsumers = true, 
                                                                bool takeConsumptionAsInvert = false,
                                                                List<BlockDirection> justThisDirections = null,
                                                                List < BlockType > justThisType = null,
                                                                bool addSimulators = true)
        {

            if (string.IsNullOrEmpty(entityId))
                return null;

            if (Entities.TryGetValue(entityId, out var entity))
            {
                var mainres = entity.GetSummedValuesOptimized(timeframesteps, 
                                                              starttime, 
                                                              endtime, 
                                                              takeConsumptionAsInvert, 
                                                              justThisDirections, 
                                                              justThisType, 
                                                              addSimulators);

                if (withSubConsumers && mainres != null)
                {
                    Queue<IEntity> queue = new Queue<IEntity>();
                    queue.Enqueue(entity);

                    var tmpEntity = new BaseEntity() { Name = "tmp", ParentId = entity.ParentId, Id = entityId + "-tmp" };

                    while (queue.Count > 0)
                    {
                        var e = queue.Dequeue();
                        if (e != null)
                        {
                            foreach (var childId in e.Children)
                            {
                                if (Entities.TryGetValue(childId, out var sbc))
                                {
                                    tmpEntity.AddBlocks(sbc.BlocksOrderByTime.ToList());

                                    if (sbc.IsParent)
                                        queue.Enqueue(sbc);
                                }
                            }
                        }
                    }

                    var re = tmpEntity.GetSummedValuesOptimized(timeframesteps, 
                                                                starttime, 
                                                                endtime, 
                                                                takeConsumptionAsInvert, 
                                                                justThisDirections, 
                                                                justThisType, 
                                                                addSimulators);
                    if (re != null)
                    {
                        foreach(var block in re.Where(r => r.Amount > 0 || r.Amount < 0))
                        {
                            var r = mainres.First(b => b.StartTime == block.StartTime);
                            if (r != null)
                                r.Amount += block.Amount;
                        }
                    }
                }
                return mainres;
            }

            return null;
        }

        /// <summary>
        /// Get all Blocks with the all childern blocks
        /// </summary>
        /// <param name="entityId">consumer Id</param>
        /// <returns></returns>
        public virtual List<IBlock> GetBlocksOfEntityWithChildernBlocks(string entityId)
        {
            if (string.IsNullOrEmpty(entityId))
                return null;

            if (Entities.TryGetValue(entityId, out var entity))
            {
                var tmpEntity = new BaseEntity() { Name = "tmp", ParentId = entity.ParentId, Id = entityId + "-tmp" };
                tmpEntity.AddBlocks(entity.BlocksOrderByTime);

                Queue<IEntity> queue = new Queue<IEntity>();
                queue.Enqueue(entity);

                while (queue.Count > 0)
                {
                    var e = queue.Dequeue();
                    if (e != null)
                    {
                        foreach (var childId in e.Children)
                        {
                            if (Entities.TryGetValue(childId, out var sbc))
                            {
                                tmpEntity.AddBlocks(sbc.BlocksOrderByTime.ToList());

                                if (sbc.IsParent)
                                    queue.Enqueue(sbc);
                            }
                        }
                    }
                }
                return tmpEntity.BlocksOrderByTime.ToList();
            }

            return null;
        }

        /// <summary>
        /// Get list of the blocks of whole group based on setup timespan and step and specific timegrame and window
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// Window will cut the Amounts which are outside of specific time window. 
        /// For example window can be 8:00-20:00. Only consumption in this window is added to final consumption.
        /// If you set invertwindow to true with 8:00-20:00 times, it will take consumption just between 0:00-8:00 and 20:00-24:00 and add it to final consumption.
        /// </summary>
        /// <param name="entityId">consumer Id</param>
        /// <param name="withSubConsumers">when this is set, the function will load the same consumption of all subconsumers of this consumer. Then it is summed together as one output profile of consumpion.</param>
        /// <param name="timeframesteps">enum of step of timeframe to recalculate the consumption into blocks, for example based on minutes</param>
        /// <param name="starttime">start datetime of the recalculation frame</param>
        /// <param name="endtime">end datetime of the recalculation frame</param>
        /// <param name="blockwindowstarttime">Start of time window. Takes just Hours parameter!</param>
        /// <param name="blockwindowendtime">End of time window. Takes just Hours parameter!</param>
        /// <returns></returns>
        public virtual List<IBlock> GetConsumptionOfEntityWithWindow(string entityId, 
                                                                          BlockTimeframe timeframesteps, 
                                                                          DateTime starttime, 
                                                                          DateTime endtime,
                                                                          DateTime blockwindowstarttime,
                                                                          DateTime blockwindowendtime,
                                                                          bool invertWindow = false,
                                                                          bool withSubConsumers = true, 
                                                                          bool takeConsumptionAsInvert = false,
                                                                          List<BlockDirection> justThisDirections = null,
                                                                          List<BlockType> justThisType = null,
                                                                          bool addSimulators = true)
        {

            if (string.IsNullOrEmpty(entityId))
                return null;

            if (Entities.TryGetValue(entityId, out var entity))
            {
                var mainres = entity.GetSummedValuesWithHourWindow(timeframesteps, 
                                                                  starttime, 
                                                                  endtime, 
                                                                  blockwindowstarttime, 
                                                                  blockwindowendtime, 
                                                                  invertWindow, 
                                                                  takeConsumptionAsInvert, 
                                                                  justThisDirections,
                                                                  justThisType,
                                                                  addSimulators);

                if (withSubConsumers && mainres != null)
                {
                    var subsres = new Dictionary<string, List<IBlock>>();

                    Queue<IEntity> queue = new Queue<IEntity>();
                    queue.Enqueue(entity);

                    var tmpEntity = new BaseEntity() { Name = "tmp", ParentId = entity.ParentId, Id = entityId + "-tmp" };

                    while (queue.Count > 0)
                    {
                        var e = queue.Dequeue();
                        if (e != null)
                        {
                            foreach (var childId in e.Children)
                            {
                                if (Entities.TryGetValue(childId, out var sbc))
                                {
                                    tmpEntity.AddBlocks(sbc.BlocksOrderByTime.ToList());

                                    if (sbc.IsParent)
                                        queue.Enqueue(sbc);
                                }

                            }
                        }
                    }

                    var re = tmpEntity.GetSummedValuesWithHourWindow(timeframesteps,
                                                                    starttime,
                                                                    endtime,
                                                                    blockwindowstarttime,
                                                                    blockwindowendtime,
                                                                    invertWindow,
                                                                    takeConsumptionAsInvert, 
                                                                    justThisDirections,
                                                                    justThisType, 
                                                                    addSimulators);
                    if (re != null)
                    {
                        foreach (var block in re.Where(r => r.Amount > 0 || r.Amount < 0))
                        {
                            var r = mainres.First(b => b.StartTime == block.StartTime);
                            if (r != null)
                                r.Amount += block.Amount;
                        }
                    }
                }
                return mainres;
            }

            return null;
        }

        /// <summary>
        /// Get recalculated power consumption represented as list of Blocks split based on setted timegrame
        /// </summary>
        /// <param name="consumerId">consumer Id</param>
        /// <param name="withChilds">when this is set, the function will load the same consumption of all subconsumers of this consumer. Then it is summed together as one output profile of consumpion.</param>
        /// <param name="timeframesteps">enum of step of timeframe to recalculate the consumption into blocks, for example based on minutes</param>
        /// <param name="starttime">start datetime of the recalculation frame</param>
        /// <param name="endtime">end datetime of the recalculation frame</param>
        /// <returns></returns>
        public virtual List<IBlock> GetRecalculatedBlocks(List<string> ids, BlockTimeframe timeframesteps, DateTime starttime, DateTime endtime, EntityType eetype = EntityType.Consumer, bool withChilds = true)
        {

            if (ids == null) return null;
            if (ids.Count == 0) return null;
            var parentId = string.Empty;

            var subsres = new Dictionary<string, List<IBlock>>();

            foreach (var id in ids)
            {
                // Get entity from Sources or Consumers dicts
                var entity = GetEntity(id, eetype);
                if (entity == null) return null;

                if (string.IsNullOrEmpty(parentId))
                    parentId = id;

                Queue<IEntity> subs = new Queue<IEntity>();
                subs.Enqueue(entity);

                //explore the tree and load all childs blocks also if required (withChilds = true)
                while (subs.Count > 0)
                {
                    var ent = subs.Dequeue();
                    if (ent != null)
                    {
                        if (ent.Blocks.Count > 0)
                        {
                            var r = ent.GetSummedValues(timeframesteps, starttime, endtime);
                            if (r != null)
                                subsres.TryAdd(ent.Id, r);
                        }
                        if (withChilds)
                        {
                            foreach (var se in ent.Children)
                            {
                                var e = GetEntity(se, eetype);
                                if (e != null)
                                    subs.Enqueue(e);
                            }
                        }
                    }
                }
            }
            var result = new List<IBlock>();
            if (subsres.Count > 0)
            {
                //create empty list of final block to sum the subresults in it
                if (eetype == EntityType.Source)
                    result = BlockHelpers.CreateEmptyBlocks(timeframesteps, starttime, endtime, parentId, 0, BlockDirection.Created, BlockType.Calculated);
                else if (eetype == EntityType.Consumer)
                    result = BlockHelpers.CreateEmptyBlocks(timeframesteps, starttime, endtime, parentId, 0, BlockDirection.Consumed, BlockType.Calculated);
                else if (eetype == EntityType.Both)
                    result = BlockHelpers.CreateEmptyBlocks(timeframesteps, starttime, endtime, parentId, 0, BlockDirection.Mix, BlockType.Calculated);

                // add all items of subresults to the final list
                if (result != null && result.Count > 0)
                {
                    foreach (var sres in subsres)
                    {
                        foreach (var res in sres.Value.Where(r => r.Amount > 0 || r.Amount < 0))
                        {
                            var r = result.First(b => b.StartTime == res.StartTime);
                            if (r != null)
                                r.Amount += res.Amount;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get Entity if exists
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual IEntity GetEntity(string id, EntityType type)
        {
            IEntity entity = null;

            if (Entities.TryGetValue(id, out var cs))
                entity = cs as IEntity;
            return entity;
        }

        /// <summary>
        /// Get all entity blocks
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IEnumerable<IBlock> GetEntityBlocks(string id)
        {
            IEntity entity = null;

            if (Entities.TryGetValue(id, out var cs))
                entity = cs as IEntity;
            if (entity != null)
                return entity.BlocksOrderByTime;
            else
                return new List<IBlock>();
        }

        /// <summary>
        /// Find Entity by Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IEntity FindEntityByName(string name)
        {
            var entity = Entities.Values.Where(e => e.Name == name).FirstOrDefault();
            if (entity != null)
                return entity;
            else
                return null;
        }

        /// <summary>
        /// Remove all blocks in entity
        /// </summary>
        /// <param name="id"></param>
        public virtual void RemoveAllEntityBlocks(string id)
        {
            if (Entities.TryGetValue(id, out var entity))
                entity.Blocks.Clear();
        }

        /// <summary>
        /// Change Blocks direction all blocks in entity
        /// </summary>
        /// <param name="id"></param>
        public virtual void ChangeAllEntityBlocksDirection(string id, BlockDirection direction, BlockDirection originalDirection = BlockDirection.Mix)
        {
            if (Entities.TryGetValue(id, out var entity))
                entity.ChangeAllBlocksDirection(direction, originalDirection);
        }
        /// <summary>
        /// Change Blocks direction all blocks in entity
        /// </summary>
        /// <param name="id"></param>
        public virtual void ChangeAllEntityBlocksDirection(string id, BlockDirection direction, List<string> ids, BlockDirection originalDirection = BlockDirection.Mix)
        {
            if (Entities.TryGetValue(id, out var entity))
            {
                if (ids == null)
                    entity.ChangeAllBlocksDirection(direction, originalDirection);
                else 
                    entity.ChangeAllBlocksDirection(direction, ids, originalDirection);
            }
        }
        /// <summary>
        /// Change Blocks type all blocks in entity
        /// </summary>
        /// <param name="type"></param>
        public virtual void ChangeAllBlocksType(string id, BlockType type, BlockType originalType = BlockType.Simulated)
        {
            if (Entities.TryGetValue(id, out var entity))
                entity.ChangeAllBlocksType(type, originalType);
        }
        /// <summary>
        /// Change Blocks type specified blocks in entity
        /// </summary>
        /// <param name="type"></param>
        public virtual void ChangeAllBlocksType(string id, BlockType type, List<string> ids, BlockType originalType = BlockType.Simulated)
        {
            if (Entities.TryGetValue(id, out var entity))
            {
                if (ids == null)
                    entity.ChangeAllBlocksType(type, originalType);
                else
                    entity.ChangeAllBlocksType(type, ids, originalType);
            }
        }

        public class TreeQueue
        {
            public TreeItem? Parent { get; set; }
            public IEntity? Entity { get; set; }
        }

        /// <summary>
        /// Get Entities Tree
        /// </summary>
        /// <param name="rootId"></param>
        /// <returns></returns>
        public virtual TreeItem GetTree(string rootId)
        {
            if (string.IsNullOrEmpty(rootId)) return null;

            // Get entity from Sources or Consumers dicts
            var entity = GetEntity(rootId, EntityType.Both);
            if (entity == null) return null;

            var result = new TreeItem() { Name = entity.Name, Id = entity.Id, Type = entity.Type, Depth = 0, Entity  = entity };

            Queue<TreeQueue> queue = new Queue<TreeQueue>();
            foreach (var se in entity.Children)
            {
                var e = GetEntity(se, EntityType.Both);
                if (e != null)
                    queue.Enqueue(new TreeQueue() { Parent = result, Entity = e });
            }

            //explore the tree and load all childerns
            while (queue.Count > 0)
            {
                var q = queue.Dequeue();
                if (q != null && q.Parent != null && q.Entity != null)
                {
                    var child = new TreeItem()
                    {
                        Name = q.Entity.Name, Id = q.Entity.Id, Type = q.Entity.Type, Depth = q.Parent.Depth + 1, Parent = q.Parent,
                        Entity = q.Entity
                    };
                    q.Parent.AddChild(child);

                    foreach (var se in q.Entity.Children)
                    {
                        var e = GetEntity(se, EntityType.Both);
                        if (e != null)
                            queue.Enqueue(new TreeQueue() { Parent = child, Entity = e });
                    }
                }
            }

            return result;
        }
    }
}
