using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Energy.Blocks
{
    public static class BlockHelpers
    {
        public static List<IBlock> CreateYearBlocks(int startyear, int endyear, string parentId, double energyAmount, BlockDirection direction, BlockType blocktype)
        {

            var list = EntitiesBlocks.Blocks.BlockHelpers.CreateYearBlocks(startyear, endyear, parentId, energyAmount, (EntitiesBlocks.Blocks.BlockDirection)direction, (EntitiesBlocks.Blocks.BlockType)blocktype);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }

        public static List<IBlock> CreateEmptyBlocks(BlockTimeframe timeframesteps, DateTime starttime, DateTime endtime, string parentId, double energyAmount, BlockDirection direction, BlockType blocktype)
        {
            var list = EntitiesBlocks.Blocks.BlockHelpers.CreateEmptyBlocks((EntitiesBlocks.Blocks.BlockTimeframe)timeframesteps, starttime, endtime, parentId, energyAmount, (EntitiesBlocks.Blocks.BlockDirection)direction, (EntitiesBlocks.Blocks.BlockType)blocktype);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }

        public static TimeSpan GetTimeSpanBasedOntimeframe(BlockTimeframe timeframesteps)
        {
            return EntitiesBlocks.Blocks.BlockHelpers.GetTimeSpanBasedOntimeframe((EntitiesBlocks.Blocks.BlockTimeframe)timeframesteps);
        }

        public static List<IBlock> GetResultBlocks(BlockTimeframe timeframesteps,
                                                        DateTime starttime,
                                                        DateTime endtime,
                                                        string parentId)
        {
            var list = EntitiesBlocks.Blocks.BlockHelpers.GetResultBlocks((EntitiesBlocks.Blocks.BlockTimeframe)timeframesteps,
                                                                          starttime,    
                                                                          endtime,
                                                                          parentId);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }

        /// <summary>
        /// Create repetitive block with specified duration and off period. 
        /// For example: It can run multiple days and then off even for few hours, and again, again. 
        /// Dates firstrun and endrun limit whole time frame, then the blocks are inside of this timeframe with specified period of on (endtime - starttime) and offtime
        /// All blocks after the first one are related to the first id through RepetitiveSourceBlockId property in Block
        /// </summary>
        /// <param name="firstrun">Start of the period, where repetitive blocks starts</param>
        /// <param name="endrun">End of the period, where repetitive blocks ends</param>
        /// <param name="starttime">Start time of the block</param>
        /// <param name="endtime">End time of the block</param>
        /// <param name="offtime">Off time period between two repetitive blocks</param>
        /// <param name="parentId">Energy entity Id</param>
        /// <param name="energyAmount">Amount of energy in one block</param>
        /// <param name="direction">Direction of block</param>
        /// <param name="blocktype">Block type</param>
        /// <returns></returns>
        public static List<IBlock> CreateRepetitiveBlock(DateTime firstrun,
                                                         DateTime endrun,
                                                         DateTime starttime,
                                                         DateTime endtime,
                                                         TimeSpan offtime,
                                                         double energyAmount,
                                                         string sourceId,
                                                         string parentId,
                                                         BlockDirection direction,
                                                         BlockType blocktype,
                                                         string name = null,
                                                         string starthash = null)
        {
            var list = EntitiesBlocks.Blocks.BlockHelpers.CreateRepetitiveBlock(firstrun,
                                                                                 endrun,
                                                                                 starttime,
                                                                                 endtime,
                                                                                 offtime,
                                                                                 energyAmount,
                                                                                 sourceId,
                                                                                 parentId,
                                                                                 (EntitiesBlocks.Blocks.BlockDirection)direction,
                                                                                 (EntitiesBlocks.Blocks.BlockType)blocktype,
                                                                                 name,
                                                                                 starthash);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }

        /// <summary>
        /// Create repetitive block with specified duration which must fit into one day
        /// For example it will run everyday from 8:00 to 15:00
        /// All blocks after the first one are related to the first id through RepetitiveSourceBlockId property in Block
        /// </summary>
        /// <param name="firstrun">Start of the period, where repetitive blocks starts. Function takes just year, month and day from provided datetime.</param>
        /// <param name="endrun">End of the period, where repetitive blocks ends. Function takes just year, month and day from provided datetime.</param>
        /// <param name="starttime">Start time of the block. Function takes just hours, minutes, seconds from provided datetime</param>
        /// <param name="endtime">End time of the block. Function takes just hours, minutes, seconds from provided datetime</param>
        /// <param name="parentId">entity Id</param>
        /// <param name="power">Power/h of block</param>
        /// <param name="direction">Direction of block</param>
        /// <param name="blocktype">Block type</param>
        /// <returns></returns>
        public static List<IBlock> CreateRepetitiveDayBlock(DateTime firstrun,
                                                            DateTime endrun,
                                                            DateTime starttime,
                                                            DateTime endtime,
                                                            double power,
                                                            string sourceId,
                                                            string parentId,
                                                            BlockDirection direction,
                                                            BlockType blocktype,
                                                            bool justWorkDays = false,
                                                            bool justWeekend = false,
                                                            string name = null,
                                                            string starthash = null)
        {
            var list = EntitiesBlocks.Blocks.BlockHelpers.CreateRepetitiveDayBlock(firstrun,
                                                                                   endrun,
                                                                                   starttime,
                                                                                   endtime,
                                                                                   power,
                                                                                   sourceId,
                                                                                   parentId,
                                                                                   (EntitiesBlocks.Blocks.BlockDirection)direction,
                                                                                   (EntitiesBlocks.Blocks.BlockType)blocktype,
                                                                                   justWorkDays,
                                                                                   justWeekend,
                                                                                   name,
                                                                                   starthash);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }


        #region PVEHelpers

        /// <summary>
        /// Basic profile of production of 1kW PVE in (data from Czech Republic)
        /// </summary>
        public static List<double> PVEBasicYearProfile = new List<double>()
        {
            42,
            61,
            98,
            122,
            148,
            138,
            157,
            144,
            108,
            89,
            39,
            31
        };

        /// <summary>
        /// Create simulated energy blocks for year time horizonts. The block duration are "Month". 
        /// This function will simulate blocks of PVE of specified Peak power. 
        /// The profile of the production across the different months are loaded from PVEBasicYearProfile
        /// </summary>
        /// <param name="startyear">for example 2022</param>
        /// <param name="endyear">for example 2023</param>
        /// <param name="parentId">Energy entity Id</param>
        /// <param name="PVEPeakPower">peak power of the PVE. for example 5kW</param>
        /// <param name="energyAmountYearProfile">if you want to specify different energy profile, pass it here. otherwise the PVEBasicYearProfile is used.</param>
        /// <returns></returns>
        public static List<IBlock> PVECreateYearBlocks(int startyear,
                                                            int endyear,
                                                            string parentId,
                                                            double PVEPeakPower,
                                                            List<double> energyAmountYearProfile = null,
                                                            string starthash = null)
        {
            var list = EntitiesBlocks.Blocks.BlockHelpers.PVECreateYearBlocks(startyear,
                                                                              endyear,
                                                                              parentId,
                                                                              PVEPeakPower,
                                                                              energyAmountYearProfile,
                                                                              starthash);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }

        /// <summary>
        /// Create simulated energy blocks for year time horizonts. The block duration are "Day". 
        /// This function will simulate blocks of PVE of specified Peak power. 
        /// The profile of the production across the different months are loaded from PVEBasicYearProfile
        /// </summary>
        /// <param name="startyear">for example 2022</param>
        /// <param name="endyear">for example 2023</param>
        /// <param name="parentId">Energy entity Id</param>
        /// <param name="PVEPeakPower">peak power of the PVE. for example 5kW</param>
        /// <param name="energyAmountYearProfile">if you want to specify different energy profile, pass it here. otherwise the PVEBasicYearProfile is used.</param>
        /// <returns></returns>
        public static List<IBlock> PVECreateYearDaysBlocks(int startyear,
                                                            int endyear,
                                                            DateTime startsun,
                                                            DateTime endsun,
                                                            string parentId,
                                                            double PVEPeakPower,
                                                            List<double> energyAmountYearProfile = null,
                                                            string starthash = null)
        {
            var list = EntitiesBlocks.Blocks.BlockHelpers.PVECreateYearDaysBlocks(startyear,
                                                                              endyear,
                                                                              startsun,
                                                                              endsun,
                                                                              parentId,
                                                                              PVEPeakPower,
                                                                              energyAmountYearProfile,
                                                                              starthash);
            var result = new List<IBlock>();
            foreach (var l in list)
                result.Add(l as IBlock);
            return result;
        }

        #endregion

    }
}
