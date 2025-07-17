using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public abstract class CommonBlock : IBlock
    {
        /// <summary>
        /// Unique identifier of the block
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Name of the block
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Descrioption of the block
        /// </summary>
        [JsonPropertyName("desc")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Id of the source which created this block
        /// </summary>
        [JsonPropertyName("srcId")]
        public string SourceId { get; set; } = string.Empty;
        /// <summary>
        /// Parent Id of the block
        /// </summary>
        [JsonPropertyName("parId")]
        public string ParentId { get; set; } = string.Empty;
        /// <summary>
        /// Alocation Scheme Id
        /// </summary>
        [JsonPropertyName("allocId")]
        public string AllocationSchemeId { get; set; } = string.Empty;
        /// <summary>
        /// Block type - real block, simulation
        /// </summary>
        [JsonPropertyName("type")]
        public BlockType Type { get; set; } = BlockType.Simulated;
        /// <summary>
        /// Define if the block has been created or consumed
        /// </summary>
        [JsonPropertyName("dir")]
        public BlockDirection Direction { get; set; } = BlockDirection.Created;
        /// <summary>
        /// Start time of this block
        /// </summary>
        [JsonPropertyName("start")]
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private DateTime _startTime = DateTime.MinValue;
        /// <summary>
        /// Time Frame of the block, it means the time unit related to the amount
        /// </summary>
        [JsonPropertyName("frame")]
        public TimeSpan Timeframe
        {
            get => _timeFrame;
            set
            {
                _timeFrame = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private TimeSpan _timeFrame = new TimeSpan();
        /// <summary>
        /// End time of this block, calculated from Starttime and timeframe
        /// </summary>
        [JsonPropertyName("end")]
        public DateTime EndTime { get => StartTime + Timeframe; }

        /// <summary>
        /// Amount, for example energy in kWh consumed over whole timeframe
        /// </summary>
        [JsonPropertyName("amt")]
        public double Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private double _amount = 0.0;
        /// <summary>
        /// Flag for the block which has been already used
        /// </summary>
        [JsonPropertyName("used")]
        public bool Used { get; set; } = false;

        /// <summary>
        /// Last change of the parameters of the block
        /// </summary>
        [JsonPropertyName("changed")]
        public DateTime LastChange { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Average Amount per second, for example energy power consumption per second = kWs
        /// </summary>
        [JsonPropertyName("avgPerS")]
        public double AvgConsumptionPerSecond { get => (EndTime - StartTime).TotalSeconds != 0 ? Amount / (EndTime - StartTime).TotalSeconds : 0; }
        /// <summary>
        /// Average Amount per hour, for example energy power consumption per hour = kWh
        /// </summary>
        [JsonPropertyName("avgPerH")]
        public double AvgConsumptionPerHour { get => (EndTime - StartTime).TotalHours != 0 ? Amount / (EndTime - StartTime).TotalHours : 0; }
        /// <summary>
        /// Tarrif applied during the block
        /// For example LowTarrif, HighTarrif, etc.
        /// </summary>
        [JsonPropertyName("tar")]
        public int Tarrif { get; set; } = 0;

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
        public IBlock GetBlock(BlockType type,
                               BlockDirection direction,
                               DateTime starttime,
                               TimeSpan timeframe,
                               double amount = 0,
                               string? source = null,
                               string? name = null,
                               string? description = null,
                               string? owner = null)
        {
            var e = new BaseBlock();
            e.Id = Guid.NewGuid().ToString();
            e.Type = type;
            e.Direction = direction;
            e.Timeframe = timeframe;
            e.StartTime = starttime;
            if (amount > 0)
                e.Amount = amount;
            if (source != null)
                e.SourceId = source;
            if (name != null)
                e.Name = name;
            if (description != null)
                e.Description = description;
            if (owner != null)
                e.ParentId = owner;

            return e;
        }

        /// <summary>
        /// Get Empty block with predefined parameters such as type or direction based on powe consumption of device
        /// </summary>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <param name="timeframe"></param>
        /// <param name="poweconsumption">Amount/power per hour, for example consumption of energy of device in kW</param>
        /// <param name="source"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public IBlock GetBlockByPower(BlockType type,
                                      BlockDirection direction,
                                      DateTime starttime,
                                      TimeSpan timeframe,
                                      double powerconsumption = 0,
                                      string? source = null,
                                      string? name = null,
                                      string? description = null,
                                      string? owner = null)
        {

            var amount = powerconsumption * timeframe.TotalHours;

            var e = new BaseBlock();
            e.Id = Guid.NewGuid().ToString();
            e.Type = type;
            e.Direction = direction;
            e.Timeframe = timeframe;
            e.StartTime = starttime;
            if (amount > 0)
                e.Amount = amount;
            if (source != null)
                e.SourceId = source;
            if (owner != null)
                e.ParentId = owner;
            if (name != null)
                e.Name = name;
            if (description != null)
                e.Description = description;

            return e;
        }

        /// <summary>
        /// Fill object with source IBlock
        /// </summary>
        /// <param name="block"></param>
        public virtual void Fill(IBlock block)
        {
            foreach (var param in typeof(IBlock).GetProperties())
            {
                var value = param.GetValue(block);
                if (param.CanWrite)
                    param.SetValue(this, value);
            }
        }

        /// <summary>
        /// Set Block Total amount based on power per hour.
        /// The StartTime and Timeframe must be already set
        /// </summary>
        /// <param name="powerperhour"></param>
        public virtual void SetAmountByPower(double powerperhour)
        {
            if (powerperhour > 0)
            {
                var amount = powerperhour * Timeframe.TotalHours;
                Amount = amount;
            }
        }
    }
}
