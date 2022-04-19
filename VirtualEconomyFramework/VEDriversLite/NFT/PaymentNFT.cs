using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Payment NFT is used for buying NFTs
    /// </summary>
    public class PaymentNFT : CommonNFT
    {
        /// <summary>
        /// Create empty NFT class
        /// </summary>
        public PaymentNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Payment;
            TypeText = "NFT Payment";
        }

        /// <summary>
        /// original NFT hash which was sold
        /// </summary>
        public string NFTUtxoTxId { get; set; } = string.Empty;
        /// <summary>
        /// original NFT index which was sold
        /// </summary>
        public int NFTUtxoIndex { get; set; } = 0;
        /// <summary>
        /// Sender of the payment
        /// </summary>
        public string Sender { get; set; } = string.Empty;
        /// <summary>
        /// Original payment hash (can be from different blockchain or payment method)
        /// </summary>
        public string OriginalPaymentTxId { get; set; } = string.Empty;
        /// <summary>
        /// When payment match the NFT which was bought this is set
        /// </summary>
        public bool Matched { get; set; } = false;
        /// <summary>
        /// If you receive payment and NFT is already sold this is set to true
        /// </summary>
        public bool AlreadySoldItem { get; set; } = false;
        /// <summary>
        /// If this payment was returned because NFT is not available this will be set as true
        /// </summary>
        public bool Returned { get; set; } = false;
        /// <summary>
        /// Fill basic parameters
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as PaymentNFT;
            NFTUtxoTxId = pnft.NFTUtxoTxId;
            NFTUtxoIndex = pnft.NFTUtxoIndex;
            Matched = pnft.Matched;
            Sender = pnft.Sender;
            AlreadySoldItem = pnft.AlreadySoldItem;
            Returned = pnft.Returned;
            OriginalPaymentTxId = pnft.OriginalPaymentTxId;
        }
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
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
        /// <summary>
        /// Find and parse origin data
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            return Task.CompletedTask;
        }
        /// <summary>
        /// Get the NFT data for the NFT
        /// </summary>
        /// <param name="address">Address of the sender</param>
        /// <param name="key">Private key of the sender for encryption</param>
        /// <param name="receiver">receiver of the NFT</param>
        /// <returns></returns>
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
