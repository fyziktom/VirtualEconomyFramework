using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Cryptocurrencies
{
    /// <summary>
    /// Price service class.
    /// This service can load the exchange rates, calculate conversion, etc.
    /// </summary>
    public class PriceService
    {
        private static object _lock = new object();

        /// <summary>
        /// Main empty constructor
        /// </summary>
        public PriceService() { }
        /// <summary>
        /// Constructor for specific address
        /// </summary>
        /// <param name="relatedAddress"></param>
        /// <param name="relatedAddressType"></param>
        public PriceService(string relatedAddress, CurrencyTypes relatedAddressType)
        {
            if (!string.IsNullOrEmpty(relatedAddress))
                RelatedAddress = relatedAddress;

            RelatedAddressCurrency = relatedAddressType;
        }

        private bool isActive = false;
        /// <summary>
        /// If the PriceService is Active this is set to true
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        /// <summary>
        /// Minimal amount to use in the PriceService
        /// </summary>
        public const double MinimalAmount = 0.001;
        /// <summary>
        /// If the PriceService is related to some specific address it should be filled here
        /// </summary>
        public string RelatedAddress { get; set; } = string.Empty;
        /// <summary>
        /// Main Currency on the related account
        /// </summary>
        public CurrencyTypes RelatedAddressCurrency { get; set; } = CurrencyTypes.NEBL;
        /// <summary>
        /// PriceCalculator instance
        /// </summary>
        public PriceCalculator CurrenciesCalculator { get; set; } = new PriceCalculator();
        /// <summary>
        /// Actual Currency rates against the USD
        /// </summary>
        public IDictionary<CurrencyTypes, double> ActualCurrenciesRates = new ConcurrentDictionary<CurrencyTypes, double>();
        /// <summary>
        /// When the prices have been refreshed this event is fired.
        /// </summary>
        public event EventHandler<IDictionary<CurrencyTypes, double>> PricesRefreshed;

        private IExchangeRatesAPI exchangeRatesAPI = null;

        /// <summary>
        /// Round or set MinimalAmount if the amound will be under it.
        /// </summary>
        /// <param name="number">Input number to round</param>
        /// <param name="decimals">Number of digits</param>
        /// <returns></returns>
        public static double RoundOneNumber(double number, int decimals = 4)
        {
            return (Math.Round(number, decimals) > 0) ? Math.Round(number, decimals) : MinimalAmount;
        }
        /// <summary>
        /// Return specific selected Currency rate against the USD
        /// </summary>
        /// <param name="type">CurrencyType</param>
        /// <param name="round">Round it with RoundOneNumber function</param>
        /// <returns></returns>
        public double GetPrice(CurrencyTypes type, bool round = false)
        {
            if (ActualCurrenciesRates.TryGetValue(type, out var value))
                return (round)?RoundOneNumber(value):value;
            else
                return 0.0;
        }
        /// <summary>
        /// Initialize the PriceService. It will load and init the Exchange Rates API provider as well
        /// </summary>
        /// <param name="typeOfAPI">Type of used IExchangeRatesAPI provider</param>
        /// <param name="relatedAddress">Related address for this PriceService</param>
        /// <param name="relatedAddressType">Related address currency for this PriceService</param>
        /// <returns></returns>
        public async Task InitPriceService(ExchangeRatesAPITypes typeOfAPI = ExchangeRatesAPITypes.Coingecko, string relatedAddress = "", CurrencyTypes relatedAddressType = CurrencyTypes.NEBL)
        {
            try
            {
                if (exchangeRatesAPI != null && exchangeRatesAPI.IsRefreshing)
                    return;
                if (!string.IsNullOrEmpty(relatedAddress))
                {
                    CurrenciesCalculator.RelatedAddress = relatedAddress;
                    CurrenciesCalculator.RelatedAddressCurrency = relatedAddressType;
                }
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

        /// <summary>
        /// This function will refresh the prices in the ActualCurrenciesRates dictionary
        /// It also set the prices into PriceCalculator instance and fires the PriceRefreshed Event.
        /// </summary>
        /// <returns></returns>
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
