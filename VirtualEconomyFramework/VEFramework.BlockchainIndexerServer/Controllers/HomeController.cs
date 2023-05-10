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
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;
using VEDriversLite.StorageDriver.Helpers;
using System.IO;
using Newtonsoft.Json;
using VEFramework.BlockchainIndexerServer.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using VEDriversLite.Indexer.Dto;

namespace VEFramework.BlockchainIndexerServer.Controllers
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
        [AllowCrossSiteJsonAttribute]
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
        [AllowCrossSiteJsonAttribute]
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
        /// Get transaction info from the node
        /// </summary>
        /// <returns>Transaction info response</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetTransactionInfo/{txid}")]
        public async Task<GetTransactionInfoResponse> GetTransactionInfo(string txid)
        {
            try
            {
                if (!string.IsNullOrEmpty(txid))
                {
                    var info = await MainDataContext.Node.GetTx(txid);
                    return info;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get tx info for txid: {txid}!");
            }
        }

        /// <summary>
        /// Get block info from the node
        /// </summary>
        /// <returns>Block info response</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetBlockInfo/{blockhash}")]
        public async Task<GetBlockResponse> GetBlockInfo(string hash)
        {
            try
            {
                if (!string.IsNullOrEmpty(hash))
                {
                    var info = await MainDataContext.Node.GetBlock(hash);
                    return info;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get block info for hash: {hash}!");
            }
        }

        /// <summary>
        /// Get block info from the node
        /// </summary>
        /// <returns>Block info response</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetBlockInfo/{blocknumber}")]
        public async Task<GetBlockResponse> GetBlockInfo(int number)
        {
            try
            {
                if (number >= 0)
                {
                    var info = await MainDataContext.Node.GetBlockByNumber(number);
                    return info;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get block info with number: {number}!");
            }
        }

        /// <summary>
        /// Get latest blockheight
        /// </summary>
        /// <returns>Block info response</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetLatestBlockheight")]
        public async Task<int> GetLatestBlockheight()
        {
            try
            {
                var info = await MainDataContext.Node.GetLatestBlockNumber();
                return info;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get latest block number.");
            }
        }

        public class BroadcastTransactionRequestDto
        {
            public string network { get; set; } = "neblio";
            public string tx_hex { get; set; } = string.Empty;
        }
        public class BroadcastTransactionResponseDto
        {
            public BroadcastDataResponseDto data { get; set; } = new BroadcastDataResponseDto();
        }
        public class BroadcastDataResponseDto
        {
            public string network { get; set; } = "neblio";
            public string txid { get; set; } = string.Empty;
        }

        /// <summary>
        /// Broadcast raw signed transaction through node
        /// </summary>
        /// <returns>Transaction Id</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("BroadcastTransaction")]
        public async Task<BroadcastTransactionResponseDto> BroadcastTransaction([FromBody] BroadcastTransactionRequestDto data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.tx_hex))
                    return (new BroadcastTransactionResponseDto());
                var resp = new BroadcastTransactionResponseDto();
                if (data.network == "neblio")
                {
                    var txid = await MainDataContext.Node.BroadcastRawTx(data.tx_hex);
                    if (!string.IsNullOrEmpty(txid))
                    {
                        resp.data = new BroadcastDataResponseDto() { network = "neblio", txid = txid };
                        return resp;
                    }
                    else
                        return (new BroadcastTransactionResponseDto());
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Broadcast transaction.! Exception: " + ex.Message);
            }

            return (new BroadcastTransactionResponseDto());
        }


        /// <summary>
        /// Get address info
        /// </summary>
        /// <returns>Block info response</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetAddressInfo/{address}")]
        public async Task<IndexedAddress> GetAddressInfo(string address)
        {
            try
            {
                if (!string.IsNullOrEmpty(address))
                {
                    var info = MainDataContext.Node.GetAddressInfo(address);
                    return info;
                }

                return null;
            }
            catch (Exception ex)
            { 
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get address info for address: {address}!");
            }
        }

        /// <summary>
        /// Get address Utxos list
        /// </summary>
        /// <returns>Utxos list</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetAddressUtxos/{address}")]
        public async Task<List<IndexedUtxo>> GetAddressUtxos(string address)
        {
            try
            {
                if (!string.IsNullOrEmpty(address))
                {
                    var info = MainDataContext.Node.GetAddressUtxosObjects(address);
                    return info.ToList();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get address info for address: {address}!");
            }
        }

        /// <summary>
        /// Get address Utxos list
        /// </summary>
        /// <returns>Utxos list</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetAddressTokenUtxos/{address}")]
        public async Task<List<IndexedUtxo>> GetAddressTokenUtxos(string address)
        {
            try
            {
                if (!string.IsNullOrEmpty(address))
                {
                    var info = MainDataContext.Node.GetAddressTokenUtxosObjects(address);
                    return info.ToList();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get address info for address: {address}!");
            }
        }

        /// <summary>
        /// Get address transaction list
        /// </summary>
        /// <returns>List of tx ids</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetAddressTransactions/{address}/{skip}/{take}")]
        public async Task<List<string>> GetAddressTransactions(string address, int skip, int take)
        {
            try
            {
                if (!string.IsNullOrEmpty(address))
                {
                    var info = MainDataContext.Node.GetAddressTransactions(address, skip, take);
                    return info;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get address info for address: {address}!");
            }
        }

        /// <summary>
        /// Get address tokens supplies
        /// </summary>
        /// <returns>List of tx ids</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetAddressTokensSupplies/{address}")]
        public async Task<IDictionary<string, TokenSupplyDto>> GetAddressTokensSupplies(string address)
        {
            try
            {
                if (!string.IsNullOrEmpty(address))
                {
                    var info = MainDataContext.Node.GetAddressTokenSupplies(address);
                    return info;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get address info for address: {address}!");
            }
        }


        #endregion
    }
}
