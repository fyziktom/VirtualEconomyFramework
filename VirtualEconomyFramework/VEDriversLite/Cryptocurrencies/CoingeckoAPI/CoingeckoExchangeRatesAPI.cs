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
    public class CoingeckoUsdPriceDto
    {
        public double usd { get; set; }
    }
    public class CoingeckoDto
    {
        public CoingeckoUsdPriceDto neblio { get; set; } = new CoingeckoUsdPriceDto();
        public CoingeckoUsdPriceDto dogecoin { get; set; } = new CoingeckoUsdPriceDto();
    }
    public class CoingeckoExchangeRatesAPI : CommonExchangeRatesAPI, IDisposable
    {
        public CoingeckoExchangeRatesAPI()
        {
            Name = "Coingecko";
            APIUrl = "https://api.coingecko.com/api/v3";
            Type  = ExchangeRatesAPITypes.Coingecko;
        }

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
                    respmsg = await resp.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during reading response message from Coingecko API. " + ex.Message);
                }
                if (string.IsNullOrEmpty(respmsg))
                {
                    Console.WriteLine("Cannot obtain or parse response message. Exiting the Coingecko read API function. " + ((resp != null)?resp.StatusCode.GetTypeCode().ToString():string.Empty) );
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
