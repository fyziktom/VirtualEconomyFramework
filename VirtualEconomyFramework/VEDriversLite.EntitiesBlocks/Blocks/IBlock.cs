using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public enum BlockTimeframe
    {
        Second,
        Minute,
        Hour,
        Day,
        Week,
        Month,
        Year,
        QuaterHour,
        QuaterYear,
        FiveMinutes
    }
    public enum BlockType
    {
        Simulated,
        Calculated,
        Real,
        Record,
        Forwarded,
        Over,
        Received,
        Sold,
        Bought,
        Rent,
        NotCovered,
        Mix,
        Initial
    }
    public enum BlockDirection
    {
        Created,
        Stored,
        Consumed,
        Delivered,
        Mix
    }
    public interface IBlock
    {
        /// <summary>
        /// Unique identifier of the block
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Name of the block
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Descrioption of the block
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Id of the source which created this block
        /// </summary>
        string SourceId { get; set; }
        /// <summary>
        /// Parent Id of the block
        /// </summary>
        string ParentId { get; set; }
        /// <summary>
        /// Alocation Scheme Id
        /// </summary>
        string AllocationSchemeId { get; set; }

        /// <summary>
        /// Block type - real block, simulation
        /// </summary>
        BlockType Type { get; set; }
        /// <summary>
        /// Define if the block of has been created or consumed
        /// </summary>
        BlockDirection Direction { get; set; }
        /// <summary>
        /// Start time of this block
        /// </summary>
        DateTime StartTime { get; set; }
        /// <summary>
        /// Time Frame of the block, it means the time unit related to the amount
        /// </summary>
        TimeSpan Timeframe { get; set; }
        /// <summary>
        /// End time of this block, calculated from Starttime and timeframe
        /// </summary>
        DateTime EndTime { get; }

        /// <summary>
        /// Amount, for example of the energy in kWh consumed over whole timeframe
        /// </summary>
        double Amount { get; set; }
        /// <summary>
        /// Flag for the block which has been already used
        /// </summary>
        bool Used { get; set; }

        /// <summary>
        /// Last change of the parameters of the block
        /// </summary>
        DateTime LastChange { get; set; }

        /// <summary>
        /// Average Amount per second, for example energy power consumption per second = kWs
        /// </summary>
        double AvgConsumptionPerSecond { get; }
        /// <summary>
        /// Average Amount per hour, for example energy power consumption per hour = kWh
        /// </summary>
        double AvgConsumptionPerHour { get; }
        /// <summary>
        /// Tarrif applied during the block
        /// For example LowTarrif, HighTarrif, etc.
        /// </summary>
        int Tarrif { get; set; }

        /// <summary>
        /// Get Empty block with predefined parameters such as type or direction
        /// </summary>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <param name="timeframe"></param>
        /// <param name="amount">amount over whole timeframe</param>
        /// <param name="source"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        IBlock GetBlock(BlockType type,
                        BlockDirection direction,
                        DateTime starttime,
                        TimeSpan timeframe,
                        double amount = 0,
                        string? source = null,
                        string? name = null,
                        string? description = null,
                        string? owner = null);

        /// <summary>
        /// Get Empty block with predefined parameters such as type or direction based on powe consumption of device
        /// </summary>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <param name="timeframe"></param>
        /// <param name="poweconsumption">Amount/Power by Hour of device in kW</param>
        /// <param name="source"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        IBlock GetBlockByPower(BlockType type,
                               BlockDirection direction,
                               DateTime starttime,
                               TimeSpan timeframe,
                               double powerconsumption = 0,
                               string? source = null,
                               string? name = null,
                               string? description = null,
                               string? owner = null);

        /// <summary>
        /// Fill object with source IBlock
        /// </summary>
        /// <param name="block"></param>
        void Fill(IBlock block);

        /// <summary>
        /// Set Block Total amount based on power per hour.
        /// The StartTime and Timeframe must be already set
        /// </summary>
        /// <param name="powerperhour"></param>
        void SetAmountByPower(double powerperhour);
    }
}
