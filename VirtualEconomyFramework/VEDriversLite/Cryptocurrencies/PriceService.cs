using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Cryptocurrencies
{
    public class PriceService
    {
        private static object _lock = new object();

        public PriceService() { }
        public PriceService(string relatedAddress, CurrencyTypes relatedAddressType)
        {
            if (!string.IsNullOrEmpty(relatedAddress))
                RelatedAddress = relatedAddress;

            RelatedAddressCurrency = relatedAddressType;
        }

        private bool isActive = false;
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        public const double MinimalAmount = 0.001;
        public string RelatedAddress { get; set; } = string.Empty;
        public PriceCalculator CurrenciesCalculator { get; set; } = new PriceCalculator();
        public CurrencyTypes RelatedAddressCurrency { get; set; } = CurrencyTypes.NEBL;

        public event EventHandler<IDictionary<CurrencyTypes,double>> PricesRefreshed;

        public IDictionary<CurrencyTypes, double> ActualCurrenciesRates = new ConcurrentDictionary<CurrencyTypes, double>();

        private IExchangeRatesAPI exchangeRatesAPI = null;

        public static double RoundOneNumber(double number, int decimals = 4)
        {
            return (Math.Round(number, decimals) > 0) ? Math.Round(number, decimals) : MinimalAmount;
        }
        public double GetPrice(CurrencyTypes type, bool round = false)
        {
            if (ActualCurrenciesRates.TryGetValue(type, out var value))
                return (round)?RoundOneNumber(value):value;
            else
                return 0.0;
        }
        public async Task InitPriceService(ExchangeRatesAPITypes typeOfAPI = ExchangeRatesAPITypes.Coingecko)
        {
            try
            {
                if (exchangeRatesAPI != null && exchangeRatesAPI.IsRefreshing)
                    return;
                exchangeRatesAPI = await ExchangeRatesAPIFactory.GetExchangeRatesAPI(typeOfAPI);
                if (exchangeRatesAPI != null)
                {
                    exchangeRatesAPI.PricesRefreshed -= ExchangeRatesAPI_PricesRefreshed;
                    exchangeRatesAPI.PricesRefreshed += ExchangeRatesAPI_PricesRefreshed;
                    await exchangeRatesAPI.StartRefreshingData();
                    if (exchangeRatesAPI.IsRefreshing)
                    {
                        IsActive = exchangeRatesAPI.IsRefreshing;
                        await RefreshPricesAsync();
                    }
                    else
                        IsActive = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot init Price service. " + ex.Message);
            }

            return;
        }

        private void ExchangeRatesAPI_PricesRefreshed(object sender, string e)
        {
            RefreshPricesAsync();
        }

        private async Task RefreshPricesAsync()
        {
            var prices = await exchangeRatesAPI.GetPrice(new CurrencyTypes[2] { CurrencyTypes.NEBL, CurrencyTypes.DOGE });
            if (prices != null)
            {
                var nup = 0.0;
                var dup = 0.0;
                foreach (var price in prices)
                {
                    if (price.Key == CurrencyTypes.NEBL && price.Value != 0)
                        nup = price.Value;
                    if (price.Key == CurrencyTypes.DOGE && price.Value != 0)
                        dup = price.Value;
                }
                lock (_lock)
                {
                    ActualCurrenciesRates.Clear();
                    ActualCurrenciesRates = prices;
                }
                await CurrenciesCalculator.SetPrices(nup, dup);

                PricesRefreshed?.Invoke(this, ActualCurrenciesRates);
            }
        }
    }
}
