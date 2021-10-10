using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Cryptocurrencies.Dto;

namespace VEDriversLite.Cryptocurrencies
{
    public abstract class CommonExchangeRatesAPI : IExchangeRatesAPI, IDisposable
    {
        public ExchangeRatesAPITypes Type { get; set; } = ExchangeRatesAPITypes.Test;
        public string Name { get; set; } = string.Empty;
        public string APIUrl { get; set; } = string.Empty;
        public bool IsRefreshing { get; set; } = false;
        public ConcurrentDictionary<string, PriceDto> Prices { get; set; } = new ConcurrentDictionary<string, PriceDto>();
        public IEnumerable<string> AvailablePairsData { get => Prices.Keys; }

        public event EventHandler<string> PricesRefreshed;

        private System.Threading.Timer priceRefreshTimer;

        public abstract Task<bool> GetPriceFromAPI();

        public virtual async Task<IDictionary<CurrencyTypes, double>> GetPrice(CurrencyTypes[] currencies, CurrencyTypes vs_currency = CurrencyTypes.USD)
        {
            if (currencies == null || vs_currency == null) return null;

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
        public virtual async Task<bool> StopRefreshingData()
        {
            if (priceRefreshTimer != null)
                await priceRefreshTimer.DisposeAsync();
            IsRefreshing = false;
            return true;
        }

        public virtual void Dispose()
        {
            if (priceRefreshTimer != null)
                priceRefreshTimer.Dispose();
            IsRefreshing = false;
        }
    }
}
