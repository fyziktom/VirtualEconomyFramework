using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Cryptocurrencies
{
    /// <summary>
    /// Price Calculator for the automated conversion
    /// </summary>
    public class PriceCalculator
    {
        private object _lock = new object();

        /// <summary>
        /// Minimal amount to use in the calculator
        /// </summary>
        public const double MinimalAmount = 0.001;

        private double neblioAmount = 1;
        /// <summary>
        /// Amount of Neblio
        /// It can be input directly and all other prices are calculated based on this input.
        /// Otherwise it offers value which is recalculated based on the other currency input.
        /// </summary>
        public double NeblioAmount
        {
            get => neblioAmount;
            set
            {
                var tmp = neblioAmount;
                if (value >= MinimalAmount)
                    neblioAmount = value;
                else
                    neblioAmount = MinimalAmount;
                if (tmp != neblioAmount)
                {
                    RecalcAmounts(CurrencyTypes.NEBL);
                    PriceEdited?.Invoke(this, CurrencyTypes.NEBL);
                }
            }
        }

        private double dogeAmount = 1;
        /// <summary>
        /// Amount of Doge
        /// It can be input directly and all other prices are calculated based on this input.
        /// Otherwise it offers value which is recalculated based on the other currency input.
        /// </summary>
        public double DogeAmount
        {
            get => dogeAmount;
            set
            {
                var tmp = dogeAmount;
                if (value >= MinimalAmount)
                    dogeAmount = value;
                else
                    dogeAmount = MinimalAmount;
                if (tmp != dogeAmount)
                {
                    RecalcAmounts(CurrencyTypes.DOGE);
                    PriceEdited?.Invoke(this, CurrencyTypes.DOGE);
                }
            }
        }

        private double usdAmount = 1;
        /// <summary>
        /// Amount of USD
        /// It can be input directly and all other prices are calculated based on this input.
        /// Otherwise it offers value which is recalculated based on the other currency input.
        /// </summary>
        public double USDAmount
        {
            get => usdAmount;
            set
            {
                var tmp = usdAmount;
                if (value >= MinimalAmount)
                    usdAmount = value;
                else
                    usdAmount = MinimalAmount;
                if (tmp != usdAmount)
                {
                    RecalcAmounts(CurrencyTypes.USD);
                    PriceEdited?.Invoke(this, CurrencyTypes.USD);
                }
            }
        }

        private double czkAmount = 1;
        /// <summary>
        /// Amount of CZK - not available now - need own API provider
        /// It can be input directly and all other prices are calculated based on this input.
        /// Otherwise it offers value which is recalculated based on the other currency input.
        /// </summary>
        public double CZKAmount
        {
            get => czkAmount;
            set
            {
                czkAmount = 0.0; //todo
                /*
                var tmp = czkAmount;
                if (value > MinimalAmount)
                    czkAmount = value;
                else
                    czkAmount = MinimalAmount;
                recalcAmounts(CurrencyTypes.CZK);
                if (tmp != czkAmount)
                  PriceEdited?.Invoke(this, CurrencyTypes.CZK);
                */
            }
        }
        /// <summary>
        /// If the Price calculator is related to some specific address it should be filled here
        /// </summary>
        public string RelatedAddress { get; set; } = string.Empty;
        /// <summary>
        /// Main Currency on the related account
        /// </summary>
        public CurrencyTypes RelatedAddressCurrency { get; set; } = CurrencyTypes.NEBL;

        /// <summary>
        /// Loaded price rate of Neblio/USD
        /// Must be loaded from external source with use of SetPrices function
        /// </summary>
        public double NeblUsdPrice = MinimalAmount;
        /// <summary>
        /// Loaded price rate of Dogecoin/USD
        /// Must be loaded from external source with use of SetPrices function
        /// </summary>
        public double DogecoinUsdPrice = MinimalAmount;

        private double neblDogecoinPrice = MinimalAmount;
        /// <summary>
        /// Loaded price rate of Neblio/Dogecoin
        /// It is calculated automatically in the SetPrices function
        /// </summary>
        public double NeblDogecoinPrice { get => (neblDogecoinPrice > MinimalAmount) ? RoundOne(neblDogecoinPrice) : MinimalAmount; }
        /// <summary>
        /// When some price is edited this event is fired and it contains the information which currency was changed
        /// </summary>
        public event EventHandler<CurrencyTypes> PriceEdited;

        /// <summary>
        /// Set prices of exchange rates. You must fill this otherwise class will not work
        /// You can use the IExchangeRatesAPI implementations as the source of these rates.
        /// If this class is used by PriceService class, the prices are loaded inside of this class from IExchangeRatesAPI provider.
        /// </summary>
        /// <param name="neblUSDPrice">Exchange rate for neblio/usd price</param>
        /// <param name="dogecoinUsdPrice">Exchange rate for dogecoin/usd price</param>
        /// <param name="recalc">set true if you want to recalculate prices after load of the new exchange rates</param>
        /// <param name="vs_curr_type">if the recal is true, you should select the vs_currency for recalculation. Default is USD</param>
        /// <returns></returns>
        public async Task SetPrices(double neblUSDPrice, double dogecoinUsdPrice, bool recalc = false, CurrencyTypes vs_curr_type = CurrencyTypes.USD)
        {
            if (NeblUsdPrice == 0 || DogecoinUsdPrice == 0) return;

            NeblUsdPrice = neblUSDPrice;
            DogecoinUsdPrice = dogecoinUsdPrice;
            neblDogecoinPrice = NeblUsdPrice / DogecoinUsdPrice;
            if (recalc)
                await RecalcAmounts(vs_curr_type);
        }

        /// <summary>
        /// If you want to add 1 to amount of some specific currency you can use this function.
        /// This is good for the +1 Button.
        /// </summary>
        /// <param name="curr">Selected currency to increment</param>
        /// <returns></returns>
        public async Task AddOneToAmount(CurrencyTypes curr)
        {
            switch (curr)
            {
                case CurrencyTypes.NEBL:
                    neblioAmount++;
                    break;
                case CurrencyTypes.DOGE:
                    dogeAmount++;
                    break;
                case CurrencyTypes.USD:
                    usdAmount++;
                    break;
                case CurrencyTypes.CZK:
                    czkAmount++;
                    break;
            }
            await RecalcAmounts(curr);
        }
        /// <summary>
        /// If you want to remove 1 to amount of some specific currency you can use this function.
        /// This is good for the -1 Button.
        /// If the price after remove one is below MinimalAmount constant it is replaced with MinimalAmount
        /// </summary>
        /// <param name="curr">Selected currency to decrement</param>
        /// <returns></returns>
        public async Task RemoveOneFromAmount(CurrencyTypes curr)
        {
            switch (curr)
            {
                case CurrencyTypes.NEBL:
                    neblioAmount--;
                    if (neblioAmount < MinimalAmount) neblioAmount = MinimalAmount;
                    break;
                case CurrencyTypes.DOGE:
                    dogeAmount--;
                    if (dogeAmount < MinimalAmount) dogeAmount = MinimalAmount;
                    break;
                case CurrencyTypes.USD:
                    usdAmount--;
                    if (usdAmount < MinimalAmount) usdAmount = MinimalAmount;
                    break;
                case CurrencyTypes.CZK:
                    czkAmount--;
                    if (czkAmount < MinimalAmount) czkAmount = MinimalAmount;
                    break;
            }
            await RecalcAmounts(curr);
        }

        /// <summary>
        /// Recalculate the amounts based on some specific currency. 
        /// For example if you will change the USD it must recalculate price of Neblio and Dogecoin. If you will change Neblio it will recalculate USD and Dogecoin, etc.
        /// </summary>
        /// <param name="curr">Selected currency as base - this currecny was changed and other should be recalculated automatically</param>
        /// <returns></returns>
        public async Task RecalcAmounts(CurrencyTypes curr = CurrencyTypes.USD)
        {
            if (curr == CurrencyTypes.CZK)
            {
                Console.WriteLine("CZK is not supported yet.");
                return;
            }
            if (NeblDogecoinPrice == 0 || NeblUsdPrice == 0 || DogecoinUsdPrice == 0 || NeblDogecoinPrice == MinimalAmount) return;
            SetDefaultPricesIfNotValid();

            switch (curr)
            {
                case CurrencyTypes.USD:
                    neblioAmount = usdAmount / NeblUsdPrice;
                    dogeAmount = usdAmount / DogecoinUsdPrice;
                    break;
                case CurrencyTypes.NEBL:
                    usdAmount = neblioAmount * NeblUsdPrice;
                    dogeAmount = neblioAmount * NeblDogecoinPrice;
                    break;
                case CurrencyTypes.DOGE:
                    neblioAmount = dogeAmount / neblDogecoinPrice;
                    usdAmount = dogeAmount * DogecoinUsdPrice;
                    break;
            }

            RoundAll();
        }

        /// <summary>
        /// Check the prices and if there is price below MinimalAmount it will replace it with the MinimalAmount
        /// </summary>
        private void SetDefaultPricesIfNotValid()
        {
            if (usdAmount < MinimalAmount) usdAmount = MinimalAmount;
            if (neblioAmount < MinimalAmount) neblioAmount = MinimalAmount;
            if (dogeAmount < MinimalAmount) dogeAmount = MinimalAmount;
        }
        /// <summary>
        /// Round or set MinimalAmount if the amound will be under it.
        /// </summary>
        /// <param name="number">Input number to round</param>
        /// <param name="decimals">Number of digits</param>
        /// <returns></returns>
        private double RoundOne(double number, int decimals = 4)
        {
            return (Math.Round(number, decimals) > 0) ? Math.Round(number, decimals) : MinimalAmount;
        }
        /// <summary>
        /// RoundOne all available currencies in the calculator
        /// </summary>
        /// <param name="decimals">Number of digits</param>
        /// <returns></returns>
        private void RoundAll(int decimals = 4)
        {
            usdAmount = RoundOne(usdAmount, decimals);
            neblioAmount = RoundOne(neblioAmount, decimals);
            dogeAmount = RoundOne(dogeAmount, decimals);
        }
    }
}
