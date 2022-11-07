using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Consumers.Measuring
{
    /// <summary>
    /// Network of the measure spots. Spots can be from simulated source or load real values.
    /// </summary>
    public class MeasureSpotsNetwork
    {
        /// <summary>
        /// Name of the Measure Spots Network
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Id of Measure Spots Network
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Id of parent of this network
        /// </summary>
        public string ParentId { get; set; } = string.Empty;
        public ConcurrentDictionary<string, MeasureSpot> MeasureSpots { get; set; } = new ConcurrentDictionary<string, MeasureSpot>();

        /// <summary>
        /// Get Measured Data profile of some MeasureSpot, based on its Id.
        /// The profile is identified by OBIS code
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obis"></param>
        /// <returns></returns>
        public DataProfile? GetMeasuredProfile(string id, OBIS obis)
        {
            if (MeasureSpots.TryGetValue(id, out var ms))
            {
                if (ms.Profiles.TryGetValue(obis, out var data))
                    return data;
            }
            return null;
        }

        /// <summary>
        /// Clear all values in measured profile in some MeasureSpot
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obis"></param>
        /// <returns></returns>
        public bool ClearMeasuredProfile(string id, OBIS obis)
        {
            if (MeasureSpots.TryGetValue(id, out var ms))
            {
                if (ms.Profiles.TryGetValue(obis, out var data))
                {
                    data.ProfileData.Clear();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Load data to measured Sport profile. It clear original profile data before load the new data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obis"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool LoadDataToMeasuredSpot(string id, OBIS obis, DataProfile profile)
        {
            if (MeasureSpots.TryGetValue(id, out var ms))
            {
                if (ms.Profiles.TryGetValue(obis, out var data))
                {
                    data.ProfileData.Clear();
                    data.Name = profile.Name;
                    data.Type = profile.Type;
                    foreach(var d in profile.ProfileData)
                        data.ProfileData.TryAdd(d.Key,d.Value);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add data to measured Sport profile. It keeps original profile data before load the new data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="obis"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool AddDataToMeasuredSpot(string id, OBIS obis, DataProfile profile)
        {
            if (MeasureSpots.TryGetValue(id, out var ms))
            {
                if (ms.Profiles.TryGetValue(obis, out var data))
                {
                    foreach (var d in profile.ProfileData)
                        data.ProfileData.TryAdd(d.Key, d.Value);

                    return true;
                }
            }
            return false;
        }


        #region ParseRawData
        public OBIS ParseOBIS(string input)
        {
            if (input.Contains("1.8.0"))
                return OBIS.code180;
            else if (input.Contains("1.8.1"))
                return OBIS.code181;
            else if (input.Contains("1.8.2"))
                return OBIS.code182;
            else if (input.Contains("1.8.3"))
                return OBIS.code183;
            else if (input.Contains("1.8.4"))
                return OBIS.code184;
            else
                return OBIS.code180;
        }

        public bool LoadFromRawData(string input, string separator = ";")
        {
            using (var reader = new StringReader(input))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var split = line.Split(separator);
                        if (split != null &&
                            split.Length == 99) // 3 + 24 * 4 ....date, id, obis, 15min per hour per day
                        {
                            if (DateTime.TryParse(split[0], out var date))
                            {
                                var id = split[1];
                                if (!string.IsNullOrEmpty(id))
                                {
                                    if (!MeasureSpots.TryGetValue(id, out var ms))
                                    {
                                        ms = new MeasureSpot()
                                        {
                                            Id = id,
                                            ParentId = Id
                                        };

                                        ms.Profiles.TryAdd(OBIS.code180, new DataProfile() { Name = nameof(OBIS.code180), Type = DataProfileType.AddCoeficient });
                                        ms.Profiles.TryAdd(OBIS.code182, new DataProfile() { Name = nameof(OBIS.code182), Type = DataProfileType.AddCoeficient });
                                        ms.Profiles.TryAdd(OBIS.code183, new DataProfile() { Name = nameof(OBIS.code183), Type = DataProfileType.AddCoeficient });

                                        MeasureSpots.TryAdd(id, ms);
                                    }

                                    var obis = ParseOBIS(split[2]);

                                    if (ms.Profiles.TryGetValue(obis, out var profile))
                                    {
                                        profile.ProfileData.Clear();

                                        var timespan = new TimeSpan(0, 15, 0);
                                        date = date.Add(timespan); //input data starts at 0:15 timestamp

                                        for (var i = 3; i < split.Length; i++)
                                        {
                                            if (double.TryParse(split[i].Replace(",", "."),
                                                                NumberStyles.Any,
                                                                CultureInfo.InvariantCulture,
                                                                out var val))
                                                profile.ProfileData.TryAdd(date, val);
                                            else
                                                profile.ProfileData.TryAdd(date, 0);

                                            date = date.Add(timespan);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        #endregion

    }
}
