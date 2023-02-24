using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Entities
{
    public enum SimulatorTypes
    {
        None,
        PVE,
        Battery,
        TDDConsumption,
        Device,
        Custom,
    }
    public interface ISimulator
    {
        /// <summary>
        /// Id of simulator
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Parent Id of this simulator
        /// </summary>
        string ParentId { get; set; }
        /// <summary>
        /// Loaded config for simulator
        /// </summary>
        string LoadedConfig { get; set; }
        /// <summary>
        /// Type of simulator
        /// </summary>
        SimulatorTypes Type { get; set; }
        DataProfile GetData(BlockTimeframe timeframe, DateTime start, DateTime end, Dictionary<string,DataProfile> inputProfiles, Dictionary<string, List<IBlock>> inputBlocks, Dictionary<string,object> options);
        IEnumerable<IBlock> GetBlocks(BlockTimeframe timeframe, DateTime start, DateTime end, Dictionary<string,DataProfile> inputProfiles, Dictionary<string, List<IBlock>> inputBlocks, Dictionary<string,object> options);
        (bool, string) ImportConfig(string config);
        (bool, string) ExportConfig();
    }
}
