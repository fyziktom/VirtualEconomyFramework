using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Tree;

namespace VEDriversLite.EntitiesBlocks.Handlers
{
    public interface IEntitiesHandler
    {
        /// <summary>
        /// dictionary of all sources in the network, where the key is the unieque Id of the source
        /// </summary>
        IEnumerable<IEntity> Sources { get; }
        /// <summary>
        /// dictionary of all consumers in the network, where the key is the uniue Id of the source
        /// </summary>
        IEnumerable<IEntity> Consumers { get; }
        /// <summary>
        /// dictionary of all entities in the network, where the key is the uniue Id of the entity
        /// </summary>
        ConcurrentDictionary<string, IEntity> Entities { get; set; }

        /// <summary>
        /// Alocation schemes for split the amount of some block (for example automatic split shared PVE source between flats).
        /// </summary>
        ConcurrentDictionary<string, AlocationScheme> AlocationSchemes { get; set; }

        /// <summary>
        /// Label of the unit of the Amount. For example "kWh" for energy application
        /// </summary>
        string UnitLabel { get; set; }

        /// <summary>
        /// Create new Entity and add it to the entities list
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sourceName"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        (bool, (string, string)) AddEntity(IEntity entity, string sourceName, string parentId, string id = null, List<IBlock> blocks = null);

        /// <summary>
        /// remove the specific Entity
        /// </summary>
        /// <param name="id">consumer id</param>
        /// <returns></returns>
        (bool, string) RemoveEntity(string id);
        /// <summary>
        /// Add block to the entity. 
        /// </summary>
        /// <param name="id">Id of the consumer</param>
        /// <param name="block">Block</param>
        /// <returns></returns>
        (bool, string) AddBlockToEntity(string id, IBlock block);
        /// <summary>
        /// Add blocks to the entity. 
        /// </summary>
        /// <param name="id">Id of the consumer</param>
        /// <param name="blocks">list of the Blocks</param>
        /// <returns></returns>
        (bool, string) AddBlocksToEntity(string id, List<IBlock> blocks);
        /// <summary>
        /// Add simulator to specific entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="simulator"></param>
        /// <returns></returns>
        (bool, string) AddSimulatorToEntity(string id, ISimulator simulator);
        /// <summary>
        /// Remove simulators from entity. You can add multiple Ids in one command
        /// </summary>
        /// <param name="simulatorIds"></param>
        /// <returns></returns>
        (bool, string) RemoveSimulatorsFromEntity(string id, List<string> simulatorIds);
        /// <summary>
        /// Change block parameters to the consumer. 
        /// </summary>
        /// <param name="id">Id of the consumer</param>
        /// <param name="type">type of Block</param>
        /// <returns></returns>
        (bool, string) ChangEntityBlockParameters(string id,
                                                  string blockId,
                                                  string name = null,
                                                  string description = null,
                                                  BlockType? type = null,
                                                  double? amount = null,
                                                  BlockDirection? direction = null,
                                                  DateTime? startTime = null,
                                                  TimeSpan? timeframe = null);
        /// <summary>
        /// Remove blocks from the entity. 
        /// </summary>
        /// <param name="id">Id of the consumer</param>
        /// <param name="blocks">list of the Ids of the Blocks</param>
        /// <returns></returns>
        (bool, string) RemoveBlocksFromEntity(string id, List<string> blocks);
        /// <summary>
        /// Add symbolic connection between some two subsource in the network. 
        /// each entity has list of childerns, means relation to other entities
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="subentityId"></param>
        /// <returns></returns>
        (bool, string) AddSubEntityToEntity(string entityId, string subentityId);
        /// <summary>
        /// Remove symbolic connection between some two entities in the network. 
        /// each entity has list of subentities, means relation to other entities
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="subentityId"></param>
        /// <returns></returns>
        (bool, string) RemoveSubEntityFromEntity(string entityId, string subentityId);
        /// <summary>
        /// Change Blocks direction all blocks in entity
        /// </summary>
        /// <param name="id"></param>
        void ChangeAllEntityBlocksDirection(string id, BlockDirection direction, BlockDirection originalDirection = BlockDirection.Mix);
        /// <summary>
        /// Change Blocks direction all blocks in entity
        /// </summary>
        /// <param name="id"></param>
        void ChangeAllEntityBlocksDirection(string id, BlockDirection direction, List<string> ids, BlockDirection originalDirection = BlockDirection.Mix);
        /// <summary>
        /// Change Blocks type all blocks in entity
        /// </summary>
        /// <param name="type"></param>
        void ChangeAllBlocksType(string id, BlockType type, BlockType originalType = BlockType.Simulated);
        /// <summary>
        /// Change Blocks type specified blocks in entity
        /// </summary>
        /// <param name="type"></param>
        void ChangeAllBlocksType(string id, BlockType type, List<string> ids, BlockType originalType = BlockType.Simulated);

        /// <summary>
        /// Get recalculated power consumption represented as list of Blocks split based on setted timegrame
        /// </summary>
        /// <param name="entityId">consumer Id</param>
        /// <param name="withSubConsumers">when this is set, the function will load the same consumption of all subconsumers of this consumer. Then it is summed together as one output profile of consumpion.</param>
        /// <param name="timeframesteps">enum of step of timeframe to recalculate the consumption into blocks, for example based on minutes</param>
        /// <param name="starttime">start datetime of the recalculation frame</param>
        /// <param name="endtime">end datetime of the recalculation frame</param>
        /// <returns></returns>
        List<IBlock> GetConsumptionOfEntity(string entityId,
                                            BlockTimeframe timeframesteps,
                                            DateTime starttime,
                                            DateTime endtime,
                                            bool withSubConsumers = true,
                                            bool takeConsumptionAsInvert = false,
                                            List<BlockDirection> justThisDirections = null, 
                                            List<BlockType> justThisType = null,
                                            bool addSimulators = true);
        /// <summary>
        /// Get all Blocks with the all childern blocks
        /// </summary>
        /// <param name="entityId">consumer Id</param>
        /// <returns></returns>
        List<IBlock> GetBlocksOfEntityWithChildernBlocks(string entityId);

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
        List<IBlock> GetConsumptionOfEntityWithWindow(string entityId,
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
                                                      bool addSimulators = true);
        /// <summary>
        /// Get recalculated power consumption represented as list of Blocks split based on setted timegrame
        /// </summary>
        /// <param name="consumerId">consumer Id</param>
        /// <param name="withChilds">when this is set, the function will load the same consumption of all subconsumers of this consumer. Then it is summed together as one output profile of consumpion.</param>
        /// <param name="timeframesteps">enum of step of timeframe to recalculate the consumption into blocks, for example based on minutes</param>
        /// <param name="starttime">start datetime of the recalculation frame</param>
        /// <param name="endtime">end datetime of the recalculation frame</param>
        /// <returns></returns>
        List<IBlock> GetRecalculatedBlocks(List<string> ids, BlockTimeframe timeframesteps, DateTime starttime, DateTime endtime, EntityType eetype = EntityType.Consumer, bool withChilds = true);

        /// <summary>
        /// Get Entity if exists
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IEntity GetEntity(string id, EntityType type);
        /// <summary>
        /// Get all entity blocks
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<IBlock> GetEntityBlocks(string id);
        /// <summary>
        /// Find Entity by Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEntity FindEntityByName(string name);
        /// <summary>
        /// Remove all blocks in entity
        /// </summary>
        /// <param name="id"></param>
        void RemoveAllEntityBlocks(string id);
        /// <summary>
        /// Get Entities Tree
        /// </summary>
        /// <param name="rootId"></param>
        /// <returns></returns>
        TreeItem GetTree(string rootId);
    }
}
