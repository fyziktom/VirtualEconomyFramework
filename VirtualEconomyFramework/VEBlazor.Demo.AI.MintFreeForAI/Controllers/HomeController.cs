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
using VEBlazor.Demo.AI.MintFreeForAI.Common;
using VEDriversLite.NFT.Dto;
using VEDriversLite.StorageDriver.Helpers;
using VEDriversLite.AI.OpenAI.Dto;
using System.IO;
using Newtonsoft.Json;
using Blazorise;

namespace VEBlazor.Demo.AI.MintFreeForAI.Controllers
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
            /// Input NFT Link
            /// </summary>
            public List<NFTDataItem> dataitems { get; set; } = new List<NFTDataItem>();
            /// <summary>
            /// Additional info such as original texts, etc.
            /// </summary>
            public AdditionalInfo additionalinfo { get; set; } = new AdditionalInfo();
        }

        public class AdditionalInfo
        {
            public string OrigUserBaseText { get; set; } = string.Empty;
            public string OrigAIText { get; set; } = string.Empty;
            public string OrigAIName { get; set; } = string.Empty;
            public string OrigAIDescription { get; set; } = string.Empty;
            public string OrigAITags { get; set; } = string.Empty;

        }
        [HttpPost]
        [Route("MintPostNFT")]
        public async Task<string> MintPostNFT([FromBody] VEDLMintPostNFTDto data)
        {
            try
            {
                var nft = new PostNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Text = data.text.Replace("%0A","\n");
                nft.Author = data.author;
                nft.Tags = data.tags;
                nft.DataItems = data.dataitems;

                data.additionalinfo.OrigUserBaseText = data.additionalinfo.OrigUserBaseText.Replace("%0A", "\n");
                data.additionalinfo.OrigAIText = data.additionalinfo.OrigAIText.Replace("%0A", "\n");

                var addInfo = JsonConvert.SerializeObject(data.additionalinfo);
                var file = System.Text.Encoding.UTF8.GetBytes(addInfo);
                using (Stream ms = new MemoryStream(file))
                {
                    ms.Seek(0, SeekOrigin.Begin); 
                    var result = await VEDLDataContext.Storage.SaveFileToIPFS(new VEDriversLite.StorageDriver.StorageDrivers.Dto.WriteStreamRequestDto()
                    {
                        Data = ms,
                        Filename = "additionalInfo.json",
                        DriverType = VEDriversLite.StorageDriver.StorageDrivers.StorageDriverType.IPFS,
                        BackupInLocal = false
                    });
                    if (result.Item1)
                    {
                        var hash = IPFSHelpers.GetHashFromIPFSLink(result.Item2);
                        nft.DataItems.Add(new NFTDataItem()
                        {
                            Hash = hash,
                            Storage = DataItemStorageType.IPFS,
                            Type = DataItemType.JSON,
                            TagsList = new List<string>() { "AdditionalInfo" }
                        });
                    }
                }

                var it = data.dataitems.FirstOrDefault(i => i.IsMain);
                if (it != null)
                    nft.ImageLink = IPFSHelpers.GetIPFSLinkFromHash(it.Hash);

                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(MainDataContext.MainAccount, out var account))
                {
                    res = await account.MintNFT(nft, data.receiver);
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
                res = await MainDataContext.Assistant.SendSimpleQuestion(data.basetext, 500);

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
