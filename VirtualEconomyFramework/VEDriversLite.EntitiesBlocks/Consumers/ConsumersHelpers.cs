using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Consumers
{
    public static class ConsumersHelpers
    {
        /// <summary>
        /// input format: each line one hour in day
        /// line has 18 columns: yyyy.MM.dd;hh;hh in year;tdd1;tdd2;tdd3;tdd4;tdd5-1;tdd5-2;tdd5-3;tdd5-4;tdd5-5;tdd5-6;tdd5-6;tdd5-7;tdd5-8;tdd6;tdd7;tdd8
        /// 
        /// Example of input lines:
        /// .....
        /// 01.01.2022;22;22;0,42604;0,46294;0,57803;0,60948;0,61215;0,44617;0,34225;0,57982;0,46306;0,54786;0,35975;0,40155;0,91362;0,73156;1,00000
        /// 01.01.2022;23;23;0,39899;0,50389;0,55672;0,48849;0,70811;0,65061;0,30351;0,52555;0,46054;0,55144;0,42253;0,52240;0,78920;0,73691;1,00000
        /// 01.01.2022;24;24;0,38224;0,55918;0,54215;0,37810;0,49756;0,55069;0,29684;0,44409;0,36893;0,65560;0,43412;0,59024;0,66708;0,67479;1,00000
        /// 02.01.2022;1;25;0,36637;0,63786;0,53699;0,33128;0,31026;0,43343;0,29851;0,33727;0,45689;0,46314;0,29984;0,43113;0,58937;0,57421;1,00000
        /// 02.01.2022;2;26;0,35432;0,65417;0,51171;0,28848;0,25181;0,66867;0,28657;0,28206;0,33987;0,65655;0,36456;0,33223;0,53710;0,53811;1,00000
        /// .....
        /// </summary>
        /// <param name="input"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<DataProfile> LoadTDDs(string input, string separator = ";")
        {
            var result = new List<DataProfile>()
            {
             new DataProfile() { Name = "TDD1", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD2", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD3", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD4", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-1", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-2", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-3", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-4", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-5", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-6", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-7", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD5-8", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD6", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD7", Type = DataProfileType.MultiplyCoeficient },
             new DataProfile() { Name = "TDD8", Type = DataProfileType.MultiplyCoeficient },
            };

            using (var reader = new StringReader(input))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var split = line.Split(separator);
                        if (split != null &&
                            split.Length == 18)
                        {
                            if (DateTime.TryParse(split[0], out var date) &&
                                int.TryParse(split[1], out var hour))
                            {
                                date = date.AddHours(hour - 1);

                                var index = 3;
                                foreach (var res in result)
                                {
                                    if (index < split.Length)
                                    {
                                        if (double.TryParse(split[index].Replace(",", "."),
                                                            NumberStyles.Any,
                                                            CultureInfo.InvariantCulture,
                                                            out var tdd))
                                            res.ProfileData.TryAdd(date, tdd);
                                    }
                                    index++;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static (List<IBlock>, DataProfile) GetConsumptionBlocksBasedOnTDD(DataProfile tdd, DateTime start, DateTime end, BlockTimeframe timeframe, string parentId)
        {
            var data = GetConsumptionBasedOnTDD(tdd, start, end, timeframe);
            if (data != null)
            {
                var list = DataProfileHelpers.ConvertDataProfileToBlocks(data,
                                                                         BlockDirection.Consumed,
                                                                         BlockType.Simulated,
                                                                         parentId,
                                                                         0,
                                                                         false,
                                                                         "",
                                                                         "",
                                                                         data.Id).ToList();

                return (list, data);
            }
            return (new List<IBlock>(), new DataProfile());
        }

        public static DataProfile GetConsumptionBasedOnTDD(DataProfile tdd, DateTime start, DateTime end, BlockTimeframe timeframe)
        {
            var result = new DataProfile();

            var tmp = start;
            var ts = BlockHelpers.GetTimeSpanBasedOntimeframe(timeframe, start);

            while (tmp < end)
            {
                var tddend = tmp + ts;
                var tddtime = tmp;

                var addvalue = 0.0;
                while (tddtime < tddend)
                {
                    if (tdd.ProfileData.TryGetValue(tddtime, out var tddvalue))
                        addvalue += tddvalue;

                    tddtime = tddtime.AddHours(1);
                }

                result.ProfileData.TryAdd(tmp, addvalue);

                tmp += ts;
            }

            return result;
        }
    }
}
