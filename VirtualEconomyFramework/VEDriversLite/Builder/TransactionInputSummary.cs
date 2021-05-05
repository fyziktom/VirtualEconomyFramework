using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builder
{
    public class TransactionInputSummary
    {
        public double TotalNeblioAmount { get; set; } = 0.0;
        public Dictionary<GetTokenMetadataResponse, int> Tokens { get; set; } = new Dictionary<GetTokenMetadataResponse, int>();

    }
}
