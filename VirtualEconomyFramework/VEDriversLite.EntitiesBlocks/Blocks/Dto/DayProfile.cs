using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks.Dto
{
    public enum DayProfileType
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

    public class DayProfile
    {
        public Dictionary<DateTime, double> ProfileData { get; set; } = new Dictionary<DateTime, double>();

        public string Name { get; set; } = string.Empty;
        public DayProfileType Type { get; set; } = DayProfileType.Default;

        [JsonIgnore]
        public DateTime FirstDate { get => ProfileData.Keys.ToList().OrderBy(x => x).FirstOrDefault(); }
        [JsonIgnore]
        public DateTime LastDate { get => ProfileData.Keys.ToList().OrderByDescending(x => x).FirstOrDefault(); }

    }
}
