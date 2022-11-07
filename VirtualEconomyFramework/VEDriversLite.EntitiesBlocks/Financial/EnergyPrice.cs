using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.Enums;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Consumers.Measuring;

namespace VEDriversLite.EntitiesBlocks.Financial
{
    public class EnergyPrice
    {
        /// <summary>
        /// Minimum price in timeframe
        /// </summary>
        public double MinPrice { get; set; } = 10.0;
        /// <summary>
        /// Maximum price in timeframe
        /// </summary>
        public double MaxPrice { get; set; } = 10.0;
        /// <summary>
        /// Open Price in timeframe
        /// </summary>
        public double OpenPrice { get; set; } = 10.0;
        /// <summary>
        /// Close price in timeframe
        /// </summary>
        public double ClosePrice { get; set; } = 10.0;
        /// <summary>
        /// Average price between min and max
        /// </summary>
        public double AveragePrice { get => MinPrice + ((MaxPrice - MinPrice) / 2); }
        /// <summary>
        /// Time of Opening for this price
        /// </summary>
        public DateTime OpeningTime { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Timeframe of spot price of energy
        /// </summary>
        public BlockTimeframe TimeFrame { get; set; } = BlockTimeframe.Hour;
        /// <summary>
        /// Amount of the energy in kWh related to this price
        /// </summary>
        public double Amount { get; set; } = 1.0;
        /// <summary>
        /// Price per one unit if the Amount is 1.0. it is same as Price
        /// </summary>
        public double PricePerOneUnit { get => Math.Abs(Amount) > 0 ? AveragePrice / Amount : 0; }
        /// <summary>
        /// Returns true if the Amount is equal 1.0 => it means it is unit price
        /// </summary>
        public bool IsUnitPrice { get => Amount == 1.0; }
        /// <summary>
        /// Currency type
        /// </summary>
        public CurrencyTypes Currency { get; set; } = CurrencyTypes.CZK;
        /// <summary>
        /// Type of energy tarrif
        /// </summary>
        public OBIS Tarrif { get; set; } = OBIS.code181;
    }
}
