using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builder
{
    public class TransactionOutputSummary
    {
        public double TotalNeblioAmount { get; set; } = 0.0001;
        public Dictionary<GetTokenMetadataResponse, int> Tokens { get; set; } = new Dictionary<GetTokenMetadataResponse, int>();
    }
}
