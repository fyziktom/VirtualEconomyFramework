using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Cryptocurrencies.Dto
{
    public class PriceDto
    {
        public CurrencyTypes Currency { get; set; } = CurrencyTypes.NEBL;
        public CurrencyTypes VS_Currency { get; set; } = CurrencyTypes.USD;
        public string TextName
        {
            get => GetName(Currency, VS_Currency);
        }

        public double Value { get; set; } = 0.0;

        public static string GetName(CurrencyTypes curr, CurrencyTypes vscurr)
        {
            var first = "neblio";
            switch (curr)
            {
                case CurrencyTypes.NEBL:
                    first = "neblio";
                    break;
                case CurrencyTypes.DOGE:
                    first = "dogecoin";
                    break;
                case CurrencyTypes.USD:
                    first = "usd";
                    break;
            }
            var second = "usd";
            switch (vscurr)
            {
                case CurrencyTypes.NEBL:
                    second = "neblio";
                    break;
                case CurrencyTypes.DOGE:
                    second = "dogecoin";
                    break;
                case CurrencyTypes.USD:
                    second = "usd";
                    break;
            }
            return $"{first}/{second}";
        }
    }
}
