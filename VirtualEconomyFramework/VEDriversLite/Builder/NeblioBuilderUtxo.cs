using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builder
{
    public class NeblioBuilderUtxo
    {
        public NeblioBuilderUtxo(Utxos utxo)
        {
            Utxo = utxo;
        }
        public bool Used { get; set; } = false;
        public Utxos Utxo { get; set; } = new Utxos();
        public int TotalTokens { get; set; } = 0;
        public List<GetTokenMetadataResponse> TokenInfo { get; set; } = new List<GetTokenMetadataResponse>();
        public GetTransactionInfoResponse TxInfo { get; set; } = new GetTransactionInfoResponse();

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
