using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public abstract class CommonBlock : IBlock
    {
        /// <summary>
        /// Unique identifier of the block
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Name of the block
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Descrioption of the block
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Id of the source which created this block
        /// </summary>
        public string SourceId { get; set; } = string.Empty;
        /// <summary>
        /// Parent Id of the block
        /// </summary>
        public string ParentId { get; set; } = string.Empty;
        /// <summary>
        /// Set when this block is parent of some repetitive blocks group.
        /// It means based on this block other was created and refers to its Id
        /// </summary>
        public bool IsRepetitiveSource { get; set; } = false;
        /// <summary>
        /// First Block of repetitive blocks Id
        /// </summary>
        public string RepetitiveSourceBlockId { get; set; } = string.Empty;
        /// <summary>
        /// Indicate if this block is related to some repetitive block
        /// </summary>
        public bool IsRepetitiveChild { get; set; } //=> !string.IsNullOrEmpty(RepetitiveSourceBlockId); }
        /// <summary>
        /// Indicate if the block fits in one day period (24h and between 0:00 - 24:00)
        /// </summary>
        public bool IsInDayOnly { get; set; } = false;
        /// <summary>
        /// Indicate if the block is repetitive with the specified off time between blocks
        /// </summary>
        public bool IsOffPeriodRepetitive { get; set; } = false;
        /// <summary>
        /// Indicate if repetitive block is only in week days
        /// </summary>
        public bool JustInWeek { get; set; } = false;
        /// <summary>
        /// Indicate if repetitive block is only in weekend days
        /// </summary>
        public bool JustInWeekends { get; set; } = false;
        /// <summary>
        /// Block type - real block, simulation
        /// </summary>
        public BlockType Type { get; set; } = BlockType.Simulated;
        /// <summary>
        /// Define if the block has been created or consumed
        /// </summary>
        public BlockDirection Direction { get; set; } = BlockDirection.Created;
        /// <summary>
        /// Start time of this block
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private DateTime _startTime = DateTime.UtcNow;
        /// <summary>
        /// Time Frame of the block, it means the time unit related to the amount
        /// </summary>
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
        public DateTime EndTime { get => StartTime + Timeframe; }

        /// <summary>
        /// First time of run of the repetitive block
        /// </summary>
        public DateTime? RepetitiveFirstRun
        {
            get => _repetitiveFirstRun;
            set
            {
                _repetitiveFirstRun = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private DateTime? _repetitiveFirstRun = null;
        /// <summary>
        /// Last time of run of repetitive block
        /// </summary>
        public DateTime? RepetitiveEndRun
        {
            get => _repetitiveEndRun;
            set
            {
                _repetitiveEndRun = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private DateTime? _repetitiveEndRun = null;
        /// <summary>
        /// Off period between the blocks
        /// </summary>
        public TimeSpan? OffPeriod
        {
            get => _offPeriod;
            set
            {
                _offPeriod = value;
                LastChange = DateTime.UtcNow;
            }
        }
        private TimeSpan? _offPeriod = null;

        /// <summary>
        /// Amount, for example energy in kWh consumed over whole timeframe
        /// </summary>
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
        public bool Used { get; set; } = false;

        /// <summary>
        /// Last change of the parameters of the block
        /// </summary>
        public DateTime LastChange { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Average Amount per second, for example energy power consumption per second = kWs
        /// </summary>
        public double AvgConsumptionPerSecond { get => Amount / (EndTime - StartTime).TotalSeconds; }
        /// <summary>
        /// Average Amount per hour, for example energy power consumption per hour = kWh
        /// </summary>
        public double AvgConsumptionPerHour { get => Amount / (EndTime - StartTime).TotalHours; }

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
