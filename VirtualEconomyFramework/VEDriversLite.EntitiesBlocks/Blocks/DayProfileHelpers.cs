using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public static class DayProfileHelpers
    {

        /// <summary>
        /// Load profile from set of the data where each line represents one data.
        /// Line should has structure: DateTime separator Value
        /// Value is expected to be type of double
        /// </summary>
        /// <param name="inputdata"></param>
        /// <param name="separator">separator can be any char or string. It must be same as in input data between DateTime and Value</param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DayProfile LoadDayProfileFromRawData(string inputdata, string separator, string name, DayProfileType type)
        {
            var result = new DayProfile()
            {
                Name = name,
                Type = type
            };
            
            using (var reader = new StringReader(inputdata))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var split = line.Split(separator);
                        if (split != null && split.Length > 1)
                            if (DateTime.TryParse(split[0], out var date) &&
                                double.TryParse(split[1], out var value))
                                    result.ProfileData.TryAdd(date, value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Load profile data from list of double values.
        /// </summary>
        /// <param name="name">Name of the profile</param>
        /// <param name="values">List of double values</param>
        /// <param name="start">Function need to know Start datetime to create DateTime identification in dictionary.</param>
        /// <param name="end">Function will load data only up to this datetime</param>
        /// <param name="type">type of the DayProfile</param>
        /// <param name="timeframe">timeframe will set the step of DateTime stamp for the next loaded value</param>
        /// <returns></returns>
        public static DayProfile LoadDayProfileFromListOfValuesAndTimeframe(string name, 
                                                                            List<double> values, 
                                                                            DateTime start, 
                                                                            DateTime end, 
                                                                            DayProfileType type,
                                                                            BlockTimeframe timeframe)
        {
            var profile = new DayProfile()
            {
                Name = name,
                Type = type,
            };

            var tmp = start;
            foreach(var value in values)
            {
                profile.ProfileData.TryAdd(tmp, value);
                tmp = tmp.AddSeconds(BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, tmp).TotalSeconds);
                if (tmp >= end)
                    break;
            }

            return profile;
        }

        /// <summary>
        /// Export Day profile to json
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static string ExportDayProfileToJson(DayProfile profile)
        {
            return JsonConvert.SerializeObject(profile);
        }

        /// <summary>
        /// Load profile from the json with proper name of properties
        /// </summary>
        /// <param name="profilejson"></param>
        /// <returns></returns>
        public static DayProfile ImportDayProfileFromJson(string profilejson)
        {
            try
            {
                var profile = JsonConvert.DeserializeObject<DayProfile>(profilejson);
                return profile;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load DayProfile from json. Error: " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Take input IBlock list and iterate over all items and do the action based on the type of the block.
        /// It loads the profile data for each Block date (takes just yyyy.MM.dd) and do action value with IBlock.Amount
        /// For example SumCoeficient DayProfileType will set the function IBlock.Amount += value; etc.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static IEnumerable<IBlock> ActionBlockWithDayProfile(IEnumerable<IBlock> input, DayProfile profile)
        {
            var counter = 0;

            foreach (var inp in input)
            {
                if (profile.ProfileData.TryGetValue(new DateTime(inp.StartTime.Year, inp.StartTime.Month, inp.StartTime.Day), out var pr))
                {
                    switch (profile.Type)
                    {
                        case DayProfileType.AddCoeficient:
                            if (pr != 0)
                                inp.Amount += pr;
                            break;
                        case DayProfileType.SubtractCoeficient:
                            if (pr != 0)
                                inp.Amount -= pr;
                            break;
                        case DayProfileType.MultiplyCoeficient:
                            inp.Amount *= pr;
                            break;
                        case DayProfileType.DivideCoeficient:
                            if (pr != 0)
                                inp.Amount /= pr;
                            break;
                    }
                    
                    yield return inp;
                    counter++;
                }
            }
        }
    }
}
