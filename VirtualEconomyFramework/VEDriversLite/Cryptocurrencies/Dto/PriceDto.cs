using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Cryptocurrencies.Dto
{
    /// <summary>
    /// Common Price Dto
    /// </summary>
    public class PriceDto
    {
        /// <summary>
        /// Main currency
        /// </summary>
        public CurrencyTypes Currency { get; set; } = CurrencyTypes.NEBL;
        /// <summary>
        /// Related currency
        /// It means Currency / VS_Currency, for example NEBL/USD
        /// </summary>
        public CurrencyTypes VS_Currency { get; set; } = CurrencyTypes.USD;
        /// <summary>
        /// Text name - label of the currency/vs_currency
        /// </summary>
        public string TextName
        {
            get => GetName(Currency, VS_Currency);
        }

        /// <summary>
        /// Actual value
        /// </summary>
        public double Value { get; set; } = 0.0;
        /// <summary>
        /// Function ofr get name of currency/vs_currency
        /// </summary>
        /// <param name="curr"></param>
        /// <param name="vscurr"></param>
        /// <returns></returns>
        public static string GetName(CurrencyTypes curr, CurrencyTypes vscurr)
        {
            var first = "neblio";
            switch (curr)
            {
                case CurrencyTypes.NEBL:
                    first = "neblio";
                    break;
                case CurrencyTypes.DOGE:
                    first = "dogecoin";
                    break;
                case CurrencyTypes.USD:
                    first = "usd";
                    break;
            }
            var second = "usd";
            switch (vscurr)
            {
                case CurrencyTypes.NEBL:
                    second = "neblio";
                    break;
                case CurrencyTypes.DOGE:
                    second = "dogecoin";
                    break;
                case CurrencyTypes.USD:
                    second = "usd";
                    break;
            }
            return $"{first}/{second}";
        }
    }
}
