using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Cryptocurrencies.Dto;

namespace VEDriversLite.Cryptocurrencies
{
    /// <summary>
    /// Common class structure for ExchangeRatesAPI
    /// </summary>
    public abstract class CommonExchangeRatesAPI : IExchangeRatesAPI
    {
        /// <summary>
        /// Type of the Exchange provider
        /// </summary>
        public ExchangeRatesAPITypes Type { get; set; } = ExchangeRatesAPITypes.Test;
        /// <summary>
        /// Name of the provider
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Main API url of the provider
        /// </summary>
        public string APIUrl { get; set; } = string.Empty;
        /// <summary>
        /// If the exchange rates provider is in autorefreshing mode this is true
        /// </summary>
        public bool IsRefreshing { get; set; } = false;
        /// <summary>
        /// Last state of the prices
        /// Key is the pair like neblio/usd or dogecoin/usd
        /// </summary>
        public ConcurrentDictionary<string, PriceDto> Prices { get; set; } = new ConcurrentDictionary<string, PriceDto>();
        /// <summary>
        /// Available pairs in the Prices dictionary
        /// For example neblio/usd, dogecoin/usd, etc.
        /// Please check this list if you need key for the dictionary to get exact pair
        /// </summary>
        public IEnumerable<string> AvailablePairsData { get => Prices.Keys; }
        /// <summary>
        /// This event fires when prices are successfully refreshed
        /// </summary>
        public event EventHandler<string> PricesRefreshed;

        private System.Threading.Timer priceRefreshTimer;

        /// <summary>
        /// Return prices from the API
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> GetPriceFromAPI();

        /// <summary>
        /// Get price from the list of last refreshed prices.
        /// You can obtain more currencies against one vs_currency.
        /// If the prices have not been loaded yet, it will try to obtain them from the API.
        /// </summary>
        /// <param name="currencies">Array of the currency prices what you want to obtain against the vs_currency. 
        /// For example input new CurrencyTypes[2]{ CurrencyTypes.NEBL, CurrencyTypes.DOGE }</param>
        /// <param name="vs_currency">All currencies requested are related to this vs_currency. If you will not fill this parameter. the USD is selected as default</param>
        /// <returns>Dictionary of the prices of inputed currencies</returns>
        public virtual async Task<IDictionary<CurrencyTypes, double>> GetPrice(CurrencyTypes[] currencies, CurrencyTypes vs_currency = CurrencyTypes.USD)
        {
            if (currencies == null) return null;

            var res = new Dictionary<CurrencyTypes, double>();
            if (!IsRefreshing)
                await GetPriceFromAPI();

            foreach (var curr in currencies)
            {
                if (Prices.TryGetValue(PriceDto.GetName(curr, vs_currency), out var price))
                    res.Add(curr, price.Value);
                else
                    Console.WriteLine("Cannot obtain data from the API or not supported Cryptocurrency - " + curr.ToString());
            }
            return res;
        }
        /// <summary>
        /// Start auto refreshing of the data. It is protected with checking the IsRefreshing to prevent double start.
        /// </summary>
        /// <param name="period">You can change the period - default 30000ms</param>
        /// <returns>true if successful start</returns>
        public virtual async Task<bool> StartRefreshingData(int period = 30000)
        {
            if (await GetPriceFromAPI())
                PricesRefreshed?.Invoke(this, "Price Data Refreshed.");
            if (priceRefreshTimer != null)
                await priceRefreshTimer.DisposeAsync();

            priceRefreshTimer = new Timer(async (object stateInfo) =>
            {
                try
                {
                    if (await GetPriceFromAPI())
                        PricesRefreshed?.Invoke(this, "Price Data Refreshed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot read tx details. " + ex.Message);
                }
            }, new System.Threading.AutoResetEvent(false), period, period);

            IsRefreshing = true;
            return true;
        }
        /// <summary>
        /// Stop the refreshing of the data
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> StopRefreshingData()
        {
            if (priceRefreshTimer != null)
                await priceRefreshTimer.DisposeAsync();
            IsRefreshing = false;
            return true;
        }
        /// <summary>
        /// Stop the auto refreshing of the data.
        /// </summary>
        /// <returns>true if successful stop</returns>
        public virtual void Dispose()
        {
            Prices.Clear();
            Prices = null;
            Name = null;
            APIUrl = null;
            if (priceRefreshTimer != null)
                priceRefreshTimer.Dispose();
            IsRefreshing = false;
        }
    }
}
