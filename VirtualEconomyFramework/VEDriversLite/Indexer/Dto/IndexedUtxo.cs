using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Indexer.Dto
{
    public class IndexedUtxo : IndexedItem
    {
        public string TransactionHashAndN { get; set; } = string.Empty;
        public string? OwnerAddress { get; set; } = string.Empty;        
        public bool Used { get; set; } = false;
        public bool TokenUtxo { get; set; } = false;
        public double Value { get; set; } = 0.0;
        public double Blockheight { get; set; } = 0.0;
        public DateTime Blocktime { get; set; } = DateTime.MinValue;
        public string UsedInTxHash { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public double TokenAmount { get; set; } = 0.0;
        public string TokenSymbol { get; set; } = string.Empty;
    }
}
