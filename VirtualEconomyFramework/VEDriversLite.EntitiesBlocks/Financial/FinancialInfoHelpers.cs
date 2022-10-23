using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.Enums;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Financial
{
    public static class FinancialInfoHelpers
    {
        /// <summary>
        /// Statistic value of price for calculation costs of the PVE per Wp
        /// </summary>
        public static double AvgPricePerWPeakForPVE { get; set; } = 1.26;
        /// <summary>
        /// Statistic value of price for calculation costs of the Storage per W capacity
        /// </summary>
        public static double AvgPricePerWCapacityForStorage { get; set; } = 0.2;


        /// <summary>
        /// Load Energy prices list from DataProfile
        /// The timeframe is approximated/found based on the timespan between data samples in the DataProfile
        /// </summary>
        /// <param name="inputProfile"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static IEnumerable<EnergyPrice> GetEnergyPricesFromDataProfile(DataProfile inputProfile, CurrencyTypes currency)
        {
            var laststeptime = inputProfile.FirstDate;
            if (inputProfile.ProfileData.Count > 1)
                laststeptime = inputProfile.FirstDate - (inputProfile.ProfileData.Keys.Skip(1).Take(1).First() - inputProfile.FirstDate);

            foreach (var data in inputProfile.ProfileData)
            {
                var energyPrice = new EnergyPrice()
                {
                    Amount = 1.0,
                    OpeningTime = data.Key,
                    Currency = currency,
                    MaxPrice = data.Value,
                    TimeFrame = BlockHelpers.GetTimeframeBasedOnTimespan(data.Key - laststeptime)
                };
                laststeptime = data.Key;

                yield return energyPrice;
            }
        }
    }
}
