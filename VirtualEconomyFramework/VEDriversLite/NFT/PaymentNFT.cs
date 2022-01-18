using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class PaymentNFT : CommonNFT
    {
        public PaymentNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Payment;
            TypeText = "NFT Payment";
        }

        public string NFTUtxoTxId { get; set; } = string.Empty;
        public int NFTUtxoIndex { get; set; } = 0;
        public string Sender { get; set; } = string.Empty;
        public string OriginalPaymentTxId { get; set; } = string.Empty;
        public bool Matched { get; set; } = false;
        public bool AlreadySoldItem { get; set; } = false;
        public bool Returned { get; set; } = false;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as PaymentNFT;
            NFTUtxoTxId = pnft.NFTUtxoTxId;
            NFTUtxoIndex = pnft.NFTUtxoIndex;
            Sender = pnft.Sender;
            AlreadySoldItem = pnft.AlreadySoldItem;
            Returned = pnft.Returned;
            OriginalPaymentTxId = pnft.OriginalPaymentTxId;
        }

        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("NFTUtxoTxId", out var nfttxid))
            {
                NFTUtxoTxId = nfttxid;
                NFTOriginTxId = nfttxid;
            }
            if (metadata.TryGetValue("Sender", out var sender))
                Sender = sender;
            if (metadata.TryGetValue("NFTUtxoIndex", out var index))
                if (!string.IsNullOrEmpty(index))
                    NFTUtxoIndex = Convert.ToInt32(index);
            if (metadata.TryGetValue("AlreadySold", out var aldsold))
            {
                if (!string.IsNullOrEmpty(aldsold))
                {
                    if (aldsold == "true")
                        AlreadySoldItem = true;
                    else
                        AlreadySoldItem = false;
                }
                else
                {
                    AlreadySoldItem = false;
                }
            }
            else
            {
                AlreadySoldItem = false;
            }
            if (metadata.TryGetValue("Returned", out var rtn))
            {
                if (!string.IsNullOrEmpty(rtn))
                {
                    if (rtn == "true")
                        Returned = true;
                    else
                        Returned = false;
                }
                else
                {
                    Returned = false;
                }
            }
            else
            {
                Returned = false;
            }

            if (metadata.TryGetValue("OriginalPaymentTxId", out var optxid))
                OriginalPaymentTxId = optxid;
        }

        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();
            metadata.Add("Sender", Sender);
            metadata.Add("NFTUtxoTxId", NFTUtxoTxId);
            metadata.Add("NFTUtxoIndex", NFTUtxoIndex.ToString());
            if (AlreadySoldItem)
                metadata.Add("AlreadySold", "true");
            if (Returned)
                metadata.Add("Returned", "true");
            if (!string.IsNullOrEmpty(OriginalPaymentTxId))
                metadata.Add("OriginalPaymentTxId", OriginalPaymentTxId);
            return metadata;
        }
    }
}
