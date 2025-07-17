using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public interface IRepetitiveBlock : IBlock
    {
        /// <summary>
        /// Set when this block is parent of some repetitive blocks group.
        /// It means based on this block other was created and refers to its Id
        /// </summary>
        bool IsRepetitiveSource { get; set; }
        /// <summary>
        /// First Block of repetitive blocks Id
        /// </summary>
        string RepetitiveSourceBlockId { get; set; }
        /// <summary>
        /// DataProfile Id for creating repetitive line
        /// </summary>
        string RepetitiveSourceDataProfileId { get; set; }
        /// <summary>
        /// Indicate if this block is related to some repetitive block
        /// </summary>
        bool IsRepetitiveChild { get; set; }
        /// <summary>
        /// Indicate if the block fits in one day period (24h and between 0:00 - 24:00)
        /// </summary>
        bool IsInDayOnly { get; set; }
        /// <summary>
        /// Indicate if the block is repetitive with the specified off time between blocks
        /// </summary>
        bool IsOffPeriodRepetitive { get; set; }
        /// <summary>
        /// Indicate if repetitive block is only in week days
        /// </summary>
        bool JustInWeek { get; set; }
        /// <summary>
        /// Indicate if repetitive block is only in weekend days
        /// </summary>
        bool JustInWeekends { get; set; }

        /// <summary>
        /// First time of run of the repetitive block
        DateTime? RepetitiveFirstRun { get; set; }
        /// <summary>
        /// Last time of run of repetitive block
        /// </summary>
        DateTime? RepetitiveEndRun { get; set; }
        /// <summary>
        /// Off period between the blocks
        /// </summary>
        TimeSpan? OffPeriod { get; set; }
    }
}
