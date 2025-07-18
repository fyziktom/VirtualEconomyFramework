using Ipfs;
using Ipfs.Http;
using NBitcoin;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Tsp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.Common;
using VEDriversLite.Events;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.DevicesNFTs;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Enum for loading of image/music/data stage
    /// </summary>
    public enum LoadingImageStages
    {
        /// <summary>
        /// Loading image stage - not started
        /// </summary>
        NotStarted,
        /// <summary>
        /// Loading image stage - loading in progress
        /// </summary>
        Loading,
        /// <summary>
        /// Loading image stage - Loaded successfully
        /// </summary>
        Loaded,
        /// <summary>
        /// Loading image stage - load failed
        /// </summary>        
        Failed
    }
    /// <summary>
    /// Dto for search of the Origin of the NFT
    /// </summary>
    public class LoadNFTOriginDataDto
    {
        /// <summary>
        /// Actual Utxo
        /// </summary>
        public string Utxo { get; set; } = string.Empty;
        /// <summary>
        /// Source Utxo
        /// </summary>
        public string SourceTxId { get; set; } = string.Empty;
        /// <summary>
        /// Origin of the NFT Utxo if known
        /// </summary>
        public string NFTOriginTxId { get; set; } = string.Empty;
        /// <summary>
        /// Tag which is used to identify the NFT is "used" stage - for example ticket
        /// </summary>
        public bool Used { get; set; } = false;
        /// <summary>
        /// Metadata from the NFT history moment
        /// </summary>
        public Dictionary<string, string> NFTMetadata { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Dto for the parse response of the IPFS server
    /// </summary>
    public class IPFSResponse
    {
        /// <summary>
        /// Name of the File
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Hash of the file
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// Size of the file
        /// </summary>
        public string Size { get; set; }
    }
    /// <summary>
    /// Helper static class for the NFT Operations and especially preparation of the NFT data
    /// </summary>
    public static class NFTHelpers
    {
        /// <summary>
        /// List of the allowed tokens. If you want to use your own tokens you can add them here
        /// The hash is the Token hash of the NTP1 token created on the Neblio network
        /// </summary>
        public static List<string> AllowedTokens = new List<string>() {
                "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8", //VENFT
                "LaAUG3WSAHWkrVtNYcd7CLdCYrA4phy1gjChvW", //BDP
                "La7DnXkx3YKeVy9QPRUKKdjCLo5wXanUu5XHsV", //WDOGE
                Coruzant.CoruzantNFTHelpers.CoruzantTokenId,
                HardwarioNFTHelpers.TokenId };

        /// <summary>
        /// Main default tokens in VEFramework - VENFT
        /// </summary>
        public static string TokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        /// <summary>
        /// Main default Data storage tokens in VEFramework - BDP
        /// </summary>
        public static string BDPTokenId = "LaAUG3WSAHWkrVtNYcd7CLdCYrA4phy1gjChvW";
        /// <summary>
        /// WDOGE token
        /// </summary>
        public static string WDOGETokenId = "La7DnXkx3YKeVy9QPRUKKdjCLo5wXanUu5XHsV";
        /// <summary>
        /// Main default tokens symbol in VEFramework - VENFT
        /// </summary>
        private static string TokenSymbol = "VENFT";

        /// <summary>
        /// New event happened - IEventInfo type of the message
        /// </summary>
        public static event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is called profile nft is found in the list of nfts
        /// </summary>
        public static event EventHandler<INFT> ProfileNFTFound;
        /// <summary>
        /// This event is called profile nft is found in the list of nfts
        /// </summary>
        public static event EventHandler<string> NFTLoadingStateChanged;

        /// <summary>
        /// Init handler to receive event info messages from Neblio Transaction Helpers class
        /// </summary>
        public static void InitHandlers()
        {
            NeblioTransactionHelpers.NewEventInfo += NeblioTransactionHelpers_NewEventInfo;
        }
        /// <summary>
        /// Deinit handler to receive event info messages from Neblio Transaction Helpers class
        /// </summary>
        public static void DeInitHandlers()
        {
            NeblioTransactionHelpers.NewEventInfo -= NeblioTransactionHelpers_NewEventInfo;
        }

        /// <summary>
        /// Return true if the type is allowed to buy and sell
        /// Actually are supported Image, Music, Post, Ticket NFTs.
        /// </summary>
        /// <param name="nftType"></param>
        /// <returns></returns>
        public static bool IsBuyableNFT(NFTTypes nftType)
        {
            if (nftType == NFTTypes.Image || 
                nftType == NFTTypes.Music || 
                nftType == NFTTypes.Post || 
                nftType == NFTTypes.App || 
                nftType == NFTTypes.XrayImage || 
                nftType == NFTTypes.Ticket)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Process new received info from Neblio Transaction Helpers class. Now just resend higher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void NeblioTransactionHelpers_NewEventInfo(object sender, IEventInfo e)
        {
            NewEventInfo?.Invoke(null, e);
        }


        /// <summary>
        /// Add allowed tokens during the initialization
        /// </summary>
        /// <returns></returns>
        public static async Task InitNFTHelpers()
        {
            await NeblioAPIHelpers.LoadAllowedTokensInfo(AllowedTokens);
        }

        /// <summary>
        /// This function will iterate through inputs of tx from the point of the utxo to find tx where this 1 token was splited from some lot.
        /// Metadata from this founded tx is returned for load to NFT carrier.
        /// This method is now used for "original" NFTs. It is for example Image and Music. 
        /// </summary>
        /// <param name="utxo"></param>
        /// <param name="checkIfUsed">if you are checking the NFT ticket you should set this flag</param>
        /// <returns></returns>
        public static async Task<LoadNFTOriginDataDto> LoadNFTOriginData(string utxo, bool checkIfUsed = false, GetTransactionInfoResponse txinfo = null)
        {
            var result = new LoadNFTOriginDataDto();
            var txid = utxo;
            var firstRun = true;

            while (true)
            {
                try
                {
                    if (firstRun && txinfo != null)
                        firstRun = false;
                    else
                        txinfo = await NeblioAPIHelpers.GetTransactionInfo(txid);
                    
                    var check = await CheckIfMintTx(txid, txinfo);
                    if (check.Item1)
                    {
                        var meta = await CheckIfContainsNFTData(txid, txinfo);
                        if (meta != null)
                        {
                            result.NFTMetadata = meta;
                            result.SourceTxId = check.Item2;
                            result.NFTOriginTxId = txid;
                            if (checkIfUsed && !result.Used)
                                if (meta.TryGetValue("Used", out var u))
                                    if (u == "true")
                                        result.Used = true;
                            return result;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (checkIfUsed && !result.Used)
                        {
                            var meta = await CheckIfContainsNFTData(txid, txinfo);
                            if (meta != null && meta.TryGetValue("Used", out var u))
                                if (u == "true")
                                    result.Used = true;
                        }

                        txid = check.Item2;
                    }

                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Problem in loading of NFT origin! " + ex.Message);
                }
            }
        }

        /// <summary>
        /// This will load just last transaction metadata if it is NFT metadata. 
        /// This function will not iterate to start/origin. It is used for example in Post
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<LoadNFTOriginDataDto> LoadLastData(string utxo)
        {
            var result = new LoadNFTOriginDataDto();
            var txid = utxo;
            try
            {
                var meta = await NeblioAPIHelpers.GetTransactionMetadata(TokenId, utxo);
                if (meta.TryGetValue("NFT", out var value))
                    if (!string.IsNullOrEmpty(value) && value == "true")
                    {
                        result.NFTMetadata = meta;
                        if (meta.TryGetValue("SourceUtxo", out var sourceTxId))
                            result.NFTOriginTxId = sourceTxId;

                        return result;
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem in loading of NFT origin!" + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// this function will obtain tx metadata and check if it contains flag NFT true
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> CheckIfContainsNFTData(string utxo, GetTransactionInfoResponse info = null)
        {
            if (info == null)
                info = await NeblioAPIHelpers.GetTransactionInfo(utxo);

            //var meta = await NeblioAPIHelpers.GetTransactionMetadata(TokenId, utxo);
            try
            {
                var metadata = string.Empty;
                if (info.Vout.Where(o => o.ScriptPubKey.Type == "nulldata").Any())
                    metadata = info.Vout.Where(o => o.ScriptPubKey.Type == "nulldata").FirstOrDefault()?.ScriptPubKey.Asm ?? string.Empty;

                var meta = NeblioTransactionHelpers.ParseCustomMetadata(metadata);

                if (meta.TryGetValue("NFT", out var value))
                    if (!string.IsNullOrEmpty(value) && value == "true")
                        return meta;
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync("Cannot discover if tx contains NFT data. " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// This function will check if the transaction is mint transaction. it means if the input to this tx was lot of the tokens.
        /// This kind of transaction means origin for the NFTs.
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> CheckIfMintTx(string utxo, GetTransactionInfoResponse info = null)
        {
            if (info == null)
                info = await NeblioAPIHelpers.GetTransactionInfo(utxo);

            if (info != null && info.Vin != null && info.Vin.Count > 0)
            {
                var vin = info.Vin.ToArray()?[0];
                if (vin.Tokens != null && vin.Tokens.Count > 0)
                {
                    var vintok = vin.Tokens.ToArray()?[0];
                    if (vintok != null)
                    {
                        if (vintok.Amount > 1)
                            return (true, vin.Txid);
                        else if (vintok.Amount == 1)
                            return (false, vin.Txid);
                    }
                }
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// this function will search the utxos, get all nfts utxos (if utxos list is not loaded) and return last profile nft which is founded on the address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="utxos">leave null if you need to obtain new utxos nft list</param>
        /// <returns></returns>
        public static async Task<INFT> FindProfileOfAddress(string address, ICollection<Utxos> utxos = null)
        {
            if (utxos == null)
                utxos = await NeblioAPIHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            INFT profile = null;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime);
                            if (nft != null)
                                if (nft.Type == NFTTypes.Profile && profile == null)
                                    profile = nft;
                        }

            return profile;
        }

        /// <summary>
        /// this function will search the utxos, get all nfts utxos (if utxos list is not loaded) and return last profile nft which is founded on the address.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="nftOriginTxId"></param>
        /// <param name="utxos">leave null if you need to obtain new utxos nft list</param>
        /// <returns></returns>
        public static async Task<INFT> FindEventOnTheAddress(string address, string nftOriginTxId, ICollection<Utxos> utxos = null)
        {
            if (utxos == null)
                utxos = await NeblioAPIHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            INFT eventNFT = null;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, true, true, NFTTypes.Event);
                            if (nft != null)
                                if (nft.NFTOriginTxId == nftOriginTxId)
                                {
                                    eventNFT = nft;
                                    return eventNFT;
                                }
                        }

            if (eventNFT == null)
            {
                var nft = await NFTFactory.GetNFT("", nftOriginTxId, 0, 0, true, true, NFTTypes.Event);
                eventNFT = nft;
            }
            return eventNFT;
        }
        /// <summary>
        /// This function will load NFTs and during it find the profile NFT
        /// </summary>
        /// <param name="address"></param>
        /// <returns>profile NFT and list of all NFTs</returns>
        public static async Task<(INFT, List<INFT>)> LoadAddressNFTsWithProfile(string address)
        {
            List<INFT> nfts = new List<INFT>();
            var utxos = await NeblioAPIHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            INFT profile = null;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime);
                            if (nft != null)
                            {
                                if (nft.Type == NFTTypes.Profile && profile == null)
                                    profile = nft;
                                if (!(nfts.Any(n => n.Utxo == nft.Utxo)))
                                    nfts.Add(nft);
                            }
                        }

            return (profile, nfts);
        }

        /// <summary>
        /// This function will find all NFTs and load them to the carriers. 
        /// If you already have list of utxos and NFTs you can provide it and it will load just the changes.
        /// </summary>
        /// <param name="address">Address with this NFTs</param>
        /// <param name="inutxos">Input Utxos collection</param>
        /// <param name="innfts">Input NFTs collection</param>
        /// <param name="fireProfileEvent">If you find profile, fire the event</param>
        /// <param name="maxLoadedItems">Limit number of loaded items</param>
        /// <param name="withoutMessages">Do not load messages</param>
        /// <param name="justMessages">Load just messages</param>
        /// <param name="justPayments">Load just Payments</param>
        /// <returns></returns>
        public static async Task<List<INFT>> LoadAddressNFTs(string address,
                                                     ICollection<Utxos> inutxos = null,
                                                     ICollection<INFT> innfts = null,
                                                     bool fireProfileEvent = false,
                                                     int maxLoadedItems = 0,
                                                     bool withoutMessages = false,
                                                     bool justMessages = false,
                                                     bool justPayments = false)
        {
            var fireProfileEventTmp = fireProfileEvent;
            List<INFT> nfts = new List<INFT>();
            ICollection<Utxos> uts = null;
            if (inutxos == null)
                uts = await NeblioAPIHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            else
                uts = await NeblioAPIHelpers.GetAddressNFTsUtxos(address, AllowedTokens, new GetAddressInfoResponse() { Utxos = inutxos });
            var utxos = uts;//.OrderBy(u => u.Blocktime).Reverse().ToList();

            NFTLoadingStateChanged?.Invoke(address, "Loading of the NFTs Started.");

            foreach (var u in utxos)
            {
                if (maxLoadedItems > 0 && nfts.Count > maxLoadedItems)
                    break;

                var inn = innfts.FirstOrDefault(n => n.Utxo == u.Txid && n.UtxoIndex == u.Index);
                if (inn != null)
                {
                    nfts.Add(inn);
                    continue;
                }

                var t = u.Tokens.FirstOrDefault();

                if (t != null && t.Amount == 1)
                {
                    try
                    {
                        var metadata = string.Empty;
                        if (u.Blockheight == -1 && t.AdditionalProperties != null && t.AdditionalProperties.Count > 0)
                        {
                            if (t.AdditionalProperties.TryGetValue("metadata", out var meta))
                                metadata = meta.ToString();
                        }

                        INFT nft = null;
                        if (!withoutMessages)
                        {
                            if (!justMessages && !justPayments)
                                nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, address: address, metadataString:metadata);
                            else if (justMessages && !justPayments)
                                nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, loadJustType: true, justType: NFTTypes.Message, address: address, metadataString: metadata);
                            else if (!justMessages && justPayments)
                                nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, loadJustType: true, justType: NFTTypes.Payment, address: address, metadataString: metadata);
                        }
                        else
                        {
                            nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, skipTheType: true, skipType: NFTTypes.Message, address: address, metadataString: metadata);
                        }
                        if (nft != null)
                        {
                            if (fireProfileEventTmp && nft.Type == NFTTypes.Profile)
                            {
                                ProfileNFTFound.Invoke(address, nft);
                                fireProfileEventTmp = false;
                            }
                            nft.UtxoIndex = (int)u.Index;
                            //if (!(nfts.Any(n => n.Utxo == nft.Utxo))) // todo TEST in cases with first minting on address
                            nfts.Add(nft);
                            NFTLoadingStateChanged?.Invoke(address, $"Loaded {nfts.Count} NFT of {utxos.Count}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Some trouble with loading NFT." + ex.Message);
                    }
                }
            }

            return nfts;
        }

        /// <summary>
        /// Returns list of NFTs with the data of the point of this history of some NFT.
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<List<INFT>> LoadNFTsHistory(string utxo)
        {
            try
            {
                List<INFT> nfts = new List<INFT>();
                bool end = false;
                var txid = utxo;

                while (!end)
                {
                    end = true;

                    GetTransactionInfoResponse txinfo = null;
                    List<Vin> vins = new List<Vin>();
                    try
                    {
                        txinfo = await NeblioAPIHelpers.GetTransactionInfo(txid);
                        vins = txinfo.Vin.ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot load transaction info." + ex.Message);
                    }

                    if (vins != null)
                    {
                        foreach (var vin in vins)
                        {
                            if (vin.Tokens != null)
                            {
                                var toks = vin.Tokens.ToList();
                                if (toks != null && toks.Count > 0 && toks[0] != null && toks[0].Amount > 0)
                                {
                                    try
                                    {
                                        var nft = await NFTFactory.GetNFT(toks[0].TokenId, txinfo.Txid, 0, (double)txinfo.Blocktime, true);
                                        if (nft != null)
                                        {
                                            if (!(nfts.Any(n => n.Utxo == nft.Utxo)))
                                                nfts.Add(nft);
                                            // go to previous tx
                                            txid = vin.Txid;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error while loading NFT history step" + ex.Message);
                                    }

                                    if (toks[0].Amount > 1)
                                        end = true;
                                    else if (toks[0].Amount == 1)
                                        end = false;
                                }
                            }
                        }
                    }
                }

                return nfts;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                Console.WriteLine("Exception during loading NFT history: " + msg);
                return new List<INFT>();
            }
        }
        /// <summary>
        /// Load just the Messages on the address - between the addresses
        /// </summary>
        /// <param name="aliceAddress"></param>
        /// <param name="bobAddress"></param>
        /// <param name="innfts"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<List<INFT>> LoadAddressNFTMessages(string aliceAddress, string bobAddress, ICollection<INFT> innfts = null)
        {
            if (innfts == null)
                throw new Exception("Input NFT array cannot be null"); // todo load nfts when list is null or empty

            var nftmessages = new List<INFT>();
            try
            {
                foreach(var nft in innfts)
                {
                    if (nft.Type == NFTTypes.Message)
                    {
                        if (nft.TxDetails.Vin == null)
                        {
                            try
                            {
                                nft.TxDetails = await NeblioAPIHelpers.GetTransactionInfo(nft.Utxo);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read tx details. " + ex.Message);
                            }
                        }
                        if (nft.TxDetails != null && nft.TxDetails.Vin != null)
                        {
                            var sender = await NeblioAPIHelpers.GetTransactionSender(nft.Utxo, nft.TxDetails);
                            var receiver = await NeblioAPIHelpers.GetTransactionReceiver(nft.Utxo, nft.TxDetails);
                            if ((sender == aliceAddress && receiver == bobAddress) || (receiver == aliceAddress && sender == bobAddress))
                                nftmessages.Add(nft);
                        }
                    }
                }

                return nftmessages;

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                Console.WriteLine("Exception during loading NFT messages: " + msg);
                return new List<INFT>();
            }
        }

        /// <summary>
        /// This function will new NFTs. 
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<MintNFTData> GetMintNFTData(string address, EncryptionKey ekey, INFT NFT, string receiver = "")
        {
            var metadata = await NFT.GetMetadata(address, await ekey.GetEncryptedKey(), receiver);
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address,
                ReceiverAddress = receiver
            };

            return dto;
        }

        /// <summary>
        /// This function will new Post NFT.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <param name="rewriteAuthor">You can rewrite author and use the Profile NFT hash,etc.</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<Transaction> GetMessageNFTTransaction(string address, string receiver, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, BitcoinSecret secret, string rewriteAuthor = "")
        {
            if (NFT.Type != NFTTypes.Message)
                throw new Exception("This is not Message NFT.");

            // thanks to filled params it will return encrypted metadata with shared secret
            var metadata = await NFT.GetMetadata(address, await ekey.GetEncryptedKey(), receiver);
            if (!string.IsNullOrEmpty(rewriteAuthor))
                if (metadata.ContainsKey("Author"))
                    metadata["Author"] = rewriteAuthor;

            try
            {
                var k = await NeblioTransactionHelpers.GetAddressAndKey(ekey);
                var key = k.Item2;
                var addressForTx = k.Item1;
                Transaction transaction;

                // send tx
                var rtxid = string.Empty;
                if (string.IsNullOrEmpty(NFT.Utxo))
                {
                    var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
                    {
                        Id = NFT.TokenId, // id of token
                        Metadata = metadata,
                        SenderAddress = address,
                        ReceiverAddress = receiver
                    };                                       

                    // send tx
                   transaction  = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, nutxos, tutxos);                    
                }
                else
                {
                    var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
                    {
                        Id = NFT.TokenId, // id of token
                        Metadata = metadata,
                        Amount = 1,
                        sendUtxo = new List<string>() { NFT.Utxo },
                        SenderAddress = address,
                        ReceiverAddress = receiver
                    };
                    transaction = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, nutxos);
                }

                return transaction;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This function will new Post NFT.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<Transaction> GetIoTMessageNFTTransaction(string address, string receiver, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            if (NFT.Type != NFTTypes.IoTMessage)
                throw new Exception("This is not Message NFT.");

            // thanks to filled params it will return encrypted metadata with shared secret
            var metadata = await NFT.GetMetadata(address, await ekey.GetEncryptedKey(), receiver);

            try
            {
                var k = await NeblioTransactionHelpers.GetAddressAndKey(ekey);
                var key = k.Item2;
                var addressForTx = k.Item1;
                Transaction transaction;

                // send tx
                if (string.IsNullOrEmpty(NFT.Utxo))
                {
                    var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
                    {
                        Id = NFT.TokenId, // id of token
                        Metadata = metadata,
                        SenderAddress = address,
                        ReceiverAddress = receiver
                    };
                    transaction = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, nutxos, tutxos);
                }
                else
                {
                    var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
                    {
                        Id = NFT.TokenId, // id of token
                        Metadata = metadata,
                        Amount = 1,
                        sendUtxo = new List<string>() { NFT.Utxo },
                        SenderAddress = address,
                        ReceiverAddress = receiver
                    };
                    transaction = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, nutxos);
                }
                return transaction;
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This function will new Post NFTs as multimint. 
        /// It means in one transaction it will create multiple 1 tokens outputs which are NFTs with same origin metadata.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns>New Tx Id Hash</returns>

        public static async Task<MintNFTData> GetMintMultiNFTData(string address, INFT NFT, string receiver = "", List<string> multipleReceivers = null)
        {
            var metadata = await NFT.GetMetadata();
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address,
                ReceiverAddress = receiver,
                MultipleReceivers = multipleReceivers
            };
            return dto;
        }
       

        /// <summary>
        /// This function will change NFT data.
        /// In NFT image and music it will be not relevant because it will always search for origin data even if you will rewrite it.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="nft">Input NFT object with data to save to metadata. Must contain Utxo hash</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<SendTokenTxData> GetChangeNFTTxData(string address, INFT nft)
        {
            var metadata = await nft.GetMetadata();
            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = nft.TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { $"{nft.Utxo}:{nft.UtxoIndex}" },
                SenderAddress = address,
                ReceiverAddress = address
            };
            return dto;
        }

        /// <summary>
        /// This function will take some NFT which was matched with some payment, coppy data and create complete payment, which will send NFT to new owner.
        /// During this the payment NFT token is send back to project address
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="payment">Payment NFT of received payment</param>
        /// <param name="NFT">NFT for sale</param>
        /// <returns></returns>
        public static async Task<SendTokenTxData> GetTxDataForOrderedNFT(string address, PaymentNFT payment, INFT NFT)
        {
            if (NFT == null)
                throw new Exception("Cannot find NFT in the address NFT list.");

            NFT.Price = 0.0;
            NFT.PriceActive = false;
            NFT.SellJustCopy = false;
            var metadata = await NFT.GetMetadata();
            metadata.Add("SoldPrice", payment.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("ReceiptFromPaymentUtxo", payment.Utxo);
            metadata.Add("SourceUtxo", NFT.NFTOriginTxId);

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = 1,
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                sendUtxo = new List<string>() { $"{payment.NFTUtxoTxId}:{payment.NFTUtxoIndex}", payment.Utxo },
                SenderAddress = address,
                ReceiverAddress = payment.Sender
            };

            return dto;
        }

        /// <summary>
        /// This function will take some NFT which was matched with some payment as copy, coppy data and create complete payment, which will send NFT to new owner.
        /// During this the payment NFT token is send back to project address
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="payment">Payment NFT of received payment</param>
        /// <param name="NFT">NFT for sale</param>
        /// <returns></returns>
        public static async Task<SendTokenTxData> GetTokenTxDataCopy(string address, PaymentNFT payment, INFT NFT)
        {
            if (NFT == null)
                throw new Exception("Cannot find NFT in the address NFT list.");

            NFT.Price = 0.0;
            NFT.PriceActive = false;
            NFT.SellJustCopy = false;
            NFT.Utxo = "";
            NFT.SourceTxId = NFT.NFTOriginTxId;
            var metadata = await NFT.GetMetadata();
            metadata.Add("SoldPrice", payment.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("ReceiptFromPaymentUtxo", payment.Utxo);
            metadata.Add("SourceUtxo", NFT.NFTOriginTxId);

            var mintingutxos = await NeblioAPIHelpers.FindUtxoForMintNFT(address, NFT.TokenId);
            
            if (mintingutxos == null || mintingutxos.Count == 0)
                throw new Exception("No minting supply available.");
            var mintutxo = mintingutxos.FirstOrDefault();
            if (mintutxo == null)
                throw new Exception("No minting supply available.");

            metadata["SourceUtxo"] = mintutxo.Txid;

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = 1,
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                sendUtxo = new List<string>() { $"{mintutxo.Txid}:{mintutxo.Index}", payment.Utxo },
                SenderAddress = address,
                ReceiverAddress = payment.Sender
            };
            return dto;
        }

        /// <summary>
        /// This function will destroy selected NFTs
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="nfts">Input NFTs to destroy</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public static async Task<SendTokenTxData> GetTxDataForDestroyNFTs(string address, ICollection<INFT> nfts, string receiver = "")
        {
            if (nfts == null || nfts.Count == 0)
                throw new Exception("You have to add NFT Utxos list");
            if (nfts.Count > 10)
                throw new Exception("Limit for one NFT destroy transaction is 10 of inputs.");

            var metadata = new Dictionary<string,string>();
            metadata.Add("NFT", "false");
            metadata.Add("Action", "Destroy of NFTs");

            var nftutxos = new List<string>();
            var tokenid = string.Empty;
            foreach(var nft in nfts)
            {
                if (string.IsNullOrEmpty(tokenid))
                    tokenid = nft.TokenId;

                if (nft.TokenId == tokenid)
                    nftutxos.Add($"{nft.Utxo}:{nft.UtxoIndex}");
            }

            if (string.IsNullOrEmpty(receiver))
                receiver = address;
            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = nftutxos.Count,
                Id = tokenid, // id of token
                Metadata = metadata,
                sendUtxo = nftutxos,
                SenderAddress = address,
                ReceiverAddress = receiver
            };
            return dto;
        }

        /// <summary>
        /// This function will send payment for some NFT.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <param name="nft">Input NFT object with data to save to metadata. It is NFT what you are buying.</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<SendTokenTxData> GetNFTPaymentData(string address, string receiver, INFT nft, ICollection<Utxos> nutxos)
        {
            if (string.IsNullOrEmpty(nft.Utxo))
                throw new Exception("Wrong token txid input.");

            if (!nft.PriceActive)
                throw new Exception("NFT is not for sale.");

            if (nutxos.Count > 20)
                throw new Exception("The Utxos for the Neblio are too many for one transaction. Please load some bigger Utxo for the big payment like this.");

            var paymentnft = new PaymentNFT("");
            paymentnft.Sender = address;
            paymentnft.NFTUtxoTxId = nft.Utxo;
            paymentnft.NFTUtxoIndex = nft.UtxoIndex;
            paymentnft.ImageLink = nft.ImageLink;
            paymentnft.Link = nft.Link;
            paymentnft.Price = nft.Price;

            var metadata = await paymentnft.GetMetadata();
            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { nft.Utxo },
                SenderAddress = address,
                ReceiverAddress = receiver
            };
            return dto;            
        }

        /// <summary>
        /// This function will return payment to the original sender.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="nft">Input PaymentNFT.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<SendTokenTxData> GetTxDataForReturnNFTPayment(string address, PaymentNFT nft)
        {
            if (string.IsNullOrEmpty(nft.Utxo))
                throw new Exception("Wrong token txid input.");

            nft.Description = "The ordered NFT was sold just right before the receiving your payment. The full amount is send back in this transaction.";
            var metadata = await nft.GetMetadata();
            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { nft.Utxo },
                SenderAddress = address,
                ReceiverAddress = nft.Sender
            };
            return dto;
        }

        /// <summary>
        /// This function will send NFT. It can be also used for write the price of the NFT.
        /// </summary>
        /// <param name="address">adress of sender</param>
        /// <param name="receiver">address of receiver</param>
        /// <param name="NFT">Input NFT object with data to save to metadata. It is NFT what you are sending.</param>
        /// <param name="priceWrite">Set this if you just want to set price of the NFT. means resend to yourself</param>
        /// <param name="price">Price must be higher than 0.0002 Neblio</param>
        /// <param name="withDogePrice">Set if Doge Price should be written</param>
        /// <param name="dogeprice">Set doge price, min 0.1</param>
        /// <returns>New Tx Id hash</returns>
        public static async Task<SendTokenTxData> GetNFTTxData(string address, string receiver, INFT NFT, bool priceWrite, double price = 0.0002, bool withDogePrice = false, double dogeprice = 1)
        {
            if ((price < 0.0002 && priceWrite) && !withDogePrice)
                throw new Exception("Price cannot be lower than 0.0002 NEBL.");

            if (priceWrite)
            {
                NFT.Price = price;
                NFT.PriceActive = true;
            }
            else
            {
                NFT.Price = 0.0;
                NFT.PriceActive = false;
            }

            if (withDogePrice)
            {
                NFT.DogePrice = dogeprice;
                NFT.DogePriceActive = true;
            }
            else
            {
                NFT.DogePrice = 0.0;
                NFT.DogePriceActive = false;
            }

            var metadata = await NFT.GetMetadata();

            metadata.Add("SourceUtxo", NFT.NFTOriginTxId);

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { $"{NFT.Utxo}:{NFT.UtxoIndex}" },
                SenderAddress = address,
                ReceiverAddress = receiver
            };

            return dto;
        }

        /// <summary>
        /// This function will write Used flag to the NFT Ticket
        /// </summary>
        /// <param name="address">adress of sender</param>
        /// <param name="NFT">Input NFT object with data to save to metadata. It is NFT what you are sending.</param>
        /// <returns>New Tx Id hash</returns>
        public static async Task<SendTokenTxData> GetTxDataForNFTTicket(string address, INFT NFT)
        {
            if (NFT.Type != NFTTypes.Ticket)
                throw new Exception("This is not NFT Ticket.");

            var metadata = await NFT.GetMetadata();

            if (!metadata.ContainsKey("Used"))
                metadata.Add("Used", "true");
            metadata.Add("SourceUtxo", NFT.NFTOriginTxId);

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { $"{NFT.Utxo}:{NFT.UtxoIndex}" },
                SenderAddress = address,
                ReceiverAddress = address
            };

            return dto;
        }

        /// <summary>
        /// This function will create Dto for issue Token tx transaction
        /// </summary>
        /// <param name="address">sender address</param>
        /// <returns></returns>
        public static async Task<IssueTokenTxData> GetTokenIssueTxData(string issuingAddress, string receiver, ulong amount, string tokenSymbol, string issuerNick, string description, string imageLink, string imageFileName, string imageType, IDictionary<string,string> metadata = null )
        {
            if (string.IsNullOrEmpty(issuingAddress))
                throw new Exception("Cannot create without address");
            if (string.IsNullOrEmpty(tokenSymbol) && tokenSymbol?.Length > 5)
                throw new Exception("Cannot create without token symbol. Or symbol is longer than 5 characters.");
            if (amount <= 0)
                throw new Exception("Amout must be bigger than 0.");

            if (string.IsNullOrEmpty(imageType))
            {
                var filename = imageFileName;
                if (string.IsNullOrEmpty(filename))
                    filename = imageLink;
                var ext = FileHelpers.GetMimeTypeFromImageFile(filename);
                if (!string.IsNullOrEmpty(ext))
                    imageType = ext;
                else
                    imageType = "image/png";
            }

            var urls = new tokenUrlCarrier()
            {
                url = imageLink,
                mimeType = imageType,
                name = "icon",
            };

            var metaurls = new List<tokenUrlCarrier>() { urls };

            MetadataOfIssuance meta = new MetadataOfIssuance()
            {
                Data = new Data2()
                {
                    Description = description,
                    Issuer = issuerNick,
                    TokenName = tokenSymbol,
                    Urls = metaurls,
                     UserData = new UserData4()
                     {
                          Meta = metadata.Select(data => new Meta3()
                          {
                              Key = data.Key,
                              Value = data.Value,
                              AdditionalProperties = new Dictionary<string, object>() { { "type", "String" } }
                          }).ToList()
                     }
                }
            };

            if (string.IsNullOrEmpty(receiver))
                receiver = issuingAddress;

            // fill input data for issue token tx
            var dto = new IssueTokenTxData()
            {
                Amount = amount,
                IssuanceMetadata = meta,
                SenderAddress = issuingAddress,
                ReceiverAddress = receiver
            };

            return dto;
        }

        /// <summary>
        /// This function will return first profile NFT in NFTs list.
        /// </summary>
        /// <param name="nfts"></param>
        /// <returns></returns>
        public static ProfileNFT FindProfileNFT(ICollection<INFT> nfts)
        {
            if (nfts != null)
                foreach (var n in nfts)
                    if (n.Type == NFTTypes.Profile)
                        return (ProfileNFT)n;
            return new ProfileNFT("");
        }

        /// <summary>
        /// This function will find profile on address if exists and parse address public key (NBitcoin class) from it.
        /// Usefull for encryption
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static async Task<(bool, PubKey)> GetPubKeyFromProfileNFTTx(string address)
        {
            var profile = await FindProfileOfAddress(address);
            //var profile = await FindProfileNFT(nfts);
            if (profile == null)
                return (false, null);
            var txhex = await NeblioAPIHelpers.GetTxHex(profile.Utxo);
            if (string.IsNullOrEmpty(txhex))
                return (false, null);
            var tx = Transaction.Parse(txhex, NeblioTransactionHelpers.Network);
            if (tx == null)
                return (false, null);
            var pubkey = tx.Inputs[0].ScriptSig.GetAllPubKeys();
            if (pubkey == null || pubkey.Count() == 0)
                return (false, null);
            else
                return (true, pubkey[0]);
        }

        /// <summary>
        /// This function will find last send transaction by some address and parse public key (NBitcoin class) from it.
        /// Usefull for encryption.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static async Task<(bool, PubKey)> GetPubKeyFromLastFoundTx(string address)
        {
            var tx = await NeblioTransactionHelpers.GetLastSentTransaction(address);
            //var profile = await FindProfileNFT(nfts);
            if (tx == null)
                return (false, null);
            var pubkey = tx.Inputs[0].ScriptSig.GetAllPubKeys();
            if (pubkey == null || pubkey.Count() == 0)
                return (false, null);
            else
                return (true, pubkey[0]);
        }

        /// <summary>
        /// This function will load the NFT based by Tx Id hash and find the owner of the NFT
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public static async Task<(bool, GetNFTOwnerDto)> GetNFTWithOwner(string txid)
        {
            var nft = await NFTFactory.GetNFT(TokenId, txid, 0, 0, true); // todo utxoindex
            if (nft != null && txid == nft.Utxo)
            {
                var txi = await NeblioAPIHelpers.GetTransactionInfo(txid);
                nft.Time = TimeHelpers.UnixTimestampToDateTime((double)txi.Blocktime);
                var tx = NBitcoin.Transaction.Parse(txi.Hex, NeblioTransactionHelpers.Network);

                if (tx != null && tx.Outputs.Count > 0 && tx.Inputs.Count > 0)
                {
                    var outp = tx.Outputs[0];
                    var inpt = tx.Inputs[0];
                    if (outp != null && inpt != null)
                    {
                        var scr = outp.ScriptPubKey;
                        var add = scr.GetDestinationAddress(NeblioTransactionHelpers.Network);
                        var utxos = await NeblioAPIHelpers.GetAddressUtxosObjects(add.ToString());
                        var addi = inpt.ScriptSig.GetSignerAddress(NeblioTransactionHelpers.Network);
                        if (utxos.FirstOrDefault(u => (u.Txid == txid && u.Value == 10000 && u.Tokens.Count > 0 && u.Tokens.FirstOrDefault()?.Amount == 1)) != null)
                        {
                            return (true, new GetNFTOwnerDto()
                            {
                                NFT = nft,
                                TxId = txid,
                                Owner = add.ToString(),
                                Sender = addi.ToString()
                            });
                        }
                        else
                        {
                            return (false, new GetNFTOwnerDto()
                            {
                                NFT = nft,
                                TxId = txid,
                                Owner = add.ToString(),
                                Sender = addi.ToString(),
                            });
                        }

                    }
                }
            }
            return (false, null);
        }
    }
}
