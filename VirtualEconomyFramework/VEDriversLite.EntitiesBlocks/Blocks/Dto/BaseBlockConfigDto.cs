using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks.Dto
{
    public class BaseBlockConfigDto
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
        public DateTime StartTime { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Time Frame of the block, it means the time unit related to the amount
        /// </summary>
        public TimeSpan Timeframe { get; set; } = new TimeSpan(1, 0, 0);

        /// <summary>
        /// First time of run of the repetitive block
        /// </summary>
        public DateTime? RepetitiveFirstRun { get; set; }
        /// <summary>
        /// Last time of run of repetitive block
        /// </summary>
        public DateTime? RepetitiveEndRun { get; set; }
        /// <summary>
        /// Off period between the blocks
        /// </summary>
        public TimeSpan? OffPeriod { get; set; }

        /// <summary>
        /// Amount, for example energy in kWh consumed over whole timeframe
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// Flag for the block which has been already used
        /// </summary>
        public bool Used { get; set; } = false;

        /// <summary>
        /// Last change of the parameters of the block
        /// </summary>
        public DateTime LastChange { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fill object with source IBlock
        /// </summary>
        /// <param name="block"></param>
        public void Fill(IBlock block)
        {
            foreach (var param in typeof(IBlock).GetProperties())
            {
                try
                {
                    if (param.CanWrite)
                    {
                        var value = param.GetValue(block);
                        var paramname = param.Name;
                        var pr = typeof(BaseBlockConfigDto).GetProperties().FirstOrDefault(p => p.Name == paramname);
                        if (pr != null)
                            pr.SetValue(this, value);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot load parameter." + ex.Message);
                }
            }
        }

        /// <summary>
        /// GetBlockFrom Dto
        /// </summary>
        public IBlock GetBlockFromDto()
        {
            if (Type == BlockType.Calculated || Type == BlockType.Simulated || Type == BlockType.Real)
            {
                var res = new BaseBlock();
                foreach (var param in typeof(BaseBlockConfigDto).GetProperties())
                {
                    try
                    {
                        if (param.CanWrite)
                        {
                            var value = param.GetValue(this);
                            var paramname = param.Name;
                            var pr = typeof(IBlock).GetProperties().FirstOrDefault(p => p.Name == paramname);
                            if (pr != null)
                                pr.SetValue(res, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot load parameter. " + ex.Message);
                    }
                }
                return res;
            }
            return null;
        }
    }
}
