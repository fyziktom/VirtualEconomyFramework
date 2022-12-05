using Ipfs.Http;
using Newtonsoft.Json;
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
using VEDriversLite.NeblioAPI;
using VEFramework.Demo.PublishingDisplay.Services.NFTs;
using VEFramework.Demo.PublishingDisplay.Services.NFTs.Dtos;

namespace VEFramework.Demo.PublishingDisplay.Services.NFTs
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
                "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7" }; // Coruzant token
        /// <summary>
        /// Infura Key for the access to the IPFS API
        /// </summary>
        public static string InfuraKey = "1urI71lwIaCjNo4b2kyL8LQ5Rlf";
        /// <summary>
        /// Infura Secret for the access to the IPFS API
        /// </summary>
        public static string InfuraSecret = "ce9c8fb81ab177c713841cecc3f9af51";
        /// <summary>
        /// Infura Url for the access to the IPFS API
        /// </summary>
        public static string InfuraAPIURL = "https://venftappserver.azurewebsites.net/api/uploadtoipfs";//"https://ipfs.infura.io:5001";
        /// <summary>
        /// IPFS Gateway address
        /// </summary>
        public static string GatewayURL = "https://ipfs.io/ipfs/";//"https://bdp.infura-ipfs.io/ipfs/";
        /// <summary>
        /// Main default tokens in VEFramework - CORZT
        /// </summary>
        public static string TokenId = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";
        /// <summary>
        /// CORZT token
        /// </summary>
        public static string CoruzantTokenId = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";
        /// <summary>
        /// Main default tokens symbol in VEFramework - VENFT
        /// </summary>
        private static string TokenSymbol = "CORZT";
        /// <summary>
        /// IPFS Client
        /// </summary>
        public static readonly IpfsClient ipfs = new IpfsClient(InfuraAPIURL);
        /// <summary>
        /// IPFS Client - special for Infura
        /// </summary>
        public static IpfsClient ipfsInfura = null;

        /// <summary>
        /// This event is called profile nft is found in the list of nfts
        /// </summary>
        public static event EventHandler<INFT> ProfileNFTFound;
        /// <summary>
        /// This event is called profile nft is found in the list of nfts
        /// </summary>
        public static event EventHandler<string> NFTLoadingStateChanged;


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
                nftType == NFTTypes.CoruzantArticle || 
                nftType == NFTTypes.XrayImage || 
                nftType == NFTTypes.Ticket)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Remove the server address from link and return just IPFS Hash
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static string GetHashFromIPFSLink(string link)
        {
            if (string.IsNullOrEmpty(link)) return string.Empty;
            var hash = link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty).Replace("https://ipfs.io/ipfs/", string.Empty).Replace("https://ipfs.infura.io/ipfs/", string.Empty).Replace(GatewayURL, string.Empty);
            return hash;
        }
        /// <summary>
        /// Get full IPFS link from the hash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string GetIPFSLinkFromHash(string? hash)
        {
            if (hash.Contains("http"))
                return hash;
            else
                return !string.IsNullOrEmpty(hash) ? string.Concat(GatewayURL, hash) : string.Empty;
        }
        /// <summary>
        /// Obsolete function - just example how to redirect upload through different server
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContentType"></param>
        /// <returns></returns>
        public static async Task<string> UploadImage(Stream stream, string fileName, string fileContentType = "multipart/form-data")
        {
            var link = string.Empty;
            try
            {
                var url = $"https://nftticketverifierapp.azurewebsites.net/api/upload";
                
                using var client = new HttpClient();
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(stream)
                    {
                        Headers =
                            {
                                ContentLength = stream.Length,
                                ContentType = new MediaTypeHeaderValue(fileContentType)
                            }
                    }, "file", fileName);
                    
                    var response = await client.PostAsync(url, content);
                    link = await response.Content.ReadAsStringAsync();
                    
                    var loaded = false;
                    var attempts = 20;
                    while (attempts > 0 && !loaded)
                    {
                        try
                        {
                            var respb = await IPFSDownloadFromInfuraAsync(GetHashFromIPFSLink(link));
                            if (respb != null)
                            {
                                var resp = new MemoryStream(respb);
                                if (resp != null && resp.Length >= (stream.Length * 0.8))
                                    loaded = true;
                                else
                                    await Task.Delay(1000);
                            }
                            /*
                            var resp = await ipfs.FileSystem.GetAsync(link.Replace("https://gateway.ipfs.io/ipfs/",string.Empty));
                            if (resp != null && resp.Length >= (stream.Length*0.8))
                                loaded = true;
                            else
                                await Task.Delay(1000);
                            */
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("File still not available: " + ex.Message);
                            await Task.Delay(1000);
                        }
                        attempts--;
                    }
                    
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot upload the image. " + ex.Message);
            }
            return link;
        }

        /// <summary>
        /// Load connection info to the internal variables of this class
        /// </summary>
        /// <param name="ipfsKey"></param>
        /// <param name="ipfsSecret"></param>
        /// <param name="apiurl"></param>
        public static void LoadConnectionInfo(string ipfsKey = "", string ipfsSecret = "", string apiurl = "")
        {
            var refresh = false;
            if (!string.IsNullOrEmpty(apiurl) && apiurl != InfuraAPIURL)
            {
                InfuraAPIURL = apiurl;
                refresh = true;
            }
            if (!string.IsNullOrEmpty(ipfsKey) && ipfsKey != InfuraKey)
            {
                InfuraKey = ipfsKey;
                refresh = true;
            }
            if (!string.IsNullOrEmpty(ipfsSecret) && ipfsSecret != InfuraSecret)
            {
                InfuraSecret = ipfsSecret;
                refresh = true;
            }
            try
            {
                if (refresh || ipfsInfura == null)
                    ipfsInfura = CreateIpfsClient(InfuraAPIURL, InfuraKey, InfuraSecret);
                ipfsInfura.UserAgent = "VEFramework";
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load ipfs client after apiurl, key and secret change. " + ex.Message);
            }

        }
        /// <summary>
        /// Create IPFS client with authentication
        /// </summary>
        /// <param name="IpfsHostUrl"></param>
        /// <param name="IpfsHostUserName"></param>
        /// <param name="IpfsHostPassword"></param>
        /// <returns></returns>
        public static IpfsClient CreateIpfsClient(string IpfsHostUrl, string IpfsHostUserName, string IpfsHostPassword)
        {
            var c = new IpfsClient(IpfsHostUrl);

            var httpClientInfo = typeof(IpfsClient).GetField("api", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var apiObj = httpClientInfo.GetValue(null);
            if (apiObj == null)
            {

                MethodInfo createMethod = typeof(IpfsClient).GetMethod("Api", BindingFlags.NonPublic | BindingFlags.Instance);
                var o = createMethod.Invoke(c, new Object[0]);
                var httpClient = o as HttpClient;

                var byteArray = Encoding.ASCII.GetBytes(IpfsHostUserName + ":" + IpfsHostPassword);
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                httpClient.DefaultRequestHeaders.Add("mode", "no-cors");
                httpClient.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
            }
            
            return c;
        }

        /// <summary>
        /// Upload file to the Infura IPFS
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContentType"></param>
        /// <returns></returns>
        public static async Task<string> UploadInfura(Stream stream, string fileName, string fileContentType = "multipart/form-data")
        {
            if (stream == null)
                return "Error. Provided null file.";
            try
            {
                if (stream.Length <= 0)
                    return string.Empty;
                var link = string.Empty;

                if (ipfsInfura == null)
                    ipfsInfura = CreateIpfsClient(InfuraAPIURL, InfuraKey, InfuraSecret);
                ipfsInfura.UserAgent = "VEFramework";
                var reslink = await ipfsInfura.FileSystem.AddAsync(stream, fileName);
                //var reslink = await ipfs.FileSystem.AddAsync(stream, fileName);
                
                if (reslink != null)
                {
                    var hash = reslink.ToLink().Id.ToString();
                    link = "https://ipfs.infura.io/ipfs/" + hash;

                    var loaded = false;
                    var attempts = 50;
                    while (attempts > 0 && !loaded)
                    {
                        try
                        {
                            //var resp = await ipfsClient.FileSystem.GetAsync(hash);
                            //var respb = await IPFSDownloadFromPublicAsync(hash);
                            var respb = await IPFSDownloadFromInfuraAsync(hash);
                            if (respb != null)
                            {
                                var resp = new MemoryStream(respb);
                                if (resp != null && resp.Length >= (stream.Length * 0.8))
                                    loaded = true;
                                else
                                    await Task.Delay(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("File still not available: " + ex.Message);                            
                            await Task.Delay(1000);
                        }
                        attempts--;
                    }
                }
                return link;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }
            return string.Empty;
        }

        /// <summary>
        /// Download file from IPFS Infura with use of credentials
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static async Task<byte[]> IPFSDownloadFromInfuraAsync(string hash)
        {
            var ipfsClient = CreateIpfsClient(InfuraAPIURL, InfuraKey, InfuraSecret);
            ipfsClient.UserAgent = "VEFramework";
            try
            {
                var cancelSource = new System.Threading.CancellationTokenSource();
                var token = cancelSource.Token;
                //using (var stream = await ipfsClient.FileSystem.ReadFileAsync(hash))
                using (var stream = await ipfsClient.PostDownloadAsync("cat", token, arg: hash))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot read the file from IPFS from Infura. " + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Pin file to IPFS Infura with use of credentials
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static async Task<bool> PinToInfuraAsync(string hash)
        {
            var ipfsClient = CreateIpfsClient(InfuraAPIURL, InfuraKey, InfuraSecret);
            ipfsClient.UserAgent = "VEFramework";
            try
            {
                var cancelSource = new System.Threading.CancellationTokenSource();
                var token = cancelSource.Token;
                //using (var stream = await ipfsClient.FileSystem.ReadFileAsync(hash))
                var _ = await ipfsClient.Pin.AddAsync(hash);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot read the file from IPFS from Infura. " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Download file from IPFS public
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static async Task<byte[]> IPFSDownloadFromPublicAsync(string hash)
        {
            ipfs.UserAgent = "VEFramework";
            try 
            { 
                using (var stream = await ipfs.FileSystem.ReadFileAsync(hash))
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot read the file from IPFS from public gateway. " + ex.Message);
            }
            return null;
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
        public static async Task<Dictionary<string, string>> CheckIfContainsNFTData(string utxo)
        {
            var meta = await NeblioAPIHelpers.GetTransactionMetadata(TokenId, utxo);

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
            var info = await NeblioAPIHelpers.GetTransactionInfo(utxo);

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
            var utxos = uts.OrderBy(u => u.Blocktime).Reverse().ToList();

            var ns = new List<INFT>();
            if (innfts != null)// && utxos.Count <= innfts.Count)
            {
                innfts.ToList().ForEach(n =>
                {
                    if (utxos.Any(u => (u.Txid == n.Utxo && u.Index == n.UtxoIndex)))
                        ns.Add(n);
                });
                innfts.Clear();
                innfts = ns.ToList();
            }

            var lastNFTTime = DateTime.MinValue;
            if (innfts != null && innfts.Count > 0)
                lastNFTTime = innfts.FirstOrDefault().Time;

            NFTLoadingStateChanged?.Invoke(address, "Loading of the NFTs Started.");

            foreach (var u in utxos)
            {
                if (maxLoadedItems > 0 && nfts.Count > maxLoadedItems) break;

                if (TimeHelpers.UnixTimestampToDateTime((double)u.Blocktime) > lastNFTTime)
                {
                    if (u.Tokens != null && u.Tokens.Count > 0)
                    {
                        foreach (var t in u.Tokens)
                        {
                            if (t.Amount == 1)
                            {
                                try
                                {
                                    INFT nft = null;
                                    if (!withoutMessages)
                                    {
                                        if (!justMessages && !justPayments)
                                            nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, address:address);
                                        else if (justMessages && !justPayments)
                                            nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait:true, loadJustType:true, justType:NFTTypes.Message, address: address);
                                        else if (!justMessages && justPayments)
                                            nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, loadJustType:true, justType:NFTTypes.Payment, address: address);
                                    }
                                    else
                                    {
                                        nft = await NFTFactory.GetNFT(t.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, wait: true, skipTheType: true, skipType: NFTTypes.Message, address: address);
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
                                catch(Exception ex)
                                {
                                    Console.WriteLine("Some trouble with loading NFT." + ex.Message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (innfts == null)
                return nfts;
            else
                nfts.ForEach(n => {
                    if (!innfts.Any(i => i.Utxo == n.Utxo && i.UtxoIndex == n.UtxoIndex))
                        innfts.Add(n);
                });
            //NFTLoadingStateChanged?.Invoke(address, "All NFTs Loaded.");
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

    }
}
