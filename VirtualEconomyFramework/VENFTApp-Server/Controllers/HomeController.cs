using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NBitcoin;
using Newtonsoft.Json;
using VENFTApp_Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Dto;
using VEDriversLite.NFT;
using VEDriversLite.Admin.Dto;
using VEDriversLite.Admin;
using VEDriversLite.NFT.Dto;
using System.IO;
using Ipfs;

namespace VENFTApp_Server.Controllers
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // We'd normally just use "*" for the allow-origin header, 
            // but Chrome (and perhaps others) won't allow you to use authentication if
            // the header is set to "*".
            // TODO: Check elsewhere to see if the origin is actually on the list of trusted domains.
            var ctx = filterContext.HttpContext;
            //var origin = ctx.Request.Headers["Origin"];
            //var allowOrigin = !string.IsNullOrWhiteSpace(origin) ? origin : "*";
            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            ctx.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            base.OnActionExecuting(filterContext);
        }
    }

    public class IPFSResponse
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Size { get; set; }
    }

    [Route("api")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpPost]
        [AllowCrossSiteJsonAttribute]
        [Route("upload")]
        public async Task<string> Upload(IFormFile file)
        {
            if (file == null)
                return "Error. Provided null file.";

            using var stream = file.OpenReadStream();
            try
            {
                var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, file.FileName);
                //var imageLink = await NFTHelpers.UploadInfura(stream, file.FileName);
                var link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                return link;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }
            return string.Empty;
        }

        [HttpPost]
        [AllowCrossSiteJsonAttribute]
        [Route("uploadinfura")]
        public async Task<string> UploadInfura(IFormFile file)
        {
            if (file == null)
                return "Error. Provided null file.";

            using var stream = file.OpenReadStream();
            try
            {
                var imageLink = await NFTHelpers.UploadInfura(stream, file.FileName);
                return imageLink;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }
            return string.Empty;
        }

        /*
        [HttpPost]
        [AllowCrossSiteJsonAttribute]
        [Route("uploadinfura")]
        public async Task<string> UploadInfura(IFormFile file)
        {
            if (file == null)
                return "Error. Provided null file.";

            using var stream = file.OpenReadStream();
            try
            {
                var id = MainDataContext.IpfsProjectID;
                var secret = MainDataContext.IpfsSecret;

                if (file.Length <= 0)
                    return string.Empty;
                var link = string.Empty;
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri($"https://{id}:{secret)@ipfs.infura.io:5001/api/v0/add";
                    var url = $"https://{id}:{secret}@ipfs.infura.io:5001/api/v0/add";

                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StreamContent(file.OpenReadStream())
                        {
                            Headers =
                            {
                                ContentLength = file.Length,
                                ContentType = new MediaTypeHeaderValue(file.ContentType)
                            }
                        }, "File", fileName);

                        var response = await client.PostAsync(url, content);
                        var ipfsresponse = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<IPFSResponse>(ipfsresponse);
                        if (res != null)
                        {
                            link = "https://gateway.ipfs.io/ipfs/" + res.Hash;
                        }
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
        */

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetAccountBalance/{address}")]
        public string GetAccountBalance(string address)
        {
            if (VEDLDataContext.Accounts.TryGetValue(address, out var account))
                return account.TotalBalance.ToString() + " NEBL";
            else
                return "0.0 NEBL";
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetVENFTOwners")]
        public async Task<IDictionary<string,TokenOwnerDto>> GetVENFTOwners()
        {
            return MainDataContext.VENFTTokenOwners;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetPublicSellNFTs")]
        public async Task<IDictionary<string, INFT>> GetPublicSellNFTs()
        {
            return MainDataContext.PublicSellNFTs;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetPublicSellNFTsPage/{pageNumber}")]
        public async Task<List<INFT>> GetPublicSellNFTsPage(int pageNumber)
        {
            var itemStart = pageNumber * 25;
            if ((itemStart + 25) < MainDataContext.PublicSellNFTsList.Count)
            {
                return MainDataContext.PublicSellNFTsList.GetRange(itemStart, 25);
            }
            else if (((itemStart + 25) >= MainDataContext.PublicSellNFTsList.Count) && itemStart < MainDataContext.PublicSellNFTsList.Count)
            {
                var diff = MainDataContext.PublicSellNFTsList.Count - itemStart;
                return MainDataContext.PublicSellNFTsList.GetRange(itemStart, diff);
            }

            return new List<INFT>();
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetAddressTabNFTs/{address}")]
        public async Task<List<INFT>> GetAddressTabNFTs(string address)
        {
            var res = new List<INFT>();
            if (MainDataContext.VENFTOwnersTabs.TryGetValue(address, out var tab))
            {
                tab.NFTs.ForEach(n => {
                    n.TxDetails = null;
                    res.Add(n); 
                });
            }
            return res;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetStoredTabsAddresses")]
        public async Task<List<string>> GetStoredTabsAddresses()
        {
            return MainDataContext.VENFTOwnersTabs.Keys.ToList();
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetNFT/{utxo}")]
        public async Task<INFT> GetNFT(string utxo)
        {
            if (!MainDataContext.NFTs.TryGetValue(utxo, out var nft))
                nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, 0, 0, true);
            return nft;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetNFT/{utxo}/{index}")]
        public async Task<INFT> GetNFT(string utxo, int index)
        {
            if (!MainDataContext.NFTs.TryGetValue(utxo, out var nft))
                nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, index, 0, true);
            return nft;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetAllNFTsBasicInfo/")]
        public async Task<List<NFTHash>> GetAllNFTsBasicInfo()
        {
            return VEDLDataContext.NFTHashs.Values.ToList();
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetNFTBasicInfo/{shorthash}")]
        public async Task<NFTHash> GetAllNFTsBasicInfo(string shorthash)
        {
            if (VEDLDataContext.NFTHashs.TryGetValue(shorthash, out var nfth))
                return nfth;
            else
                return new NFTHash();
        }
        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetNFTByShortHash/{shorthash}")]
        public async Task<INFT> GetNFTByShortHash(string shorthash)
        {
            if (VEDLDataContext.NFTHashs.TryGetValue(shorthash, out var nfth))
                return await GetNFT(nfth.TxId, nfth.Index);
            else
                return new ImageNFT("");
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetShortenHash/{txid}/{index}")]
        public async Task<string> GetShortenHash(string txid, int index)
        {
            return NFTHash.GetShortHash(txid, index);
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("RequestAdminAction/{admin}/{address}/{actionType}")]
        public async Task<string> RequestAdminAction(string admin, string address, AdminActionTypes actionType)
        {
            try
            {
                var msg = string.Empty;
                if (VEDLDataContext.Accounts.TryGetValue(admin, out var acc))
                {
                    var areq = AdminActionFactory.GetAdminAction(admin, actionType, address);
                    if (areq == null)
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot create this type of action.");
                    msg = areq.CreateNewMessage();
                    VEDLDataContext.AdminActionsRequests.TryAdd(areq.Message, areq);
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot find admin account.");
                }

                if (!string.IsNullOrEmpty(msg))
                    return msg;
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Create Admin Action Request!" + ex.Message);
            }
        }

        public class CreateNewEmptyAccountDto
        {
            /// <summary>
            /// Admin credentials info
            /// Include Admin Address, Message and Signature of this message
            /// </summary>
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();

            /// <summary>
            /// Input address
            /// </summary>
            public string address { get; set; } = string.Empty;
        }
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("CreateNewEmptyNeblioAccount")]
        public async Task<(bool, string)> CreateNewEmptyNeblioAccount([FromBody] CreateNewEmptyAccountDto data)
        {
            try
            {
                var res = await AccountHandler.AddEmptyNeblioAccount(data.adminCredentials, data.address, MainDataContext.IsAPIWithCredentials);
                if (res)
                    return (true, "OK");
                else
                    return (false, "Cannot add new account.");
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Add new empty neblio account!" + ex.Message);
            }
        }

        public class CreateReadOnlyAccountDto
        {
            /// <summary>
            /// Admin credentials info
            /// Include Admin Address, Message and Signature of this message
            /// </summary>
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();

            /// <summary>
            /// Input address
            /// </summary>
            public string address { get; set; } = string.Empty;
        }
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("CreateReadOnlyNeblioAccount")]
        public async Task<(bool, string)> CreateReadOnlyNeblioAccount([FromBody] CreateReadOnlyAccountDto data)
        {
            try
            {
                var res = await AccountHandler.AddReadOnlyNeblioAccount(data.adminCredentials, data.address, MainDataContext.IsAPIWithCredentials);
                if (res)
                    return (true, "OK");
                else
                    return (false, "Cannot add new account.");
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Add new empty neblio account!" + ex.Message);
            }
        }

        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("ImportBackup")]
        public async Task<string> ImportBackup([FromBody] ImportBackupDto data)
        {
            try
            {
                var res = await AccountHandler.LoadVENFTBackup(data.adminCredentials, data.dto, MainDataContext.IsAPIWithCredentials);
                if (res)
                    return "OK";
                else
                    return "Cannot Import VENFT Backup Data.";
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Import VENFT Backup Data. " + ex.Message);
            }
        }

        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("ConnectDogeAccount")]
        public async Task<string> ConnectDogeAccount([FromBody] ConnectDogeAccountDto data)
        {
            try
            {
                var res = await AccountHandler.ConnectDogeAccount(data.adminCredentials, data, MainDataContext.IsAPIWithCredentials);
                if (res)
                    return "OK";
                else
                    return "Cannot Connect doge account.";
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Connect doge account. " + ex.Message);
            }
        }
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("DisconnectDogeAccount")]
        public async Task<string> DisconnectDogeAccount([FromBody] ConnectDogeAccountDto data)
        {
            try
            {
                var res = await AccountHandler.DisconnectDogeAccount(data.adminCredentials, data, MainDataContext.IsAPIWithCredentials);
                if (res)
                    return "OK";
                else
                    return "Cannot Disconnect doge account.";
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Disconnect doge account. " + ex.Message);
            }
        }
    }
}
