using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Cryptocurrencies.Dto;

namespace VEDriversLite.Cryptocurrencies.CoingeckoAPI
{
    /// <summary>
    /// Dto for Usd price
    /// </summary>
    public class CoingeckoUsdPriceDto
    {
        /// <summary>
        /// USD price
        /// </summary>
        public double usd { get; set; }
    }
    /// <summary>
    /// Class for deserialization the data from the Coingecko
    /// It works now just for the neblio and dogecoin
    /// </summary>
    public class CoingeckoDto
    {
        /// <summary>
        /// Neblio price against the USD
        /// </summary>
        public CoingeckoUsdPriceDto neblio { get; set; } = new CoingeckoUsdPriceDto();
        /// <summary>
        /// Dogecoin price against the USD
        /// </summary>
        public CoingeckoUsdPriceDto dogecoin { get; set; } = new CoingeckoUsdPriceDto();
    }
    /// <summary>
    /// Coingecko Excange Rates API
    /// </summary>
    public class CoingeckoExchangeRatesAPI : CommonExchangeRatesAPI, IDisposable
    {
        /// <summary>
        /// Main constructor, fill Name to "Coingecko", basic APIUrl and type of class to Coingecko
        /// </summary>
        public CoingeckoExchangeRatesAPI()
        {
            Name = "Coingecko";
            APIUrl = "https://api.coingecko.com/api/v3";
            Type  = ExchangeRatesAPITypes.Coingecko;
        }

        /// <summary>
        /// Return prices from the API
        /// Now it obtains just the prices for neblio and dogecoin against USD
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> GetPriceFromAPI()
        {
            try
            {
                HttpClient _client = new HttpClient();
                var currency = "neblio,dogecoin";
                var vs_currency = "usd";
                var req = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/simple/price?ids={currency}&vs_currencies={vs_currency}");
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("mode", "no-cors");

                HttpResponseMessage resp = null;
                try
                {
                    resp = await _client.SendAsync(req);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error during sending request to Coingecko API. " + ex.Message);
                }
                var respmsg = string.Empty;
                try
                {
                    if (resp != null && resp.IsSuccessStatusCode)
                        if (resp.Content != null)
                            respmsg = await resp.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during reading response message from Coingecko API. " + ex.Message);
                }

                if (string.IsNullOrEmpty(respmsg))
                {
                    //Console.WriteLine("Cannot obtain or parse response message. Exiting the Coingecko read API function. " + ((resp != null)?resp.StatusCode.GetTypeCode().ToString():string.Empty) );
                    return (false);
                }

                try
                {
                    var prices = JsonConvert.DeserializeObject<CoingeckoDto>(respmsg);
                    if (prices != null)
                    {
                        var nebl = prices.neblio.usd;
                        var doge = prices.dogecoin.usd;
                        var nebldoge = 0.0;
                        if (doge > 0) nebldoge = nebl / doge;
                        if (Prices.TryGetValue("neblio/usd", out var nup))
                            nup.Value = nebl;
                        else 
                        {
                            Prices.TryAdd("neblio/usd", new Dto.PriceDto()
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
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot deserialize the price. " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot get the price from the Coingecko API. " + ex.Message);
            }

            return false;
        }
    }
}
