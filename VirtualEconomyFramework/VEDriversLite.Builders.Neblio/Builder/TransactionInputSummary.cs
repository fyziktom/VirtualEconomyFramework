using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builders.Neblio
{
    /// <summary>
    /// Summary of the all inputs
    /// </summary>
    public class TransactionInputSummary
    {
        /// <summary>
        /// Amount of the all Neblio in inputs
        /// </summary>
        public double TotalNeblioAmount { get; set; } = 0.0;
        /// <summary>
        /// All tokens in the inputs
        /// </summary>
        public Dictionary<GetTokenMetadataResponse, int> Tokens { get; set; } = new Dictionary<GetTokenMetadataResponse, int>();

    }
}
