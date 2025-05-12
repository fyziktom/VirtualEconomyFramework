using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public static class BlockHelpers
    {
        public static List<IBlock> CreateYearBlocks(int startyear, int endyear, string parentId, double energyAmount, BlockDirection direction, BlockType blocktype)
        {
            if (string.IsNullOrEmpty(parentId))
                parentId = string.Empty;
            var lengthinYears = endyear - startyear;
            if (lengthinYears < 0)
            {
                var tp = endyear;
                endyear = startyear;
                startyear = endyear;
                lengthinYears *= -1;
            }
            if (lengthinYears > 0)
            {
                List<IBlock> blocks = new List<IBlock>();
                DateTime date = new DateTime(startyear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var lengthInMonths = lengthinYears * 12;
                for (var i = 1; i <= lengthInMonths; i++)
                {
                    var tmpdate = date;
                    blocks.Add(new BaseBlock()
                    {
                        Amount = energyAmount,
                        Direction = direction,
                        ParentId = parentId,
                        StartTime = date,
                        Timeframe = tmpdate.AddMonths(1) - date,
                        Used = false,
                        Type = blocktype,
                        Id = Guid.NewGuid().ToString()
                    });
                    date = date.AddMonths(1);
                }
                return blocks;
            }
            else
                return new List<IBlock>();
        }

        public static List<IBlock> CreateEmptyBlocks(BlockTimeframe timeframesteps, DateTime starttime, DateTime endtime, string parentId, double energyAmount, BlockDirection direction, BlockType blocktype)
        {
            if (string.IsNullOrEmpty(parentId))
                parentId = string.Empty;

            if (starttime == endtime)
                return new List<IBlock>();
            else if (starttime > endtime)
            {
                var tmp = starttime;
                starttime = endtime;
                starttime = tmp;
            }

            List<IBlock> blocks = new List<IBlock>();

            var tmpdate = starttime;
            var ts = GetTimeSpanBasedOntimeframe(timeframesteps, starttime);

            while (tmpdate < endtime)
            {
                blocks.Add(new BaseBlock()
                {
                    Amount = energyAmount,
                    Direction = direction,
                    ParentId = parentId,
                    StartTime = tmpdate,
                    Timeframe = ts,
                    Used = false,
                    Type = blocktype,
                    Id = Guid.NewGuid().ToString()
                });
                tmpdate += ts;
                ts = GetTimeSpanBasedOntimeframe(timeframesteps, tmpdate);
            }

            return blocks;
        }

        public static IEnumerable<IBlock> CloneBlocks(List<IBlock> inputBlocks, 
                                                      bool chainIds, 
                                                      bool setType = false, 
                                                      BlockType type = BlockType.Mix, 
                                                      bool setDirection = false, 
                                                      BlockDirection direction = BlockDirection.Mix)
        {
            foreach(var block in inputBlocks)
            {
                var b = new BaseBlock();
                b.Fill(block);
                b.Id = Guid.NewGuid().ToString();
                b.SourceId = block.Id;
                if (setType)
                    b.Type = type;
                if (setDirection)
                    b.Direction = direction;

                yield return b;
            }
        }

        public static TimeSpan GetTimeSpanBasedOntimeframe(BlockTimeframe timeframesteps, DateTime starttime)
        {
            var tmpdate = starttime;
            TimeSpan ts = new TimeSpan();
            switch (timeframesteps)
            {
                case BlockTimeframe.Second:
                    ts = tmpdate.AddSeconds(1) - starttime;
                    break;
                case BlockTimeframe.Minute:
                    ts = tmpdate.AddMinutes(1) - starttime;
                    break;
                case BlockTimeframe.Hour:
                    ts = tmpdate.AddHours(1) - starttime;
                    break;
                case BlockTimeframe.Day:
                    ts = tmpdate.AddDays(1) - starttime;
                    break;
                case BlockTimeframe.Week:
                    ts = tmpdate.AddDays(7) - starttime;
                    break;
                case BlockTimeframe.Month:
                    ts = tmpdate.AddMonths(1) - starttime;
                    break;
                case BlockTimeframe.Year:
                    ts = tmpdate.AddYears(1) - starttime;
                    break;
                case BlockTimeframe.QuaterHour:
                    ts = tmpdate.AddMinutes(15) - starttime;
                    break;
                case BlockTimeframe.QuaterYear:
                    ts = tmpdate.AddMonths(3) - starttime;
                    break;
                default:
                    ts = tmpdate.AddDays(1) - starttime;
                    break;
            }
            return ts;
        }

        /// <summary>
        /// Approximated timeframe based on timespan 
        /// </summary>
        /// <param name="ts">End time - Start time = timespan</param>
        /// <returns></returns>
        public static BlockTimeframe GetTimeframeBasedOnTimespan(TimeSpan ts)
        {            
            if (ts.TotalSeconds == 1)
                return BlockTimeframe.Second;
            else if (ts.TotalSeconds > 30 && ts.TotalMinutes < 5)
                return BlockTimeframe.Minute;
            else if (ts.TotalMinutes >= 5 && ts.TotalMinutes < 30)
                return BlockTimeframe.QuaterHour;
            else if (ts.TotalMinutes >= 30 && ts.TotalMinutes < 90)
                return BlockTimeframe.Hour;
            else if (ts.TotalDays >= 5 && ts.TotalDays < 9)
                return BlockTimeframe.Week;
            else if (ts.TotalDays >= 20 && ts.TotalDays < 40)
                return BlockTimeframe.Month;
            else if (ts.TotalDays >= 60 && ts.TotalDays < 200)
                return BlockTimeframe.QuaterYear;
            else if (ts.TotalDays >= 200 && ts.TotalDays < 400)
                return BlockTimeframe.Year;

            return BlockTimeframe.Hour;
        }

        public static List<IBlock> GetResultBlocks(BlockTimeframe timeframesteps,
                                                        DateTime starttime,
                                                        DateTime endtime,
                                                        string parentId)
        {
            var result = new List<IBlock>();
            var tmpdate = starttime;
            var ts = GetTimeSpanBasedOntimeframe(timeframesteps, starttime);

            while (tmpdate < endtime)
            {
                result.Add(new BaseBlock()
                {
                    Id = Guid.NewGuid().ToString(),
                    Direction = BlockDirection.Consumed,
                    Type = BlockType.Calculated,
                    StartTime = tmpdate,
                    Timeframe = ts,
                    ParentId = parentId,
                });
                tmpdate += ts;
                ts = GetTimeSpanBasedOntimeframe(timeframesteps, tmpdate);
            }

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
            if (string.IsNullOrEmpty(parentId))
                parentId = string.Empty;


            var result = new List<IBlock>();
            var tmpdate = firstrun;
            var ts = endtime - starttime;

            // to identify repetitive blocks with first one
            var firstBlockId = string.Empty;
            var counter = 0;

            while (tmpdate < endrun)
            {
                var hash = Guid.NewGuid().ToString();

                // store the first block Id to let others refer to it
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = hash;
                if (!string.IsNullOrEmpty(starthash))
                    firstBlockId = starthash;

                result.Add(new BaseBlock()
                {
                    Id = counter == 0 && !string.IsNullOrEmpty(starthash) ? starthash : hash,
                    Name = name != null ? name : string.Empty,
                    IsRepetitiveSource = counter == 0,
                    IsRepetitiveChild = counter > 0,
                    RepetitiveSourceBlockId = counter > 0 ? firstBlockId : string.Empty,
                    IsOffPeriodRepetitive = true,
                    RepetitiveFirstRun = counter == 0 ? firstrun : null,
                    RepetitiveEndRun = counter == 0 ? endrun : null,
                    OffPeriod = counter == 0 ? offtime : null,
                    Direction = direction,
                    Type = blocktype,
                    StartTime = tmpdate,
                    Timeframe = ts,
                    ParentId = parentId,
                    SourceId = sourceId,
                    Amount = energyAmount * ts.TotalHours,
                    Used = false,
                });
                tmpdate += endtime - starttime + offtime;
                counter++;
            }

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
            if (string.IsNullOrEmpty(parentId))
                parentId = string.Empty;

            var result = new List<IBlock>();
            var tmpdate = starttime;
            var ts = endtime - starttime;
            if (ts.TotalHours > 24) //total hours must be lower or equal to 24 hours.
                return result;

            var end = new DateTime(endrun.Year, endrun.Month, endrun.Day, 0, 0, 0, DateTimeKind.Utc);

            // to identify repetitive blocks with first one
            var firstBlockId = string.Empty;
            var counter = 0;

            while (tmpdate < end)
            {
                if (!justWorkDays && !justWeekend || justWorkDays && tmpdate.DayOfWeek >= DayOfWeek.Monday && tmpdate.DayOfWeek <= DayOfWeek.Friday ||
                                     justWeekend && (tmpdate.DayOfWeek == DayOfWeek.Saturday || tmpdate.DayOfWeek == DayOfWeek.Sunday))
                {
                    var hash = Guid.NewGuid().ToString();

                    // store the first block Id to let others refer to it
                    if (string.IsNullOrEmpty(firstBlockId))
                        firstBlockId = hash;
                    if (!string.IsNullOrEmpty(starthash))
                        firstBlockId = starthash;

                    result.Add(new BaseBlock()
                    {
                        Id = counter == 0 && !string.IsNullOrEmpty(starthash) ? starthash : hash,
                        Name = name != null ? name : string.Empty,
                        IsRepetitiveSource = counter == 0,
                        IsRepetitiveChild = counter > 0,
                        RepetitiveSourceBlockId = counter > 0 ? firstBlockId : string.Empty,
                        IsOffPeriodRepetitive = false,
                        IsInDayOnly = true,
                        JustInWeek = justWorkDays,
                        JustInWeekends = justWeekend,
                        RepetitiveFirstRun = counter == 0 ? firstrun : null,
                        RepetitiveEndRun = counter == 0 ? endrun : null,
                        Direction = direction,
                        Type = blocktype,
                        StartTime = tmpdate,
                        Timeframe = ts,
                        ParentId = parentId,
                        SourceId = sourceId,
                        Amount = power * ts.TotalHours,
                        Used = false,
                    });
                    counter++;
                }
                tmpdate = tmpdate.AddDays(1);
            }

            return result;
        }

        public static IReadOnlyCollection<IBlock> GetFilteredBlocksByTimeRanges(IReadOnlyCollection<IBlock> blocks, List<(DateTime start, DateTime end)> ranges)
        {
            var resultBlocks = new List<IBlock>();
            foreach (var block in blocks) 
            {
                foreach (var range in ranges)
                {
                    if (block.StartTime >= range.start && block.StartTime < range.end)
                    {
                        resultBlocks.Add(block);
                        break;
                    }
                    else if (block.StartTime < range.start && block.StartTime + block.Timeframe > range.start)
                    {
                        resultBlocks.Add(block);
                        break;
                    }
                    else if (block.StartTime < range.end && block.StartTime + block.Timeframe > range.end)
                    {
                        resultBlocks.Add(block);
                        break;
                    }
                }
            }
            return resultBlocks;
        }

        /// <summary>
        /// Draw blocks in console
        /// </summary>
        /// <param name="heading"></param>
        /// <param name="blocks"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static void DrawBlocks(string heading, List<IBlock> blocks, DateTime start, DateTime end)
        {
            Console.WriteLine($"-------------------{heading}--------------------:");
            Console.WriteLine("Results:");
            Console.WriteLine($"StartTime: {start}");
            Console.WriteLine($"EndTime: {end}");
            Console.WriteLine($"Calculated Data:");
            var total = 0.0;
            foreach (var block in blocks)
            {
                Console.WriteLine($"\t{block.StartTime.ToString("yyyy_MM_dd-hh:mm")} - {block.EndTime.ToString("yyyy_MM_dd-hh:mm")}, Amount: {Math.Round(block.Amount, 2)} kWh.");
                total += block.Amount;
            }
            Console.WriteLine($"Total Bilance: {total} kWh");
            Console.WriteLine("--------------------END------------------:");

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
                                                            List<double>? energyAmountYearProfile = null,
                                                            string starthash = null)
        {
            if (string.IsNullOrEmpty(parentId))
                parentId = string.Empty;
            var lengthinYears = endyear - startyear;
            if (lengthinYears < 0)
            {
                endyear = startyear;
                startyear = endyear;
                lengthinYears *= -1;
            }
            if (lengthinYears == 0)
                throw new Exception("endyear must be bigger than startyear.");

            // to identify repetitive blocks with first one
            var firstBlockId = string.Empty;
            var counter = 0;

            List<IBlock> blocks = new List<IBlock>();
            DateTime date = new DateTime(startyear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var lengthInMonths = lengthinYears * 12;
            for (var i = 1; i <= lengthInMonths; i++)
            {
                var tmpdate = date;
                var name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(tmpdate.Month);
                var hash = Guid.NewGuid().ToString();

                // store the first block Id to let others refer to it
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = hash;
                if (!string.IsNullOrEmpty(starthash))
                    firstBlockId = starthash;

                blocks.Add(new BaseBlock()
                {
                    Name = name,
                    Amount = PVEBasicYearProfile[date.Month - 1] * PVEPeakPower,
                    //IsRepetitiveSource = counter == 0 ? true : false,
                    //IsRepetitiveChild = counter > 0 ? true : false,
                    RepetitiveSourceBlockId = counter > 0 ? firstBlockId : string.Empty,
                    //IsOffPeriodRepetitive = true,
                    //RepetitiveFirstRun = counter == 0 ? date : null,
                    //RepetitiveEndRun = counter == 0 ? date.AddYears(lengthinYears) : null,
                    Direction = BlockDirection.Created,
                    ParentId = parentId,
                    StartTime = date,
                    Timeframe = tmpdate.AddMonths(1) - date,
                    Used = false,
                    Type = BlockType.Simulated,
                    Id = counter == 0 && !string.IsNullOrEmpty(starthash) ? starthash : hash
                });
                counter++;
                date = date.AddMonths(1);
            }
            return blocks;
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
                                                            List<double>? energyAmountYearProfile = null,
                                                            string? starthash = null)
        {
            if (string.IsNullOrEmpty(parentId))
                parentId = string.Empty;
            var lengthinYears = endyear - startyear;
            if (lengthinYears < 0)
            {
                endyear = startyear;
                startyear = endyear;
                lengthinYears *= -1;
            }
            if (lengthinYears == 0)
                throw new Exception("endyear must be bigger than startyear.");

            var sunduration = endsun - startsun;

            var profile = energyAmountYearProfile != null ? energyAmountYearProfile : PVEBasicYearProfile;

            List<IBlock> blocks = new List<IBlock>();
            DateTime date = new DateTime(startyear, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(startsun.Hour).AddMinutes(startsun.Minute).AddSeconds(startsun.Second);

            // to identify repetitive blocks with first one
            var firstBlockId = string.Empty;
            var counter = 0;

            var end = new DateTime(endyear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            while (date < end)
            {
                var tmpdate = date;
                var name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(tmpdate.Month);
                var hash = Guid.NewGuid().ToString();

                // store the first block Id to let others refer to it
                if (string.IsNullOrEmpty(firstBlockId))
                    firstBlockId = hash;
                if (!string.IsNullOrEmpty(starthash))
                    firstBlockId = starthash;

                blocks.Add(new BaseBlock()
                {
                    Name = name,
                    Amount = profile[date.Month - 1] * PVEPeakPower / DateTime.DaysInMonth(date.Year, date.Month),
                    //IsRepetitiveSource = counter == 0 ? true : false,
                    //IsRepetitiveChild = counter > 0 ? true : false,
                    RepetitiveSourceBlockId = counter > 0 ? firstBlockId : string.Empty,
                    //IsOffPeriodRepetitive = false,
                    //IsInDayOnly = true,
                    //RepetitiveFirstRun = counter == 0 ? date : null,
                    //RepetitiveEndRun = counter == 0 ? end : null,
                    Direction = BlockDirection.Created,
                    ParentId = parentId,
                    StartTime = date,
                    Timeframe = sunduration,
                    Used = false,
                    Type = BlockType.Simulated,
                    Id = counter == 0 && !string.IsNullOrEmpty(starthash) ? starthash : hash
                });
                counter++;
                date = date.AddDays(1);
            }
            return blocks;
        }

        #endregion

    }
}
