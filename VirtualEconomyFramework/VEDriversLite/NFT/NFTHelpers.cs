using Ipfs.Http;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.Builder;
using VEDriversLite.Events;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;

namespace VEDriversLite.NFT
{
    public enum LoadingImageStages
    {
        NotStarted,
        Loading,
        Loaded
    }
    public class LoadNFTOriginDataDto
    {
        public string Utxo { get; set; } = string.Empty;
        public string SourceTxId { get; set; } = string.Empty;
        public string NFTOriginTxId { get; set; } = string.Empty;
        public bool Used { get; set; } = false;
        public Dictionary<string, string> NFTMetadata { get; set; } = new Dictionary<string, string>();
    }

    public static class NFTHelpers
    {
        public static List<string> AllowedTokens = new List<string>() {
                "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8",
                Coruzant.CoruzantNFTHelpers.CoruzantTokenId };

        public static string TokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        private static string TokenSymbol = "VENFT";
        public static readonly IpfsClient ipfs = new IpfsClient("https://ipfs.infura.io:5001");

        public static event EventHandler<IEventInfo> NewEventInfo;

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
        /// Process new received info from Neblio Transaction Helpers class. Now just resend higher
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void NeblioTransactionHelpers_NewEventInfo(object sender, IEventInfo e)
        {
            NewEventInfo?.Invoke(null, e);
        }

        public static async Task InitNFTHelpers()
        {
            await NeblioTransactionHelpers.LoadAllowedTokensInfo(AllowedTokens);
        }

        /// <summary>
        /// This function will iterate through inputs of tx from the point of the utxo to find tx where this 1 token was splited from some lot.
        /// Metadata from this founded tx is returned for load to NFT carrier.
        /// This method is now used for "original" NFTs. It is for example Image and Music. 
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<LoadNFTOriginDataDto> LoadNFTOriginData(string utxo, bool checkIfUsed = false)
        {
            var result = new LoadNFTOriginDataDto();
            var txid = utxo;
            while (true)
            {
                try
                {
                    var check = await CheckIfMintTx(txid);
                    if (check.Item1)
                    {
                        var meta = await CheckIfContainsNFTData(txid);
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
                            var meta = await CheckIfContainsNFTData(txid);
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
                    Console.WriteLine("Problem in loading of NFT origin!");
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
                var meta = await NeblioTransactionHelpers.GetTransactionMetadata(TokenId, utxo);
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
                Console.WriteLine("Problem in loading of NFT origin!");
            }

            return null;
        }

        /// <summary>
        /// this function will obtain tx metadata and check if it contains flag NFT true
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> CheckIfContainsNFTData(string utxo)
        {
            var meta = await NeblioTransactionHelpers.GetTransactionMetadata(TokenId, utxo);

            if (meta.TryGetValue("NFT", out var value))
                if (!string.IsNullOrEmpty(value) && value == "true")
                    return meta;

            return null;
        }

        /// <summary>
        /// This function will check if the transaction is mint transaction. it means if the input to this tx was lot of the tokens.
        /// This kind of transaction means origin for the NFTs.
        /// </summary>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> CheckIfMintTx(string utxo)
        {
            var info = await NeblioTransactionHelpers.GetTransactionInfo(utxo);

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
                utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            INFT profile = null;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (double)u.Blocktime);
                            if (nft != null)
                                if (nft.Type == NFTTypes.Profile && profile == null)
                                    profile = nft;
                        }

            return profile;
        }

        /// <summary>
        /// This function will load NFTs and during it find the profile NFT
        /// </summary>
        /// <param name="address"></param>
        /// <returns>profile NFT and list of all NFTs</returns>
        public static async Task<(INFT, List<INFT>)> LoadAddressNFTsWithProfile(string address)
        {
            List<INFT> nfts = new List<INFT>();
            var utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            INFT profile = null;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (double)u.Blocktime);
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
        /// <param name="address"></param>
        /// <param name="inutxos"></param>
        /// <param name="innfts"></param>
        /// <returns></returns>
        public static async Task<List<INFT>> LoadAddressNFTs(string address, ICollection<Utxos> inutxos = null, ICollection<INFT> innfts = null)
        {
            List<INFT> nfts = new List<INFT>();
            ICollection<Utxos> uts = null;
            if (inutxos == null)
                uts = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address, AllowedTokens);
            else
                uts = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address, AllowedTokens, new GetAddressInfoResponse() { Utxos = inutxos });
            var utxos = uts.OrderBy(u => u.Blocktime).Reverse().ToList();

            var ns = new List<INFT>();
            if (innfts != null && utxos.Count <= innfts.Count)
            {
                innfts.ToList().ForEach(n =>
                {
                    if (utxos.Any(u => (u.Txid == n.Utxo && u.Index == n.UtxoIndex)))
                        ns.Add(n);
                });
                innfts = ns.ToList();
            }

            var lastNFTTime = DateTime.MinValue;
            if (innfts != null && innfts.Count > 0)
                lastNFTTime = innfts.FirstOrDefault().Time;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.Amount == 1)
                            if (TimeHelpers.UnixTimestampToDateTime((double)u.Blocktime) > lastNFTTime)
                            {
                                var nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (double)u.Blocktime);
                                if (nft != null)
                                {
                                    nft.UtxoIndex = (int)u.Index;
                                    //if (!(nfts.Any(n => n.Utxo == nft.Utxo))) // todo TEST in cases with first minting on address
                                    nfts.Add(nft);
                                }
                            }

            if (innfts == null)
                return nfts;
            else
                nfts.ForEach(n => {
                    if (!innfts.Any(i => i.Utxo == n.Utxo && i.UtxoIndex == n.UtxoIndex))
                        innfts.Add(n);
                });
            return innfts.OrderByDescending(n => n.Time).ToList();
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
                        txinfo = await NeblioTransactionHelpers.GetTransactionInfo(txid);
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
                                        var nft = await NFTFactory.GetNFT(toks[0].TokenId, txinfo.Txid, (double)txinfo.Blocktime, true);
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
                                nft.TxDetails = await NeblioTransactionHelpers.GetTransactionInfo(nft.Utxo);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read tx details. " + ex.Message);
                            }
                        }
                        if (nft.TxDetails != null && nft.TxDetails.Vin != null)
                        {
                            var sender = await NeblioTransactionHelpers.GetTransactionSender(nft.Utxo, nft.TxDetails);
                            var receiver = await NeblioTransactionHelpers.GetTransactionReceiver(nft.Utxo, nft.TxDetails);
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
        /// Calculate Neblio Fee based on total metadata length
        /// </summary>
        /// <param name="metadata">Dictionary with metadata</param>
        /// <returns>20000 as default, additional 10000 for each another 900 characters</returns>
        public static int CalculateFee(IDictionary<string,string> metadata)
        {
            var fee = 20000;
            var all = string.Empty;
            foreach (var meta in metadata)
                all += meta.Key + meta.Value;

            if (all.Length < 900)
                fee = 20000;
            else if (all.Length > 900)
                fee = 30000;
            else if (all.Length > 1900)
                fee = 40000;
            else if (all.Length > 2900)
                fee = 50000;
            else if (all.Length > 3900)
                fee = 60000;
            else
                throw new Exception("Metadata length exceed allowed maximum length");

            return fee;
        }

        /// <summary>
        /// This function will new NFTs. 
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> MintNFT(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            var metadata = await NFT.GetMetadata();
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos, fee);
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
        /// This function will new Post NFT.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> SendMessageNFT(string address, string receiver, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            if (NFT.Type != NFTTypes.Message)
                throw new Exception("This is not Message NFT.");

            // thanks to filled params it will return encrypted metadata with shared secret
            var metadata = await NFT.GetMetadata(address, await ekey.GetEncryptedKey(), receiver);
            var fee = CalculateFee(metadata);
            try
            {
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
                    rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos, fee);
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
                    rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos);
                }
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
        /// This function will new Post NFTs as multimint. 
        /// It means in one transaction it will create multiple 1 tokens outputs which are NFTs with same origin metadata.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="coppies">number of copies. one NFT is minted even 0 coppies is input</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> MintMultiNFT(string address, int coppies, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            var metadata = await NFT.GetMetadata();
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = NFT.TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintMultiNFTTokenAsync(dto, coppies, ekey, nutxos, tutxos, fee);
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
        /// This function will new Ticket NFTs. It is multimint tx
        /// It means in one transaction it will create multiple 1 tokens outputs which are NFTs with same origin metadata.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="coppies">number of copies. one NFT is minted even 0 coppies is input</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="tutxos">List of spendable token utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> MintNFTTickets(string address, int coppies, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            var metadata = await nft.GetMetadata();
            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = nft.TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintMultiNFTTokenAsync(dto, coppies, ekey, nutxos, tutxos, fee);
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
        /// This function will change NFT data.
        /// In NFT image and music it will be not relevant because it will always search for origin data even if you will rewrite it.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="nft">Input NFT object with data to save to metadata. Must contain Utxo hash</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> ChangeNFT(string address, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos)
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
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos, fee);
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
        /// This function will take some NFT which was matched with some payment, coppy data and create complete payment, which will send NFT to new owner.
        /// During this the payment NFT token is send back to project address
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="payment">Payment NFT of received payment</param>
        /// <param name="NFT">NFT for sale</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <returns></returns>
        public static async Task<string> SendOrderedNFT(string address, EncryptionKey ekey, PaymentNFT payment, INFT NFT, ICollection<Utxos> nutxos)
        {
            if (NFT == null)
                throw new Exception("Cannot find NFT in the address NFT list.");

            NFT.Price = 0.0;
            NFT.PriceActive = false;
            var metadata = await NFT.GetMetadata();
            metadata.Add("SoldPrice", payment.Price.ToString(CultureInfo.InvariantCulture));
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
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendMultiTokenAPIAsync(dto, ekey, nutxos, fee);
                if (!string.IsNullOrEmpty(rtxid))
                    return rtxid;
                else
                    throw new Exception("Sending Multi Token Transaction was not successfull.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This function will destroy selected NFTs
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="payment">Payment NFT of received payment</param>
        /// <param name="NFT">NFT for sale</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <returns></returns>
        public static async Task<string> DestroyNFTs(string address, EncryptionKey ekey, ICollection<INFT> nfts, ICollection<Utxos> nutxos)
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
            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = nftutxos.Count,
                Id = tokenid, // id of token
                Metadata = metadata,
                sendUtxo = nftutxos,
                SenderAddress = address,
                ReceiverAddress = address
            };
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.DestroyNFTAsync(dto, ekey, nutxos, fee);
                if (!string.IsNullOrEmpty(rtxid))
                    return rtxid;
                else
                    throw new Exception("Sending Multi Token Transaction was not successfull.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This function will send payment for some NFT.
        /// </summary>
        /// <param name="address">sender address</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="nft">Input NFT object with data to save to metadata. It is NFT what you are buying.</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <returns>New Tx Id Hash</returns>
        public static async Task<string> SendNFTPayment(string address, EncryptionKey ekey, string receiver, INFT nft, ICollection<Utxos> nutxos)
        {
            if (string.IsNullOrEmpty(nft.Utxo))
                throw new Exception("Wrong token txid input.");

            if (!nft.PriceActive)
                throw new Exception("NFT is not for sale.");

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
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNTP1TokenWithPaymentAPIAsync(dto, ekey, nft.Price, nutxos, fee);
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
        /// This function will send NFT. It can be also used for write the price of the NFT.
        /// </summary>
        /// <param name="address">adress of sender</param>
        /// <param name="receiver">address of receiver</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata. It is NFT what you are sending.</param>
        /// <param name="priceWrite">Set this if you just want to set price of the NFT. means resend to yourself</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <param name="price">Price must be higher than 0.0002 Neblio</param>
        /// <returns>New Tx Id hash</returns>
        public static async Task<string> SendNFT(string address, string receiver, EncryptionKey ekey, INFT NFT, bool priceWrite, ICollection<Utxos> nutxos, double price = 0.0002)
        {
            if (price < 0.0002)
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
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos, fee);
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
        /// This function will write Used flag to the NFT Ticket
        /// </summary>
        /// <param name="address">adress of sender</param>
        /// <param name="ekey">Encryption Key object of the address</param>
        /// <param name="NFT">Input NFT object with data to save to metadata. It is NFT what you are sending.</param>
        /// <param name="nutxos">List of spendable neblio utxos if you have it loaded.</param>
        /// <returns>New Tx Id hash</returns>
        public static async Task<string> UseNFTTicket(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos)
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
            var fee = CalculateFee(metadata);
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos, fee);
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
        /// This function will return first profile NFT in NFTs list.
        /// </summary>
        /// <param name="nfts"></param>
        /// <returns></returns>
        public static async Task<ProfileNFT> FindProfileNFT(ICollection<INFT> nfts)
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
            var txhex = await NeblioTransactionHelpers.GetTxHex(profile.Utxo);
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
            var nft = await NFTFactory.GetNFT(TokenId, txid, 0, true);
            if (nft != null && txid == nft.Utxo)
            {
                var txi = await NeblioTransactionHelpers.GetTransactionInfo(txid);
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
                        var utxos = await NeblioTransactionHelpers.GetAddressUtxosObjects(add.ToString());
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
