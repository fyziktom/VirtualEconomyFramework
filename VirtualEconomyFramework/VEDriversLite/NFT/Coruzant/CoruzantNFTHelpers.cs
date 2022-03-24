using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;

namespace VEDriversLite.NFT.Coruzant
{
    public static class CoruzantNFTHelpers
    {
        public static string CoruzantTokenId { get; set; } = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";

        /// <summary>
        /// Filter just Coruzant NFTs from NFT list
        /// </summary>
        /// <param name="allNFTs"></param>
        /// <returns></returns>
        public static async Task<List<INFT>> GetCoruzantNFTs(List<INFT> allNFTs)
        {
            if (allNFTs == null)
                return new List<INFT>();
            else
                return allNFTs.Where(n =>  n.Type == NFTTypes.CoruzantArticle ||
                                           n.Type == NFTTypes.CoruzantPodcast ||
                                           n.Type == NFTTypes.CoruzantPremiumArticle ||
                                           n.Type == NFTTypes.CoruzantPremiumPodcast ||
                                           n.Type == NFTTypes.CoruzantProfile).ToList();
        }

        /// <summary>
        /// This function will return first profile NFT in NFTs list.
        /// </summary>
        /// <param name="nfts"></param>
        /// <returns></returns>
        public static async Task<CoruzantProfileNFT> FindCoruzantProfileNFT(ICollection<INFT> nfts)
        {
            if (nfts != null)
                foreach (var n in nfts)
                    if (n.Type == NFTTypes.CoruzantProfile)
                        return (CoruzantProfileNFT)n;
            return new CoruzantProfileNFT("");
        }

        
        /// <summary>
        /// This function will change Coruzant Post NFT
        /// You can use this function for sending the NFT when you will fill receiver parameter
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="nft">Input NFT object with data to save to metadata. Must contain Utxo hash</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="receiver">Fill when you want to send NFT to another address</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> ChangeCoruzantPostNFT(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, NBitcoin.BitcoinSecret secret, string receiver = "")
        {
            var cnft = NFT as CoruzantArticleNFT;
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT CoruzantPost");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("AuthorProfileUtxo", cnft.AuthorProfileUtxo);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            metadata.Add("Tags", NFT.Tags);
            metadata.Add("FullPostLink", cnft.FullPostLink);
            metadata.Add("LastComment", cnft.LastComment);
            metadata.Add("LastCommentBy", cnft.LastCommentBy);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));

            var rec = address;
            if (!string.IsNullOrEmpty(receiver))
                rec = receiver;

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = CoruzantTokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { NFT.Utxo },
                SenderAddress = address,
                ReceiverAddress = rec
            };

            try
            {
                // send tx

                var transaction = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, nutxos);

                var rtxid = await NeblioTransactionHelpers.SignAndBroadcastTransaction(transaction, secret);

                if (rtxid != null)
                    return rtxid;
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }      
        
    }
}
