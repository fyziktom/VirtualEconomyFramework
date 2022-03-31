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
    /// <summary>
    /// Helper class to load and handle Coruzant NFTs
    /// Will be replaced with INFTModules soon
    /// </summary>
    public static class CoruzantNFTHelpers
    {
        /// <summary>
        /// Coruzant token ID - CORZT on Neblio Blockchain
        /// </summary>
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
        /// This function will new Coruzant Post NFT.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="coppies">number of copies</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="profileUtxo">related profile NFT utxo</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> MintCoruzantPostNFT(string address, int coppies, EncryptionKey ekey, INFT NFT, string profileUtxo, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
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
            
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = CoruzantTokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = string.Empty;
                if (coppies == 0)
                    rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos);
                else
                    rtxid = await NeblioTransactionHelpers.MintMultiNFTTokenAsync(dto, coppies, ekey, nutxos, tutxos);
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

        /// <summary>
        /// This function will change Coruzant Post NFT
        /// You can use this function for sending the NFT when you will fill receiver parameter
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata. Must contain Utxo hash</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="receiver">Fill when you want to send NFT to another address</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> ChangeCoruzantPostNFT(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, string receiver = "")
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
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos);
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

        /// <summary>
        /// This function will new Coruzant Profile NFTs. 
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <param name="nft">Input NFT</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> MintCoruzantProfileNFT(string address, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            var profile = nft as CoruzantProfileNFT;

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT CoruzantProfile");
            metadata.Add("Name", profile.Name);
            metadata.Add("Surname", profile.Surname);
            metadata.Add("Age", profile.Age.ToString());
            metadata.Add("Description", profile.Description);
            metadata.Add("Image", profile.ImageLink);
            metadata.Add("Link", profile.Link);
            metadata.Add("PersonalPageLink", profile.PersonalPageLink);
            metadata.Add("CompanyName", profile.CompanyName);
            metadata.Add("CompanyLink", profile.CompanyLink);
            metadata.Add("WorkingPosition", profile.WorkingPosition);
            
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = CoruzantTokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos);
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

        /// <summary>
        /// This function will change Coruzant Profile NFT.
        /// You can use this function for sending the NFT when you will fill receiver parameter
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="nft">Input NFT object with data to save to metadata. Must contain Utxo hash</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="receiver">Fill when you want to send NFT to another address</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> ChangeCoruzantProfileNFT(string address, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos, string receiver = "")
        {
            var profile = nft as CoruzantProfileNFT;

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT CoruzantProfile");
            metadata.Add("Name", profile.Name);
            metadata.Add("Surname", profile.Surname);
            metadata.Add("Age", profile.Age.ToString());
            metadata.Add("Description", profile.Description);
            metadata.Add("Image", profile.ImageLink);
            metadata.Add("Link", profile.Link);
            metadata.Add("PersonalPageLink", profile.PersonalPageLink);
            metadata.Add("CompanyName", profile.CompanyName);
            metadata.Add("CompanyLink", profile.CompanyLink);
            metadata.Add("WorkingPosition", profile.WorkingPosition);

            var rec = address;
            if (!string.IsNullOrEmpty(receiver))
                rec = receiver;

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = CoruzantTokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { profile.Utxo },
                SenderAddress = address,
                ReceiverAddress = rec
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos);
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
