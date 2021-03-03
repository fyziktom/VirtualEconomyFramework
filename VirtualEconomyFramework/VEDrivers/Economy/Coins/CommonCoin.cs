using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Coins
{
    public abstract class CommonCoin : ICoin
    {
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string BaseURL { get; set; } = string.Empty;
        public string OwenerName { get; set; } = string.Empty;
        public bool MetadataAvailable { get; set; } = false;
        public double? MaxSupply { get; set; } = 0;
        public double? CirculatingSuply { get; set; } = 0;
        public double? TransferFee { get; set; } = 0;
        public double? ActualBTCPrice { get; set; } = 0;

        public IEnumerable<ITransaction> Transactions { get; set; }

        public abstract Task<string> GetDetails();

    }
}
