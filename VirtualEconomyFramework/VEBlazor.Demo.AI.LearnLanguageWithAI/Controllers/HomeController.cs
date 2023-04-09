using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Admin.Dto;
using VEDriversLite.NFT;
using static VEDriversLite.AccountHandler;
using VEDriversLite.NeblioAPI;
using VEBlazor.Demo.AI.LearnLanguageWithAI.Common;
using VEDriversLite.NFT.Dto;
using VEDriversLite.StorageDriver.Helpers;
using VEDriversLite.AI.OpenAI.Dto;
using System.IO;
using Newtonsoft.Json;
using Blazorise;
using NBitcoin;

namespace VEBlazor.Demo.AI.LearnLanguageWithAI.Controllers
{
    [Route("api")]
    [ApiController]
    public class HomeController : Controller
    {

        #region NFTs

        /// <summary>
        /// Get NFT by utxo and index
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("GetNFT/{utxo}/{index}")]
        public async Task<INFT> GetNFT(string utxo, int index)
        {
            try
            {
                if (!string.IsNullOrEmpty(utxo))
                {
                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, index, 0, true);
                    return nft;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get NFT {utxo}!");
            }
        }

        /// <summary>
        /// Get NFT by utxo (0 index)
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("GetNFT/{utxo}")]
        public async Task<INFT> GetNFT(string utxo)
        {
            try
            {
                if(!string.IsNullOrEmpty(utxo))
                {
                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, 0, 0, true);
                    return nft;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get NFT {utxo}!");
            }
        }

        /// <summary>
        /// Get Last lessons of the spain
        /// </summary>
        /// <returns>NFTs list</returns>
        [HttpGet]
        [Route("GetLastLessons/{language}")]
        public async Task<List<INFT>> GetLastLessons(string language)
        {
            try
            {
                var lang = Languages.cz2es;

                try
                {
                    var l = (Languages)Enum.Parse(typeof(Languages), language);
                    if (l != null)
                        lang = l;
                }
                catch(Exception ex)
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Wrong language input!");
                }

                if (MainDataContext.MintedNFTs.Count > 20)
                {
                    return MainDataContext.MintedNFTs.Values.ToList();
                }
                else
                {
                    if (VEDLDataContext.Accounts.TryGetValue(MainDataContext.MainAccount, out var account))
                    {
                        if (lang == Languages.cz2es)
                        {
                            var nfts = account.NFTs.Where(n => n.Type == NFTTypes.Post && (n.Tags.Contains(language) || (n.Tags.Contains("lekce") && n.Tags.Contains("španělština") && n.Tags.Contains("čeština")))).ToList();
                            if (nfts != null && nfts.Count > 0)
                                return nfts;
                        }
                        else if (lang == Languages.en2es)
                        {
                            var nfts = account.NFTs.Where(n => n.Type == NFTTypes.Post && (n.Tags.Contains(language) || (n.Tags.Contains("lesson") && n.Tags.Contains("spanish") && n.Tags.Contains("english")))).ToList();
                            if (nfts != null && nfts.Count > 0)
                                return nfts;
                        }
                        else
                        {
                            var nfts = account.NFTs.Where(n => n.Type == NFTTypes.Post && n.Tags.Contains(language)).ToList();
                            if (nfts != null && nfts.Count > 0)
                                return nfts;
                        }
                    }

                    return new List<INFT>();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Lessons!");
            }
        }

        public class VEDLMintPostNFTDto
        {
            /// <summary>
            /// Receiver of NFT 
            /// </summary>
            public string receiver { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Name
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Tags
            /// </summary>
            public string tags { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Text
            /// </summary>
            public string text { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// selected language
            /// </summary>
            public Languages language { get; set; } = Languages.cz2es;
            /// <summary>
            /// Input NFT Link
            /// </summary>
            public List<NFTDataItem> dataitems { get; set; } = new List<NFTDataItem>();
        }

        [HttpPost]
        [Route("MintPostNFT")]
        public async Task<string> MintPostNFT([FromBody] VEDLMintPostNFTDto data)
        {
            try
            {
                var nft = new PostNFT("");
                nft.Text = data.text.Replace("%0A","\n");
                nft.Author = data.author;

                nft.DataItems.Add(new NFTDataItem()
                {
                    Hash = "QmaNAgTmG53Ec9BZERreXpsEKkRE4fSCYQzLGWYQNCdToF",
                    IsMain = true,
                    Storage = DataItemStorageType.IPFS,
                    Type = DataItemType.Image
                });

                var tx = nft.Text;
                if (tx.Length > 500)
                    tx = tx.Substring(0, 500);

                var txt = await MainDataContext.Assistant.GetNewDataForNFT(tx);
                if (txt.Item1)
                {
                    nft.Name = txt.Item2.Name;
                    nft.Tags = txt.Item2.Tags.Replace("#", string.Empty);
                    nft.Description = txt.Item2.Description;
                }

                var lang = Enum.GetName(typeof(Languages), data.language);
                if (!nft.Tags.Contains(lang))
                    nft.Tags += $" {lang}";

                if (data.language == Languages.cz2es)
                {
                    if (!nft.Tags.Contains("lekce"))
                        nft.Tags += " lekce";
                    if (!nft.Tags.Contains("španělština"))
                        nft.Tags += " španělština";
                    if (!nft.Tags.Contains("čeština"))
                        nft.Tags += " čeština";
                }
                else if (data.language == Languages.en2es)
                {
                    if (!nft.Tags.Contains("lesson"))
                        nft.Tags += " lesson";
                    if (!nft.Tags.Contains("spanish"))
                        nft.Tags += " spanish";
                    if (!nft.Tags.Contains("english"))
                        nft.Tags += " english";
                }
                else if (data.language == Languages.de2es)
                {
                    if (!nft.Tags.Contains("lektion"))
                        nft.Tags += " lektion";
                    if (!nft.Tags.Contains("spanisch"))
                        nft.Tags += " spanisch";
                    if (!nft.Tags.Contains("deutch"))
                        nft.Tags += " deutch";
                }
                else if (data.language == Languages.nl2es)
                {
                    if (!nft.Tags.Contains("les"))
                        nft.Tags += " les";
                    if (!nft.Tags.Contains("spaans"))
                        nft.Tags += " spaans";
                    if (!nft.Tags.Contains("nederlands"))
                        nft.Tags += " nederlands";
                }

                nft.Price = 0.5;
                nft.SellJustCopy = true;

                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(MainDataContext.MainAccount, out var account))
                {
                    var rcv = NeblioTransactionHelpers.ValidateNeblioAddress(data.receiver);

                    res = await account.MintNFT(nft, rcv);
                    await Task.Delay(500);
                    var tnft = await NFTFactory.GetNFT(NFTHelpers.TokenId, res.Item2, 0, 0, true);
                    if (tnft != null)
                        MainDataContext.MintedNFTs.TryAdd(tnft.Utxo, tnft);
                }
                else
                    res.Item2 = "Cannot find MainAccount.";
                
                return res.Item2;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot mint NFT!");
            }
        }


        #endregion

        #region Assistant

        public class AIGetTextDto
        {
            /// <summary>
            /// Base Text
            /// </summary>
            public string basetext { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("AIGetText")]
        public async Task<string> AIGetText([FromBody] AIGetTextDto data)
        {
            try
            {
                if (MainDataContext.Assistant == null)
                    throw new HttpResponseException((HttpStatusCode)501, $"Assistant is out of the service. Try later please.");

                (bool, string) res = (false, string.Empty);
                res = await MainDataContext.Assistant.SendSimpleQuestion(data.basetext, 1000);

                return res.Item2;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot mint NFT!");
            }
        }

        public class AIGetNFTDataDto
        {
            /// <summary>
            /// NFT Text
            /// </summary>
            public string text { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("AIGetNFTData")]
        public async Task<NewDataForNFTResult> AIGetNFTData([FromBody] AIGetNFTDataDto data)
        {
            try
            {
                if (MainDataContext.Assistant == null)
                    throw new HttpResponseException((HttpStatusCode)501, $"Assistant is out of the service. Try later please.");

                var res = await MainDataContext.Assistant.GetNewDataForNFT(data.text);

                return res.Item2;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot mint NFT!");
            }
        }

        public class AIGetNFTImagesDto
        {
            /// <summary>
            /// NFT Name
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// NFT Text
            /// </summary>
            public string text { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("AIGetNFTImages")]
        public async Task<List<NFTDataItem>> AIGetNFTImages([FromBody] AIGetNFTImagesDto data)
        {
            try
            {
                if (MainDataContext.Assistant == null)
                    throw new HttpResponseException((HttpStatusCode)501, $"Assistant is out of the service. Try later please.");

                var result = new List<NFTDataItem>();

                var dalleRequest = string.Empty;
                var baseForImage = await MainDataContext.Assistant.SendSimpleQuestion($"Potřebuji od AI obrázek k textu. Napiš pro AI prosím stručné zadání v angličtině. Délka zadání maximálně dvě věty. Chci obdržet jako výsledek jenom zadání. Zdrojový text: \"{data.text}\".", 100);
                if (baseForImage.Item1)
                    dalleRequest = baseForImage.Item2.Replace("\n", " ");
                else
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot create request for DALL-E. Please try it again.");

                var responseForImage = await MainDataContext.Assistant.GetImageForText($"{dalleRequest}");
                if (responseForImage.Item1 && responseForImage.Item2.Count > 0)
                {
                    foreach (var img in responseForImage.Item2)
                    {
                        var item = new VEDriversLite.NFT.Dto.NFTDataItem()
                        {
                            Storage = DataItemStorageType.IPFS,
                            IsMain = true,
                            Type = DataItemType.Image
                        };
                        var link = string.Empty;

                        try
                        {
                            await Console.Out.WriteLineAsync("Uploading image to IPFS...");
                            var bytes = Convert.FromBase64String(img);
                            using (Stream stream = new MemoryStream(bytes))
                            {
                                //Request IPFS upload with StorageDriver
                                var res = await VEDriversLite.VEDLDataContext.Storage.SaveFileToIPFS(new VEDriversLite.StorageDriver.StorageDrivers.Dto.WriteStreamRequestDto()
                                {
                                    Data = stream,
                                    Filename = $"{DateTime.UtcNow.ToString("yyyy_MM_ddThh_mm_ss")}-{data.name.Replace(' ', '_')}.png",
                                    DriverType = VEDriversLite.StorageDriver.StorageDrivers.StorageDriverType.IPFS,
                                });

                                if (res.Item1)
                                {
                                    await Console.Out.WriteLineAsync("Upload finished.");
                                    await Console.Out.WriteLineAsync("Image Link: " + res.Item2);

                                    // store link to image for later use
                                    link = res.Item2;

                                    // store IPFS hash of image in DataItem
                                    var hash = VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GetHashFromIPFSLink(res.Item2);
                                    if (!string.IsNullOrEmpty(hash))
                                        item.Hash = hash;
                                }
                                else
                                    Console.WriteLine("Cannot upload. " + res.Item2);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync("Cannot upload Image to IPFS. " + ex.Message);
                        }

                        if (!string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(item.Hash))
                        {
                            if (result.FirstOrDefault(d => d.IsMain) != null)
                                item.IsMain = false;

                            result.Add(item);
                        }
                    }

                    if (result.Count > 0)
                        return result;
                }
                else
                {
                    await Console.Out.WriteLineAsync("Cannot create images.");
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot create images!");
            }

            return new List<NFTDataItem>();
        }

        #endregion

    }
}
