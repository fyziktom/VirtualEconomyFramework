using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using static System.Reflection.Metadata.BlobBuilder;

namespace VEDriversLite.EntitiesBlocks.Blocks
{
    public static class DataProfileHelpers
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
        public static DataProfile LoadDataProfileFromRawData(string inputdata, string separator, string name, DataProfileType type)
        {
            var result = new DataProfile()
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
                        if (split != null && 
                            split.Length > 1 &&
                            DateTime.TryParse(split[0], out var date) &&
                            double.TryParse(split[1], out var value))
                        {
                            result.ProfileData.TryAdd(date, value);
                        }
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
        public static DataProfile LoadDataProfileFromListOfValuesAndTimeframe(string name, 
                                                                              List<double> values, 
                                                                              DateTime start, 
                                                                              DateTime end, 
                                                                              DataProfileType type,
                                                                              BlockTimeframe timeframe)
        {
            var profile = new DataProfile()
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
        /// Export Data profile to json
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static string ExportDataProfileToJson(DataProfile profile)
        {
            return JsonConvert.SerializeObject(profile);
        }

        /// <summary>
        /// Load profile from the json with proper name of properties
        /// </summary>
        /// <param name="profilejson"></param>
        /// <returns></returns>
        public static DataProfile? ImportDataProfileFromJson(string profilejson)
        {
            try
            {
                var profile = JsonConvert.DeserializeObject<DataProfile>(profilejson);
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
        public static IEnumerable<IBlock> ActionBlockWithDataProfile(IEnumerable<IBlock> input, DataProfile profile)
        {
            var counter = 0;

            foreach (var inp in input)
            {
                foreach (var data in profile.ProfileData.Where(d => d.Key > inp.StartTime && d.Key < inp.EndTime).Select(d => d.Value))
                {
                    switch (profile.Type)
                    {
                        case DataProfileType.AddCoeficient:
                            inp.Amount += data;
                            break;
                        case DataProfileType.SubtractCoeficient:
                            inp.Amount -= data;
                            break;
                        case DataProfileType.MultiplyCoeficient:
                            inp.Amount *= data;
                            break;
                        case DataProfileType.DivideCoeficient:
                            if (data != 0)
                                inp.Amount /= data;
                            break;
                    }

                    yield return inp;
                    counter++;
                }
            }
        }

        /// <summary>
        /// Convert input blocks to data profile.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static DataProfile ConvertBlocksToDataProfile(IEnumerable<IBlock> input, double divideEachValue = 0)
        {
            var result = new DataProfile();
            if (input == null)
                return result;

            result.Type = DataProfileType.AddCoeficient;

            foreach (var inp in input)
                result.ProfileData.TryAdd(inp.StartTime, divideEachValue > 0 ? inp.Amount / divideEachValue : inp.Amount);

            return result;
        }

        /// <summary>
        /// Convert dataprofile to IBlocks
        /// </summary>
        /// <param name="input">input dataProfile</param>
        /// <param name="direction">direction of final blocks</param>
        /// <param name="type">type of final blocks</param>
        /// <param name="parentId">parentId of final blocks</param>
        /// <returns></returns>
        public static IEnumerable<IBlock> ConvertDataProfileToBlocks(DataProfile input, 
                                                                     BlockDirection direction, 
                                                                     BlockType type, 
                                                                     string parentId, 
                                                                     double divideEachValue = 0, 
                                                                     bool withoutNullValueBlocks = false, 
                                                                     string name = "", 
                                                                     string description = "", 
                                                                     string repetitiveSourceDataProfileId = "",
                                                                     string sourceId = "")
        {
            var result = new List<IBlock>();
            
            var keys = input.ProfileData.Keys.ToArray();
            var values = input.ProfileData.Values.ToArray();
            var isChild = false;
            var repetitiveParentId = string.Empty;

            for (int i = 0; i < input.ProfileData.Count; i++)
            { 
                var k = keys[i];
                var v = values[i];

                if (!withoutNullValueBlocks || (withoutNullValueBlocks && v != 0))
                {
                    if (divideEachValue > 0)
                        v = values[i] / divideEachValue;

                    DateTime? k1 = null;

                    if (i == 0)
                        k1 = keys[1];
                    else if (i == input.ProfileData.Count - 1)
                        k1 = keys[i - 1];
                    else
                        k1 = keys[i + 1];

                    var id = Guid.NewGuid().ToString();
                    if (!isChild)
                        repetitiveParentId = id;

                    var b = new BaseBlock()
                    {
                        Name = name,
                        Description = description,
                        RepetitiveSourceDataProfileId = repetitiveSourceDataProfileId,
                        Amount = v,
                        Direction = direction,
                        ParentId = !string.IsNullOrEmpty(parentId) ? parentId : string.Empty,
                        StartTime = k,
                        Timeframe = (TimeSpan)(k1 - k),
                        Used = false,
                        SourceId = sourceId,
                        RepetitiveSourceBlockId = isChild ? repetitiveParentId : string.Empty,
                        Type = type,
                        Id = id
                    };

                    if (!isChild)
                        isChild = true;

                    result.Add(b);

                    yield return b;
                }
            }
        }
    }
}
