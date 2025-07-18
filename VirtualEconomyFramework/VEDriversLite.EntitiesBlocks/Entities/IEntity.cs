﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;

namespace VEDriversLite.EntitiesBlocks.Entities
{
    public enum EntityType
    {
        None,
        Source,
        Consumer,
        Both
    }

    public interface IEntity
    {
        /// <summary>
        /// Entity type
        /// </summary>
        EntityType Type { get; set; }
        string Id { get; set; }
        /// <summary>
        /// Name of the entity
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Entity description
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Entity location
        /// </summary>
        Coordinates Coords { get; set; }
        /// <summary>
        /// Parent Id of the entity
        /// </summary>
        string ParentId { get; set; }
        /// <summary>
        /// List of child Ids which are under this entity in hierarchy
        /// </summary>
        List<string> Children { get; set; }
        /// <summary>
        /// If the childs list contains some items it means the object is parent in the structure
        /// </summary>
        bool IsParent { get; }
        ConcurrentDictionary<string, IBlock> Blocks { get; set; }
        /// <summary>
        /// Return blocks ordered by time
        /// </summary>
        List<IBlock> BlocksOrderByTime { get; }
        /// <summary>
        /// Last change of the parameters of the entity
        /// </summary>
        DateTime LastChange { get; set; }
        /// <summary>
        /// Dictionary of simulators
        /// </summary>
        ConcurrentDictionary<string, ISimulator> Simulators { get; set; }
        /// <summary>
        /// Try to add the block to the Blocks dictionary. Block must have unique hashs
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        bool AddBlock(IBlock block);
        /// <summary>
        /// Try to add the list of the blocks to the Blocks dictionary. Blocks must have unique hashs
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        bool AddBlocks(ICollection<IBlock> blocks);
        /// <summary>
        /// Remove blocks from the dictionary of blocks.
        /// </summary>
        /// <param name="blocks">List of Ids of blocks</param>
        /// <returns></returns>
        bool RemoveBlocks(ICollection<string> blocks);
        /// <summary>
        /// Remove all blocks in entity
        /// </summary>
        /// <returns></returns>
        bool RemoveAllBlocks();
        /// <summary>
        /// Remove blocks from the dictionary of blocks which are repetitive of some source block.
        /// </summary>
        /// <param name="firstBlockId">Id of first block</param>
        /// <returns></returns>
        bool RemoveRepetitiveBlocksLine(string firstBlockId);
        /// <summary>
        /// Change Blocks direction all blocks in entity
        /// </summary>
        /// <param name="direction"></param>
        void ChangeAllBlocksDirection(BlockDirection direction, BlockDirection originalDirection = BlockDirection.Mix);
        /// <summary>
        /// Change Blocks direction specified blocks in entity
        /// </summary>
        /// <param name="direction"></param>
        void ChangeAllBlocksDirection(BlockDirection direction, ICollection<string> ids, BlockDirection originalDirection = BlockDirection.Mix);
        /// <summary>
        /// Change Blocks type all blocks in entity
        /// </summary>
        /// <param name="type"></param>
        void ChangeAllBlocksType(BlockType type, BlockType originalType = BlockType.Simulated);
        /// <summary>
        /// Change Blocks type specified blocks in entity
        /// </summary>
        /// <param name="type"></param>
        void ChangeAllBlocksType(BlockType type, ICollection<string> ids, BlockType originalType = BlockType.Simulated);
        /// <summary>
        /// Change block parameters to the consumer. 
        /// </summary>
        /// <param name="id">Id of the consumer</param>
        /// <param name="type">type of Block</param>
        /// <returns></returns>
        (bool, string) ChangeBlockParameters(string blockId,
                                             string name = null,
                                             string description = null,
                                             BlockType? type = null,
                                             double? amount = null,
                                             BlockDirection? direction = null,
                                             DateTime? startTime = null,
                                             TimeSpan? timeframe = null);

        /// <summary>
        /// Get blocks from simulators for specific timerage and timeframe
        /// </summary>
        /// <param name="timeframe"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        IEnumerable<IBlock> GetSimulatorsBlocks(BlockTimeframe timeframe,
                                                DateTime start,
                                                DateTime end);

        /// <summary>
        /// Get simulator by its Id
        /// </summary>
        /// <param name="id">Simulator Id</param>
        /// <returns></returns>
        ISimulator? GetSimulator(string id);

        /// <summary>
        /// Get simulator by its name
        /// </summary>
        /// <param name="name">Simulator name</param>
        /// <returns></returns>
        ISimulator? GetSimulatorByName(string name);

        /// <summary>
        /// Get blocks filtered based on Directions or Types. This function use Blocks list in entity
        /// </summary>
        /// <param name="justThisDirections">List of all allowed Direction of blocks</param>
        /// <param name="justThisType">List of all Types of blocks</param>
        /// <returns></returns>
        IEnumerable<IBlock> GetBlocks(ICollection<BlockDirection> justThisDirections = null,
                                      ICollection<BlockType> justThisType = null);

        /// <summary>
        /// Get blocks filtered based on Directions or Types. This function needs input blocks list
        /// </summary>
        /// <param name="blocks">Input blocks</param>
        /// <param name="justThisDirections">List of all allowed Direction of blocks</param>
        /// <param name="justThisType">List of all Types of blocks</param>
        /// <returns></returns>
        IEnumerable<IBlock> GetBlocks(ICollection<IBlock> blocks, ICollection<BlockDirection> justThisDirections = null,
                                      ICollection<BlockType> justThisType = null);
        /// <summary>
        /// Get blocks filtered based on Directions or Types. This function needs input blocks list
        /// </summary>
        /// <param name="blocks">Input blocks as enumerable</param>
        /// <param name="justThisDirections">List of all allowed Direction of blocks</param>
        /// <param name="justThisType">List of all Types of blocks</param>
        /// <returns></returns>
        IEnumerable<IBlock> GetBlocks(IEnumerable<IBlock> blocks, ICollection<BlockDirection> justThisDirections = null,
                                      ICollection<BlockType> justThisType = null);

        /// <summary>
        /// Get summed values as list of the blocks based on setup timespan and step.
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <returns></returns>
        List<IBlock> GetSummedValues(BlockTimeframe timeframesteps,
                                    DateTime starttime,
                                    DateTime endtime,
                                    bool takeConsumptionAsInvert = false,
                                    ICollection<BlockDirection> justThisDirections = null,
                                    ICollection<BlockType> justThisType = null,
                                    bool addSimulators = true);

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
        List<IBlock> GetSummedValuesOptimized(BlockTimeframe timeframesteps,
                                             DateTime starttime,
                                             DateTime endtime,
                                             bool takeConsumptionAsInvert = false,
                                             ICollection<BlockDirection> justThisDirections = null,
                                             ICollection<BlockType> justThisType = null,
                                             bool addSimulators = true,
                                             bool withRepetitive = false);

        /// <summary>
        /// Get list of the repetitive blocks based on setup timespan and step and specific timegrame
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <param name="takeConsumptionAsInvert">if this is set it will multiply "consumed" blocks with -1. Means consumption is negative in calculation</param>
        /// <returns></returns>
        List<IRepetitiveBlock> GetSummedValuesOfRepetitiveBlocks(BlockTimeframe timeframesteps,
                                                      DateTime starttime,
                                                      DateTime endtime,
                                                      bool takeConsumptionAsInvert = false,
                                                      ICollection<BlockDirection> justThisDirections = null,
                                                      ICollection<BlockType> justThisType = null, 
                                                      bool addSimulators = true);

        /// <summary>
        /// Get list of the blocks based on setup timespan and step and specific timegrame and window
        /// Timeframe will cause recalculation of the blocks of entity to timeframe like "hours", "days", "months" etc.
        /// Window will cut the Amounts which are outside of specific time window. 
        /// For example window can be 8:00-20:00. Only value in this window is added to final value.
        /// If you set invertwindow to true with 8:00-20:00 times, it will take consumption just between 0:00-8:00 and 20:00-24:00 and add it to final consumption.
        /// </summary>
        /// <param name="timeframesteps">timeframe step (for example "month")</param>
        /// <param name="starttime">start datetime (1.1.2022)</param>
        /// <param name="endtime">end date</param>
        /// <param name="takeConsumptionAsInvert">if this is set it will multiply "consumed" blocks with -1. Means consumption is negative in calculation</param>
        /// <param name="blockwindowstarttime">Start of time window. Takes just Hours parameter!</param>
        /// <param name="blockwindowendtime">End of time window. Takes just Hours parameter!</param>
        /// <returns></returns>
        List<IBlock> GetSummedValuesWithHourWindow(BlockTimeframe timeframesteps,
                                                  DateTime starttime,
                                                  DateTime endtime,
                                                  DateTime blockwindowstarttime,
                                                  DateTime blockwindowendtime,
                                                  bool invertWindow = false,
                                                  bool takeConsumptionAsInvert = false,
                                                  ICollection<BlockDirection> justThisDirections = null,
                                                  ICollection<BlockType> justThisType = null,
                                                  bool addSimulators = true);
        /// <summary>
        /// Get total consumption over all blocks
        /// </summary>
        /// <returns></returns>
        double GetTotalSummedValue(bool includeNotConsumedYet = true);
        /// <summary>
        /// Add simulator to entity
        /// </summary>
        /// <param name="simulator"></param>
        /// <returns></returns>
        (bool,string) AddSimulator(ISimulator simulator);
        /// <summary>
        /// Remove simulator
        /// </summary>
        /// <param name="simulatorIds"></param>
        /// <returns></returns>
        (bool,string) RemoveSimulator(ICollection<string> simulatorIds);
    }
}
