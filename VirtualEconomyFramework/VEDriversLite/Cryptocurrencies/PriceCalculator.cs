using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Cryptocurrencies
{
    public class PriceCalculator
    {
        private object _lock = new object();

        public const double MinimalAmount = 0.001;

        private double neblioAmount = 1;
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

        public string RelatedAddress { get; set; } = string.Empty;
        public CurrencyTypes RelatedAddressCurrency { get; set; } = CurrencyTypes.NEBL;

        public double NeblUsdPrice = MinimalAmount;
        public double DogecoinUsdPrice = MinimalAmount;

        private double neblDogecoinPrice = MinimalAmount;
        public double NeblDogecoinPrice { get => (neblDogecoinPrice > MinimalAmount) ? RoundOne(neblDogecoinPrice) : MinimalAmount; }

        public event EventHandler<CurrencyTypes> PriceEdited;


        public async Task SetPrices(double neblUSDPrice, double dogecoinUsdPrice, bool recalc = false, CurrencyTypes vs_curr_type = CurrencyTypes.USD)
        {
            if (NeblUsdPrice == 0 || DogecoinUsdPrice == 0) return;

            NeblUsdPrice = neblUSDPrice;
            DogecoinUsdPrice = dogecoinUsdPrice;
            neblDogecoinPrice = NeblUsdPrice / DogecoinUsdPrice;
            if (recalc)
                await RecalcAmounts(vs_curr_type);
        }

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

        private void SetDefaultPricesIfNotValid()
        {
            if (usdAmount <= 0) usdAmount = MinimalAmount;
            if (neblioAmount <= 0) neblioAmount = MinimalAmount;
            if (dogeAmount <= 0) dogeAmount = MinimalAmount;
        }
        private double RoundOne(double number, int decimals = 4)
        {
            return (Math.Round(number, decimals) > 0) ? Math.Round(number, decimals) : MinimalAmount;
        }
        private void RoundAll(int decimals = 4)
        {
            usdAmount = RoundOne(usdAmount, decimals);
            neblioAmount = RoundOne(neblioAmount, decimals);
            dogeAmount = RoundOne(dogeAmount, decimals);
        }
    }
}
