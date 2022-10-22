using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.Enums;

namespace VEDriversLite.EntitiesBlocks.Financial
{
    public class FinancialInfo
    {
        /// <summary>
        /// Id of financial Info
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Initial unit price of the item
        /// </summary>
        public double InitialUnitPrice { get; set; } = 0.0;
        /// <summary>
        /// Type of currency
        /// </summary>
        public CurrencyTypes CurrencyType { get; set; } = CurrencyTypes.CZK;
        /// <summary>
        /// Time when the item has been bought
        /// </summary>
        public DateTime BuyDate { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Discont value.
        /// 3%/y default value
        /// </summary>
        public Discont Discont { get; set; } = new Discont() { DiscontInPercentagePerYear = 3 };
        /// <summary>
        /// Get Disconted price based on the UTC Now time.
        /// </summary>
        public double DiscontedPrice { get => Discont.GetDiscontedValue(InitialUnitPrice, BuyDate, DateTime.UtcNow); }

        /// <summary>
        /// Get disconted price based on the end time
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public double GetDiscontedValue(DateTime end)
        {
            return Discont.GetDiscontedValue(InitialUnitPrice, BuyDate, end);
        }

    }
}
