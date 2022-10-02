using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Energy.Blocks
{
    public enum BlockTimeframe
    {
        Second = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Second,
        Minute = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Minute,
        Hour = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Hour,
        Day = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Day,
        Week = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Week,
        Month = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Month,
        Year = VEDriversLite.EntitiesBlocks.Blocks.BlockTimeframe.Year
    }
    public enum BlockType
    {
        Simulated = VEDriversLite.EntitiesBlocks.Blocks.BlockType.Simulated,
        Calculated = VEDriversLite.EntitiesBlocks.Blocks.BlockType.Calculated,
        Real = VEDriversLite.EntitiesBlocks.Blocks.BlockType.Real
    }
    public enum BlockDirection
    {
        Created = VEDriversLite.EntitiesBlocks.Blocks.BlockDirection.Created,
        Stored = VEDriversLite.EntitiesBlocks.Blocks.BlockDirection.Stored,
        Consumed = VEDriversLite.EntitiesBlocks.Blocks.BlockDirection.Consumed,
        Mix = VEDriversLite.EntitiesBlocks.Blocks.BlockDirection.Mix,
    }
    public interface IBlock : VEDriversLite.EntitiesBlocks.Blocks.IBlock
    {
    
    }
}
