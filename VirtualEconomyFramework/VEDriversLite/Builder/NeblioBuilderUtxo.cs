using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builder
{
    /// <summary>
    /// Neblio Builder Utxo object
    /// </summary>
    public class NeblioBuilderUtxo
    {
        /// <summary>
        /// Constructor which loads the Utxo hash
        /// </summary>
        /// <param name="utxo"></param>
        public NeblioBuilderUtxo(Utxos utxo)
        {
            Utxo = utxo;
        }
        /// <summary>
        /// Is Utxo already used, if yes, this is true.
        /// </summary>
        public bool Used { get; set; } = false;
        /// <summary>
        /// Origina Utxos object
        /// </summary>
        public Utxos Utxo { get; set; } = new Utxos();
        /// <summary>
        /// Total tokens in the Utxo
        /// </summary>
        public int TotalTokens { get; set; } = 0;
        /// <summary>
        /// Token info about the available tokens in this Utxo
        /// </summary>
        public List<GetTokenMetadataResponse> TokenInfo { get; set; } = new List<GetTokenMetadataResponse>();
        /// <summary>
        /// Tx info from Neblio API for this Utxo
        /// </summary>
        public GetTransactionInfoResponse TxInfo { get; set; } = new GetTransactionInfoResponse();
        /// <summary>
        /// Load info about the Utxo
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task LoadInfo()
        {
            if (string.IsNullOrEmpty(Utxo.Txid))
                throw new Exception("Cannot load Info. Utxo txid is empty.");

            TokenInfo.Clear();
            TotalTokens = 0;
            if (Utxo.Tokens.Count > 0)
            {
                foreach (var tok in Utxo.Tokens)
                {
                    var ti = await NeblioTransactionHelpers.GetTokenMetadata(tok.TokenId);
                    if (ti != null) 
                    {
                        TokenInfo.Add(ti);
                        TotalTokens += (int)tok.Amount;
                    }
                }
            }

            TxInfo = await NeblioTransactionHelpers.GetTransactionInfo(Utxo.Txid);
        }
    }
}
