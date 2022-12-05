using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks.Dto
{
    /// <summary>
    /// Type of the action of the profile data to some other value
    /// </summary>
    public enum DataProfileType
    {
        Default,
        MultiplyCoeficient,
        AddCoeficient,
        SubtractCoeficient,
        DivideCoeficient,
        EfficiencyCoeficient,
        WeatherCoeficient,
        DirtCoeficient,
    }
    /// <summary>
    /// Data profile with the values for recalculate some blocks amounts
    /// for example profile of the weather coeficient
    /// </summary>
    public class DataProfile
    {
        /// <summary>
        /// Set of the profile data identified by DateTime as key
        /// </summary>
        public Dictionary<DateTime, double> ProfileData { get; set; } = new Dictionary<DateTime, double>();
        /// <summary>
        /// Name of the profile
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Id of Data profile
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Type of the profile data action
        /// </summary>
        public DataProfileType Type { get; set; } = DataProfileType.Default;
        /// <summary>
        /// First date in the profile data values
        /// </summary>
        [JsonIgnore]
        public DateTime FirstDate { get => ProfileData.Keys.OrderBy(x => x).FirstOrDefault(); }
        /// <summary>
        /// Last date in the profile data values
        /// </summary>
        [JsonIgnore]
        public DateTime LastDate { get => ProfileData.Keys.OrderByDescending(x => x).FirstOrDefault(); }

        /// <summary>
        /// Sum of all data in ProfileData
        /// </summary>
        [JsonIgnore]
        public double DataSum { get => ProfileData.Values.Sum(); }

    }
}
