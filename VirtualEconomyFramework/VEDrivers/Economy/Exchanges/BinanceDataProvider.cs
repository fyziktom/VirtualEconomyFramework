using Binance.Net.Enums;
using Binance.Net.Interfaces;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Exchanges
{
    public interface IBinanceDataProvider
    {
        IBinanceStreamKlineData LastKline { get; }
        Action<IBinanceStreamKlineData> OnKlineData { get; set; }

        Task Start(string pricepairname, KlineInterval kli);
        Task Stop();
    }

    public class BinanceDataProvider : IBinanceDataProvider
    {
        private IBinanceSocketClient _socketClient;
        private UpdateSubscription _subscription;

        public IBinanceStreamKlineData LastKline { get; private set; }
        public Action<IBinanceStreamKlineData> OnKlineData { get; set; }

        public BinanceDataProvider(IBinanceSocketClient socketClient, string pricepairname = "NEBLBTC", KlineInterval kli = KlineInterval.OneMinute)
        {
            _socketClient = socketClient;

            Start(pricepairname, kli).Wait(); // Probably want to do this in some initialization step at application startup
        }

        public async Task Start(string pricepairname = "NEBLBTC", KlineInterval kli = KlineInterval.OneMinute)
        {
            var subResult = await _socketClient.Spot.SubscribeToKlineUpdatesAsync(pricepairname, kli, data =>
            {
                LastKline = data;
                OnKlineData?.Invoke(data);
            });
            if (subResult.Success)
                _subscription = subResult.Data;
        }

        public async Task Stop()
        {
            await _socketClient.Unsubscribe(_subscription);
        }
    }
}
