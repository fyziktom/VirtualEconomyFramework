using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Tokens
{
    public abstract class CommonToken : IToken
    {
        public string Id { get; set; } = string.Empty;
        public string TxId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string BaseURL { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string IssuerName { get; set; } = string.Empty;
        public bool MetadataAvailable { get; set; } = false;
        public double? MaxSupply { get; set; } = 0;
        public double? CirculatingSuply { get; set; } = 0;
        public double? TransferFee { get; set; } = 0;
        public double? ActualBTCPrice { get; set; } = 0;
        public double? ActualBalance { get; set; } = 0.0;
        public TransactionDirection Direction { get; set; }
        public DateTime TimeStamp { get; set; }

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public abstract Task<string> GetDetails();

    }
}
