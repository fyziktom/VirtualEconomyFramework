using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Entities
{
    public abstract class CommonSimulator : ISimulator
    {
        /// <summary>
        /// Id of simulator
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Parent Id of this simulator
        /// </summary>
        public string ParentId { get; set; } = string.Empty;
        /// <summary>
        /// Simulator name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Loaded config for simulator
        /// </summary>
        public string LoadedConfig { get; set; } = string.Empty;
        /// <summary>
        /// Type of simulator
        /// </summary>
        public SimulatorTypes Type { get; set; } = SimulatorTypes.None;
        /// <summary>
        /// Import config of simulator
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public abstract (bool, string) ImportConfig(string config);
        /// <summary>
        /// Export simulator configuration as string
        /// </summary>
        /// <returns></returns>
        public abstract (bool, string) ExportConfig();
        public virtual DataProfile GetData(BlockTimeframe timeframe, DateTime start, DateTime end, Dictionary<string, DataProfile> inputProfiles, Dictionary<string, List<IBlock>> inputBlocks, Dictionary<string, object> options)
        {
            var res = GetBlocks(timeframe, start, end, inputProfiles, inputBlocks, options).ToList();
            var result = DataProfileHelpers.ConvertBlocksToDataProfile(res);
            return result;
        }
        public abstract IEnumerable<IBlock> GetBlocks(BlockTimeframe timeframe, DateTime start, DateTime end, Dictionary<string, DataProfile> inputProfiles, Dictionary<string, List<IBlock>> inputBlocks, Dictionary<string, object> options);
        
    }
}
