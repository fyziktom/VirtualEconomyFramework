using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Cryptocurrencies.Dto;

namespace VEDriversLite.Cryptocurrencies
{
    public enum CurrencyTypes
    {
        NEBL,
        DOGE,
        USD,
        CZK
    }
    public enum ExchangeRatesAPITypes
    {
        Coingecko,
        Binance,
        Test
    }
    public interface IExchangeRatesAPI
    {
        ExchangeRatesAPITypes Type { get; set; }
        string Name { get; set; }
        string APIUrl { get; set; }
        bool IsRefreshing { get; set; }
        ConcurrentDictionary<string, PriceDto> Prices { get; set; }
        IEnumerable<string> AvailablePairsData { get; }
        event EventHandler<string> PricesRefreshed;

        Task<bool> GetPriceFromAPI();
        Task<IDictionary<CurrencyTypes, double>> GetPrice(CurrencyTypes[] currencies, CurrencyTypes vs_currency = CurrencyTypes.USD);
        Task<bool> StartRefreshingData(int period = 30000);
        Task<bool> StopRefreshingData();
    }
}
