using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Cryptocurrencies.Dto;

namespace VEDriversLite.Cryptocurrencies
{
    /// <summary>
    /// Mock of the API
    /// </summary>
    public class TestExchangeRatesAPI : CommonExchangeRatesAPI, IDisposable
    {
        /// <summary>
        /// Create Test Exchange Rates API
        /// </summary>
        public TestExchangeRatesAPI()
        {
            Name = "Test";
            APIUrl = "https://api.coingecko.com/api/v3";
            Type = ExchangeRatesAPITypes.Test;
        }

        /// <summary>
        /// Get Fake prices from the API
        /// 150 USD/ 1 NEBL
        /// 1.5 USD/ 1 DOGE
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> GetPriceFromAPI()
        {
            var nebl = 150;
            var doge = 1.5;
            var nebldoge = 0.0;
            if (doge > 0) nebldoge = nebl / doge;
            if (Prices.TryGetValue("nebl/usd", out var nup))
                nup.Value = nebl;
            else
            {
                Prices.TryAdd("nebl/usd", new Dto.PriceDto()
                {
                    Currency = CurrencyTypes.NEBL,
                    VS_Currency = CurrencyTypes.USD,
                    Value = nebl
                });
            }
            if (Prices.TryGetValue("dogecoin/usd", out var dup))
                dup.Value = doge;
            else
            {
                Prices.TryAdd("dogecoin/usd", new Dto.PriceDto()
                {
                    Currency = CurrencyTypes.DOGE,
                    VS_Currency = CurrencyTypes.USD,
                    Value = doge
                });
            }
            if (Prices.TryGetValue("nebl/dogecoin", out var ndp))
                ndp.Value = nebldoge;
            else
            {
                Prices.TryAdd("nebl/dogecoin", new Dto.PriceDto()
                {
                    Currency = CurrencyTypes.NEBL,
                    VS_Currency = CurrencyTypes.DOGE,
                    Value = nebldoge
                });
            }

            return true;
        }
           
    }
}
