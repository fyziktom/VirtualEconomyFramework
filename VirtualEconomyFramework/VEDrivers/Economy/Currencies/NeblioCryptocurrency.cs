using Binance.Net;
using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Exchanges;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy
{
    public class NeblioCryptocurrency : CommonCryptocurrency
    {
        public NeblioCryptocurrency(bool withPrice = true)
        {
            Name = "Neblio";
            Symbol = "NEBL";
            BaseURL = "https://ntp1node.nebl.io/";
            ProjectWebsite = "https://nebl.io/";
            TokensAvailable = true;
            OpenAPIAddress = "https://raw.githubusercontent.com/NeblioTeam/neblio-api-swagger-docs/master/swagger.json";

            Tokens = new List<NeblioNTP1Token>();
            KlineDataHistory = new List<IBinanceStreamKlineData>();

            if (withPrice)
            {
                // create connection to kline data of Neblio
                commonBinanceSocketClient = new BinanceSocketClient();
                binanceDataProvider = new BinanceDataProvider(commonBinanceSocketClient, "NEBLBTC", Binance.Net.Enums.KlineInterval.OneMinute);
                binanceDataProvider.OnKlineData = (data) =>
                {
                    KlineDataHistory.Add(data);
                    ActualBTCPrice = Convert.ToDouble(data.Data.CommonClose);

                //remember one hour of graph
                if (KlineDataHistory.Count > 60)
                        KlineDataHistory.RemoveAt(0);
                };
            }
            // load other detail about cryptocurrency
            try
            {
                GetDetails().GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                //todo
            }
            
        }

        private BinanceSocketClient commonBinanceSocketClient;
        private BinanceDataProvider binanceDataProvider;
        
        // Address to Neblio API Swagger OpenAPI description
        public string OpenAPIAddress { get; }

        public IEnumerable<NeblioNTP1Token> Tokens;

        public override async Task<string> GetDetails()
        {
            //Get max supply
            using var client = new HttpClient();
            var result = await client.GetStringAsync("https://explorer.nebl.io/ext/getmoneysupply");

            if (!string.IsNullOrEmpty(result))
            {
                MaxSupply = Convert.ToDouble(result);
                CirculatingSuply = MaxSupply;
            }
            else
            {
                return await Task.FromResult("ERROR");
            }

            return await Task.FromResult("OK");
        }

    }
}
