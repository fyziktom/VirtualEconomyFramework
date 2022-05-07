using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Cryptocurrencies
{
    /// <summary>
    /// Factory for creating the cryptocurrency API client.
    /// </summary>
    public static class ExchangeRatesAPIFactory
    {
        /// <summary>
        /// Function will return the Exchange Rates API driver based on the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task<IExchangeRatesAPI> GetExchangeRatesAPI(ExchangeRatesAPITypes type)
        {
            IExchangeRatesAPI api = null;
            switch (type)
            {
                case ExchangeRatesAPITypes.Coingecko:
                    api = new CoingeckoAPI.CoingeckoExchangeRatesAPI();
                    break;
                case ExchangeRatesAPITypes.Binance:
                    api = null;
                    break;
                case ExchangeRatesAPITypes.Test:
                    api = new TestExchangeRatesAPI();
                    break;
            }
            return api;
        }
    }
}
