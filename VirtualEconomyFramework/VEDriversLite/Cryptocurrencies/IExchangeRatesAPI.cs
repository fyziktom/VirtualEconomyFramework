using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Cryptocurrencies.Dto;

namespace VEDriversLite.Cryptocurrencies
{
    /// <summary>
    /// Types of supported currencies
    /// </summary>
    public enum CurrencyTypes
    {
        /// <summary>
        /// Neblio
        /// </summary>
        NEBL,
        /// <summary>
        /// Dogecoin
        /// </summary>
        DOGE,
        /// <summary>
        /// United States Dollar
        /// </summary>
        USD,
        /// <summary>
        /// Czech Crown
        /// </summary>
        CZK
    }
    /// <summary>
    /// Api exchange rates providers
    /// </summary>
    public enum ExchangeRatesAPITypes
    {
        /// <summary>
        /// Coingecko
        /// </summary>
        Coingecko,
        /// <summary>
        /// Binance
        /// </summary>
        Binance,
        /// <summary>
        /// Test/mock
        /// </summary>
        Test
    }
    /// <summary>
    /// Main interface for Exchage API service
    /// </summary>
    public interface IExchangeRatesAPI : IDisposable
    {
        /// <summary>
        /// Type of the Exchange provider
        /// </summary>
        ExchangeRatesAPITypes Type { get; set; }
        /// <summary>
        /// Name of the provider
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Main API url of the provider
        /// </summary>
        string APIUrl { get; set; }
        /// <summary>
        /// If the exchange rates provider is in autorefreshing mode this is true
        /// </summary>
        bool IsRefreshing { get; set; }
        /// <summary>
        /// Last state of the prices
        /// Key is the pair like neblio/usd or dogecoin/usd
        /// </summary>
        ConcurrentDictionary<string, PriceDto> Prices { get; set; }
        /// <summary>
        /// Available pairs in the Prices dictionary
        /// For example neblio/usd, dogecoin/usd, etc.
        /// Please check this list if you need key for the dictionary to get exact pair
        /// </summary>
        IEnumerable<string> AvailablePairsData { get; }
        /// <summary>
        /// This event fires when prices are successfully refreshed
        /// </summary>
        event EventHandler<string> PricesRefreshed;

        /// <summary>
        /// Return prices from the API
        /// </summary>
        /// <returns></returns>
        Task<bool> GetPriceFromAPI();
        /// <summary>
        /// Get price from the list of last refreshed prices.
        /// You can obtain more currencies against one vs_currency.
        /// If the prices have not been loaded yet, it will try to obtain them from the API.
        /// </summary>
        /// <param name="currencies">Array of the currency prices what you want to obtain against the vs_currency. 
        /// For example input new CurrencyTypes[2]{ CurrencyTypes.NEBL, CurrencyTypes.DOGE }</param>
        /// <param name="vs_currency">All currencies requested are related to this vs_currency. If you will not fill this parameter. the USD is selected as default</param>
        /// <returns>Dictionary of the prices of inputed currencies</returns>
        Task<IDictionary<CurrencyTypes, double>> GetPrice(CurrencyTypes[] currencies, CurrencyTypes vs_currency = CurrencyTypes.USD);
        /// <summary>
        /// Start auto refreshing of the data. It is protected with checking the IsRefreshing to prevent double start.
        /// </summary>
        /// <param name="period">You can change the period - default 30000ms</param>
        /// <returns>true if successful start</returns>
        Task<bool> StartRefreshingData(int period = 30000);
        /// <summary>
        /// Stop the auto refreshing of the data.
        /// </summary>
        /// <returns>true if successful stop</returns>
        Task<bool> StopRefreshingData();
        /// <summary>
        /// Dispose the Exchange rates API object.
        /// If the price refresh timer is running it is disposed too.
        /// </summary>
        void Dispose();
    }
}
