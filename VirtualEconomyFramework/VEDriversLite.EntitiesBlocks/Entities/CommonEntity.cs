using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;
using static System.Reflection.Metadata.BlobBuilder;

namespace VEDriversLite.EntitiesBlocks.Entities
{
    public abstract class CommonEntity : IEntity
    {
        public EntityType Type { get; set; } = EntityType.None;
        /// <summary>
        /// Unique Id of the entity
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private string _id = string.Empty;
        /// <summary>
        /// Readable Name of the entity
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                //LastChange = DateTime.UtcNow;
            }
        }
        private string _name = string.Empty;
        /// <summary>
        /// Description of the entity
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                //LastChange = DateTime.UtcNow;
            }
        }
        private string _description = string.Empty;
        /// <summary>
        /// Entity location
        /// </summary>
        public Coordinates Coords
        {
            get => _coords;
            set
            {
                _coords = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private Coordinates _coords = new Coordinates();

        /// <summary>
        /// Parent Id of the entity
        /// </summary>
        public string ParentId
        {
            get => _parentId;
            set
            {
                _parentId = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private string _parentId = string.Empty;
        /// <summary>
        /// List of child Ids which are under this entity in hierarchy
        /// </summary>
        public List<string> Children
        {
            get => _children;
            set
            {
                _children = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private List<string> _children = new List<string>();
        /// <summary>
        /// If the childs list contains some items it means the object is parent in the structure
        /// </summary>
        public bool IsParent { get => Children.Count > 0; }
        public ConcurrentDictionary<string, IBlock> Blocks { get; set; } = new ConcurrentDictionary<string, IBlock>();
        /// <summary>
        /// Return blocks ordered by time
        /// </summary>
        public List<IBlock> BlocksOrderByTime { get => Blocks.Values.OrderBy(b => b.StartTime).ToList() ?? new List<IBlock>(); }
        /// <summary>
        /// Last change of the parameters of the entity
        /// </summary>
        public DateTime LastChange { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Dictionary of simulators
        /// </summary>
        public ConcurrentDictionary<string, ISimulator> Simulators { get; set; } = new ConcurrentDictionary<string, ISimulator>();

        /// <summary>
        /// dd simulator to entity
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="simulatorId"></param>
        /// <returns></returns>
        public virtual (bool,string) AddSimulator(ISimulator simulator)
        {
            if (simulator == null) return (false, "Simulator object cannot be null.");

            if (string.IsNullOrEmpty(simulator.Id))
                simulator.Id = Guid.NewGuid().ToString();

            if (!Simulators.ContainsKey(simulator.Id))
            {
                simulator.ParentId = Id;
                Simulators.TryAdd(simulator.Id, simulator);
            }

            LastChange = DateTime.UtcNow;
            return (true, simulator.Id);
        }

        /// <summary>
        /// Remove simulator from dictionary of Simulators
        /// </summary>
        /// <param name="simulators">List of Ids of blocks</param>
        /// <returns></returns>
        public virtual (bool,string) RemoveSimulator(List<string> simulatorIds)
        {
            if (simulatorIds == null) return (false,string.Empty);
            foreach (var simulator in simulatorIds)
            {
                if (Simulators.ContainsKey(simulator))
                    Simulators.TryRemove(simulator, out var sim);
            }
            LastChange = DateTime.UtcNow;
            return (true, string.Empty);
        }

        /// <summary>
        /// Try to add the block to the Blocks dictionary. Block must have unique hashs
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public virtual bool AddBlock(IBlock block)
        {
            if (block == null) return false;

            if (!Blocks.ContainsKey(block.Id))
            {
                block.ParentId = Id;
                Blocks.TryAdd(block.Id, block);
            }
            
            LastChange = DateTime.UtcNow;
            return true;
        }
        /// <summary>
        /// Try to add the list of the blocks to the Blocks dictionary. Blocks must have unique hashs
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public virtual bool AddBlocks(List<IBlock> blocks)
        {
            if (blocks == null) return false;
            foreach (var block in blocks)
            {
                if (!Blocks.ContainsKey(block.Id))
                {
                    block.ParentId = Id;
                    Blocks.TryAdd(block.Id, block);
                }
            }
            LastChange = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Remove blocks from the dictionary of blocks.
        /// </summary>
        /// <param name="blocks">List of Ids of blocks</param>
        /// <returns></returns>
        public virtual bool RemoveBlocks(List<string> blocks)
        {
            if (blocks == null) return false;
            foreach (var block in blocks)
            {
                if (Blocks.ContainsKey(block))
                    Blocks.TryRemove(block, out var blk);
            }
            LastChange = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Remove blocks from the dictionary of blocks which are repetitive of some source block.
        /// </summary>
        /// <param name="firstBlockId">Id of first block</param>
        /// <returns></returns>
        public virtual bool RemoveRepetitiveBlocksLine(string firstBlockId)
        {
            if (string.IsNullOrEmpty(firstBlockId)) return false;

            if (Blocks.ContainsKey(firstBlockId))
                Blocks.TryRemove(firstBlockId, out var blk);

            var blocksToRemove = Blocks.Values.Where(b => b.RepetitiveSourceBlockId == firstBlockId);
            if (blocksToRemove != null)
            {
                foreach (var block in blocksToRemove)
                {
                    if (Blocks.ContainsKey(block.Id))
                        Blocks.TryRemove(block.Id, out var blk);
                }
            }

            LastChange = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Change block parameters to the consumer. 
        /// </summary>
        /// <param name="id">Id of the consumer</param>
        /// <param name="type">type of Block</param>
        /// <returns></returns>
        public (bool, string) ChangeBlockParameters(string blockId,
                                                    string name = null,
                                                    string description = null,
                                                    BlockType? type = null,
                                                    double? amount = null,
                                                    BlockDirection? direction = null,
                                                    DateTime? startTime = null,
                                                    TimeSpan? timeframe = null)


        {
            if (string.IsNullOrEmpty(blockId))
                return (false, "Cannot change blocks. Block id cannot be empty.");

            if (Blocks.TryGetValue(blockId, out var block))
            {
                if (name != null)
                    block.Name = name;
                if (description != null)
                    block.Description = description;
                if (type != null)
                    block.Type = (BlockType)type;
                if (amount != null)
                    block.Amount = (double)amount;
                if (direction != null)
                    block.Direction = (BlockDirection)direction;
                if (startTime != null)
                    block.StartTime = (DateTime)startTime;
                if (timeframe != null)
                    block.Timeframe = (TimeSpan)timeframe;
                LastChange = DateTime.UtcNow;
                return (true, $"Block {block.Name} - {blockId} in {Name} - {Id} paramters changed.");
            }
            else
                return (true, $"Cannot add blocks to the consumer {Name} - {Id}.");

        }

        /// <summary>
        /// Change Blocks direction all blocks in entity
        /// </summary>
        /// <param name="direction"></param>
        public virtual void ChangeAllBlocksDirection(BlockDirection direction, BlockDirection originalDirection = BlockDirection.Mix)
        {
            foreach(var block in Blocks.Values)
                if (block.Direction == originalDirection || originalDirection == BlockDirection.Mix)
                    block.Direction = direction;
        }

        /// <summary>
        /// Change Blocks direction specified blocks in entity
        /// </summary>
        /// <param name="direction"></param>
        public virtual void ChangeAllBlocksDirection(BlockDirection direction, List<string> ids, BlockDirection originalDirection = BlockDirection.Mix)
        {
            foreach (var id in ids)
                if (Blocks.TryGetValue(id, out var block))
                    if (block.Direction == originalDirection || originalDirection == BlockDirection.Mix)
                        block.Direction = direction;
        }

        /// <summary>
        /// Change Blocks type all blocks in entity
        /// </summary>
        /// <param name="type"></param>
        public virtual void ChangeAllBlocksType(BlockType type, BlockType originalType = BlockType.Simulated)
        {
            foreach (var block in Blocks.Values)
                if (block.Type == originalType || originalType == BlockType.Mix)
                    block.Type = type;
        }
        /// <summary>
        /// Change Blocks type specified blocks in entity
        /// </summary>
        /// <param name="type"></param>
        public virtual void ChangeAllBlocksType(BlockType type, List<string> ids, BlockType originalType = BlockType.Simulated)
        {
            foreach (var id in ids)
                if (Blocks.TryGetValue(id, out var block))
                    if (block.Type == originalType || originalType == BlockType.Mix)
                        block.Type = type;
        }

        /// <summary>
        /// Get blocks from simulators for specific timerage and timeframe
        /// </summary>
        /// <param name="timeframe"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<IBlock> GetSimulatorsBlocks(BlockTimeframe timeframe, 
                                                DateTime start,
                                                DateTime end)
        {
            var res = new List<IBlock>();

            foreach (var simulator in Simulators.Values)
            {                
                var blocks = simulator.GetBlocks(timeframe, start, end, null, null, new Dictionary<string, object>()
                {
                    {"weatherFactor", 1.0 }, // weatherFactor is not used in Tddsimulator, but it does not cause the trouble there, thats why it can be shared function like this
                    {"parentId", Id } // parentId is common for creating blocks in multiple simulators (for example PVE and TDD Consumption)
                });

                if (blocks != null)
                    foreach (var b in blocks)
                        res.Add(b);
            }

            return res;
        }

        /// <summary>
        /// Function will take new empty result blocks and combine them with simulator if simulator is required
        /// </summary>
        /// <param name="timeframe"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="takeConsumptionAsInvert"></param>
        /// <param name="addSimulators"></param>
        /// <returns></returns>
        private List<IBlock> GetResultBlocks(BlockTimeframe timeframe, DateTime start, DateTime end, bool takeConsumptionAsInvert, bool addSimulators)
        {
            var result = BlockHelpers.GetResultBlocks(timeframe, start, end, ParentId);

            if (addSimulators)
            {
                var simulated = GetSimulatorsBlocks(timeframe, start, end);

                if (simulated != null && simulated.Count > 0)
                {
                    foreach (var block in simulated)
                    {
                        var res = result.FirstOrDefault(b => b.StartTime == block.StartTime);
                        if (res != null)
                        {
                            if (takeConsumptionAsInvert)
                            {
                                if (block.Direction == BlockDirection.Consumed)
                                    res.Amount -= block.Amount;
                                else if (block.Direction == BlockDirection.Created)
                                    res.Amount += block.Amount;
                                else if (block.Direction == BlockDirection.Stored)
                                    res.Amount += block.Amount;
                            }
                            else
                            {
                                res.Amount += block.Amount;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get blocks filtered based on Directions or Types
        /// </summary>
        /// <param name="justThisDirections">List of all allowed Direction of blocks</param>
        /// <param name="justThisType">List of all Types of blocks</param>
        /// <returns></returns>
        public virtual IEnumerable<IBlock> GetBlocks(List<BlockDirection> justThisDirections = null,
                                              List<BlockType> justThisType = null)
        {
            if (justThisDirections != null && justThisDirections.Count > 0 && justThisType == null)
                return Blocks.Values.Where(b => justThisDirections.Contains(b.Direction)).OrderBy(b => b.StartTime);
            else if (justThisType != null && justThisType.Count > 0 && justThisDirections == null)
                return Blocks.Values.Where(b => justThisType.Contains(b.Type)).OrderBy(b => b.StartTime);
            else if (justThisType != null && justThisType.Count > 0 && justThisDirections != null && justThisDirections.Count > 0)
                return Blocks.Values.Where(b => justThisType.Contains(b.Type) && justThisDirections.Contains(b.Direction)).OrderBy(b => b.StartTime);

            return Blocks.Values.OrderBy(b => b.StartTime);
        }

        /// <summary>
        /// Get list of the blocks based on setup timespan and step and specific timegrame
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <param name="takeConsumptionAsInvert">if this is set it will multiply "consumed" blocks with -1. Means consumption is negative in calculation</param>
        /// <returns></returns>
        public virtual List<IBlock> GetSummedValues(BlockTimeframe timeframesteps,
                                                        DateTime starttime,
                                                        DateTime endtime,
                                                        bool takeConsumptionAsInvert = false,
                                                        List<BlockDirection> justThisDirections = null,
                                                        List<BlockType> justThisType = null,
                                                        bool addSimulators = true)
        {
            var result = GetResultBlocks(timeframesteps, starttime, endtime, takeConsumptionAsInvert, addSimulators);

            var invert = 1;
            
            var blocks = GetBlocks(justThisDirections, justThisType).Where(b => b.StartTime < endtime)
                                                                    .OrderBy(b => b.StartTime).ToList();

            var counter = 0;
            foreach (var block in blocks)
            {
                if (takeConsumptionAsInvert && block.Direction == BlockDirection.Consumed)
                    invert = -1;
                else
                    invert = 1;
                counter++;
                var results = result.Where(r => r.EndTime >= block.StartTime && r.StartTime <= block.EndTime);

                foreach (var res in results)
                {
                    // source block      |    |-----------|
                    // result block list |  |__---|-----|-____|
                    //                   -----------------------> t
                    if (block.StartTime <= res.StartTime && block.EndTime > res.StartTime && block.EndTime > res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * res.Timeframe.TotalSeconds);
                    // source block      |    |--------|
                    // result block list |  |__---|----_|_____|
                    //                   -----------------------> t
                    else if (block.StartTime <= res.StartTime && block.EndTime > res.StartTime && block.EndTime <= res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (res.EndTime - block.EndTime).TotalSeconds));
                    // source block      |          |-|
                    // result block list |  |_____|__-__|_____|
                    //                   -----------------------> t
                    else if (block.StartTime >= res.StartTime && block.EndTime > res.StartTime && block.EndTime <= res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (block.StartTime - res.StartTime).TotalSeconds - (res.EndTime - block.EndTime).TotalSeconds));
                    // source block      |           |----|
                    // result block list |  |_____|___--|-____|
                    //                   -----------------------> t
                    else if (block.StartTime > res.StartTime && block.StartTime <= res.EndTime && block.EndTime > res.StartTime && block.EndTime > res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (block.StartTime - res.StartTime).TotalSeconds));
                }
            }
            return result;
        }

        /// <summary>
        /// Optimized Get list of the blocks based on setup timespan and step and specific timegrame
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// it calculates the repetitive blocks separately
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <param name="takeConsumptionAsInvert">if this is set it will multiply "consumed" blocks with -1. Means consumption is negative in calculation</param>
        /// <returns></returns>
        public virtual List<IBlock> GetSummedValuesOptimized(BlockTimeframe timeframesteps,
                                                                 DateTime starttime,
                                                                 DateTime endtime,
                                                                 bool takeConsumptionAsInvert = false,
                                                                 List<BlockDirection> justThisDirections = null,
                                                                 List<BlockType> justThisType = null,
                                                                 bool addSimulators = true)
        {
            var result = GetResultBlocks(timeframesteps, starttime, endtime, takeConsumptionAsInvert, addSimulators);

            var invert = 1;
            var blocks = GetBlocks(justThisDirections, justThisType).Where(b => !b.IsRepetitiveSource && 
                                                                                !b.IsRepetitiveChild && 
                                                                                !b.IsOffPeriodRepetitive && 
                                                                                b.StartTime < endtime)
                                                                    .OrderBy(b => b.StartTime).ToList();
                        
            var repblocksresult = GetSummedValuesOfRepetitiveBlocks(timeframesteps, starttime, endtime, takeConsumptionAsInvert, justThisDirections, justThisType);

            var counter = 0;
            foreach (var block in blocks)
            {
                if (takeConsumptionAsInvert && block.Direction == BlockDirection.Consumed)
                    invert = -1;
                else
                    invert = 1;
                counter++;
                var results = result.Where(r => r.EndTime >= block.StartTime && r.StartTime <= block.EndTime);

                foreach (var res in results)
                {
                    // source block      |    |-----------|
                    // result block list |  |__---|-----|-____|
                    //                   -----------------------> t
                    if (block.StartTime <= res.StartTime && block.EndTime > res.StartTime && block.EndTime > res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * res.Timeframe.TotalSeconds);
                    // source block      |    |--------|
                    // result block list |  |__---|----_|_____|
                    //                   -----------------------> t
                    else if (block.StartTime <= res.StartTime && block.EndTime > res.StartTime && block.EndTime <= res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (res.EndTime - block.EndTime).TotalSeconds));
                    // source block      |          |-|
                    // result block list |  |_____|__-__|_____|
                    //                   -----------------------> t
                    else if (block.StartTime >= res.StartTime && block.EndTime > res.StartTime && block.EndTime <= res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (block.StartTime - res.StartTime).TotalSeconds - (res.EndTime - block.EndTime).TotalSeconds));
                    // source block      |           |----|
                    // result block list |  |_____|___--|-____|
                    //                   -----------------------> t
                    else if (block.StartTime > res.StartTime && block.StartTime <= res.EndTime && block.EndTime > res.StartTime && block.EndTime > res.EndTime)
                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (block.StartTime - res.StartTime).TotalSeconds));
                }
            }


            foreach (var b in repblocksresult.Where(rp => rp.Amount > 0 || rp.Amount < 0))
            {
                var res = result.First(r => r.StartTime == b.StartTime);
                if (res != null)
                    res.Amount += b.Amount;
            }

            return result;
        }

        /// <summary>
        /// Get list of the repetitive blocks based on setup timespan and step and specific timegrame
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <param name="takeConsumptionAsInvert">if this is set it will multiply "consumed" blocks with -1. Means consumption is negative in calculation</param>
        /// <returns></returns>
        public virtual List<IBlock> GetSummedValuesOfRepetitiveBlocks(BlockTimeframe timeframesteps,
                                                                          DateTime starttime,
                                                                          DateTime endtime,
                                                                          bool takeConsumptionAsInvert = false,
                                                                          List<BlockDirection> justThisDirections = null,
                                                                          List<BlockType> justThisType = null,
                                                                          bool addSimulators = true)
        {
            var result = GetResultBlocks(timeframesteps, starttime, endtime, takeConsumptionAsInvert, addSimulators);

            var invert = 1;

            var repetitiveBlocksSources = GetBlocks(justThisDirections, justThisType).Where(b => !b.IsRepetitiveChild && 
                                                                                                  b.IsRepetitiveSource && 
                                                                                                  b.StartTime < endtime)
                                                                                     .OrderBy(b => b.StartTime).ToList();

            foreach (var block in repetitiveBlocksSources)
            {
                if (takeConsumptionAsInvert && block.Direction == BlockDirection.Consumed)
                    invert = -1;
                else
                    invert = 1;

                var numberOfRepetitions = 0;
                if (block.IsOffPeriodRepetitive)
                {
                    if (block.RepetitiveEndRun != null && block.RepetitiveFirstRun != null && block.OffPeriod != null)
                    {
                        var start = block.RepetitiveFirstRun.Value;
                        var end = block.RepetitiveEndRun.Value;
                        while (start < end)
                        {
                            numberOfRepetitions++;
                            var repblockend = start.AddSeconds(block.Timeframe.TotalSeconds);
                            var results = result.Where(r => r.EndTime >= start && r.StartTime <= repblockend);
                            foreach (var res in results)
                            {
                                // source block      |    |-----------|
                                // result block list |  |__---|-----|-____|
                                //                   -----------------------> t
                                if (start <= res.StartTime && repblockend > res.StartTime && repblockend > res.EndTime)
                                    res.Amount += invert * (block.AvgConsumptionPerSecond * res.Timeframe.TotalSeconds);
                                // source block      |    |--------|
                                // result block list |  |__---|----_|_____|
                                //                   -----------------------> t
                                else if (start <= res.StartTime && repblockend > res.StartTime && repblockend <= res.EndTime)
                                    res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (res.EndTime - repblockend).TotalSeconds));
                                // source block      |          |-|
                                // result block list |  |_____|__-__|_____|
                                //                   -----------------------> t
                                else if (start >= res.StartTime && repblockend > res.StartTime && repblockend <= res.EndTime)
                                    res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (start - res.StartTime).TotalSeconds - (res.EndTime - repblockend).TotalSeconds));
                                // source block      |           |----|
                                // result block list |  |_____|___--|-____|
                                //                   -----------------------> t
                                else if (start > res.StartTime && start <= res.EndTime && repblockend > res.StartTime && repblockend > res.EndTime)
                                    res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (start - res.StartTime).TotalSeconds));
                            }

                            start = start.AddSeconds(block.Timeframe.TotalSeconds + block.OffPeriod.Value.TotalSeconds);
                        }
                    }
                }
                else if (block.IsInDayOnly)
                {
                    if (block.RepetitiveEndRun != null && block.RepetitiveFirstRun != null)
                    {
                        var dayperweek = 5;
                        if (block.JustInWeek)
                            dayperweek = 5;
                        else if (block.JustInWeekends)
                            dayperweek = 2;

                        var start = new DateTime(block.RepetitiveFirstRun.Value.Year,
                                                 block.RepetitiveFirstRun.Value.Month,
                                                 block.RepetitiveFirstRun.Value.Day, block.StartTime.Hour, block.StartTime.Minute, block.StartTime.Second);
                        //var end = new DateTime(block.RepetitiveEndRun.Value.Year,
                        //                       block.RepetitiveEndRun.Value.Month,
                        //                       block.RepetitiveEndRun.Value.Day, block.EndTime.Hour, block.EndTime.Minute, block.EndTime.Second); ;
                        while (start < block.RepetitiveEndRun.Value)
                        {
                            if ((!block.JustInWeek && !block.JustInWeekends) || (block.JustInWeek && start.DayOfWeek >= DayOfWeek.Monday && start.DayOfWeek <= DayOfWeek.Friday) ||
                                         (block.JustInWeekends && (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)))
                            {
                                numberOfRepetitions++;
                                var repblockend = start.AddHours(block.Timeframe.TotalHours);
                                var results = result.Where(r => r.EndTime >= start && r.StartTime <= repblockend);
                                foreach (var res in results)
                                {
                                    // source block      |    |-----------|
                                    // result block list |  |__---|-----|-____|
                                    //                   -----------------------> t
                                    if (start <= res.StartTime && repblockend > res.StartTime && repblockend > res.EndTime)
                                        res.Amount += invert * (block.AvgConsumptionPerSecond * res.Timeframe.TotalSeconds);
                                    // source block      |    |--------|
                                    // result block list |  |__---|----_|_____|
                                    //                   -----------------------> t
                                    else if (start <= res.StartTime && repblockend > res.StartTime && repblockend <= res.EndTime)
                                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (res.EndTime - repblockend).TotalSeconds));
                                    // source block      |          |-|
                                    // result block list |  |_____|__-__|_____|
                                    //                   -----------------------> t
                                    else if (start >= res.StartTime && repblockend > res.StartTime && repblockend <= res.EndTime)
                                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (start - res.StartTime).TotalSeconds - (res.EndTime - repblockend).TotalSeconds));
                                    // source block      |           |----|
                                    // result block list |  |_____|___--|-____|
                                    //                   -----------------------> t
                                    else if (start > res.StartTime && start <= res.EndTime && repblockend > res.StartTime && repblockend > res.EndTime)
                                        res.Amount += invert * (block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (start - res.StartTime).TotalSeconds));
                                }
                            }
                            start = start.AddDays(1);
                        }
                    }
                }


            }
            return result;
        }

        /// <summary>
        /// Get list of the blocks based on setup timespan and step and specific timegrame and window
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// Window will cut the Amounts which are outside of specific time window. 
        /// For example window can be 8:00-20:00. Only consumption in this window is added to final consumption.
        /// If you set invertwindow to true with 8:00-20:00 times, it will take consumption just between 0:00-8:00 and 20:00-24:00 and add it to final consumption.
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <param name="takeConsumptionAsInvert">if this is set it will multiply "consumed" blocks with -1. Means consumption is negative in calculation</param>
        /// <param name="blockwindowstarttime">Start of time window. Takes just Hours parameter!</param>
        /// <param name="blockwindowendtime">End of time window. Takes just Hours parameter!</param>
        /// <returns></returns>
        public virtual List<IBlock> GetSummedValuesWithHourWindow(BlockTimeframe timeframesteps,
                                                                      DateTime starttime,
                                                                      DateTime endtime,
                                                                      DateTime blockwindowstarttime,
                                                                      DateTime blockwindowendtime,
                                                                      bool invertWindow = false,
                                                                      bool takeConsumptionAsInvert = false,
                                                                      List<BlockDirection> justThisDirections = null,
                                                                      List<BlockType> justThisType = null,
                                                                      bool addSimulators = true)
        {
            var result = GetResultBlocks(timeframesteps, starttime, endtime, takeConsumptionAsInvert, addSimulators);

            var input = GetSummedValuesOptimized(BlockTimeframe.Hour, starttime, endtime, takeConsumptionAsInvert, justThisDirections, justThisType);
            if (input == null)
                return new List<IBlock>();
            if (input.Count == 0)
                return new List<IBlock>();

            List<IBlock> blocksToCount = new List<IBlock>();

            foreach (var block in input)
            {
                if (!invertWindow)
                {
                    // Window |____-----------_____
                    // Source |--------------------
                    // Result |____-----------_____
                    //         ---------------------> t
                    if (block.StartTime.Hour >= blockwindowstarttime.Hour && block.StartTime.Hour <= blockwindowendtime.Hour && block.EndTime.Hour <= blockwindowendtime.Hour)
                        blocksToCount.Add(block);
                }
                else
                {
                    // Window |____-----------_____
                    // Source |--------------------
                    // Result |----___________-----
                    //         ---------------------> t
                    if (block.StartTime.Hour < blockwindowstarttime.Hour || block.StartTime.Hour >= blockwindowendtime.Hour)
                        blocksToCount.Add(block);
                }
            }

            foreach (var block in blocksToCount)
            {
                var results = result.Where(r => r.EndTime >= block.StartTime && r.StartTime <= block.EndTime);
                foreach (var res in results)
                {
                    // source block      |    |-----------|
                    // result block list |  |__---|-----|-____|
                    //                   -----------------------> t
                    if (block.StartTime <= res.StartTime && block.EndTime > res.StartTime && block.EndTime > res.EndTime)
                        res.Amount += block.AvgConsumptionPerSecond * res.Timeframe.TotalSeconds;
                    // source block      |    |--------|
                    // result block list |  |__---|----_|_____|
                    //                   -----------------------> t
                    else if (block.StartTime <= res.StartTime && block.EndTime > res.StartTime && block.EndTime <= res.EndTime)
                        res.Amount += block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (res.EndTime - block.EndTime).TotalSeconds);
                    // source block      |          |-|
                    // result block list |  |_____|__-__|_____|
                    //                   -----------------------> t
                    else if (block.StartTime >= res.StartTime && block.EndTime > res.StartTime && block.EndTime <= res.EndTime)
                        res.Amount += block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (block.StartTime - res.StartTime).TotalSeconds - (res.EndTime - block.EndTime).TotalSeconds);
                    // source block      |           |----|
                    // result block list |  |_____|___--|-____|
                    //                   -----------------------> t
                    else if (block.StartTime > res.StartTime && block.StartTime <= res.EndTime && block.EndTime > res.StartTime && block.EndTime > res.EndTime)
                        res.Amount += block.AvgConsumptionPerSecond * (res.Timeframe.TotalSeconds - (block.StartTime - res.StartTime).TotalSeconds);
                }
            }
            return result;
        }

        /// <summary>
        /// Get total consumption over all blocks
        /// </summary>
        /// <returns></returns>
        public virtual double GetTotalSummedValue(bool includeNotConsumedYet = true)
        {
            double totalconsumption = 0.0;
            foreach (var block in BlocksOrderByTime)
                totalconsumption += block.Amount;
            return totalconsumption;
        }
    }
}
