using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Cryptocurrencies
{
    public static class ExchangeRatesAPIFactory
    {
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
