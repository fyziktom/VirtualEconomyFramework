using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builder
{
    /// <summary>
    /// Info about the outpus in the transaction
    /// </summary>
    public class TransactionOutputSummary
    {
        /// <summary>
        /// All the Neblio in the transaction outputs
        /// </summary>
        public double TotalNeblioAmount { get; set; } = 0.0001;
        /// <summary>
        /// Tokens in the outputs
        /// </summary>
        public Dictionary<GetTokenMetadataResponse, int> Tokens { get; set; } = new Dictionary<GetTokenMetadataResponse, int>();
    }
}
