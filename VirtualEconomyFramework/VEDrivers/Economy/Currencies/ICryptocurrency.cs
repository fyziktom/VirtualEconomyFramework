using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy;

namespace VEDrivers.Economy
{
    public enum CryptocurrencyTypes
    {
        Bitcoin,
        Neblio,
        ReddCoin
    }
    public interface ICryptocurrency : IUnitBase, ICommonUnitInfo
    {
        List<IBinanceStreamKlineData> KlineDataHistory { get; set; }
        bool TokensAvailable { get; set; }
        double FromSatToMainRatio { get; set; }
        Task<string> GetDetails();
    }
}
