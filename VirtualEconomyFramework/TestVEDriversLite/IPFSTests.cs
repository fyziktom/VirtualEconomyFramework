using Ipfs.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;

namespace TestVEDriversLite
{
    public static class IPFSTests
    {
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
        public static string InfuraAPIURL = "https://venftappserver.azurewebsites.net/api/uploadtoipfs";//"http://20.225.145.84:5001";//"https://ipfs.infura.io:5001";
        /// <summary>
        /// IPFS Gateway address
        /// </summary>
        public static string GatewayURL = "https://ipfs.io/ipfs/";//"https://bdp.infura-ipfs.io/ipfs/";
        /// <summary>
        /// IPFS Client
        /// </summary>
        public static readonly IpfsClient ipfs = new IpfsClient(InfuraAPIURL);
        /// <summary>
        /// IPFS Client - special for Infura
        /// </summary>
        public static IpfsClient ipfsInfura = null;

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
            catch (Exception ex)
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
                    link = GatewayURL + hash;

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
            catch (Exception ex)
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
                var rp = await ipfsClient.Pin.ListAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot read the file from IPFS from Infura. " + ex.Message);
            }
            return false;
        }


        [TestEntry]
        public static void IPFSFileUpload(string param)
        {
            IPFSFileUploadAsync(param);
        }
        public static async Task IPFSFileUploadAsync(string param)
        {
            //var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //if (split.Length < 3)
            //    throw new Exception("Please input filename");
            //var fileName = split[0];
            var filebytes = File.ReadAllBytes(param);
            var link = string.Empty;
            try
            {
                using (Stream stream = new MemoryStream(filebytes))
                {
                    var imageLink = await UploadInfura(stream, param);
                    Console.WriteLine("Image Link: " + imageLink);
                    //var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, fileName);
                    //link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }

        }

        [TestEntry]
        public static void IPFSAzureFileUpload(string param)
        {
            IPFSAzureFileUploadAsync(param);
        }
        public static async Task IPFSAzureFileUploadAsync(string param)
        {
            //var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //if (split.Length < 3)
            //    throw new Exception("Please input filename");
            //var fileName = split[0];
            var url = "http://20.225.145.84:5001";
            InfuraAPIURL = url;
            var filebytes = File.ReadAllBytes(param);
            var link = string.Empty;
            try
            {
                using (Stream stream = new MemoryStream(filebytes))
                {
                    var imageLink = await UploadInfura(stream, param);
                    Console.WriteLine("Image Link: " + imageLink);
                    //var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, fileName);
                    //link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }

        }

        [TestEntry]
        public static void IPFSTransferBetweenNodes(string param)
        {
            IPFSTransferBetweenNodesAsync(param);
        }
        public static async Task IPFSTransferBetweenNodesAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input tokenId");
            var tokenId = split[0];


            var owners = await NeblioAPIHelpers.GetTokenOwners("La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7");

            var ipfsCIDs = new List<string>();
            Console.WriteLine("Starting searching the owners:");
            Console.WriteLine("--------------------------------");
            foreach (var own in owners)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Owner {own.Address}. Loading NFTS...");
                var addnfts = await NFTHelpers.LoadAddressNFTs(own.Address);
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"----------{addnfts.Count} NFT Loaded------------");
                foreach (var nft in addnfts)
                {
                    if (!string.IsNullOrEmpty(nft.ImageLink) || nft.DataItems.Count > 0)
                    {
                        var lnk = VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetHashFromIPFSLink(nft.Link);
                        if (!string.IsNullOrEmpty(lnk))
                            if (!ipfsCIDs.Contains(lnk))
                                ipfsCIDs.Add(lnk);
                        var imagelink = VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetHashFromIPFSLink(nft.ImageLink);
                        if (!string.IsNullOrEmpty(imagelink))
                            if (!ipfsCIDs.Contains(imagelink))
                                ipfsCIDs.Add(imagelink);

                        foreach (var item in nft.DataItems)
                        {
                            if (item.Storage == VEDriversLite.NFT.Dto.DataItemStorageType.IPFS && !string.IsNullOrEmpty(item.Hash))
                                if (!ipfsCIDs.Contains(item.Hash))
                                    ipfsCIDs.Add(item.Hash);
                        }

                    }

                }
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"-------Processing of address {own.Address} done---------");
            }

            foreach (var cid in ipfsCIDs)
            {
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"-------Processing of CID {cid} done---------");

                InfuraAPIURL = "https://ipfs.infura.io:5001";
                GatewayURL = "https://bdp.infura-ipfs.io/ipfs/";

                var downloadedbytes = await IPFSDownloadFromInfuraAsync(cid);


                InfuraAPIURL = "http://20.225.145.84:5001";
                GatewayURL = "https://ipfs.io/ipfs/";

                //var filebytes = File.ReadAllBytes(param);
                var link = string.Empty;
                try
                {
                    using (Stream stream = new MemoryStream(downloadedbytes))
                    {
                        var imageLink = await UploadInfura(stream, "");
                        Console.WriteLine("Image Link: " + imageLink);
                        //var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, fileName);
                        //link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
                }
            }
        }

        [TestEntry]
        public static void IPFSTransferFileBetweenNodes(string param)
        {
            IPFSTransferFileBetweenNodesAsync(param);
        }
        public static async Task IPFSTransferFileBetweenNodesAsync(string param)
        {

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine($"-------Processing of CID {param} done---------");

            InfuraAPIURL = "https://ipfs.infura.io:5001";
            GatewayURL = "https://bdp.infura-ipfs.io/ipfs/";

            var downloadedbytes = await IPFSDownloadFromInfuraAsync(param);

            InfuraAPIURL = "http://20.225.145.84:5001";
            GatewayURL = "https://ipfs.io/ipfs/";

            //var filebytes = File.ReadAllBytes(param);
            var link = string.Empty;
            try
            {
                using (Stream stream = new MemoryStream(downloadedbytes))
                {
                    var imageLink = await UploadInfura(stream, "");
                    Console.WriteLine("Image Link: " + imageLink);
                    //var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, fileName);
                    //link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }
        }

        public class IPFSLinksNFTsDto
        {
            public string Address { get; set; } = string.Empty;
            public string Utxo { get; set; } = string.Empty;
            public int Index { get; set; } = 0;
            public string Link { get; set; } = string.Empty;
            public string ImageLink { get; set; } = string.Empty;
            public string PodcastLink { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public NFTTypes Type { get; set; } = NFTTypes.Image;
        }
        [TestEntry]
        public static void GetAllIpfsLinks(string param)
        {
            GetAllIpfsLinksAsync(param);
        }
        public static async Task GetAllIpfsLinksAsync(string param)
        {

            var owners = await NeblioAPIHelpers.GetTokenOwners(NFTHelpers.TokenId);

            var ipfsLinkNFTs = new Dictionary<string, IPFSLinksNFTsDto>();
            var ipfsCIDs = new List<string>();
            Console.WriteLine("Starting searching the owners:");
            Console.WriteLine("--------------------------------");
            foreach (var own in owners)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Owner {own.Address}. Loading NFTS...");
                var addnfts = await NFTHelpers.LoadAddressNFTs(own.Address);
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"----------{addnfts.Count} NFT Loaded------------");
                foreach (var nft in addnfts)
                {
                    if (!string.IsNullOrEmpty(nft.Link) || !string.IsNullOrEmpty(nft.ImageLink))
                    {
                        var save = false;
                        if (!string.IsNullOrEmpty(nft.Link))
                            if (nft.Link.Contains("https://gateway.ipfs.io/ipfs/"))
                                save = true;
                        if (!string.IsNullOrEmpty(nft.ImageLink))
                            if (nft.ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
                                save = true;

                        if (save)
                        {
                            var dto = new IPFSLinksNFTsDto()
                            {
                                Address = own.Address,
                                Utxo = nft.Utxo,
                                Index = nft.UtxoIndex,
                                Link = nft.Link,
                                ImageLink = nft.ImageLink,
                                Name = nft.Name,
                                Type = nft.Type
                            };
                            if (dto.Link == null)
                                dto.Link = string.Empty;
                            if (dto.ImageLink == null)
                                dto.ImageLink = string.Empty;

                            if (ipfsLinkNFTs.Values.FirstOrDefault(n => n.Link == dto.Link) != null)
                                dto.Link = string.Empty;
                            if (ipfsLinkNFTs.Values.FirstOrDefault(n => n.ImageLink == dto.ImageLink) != null)
                                dto.ImageLink = string.Empty;
                            if (ipfsLinkNFTs.Values.FirstOrDefault(n => n.PodcastLink == dto.PodcastLink) != null)
                                dto.PodcastLink = string.Empty;

                            if (nft.Type == NFTTypes.CoruzantProfile || nft.Type == NFTTypes.CoruzantArticle || nft.Type == NFTTypes.CoruzantPodcast)
                                dto.PodcastLink = (nft as CommonCoruzantNFT).PodcastLink;

                            ipfsLinkNFTs.Add($"{nft.Utxo}:{nft.UtxoIndex}", dto);
                        }
                    }

                }
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"-------Processing of address {own.Address} done---------");
            }

            var filename = $"{TimeHelpers.DateTimeToUnixTimestamp(DateTime.UtcNow)}-ipfsLinkNFTs.txt";
            Console.WriteLine($"Completed search. Saving file. {filename}");

            foreach (var link in ipfsLinkNFTs)
            {

                if (link.Value.ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
                {
                    var a = link.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty);
                    if (!ipfsCIDs.Contains(a))
                        //{
                        //ipfsCIDs.Add(a);
                        FileHelpers.AppendLineToTextFile(a, filename);
                    //}
                }
                if (link.Value.Link.Contains("https://gateway.ipfs.io/ipfs/"))
                {
                    var a = link.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty);
                    //if (!ipfsCIDs.Contains(a))
                    //{
                    //ipfsCIDs.Add(a);
                    FileHelpers.AppendLineToTextFile(a, filename);
                    //}
                }

            }
            /*
            var ipfs = new Ipfs.Http.IpfsClient("http://127.0.0.1:5001");
            foreach (var nft in ipfsLinkNFTs)
            {
                try
                {

                    if (!string.IsNullOrEmpty(nft.Value.ImageLink) && nft.Value.ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
                    {
                        Console.WriteLine("Pinning...");
                        await ipfs.Pin.AddAsync(nft.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                        Console.WriteLine("Pinned. " + nft.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Cannot pin " + nft.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                }
                try
                {
                    if (!string.IsNullOrEmpty(nft.Value.Link) && nft.Value.Link.Contains("https://gateway.ipfs.io/ipfs/"))
                    {
                        Console.WriteLine("Pinning...");
                        await ipfs.Pin.AddAsync(nft.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                        Console.WriteLine("Pinned. " + nft.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Cannot pin " + nft.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                }
                try
                {
                    if (!string.IsNullOrEmpty(nft.Value.PodcastLink) && nft.Value.PodcastLink.Contains("https://gateway.ipfs.io/ipfs/"))
                    {
                        Console.WriteLine("Pinning...");
                        await ipfs.Pin.AddAsync(nft.Value.PodcastLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                        Console.WriteLine("Pinned. " + nft.Value.PodcastLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Cannot pin " + nft.Value.PodcastLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                }
            }
            */

            //var filename = $"{TimeHelpers.DateTimeToUnixTimestamp(DateTime.UtcNow)}-ipfsLinkNFTs.json";
            //Console.WriteLine($"Completed search. Saving file. {filename}");
            //var output = JsonConvert.SerializeObject(ipfsCIDs, Formatting.Indented);
            //FileHelpers.WriteTextToFile(filename, output);
        }

        [TestEntry]
        public static void PinIPFSFile(string param)
        {
            PinIPFSFileAsync(param);
        }
        public static async Task PinIPFSFileAsync(string param)
        {
            var ipfs = new Ipfs.Http.IpfsClient("http://127.0.0.1:5001");

            try
            {
                Console.WriteLine("Start Pinning...");
                var res = await ipfs.Pin.AddAsync(param);
                Console.WriteLine("Pinned.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Cannot pin " + param + ". " + ex.Message);
            }

        }
    }
}