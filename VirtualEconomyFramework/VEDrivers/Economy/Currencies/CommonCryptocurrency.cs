using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy
{
    public abstract class CommonCryptocurrency : ICryptocurrency
    {
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string BaseURL { get; set; } = string.Empty;
        public string OwenerName { get; set; } = string.Empty;
        public double? MaxSupply { get; set; } = 0;
        public double? CirculatingSuply { get; set; } = 0;
        public double? TransferFee { get; set; } = 0;
        public double? ActualBTCPrice { get; set; } = 0;
        public bool TokensAvailable { get; set; } = false;
        public string Creator { get; set; } = string.Empty;
        public string RepositoryURL { get; set; } = string.Empty;
        public string ProjectWebsite { get; set; } = string.Empty;
        public List<IBinanceStreamKlineData> KlineDataHistory { get; set; }

        public abstract Task<string> GetDetails();
    }
}
