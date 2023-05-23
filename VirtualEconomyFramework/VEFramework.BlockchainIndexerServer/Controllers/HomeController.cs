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
using VEDriversLite.Indexer;
using NBitcoin;
using System.Text;
using VEDriversLite.Neblio;
using VEDriversLite.Common;

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

        /// <summary>
        /// Get address tokens supplies
        /// </summary>
        /// <returns>List of tx ids</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetTokenMetadata/{tokenId}")]
        public async Task<GetTokenMetadataResponse> GetTokenMetadata(string tokenId)
        {
            try
            {
                if (!string.IsNullOrEmpty(tokenId))
                {
                    if (MainDataContext.Node.TokenMetadataCache.TryGetValue(tokenId, out var tok))
                        return tok;
                }

                return new GetTokenMetadataResponse();
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get token metadata info for token Id: {tokenId}!");
            }
        }


        /// <summary>
        /// Get not signed transaction for token transfer
        /// </summary>
        /// <returns>List of tx ids</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("CreateNotSignedTokenTransaction")]
        public async Task<NBitcoin.Transaction> CreateNotSignedTokenTransaction([FromBody] SendTokenRequest data)
        {
            try
            {
                if (data != null)
                {
                    var addr = data.From.FirstOrDefault();
                    if (string.IsNullOrEmpty(addr))
                        throw new HttpResponseException((HttpStatusCode)501, $"Address is not valid, please fill at least valid address in \"From\" list!");
                    var utxos = MainDataContext.Node.GetAddressUtxosObjects(addr).ToList();
                    if (utxos == null || !utxos.Any())
                        throw new HttpResponseException((HttpStatusCode)501, $"No suitable Utxos found!");

                    var autxos = VirtualNode.ConvertIndexedUtxoToUtxo(utxos);

                    var nutxos = await NeblioAPIHelpers.GetAddressNeblUtxo(addr, 0.0002, 0.001, addinfo: new GetAddressInfoResponse() { Utxos = autxos });
                    if (nutxos == null || nutxos.Count == 0)
                        throw new HttpResponseException((HttpStatusCode)501, $"You dont have Neblio on the address. Probably waiting for more than {NeblioAPIHelpers.MinimumConfirmations} confirmations.");

                    var totalTokensToSend = 0.0;
                    var tokenId = "";
                    var receivers = new List<string>();
                    foreach(var to in data.To)
                    {
                        var tmpAddr = NeblioTransactionHelpers.ValidateNeblioAddress(to.Address);
                        if (string.IsNullOrEmpty(tmpAddr))
                            throw new HttpResponseException((HttpStatusCode)501, $"Address of receiver {to.Address} is not valid Neblio Blockchain Address.");
                        receivers.Add(tmpAddr);

                        totalTokensToSend += to.Amount ?? 0.0;
                        if (!string.IsNullOrEmpty(to.TokenId) && string.IsNullOrEmpty(tokenId))
                            tokenId = to.TokenId;
                        else if (!string.IsNullOrEmpty(to.TokenId) && !string.IsNullOrEmpty(tokenId) && to.TokenId != tokenId)
                            throw new HttpResponseException((HttpStatusCode)501, $"All tokens in field \"To\" must be same of token Id.");
                    }

                    var tutxos = await NeblioAPIHelpers.FindUtxoForMintNFT(addr, tokenId, Convert.ToInt32(totalTokensToSend), addinfo: new GetAddressInfoResponse() { Utxos = autxos });
                    if (tutxos == null || tutxos.Count == 0)
                        throw new HttpResponseException((HttpStatusCode)501, $"You dont have Neblio on the address. Probably waiting for more than {NeblioAPIHelpers.MinimumConfirmations} confirmations.");

                    // convert metadata JObject list (which is in real Dictionary<string,string> from request to dto Metadata dictionary.
                    var meta = new Dictionary<string, string>();
                    if (data.Metadata != null && data.Metadata.UserData.Meta.Count > 0)
                    {
                        foreach (var m in data.Metadata.UserData.Meta)
                        {
                            try
                            {
                                var v = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(m.ToString());
                                meta.Add(v.Key, v.Value);
                            }
                            catch(Exception ex)
                            {
                                await Console.Out.WriteLineAsync("Cannot deserialize metadata. " + ex.Message);
                            }
                        }
                    }
                    
                    var dto = new SendTokenTxData()
                    {
                        MultipleReceivers = data.To.ToList(),
                        Amount = totalTokensToSend,
                        Id = tokenId,
                        SenderAddress = addr,
                        Metadata = meta
                    };

                    var transaction = await NeblioTransactionHelpers.SendTokensToMultipleReceiversAsync(dto, nutxos, tutxos);
                    if (transaction != null)
                        return transaction;
                    else
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot create transaction from input data!");
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot create transaction from input data. Exception message: {ex.Message}!");
            }
        }

        /// <summary>
        /// Get not signed transaction for issuing new token
        /// </summary>
        /// <returns>List of tx ids</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("CreateNotSignedIssueTransaction")]
        public async Task<NBitcoin.Transaction> CreateNotSignedIssueTransaction([FromBody] IssueTokenRequest data)
        {
            try
            {
                if (data != null)
                {
                    var addr = data.IssueAddress;
                    if (string.IsNullOrEmpty(addr))
                        throw new HttpResponseException((HttpStatusCode)501, $"Address is not valid, please fill at least valid address in \"From\" list!");
                    var utxos = MainDataContext.Node.GetAddressUtxosObjects(addr).ToList();
                    if (utxos == null || !utxos.Any())
                        throw new HttpResponseException((HttpStatusCode)501, $"No suitable Utxos found for address {addr}!");

                    var autxos = VirtualNode.ConvertIndexedUtxoToUtxo(utxos);

                    var nutxos = await NeblioAPIHelpers.GetAddressNeblUtxo(addr, 0.0002, 10.01, addinfo: new GetAddressInfoResponse() { Utxos = autxos }, latestBlockHeight: MainDataContext.LatestLoadedBlock);
                    if (nutxos == null || nutxos.Count == 0)
                        throw new HttpResponseException((HttpStatusCode)501, $"You dont have Neblio on the address. Probably waiting for more than {NeblioAPIHelpers.MinimumConfirmations} confirmations.");

                    MetadataOfIssuance meta = new MetadataOfIssuance()
                    {
                        Data = new Data2()
                        {
                            Description = data.Metadata.Description,
                            Issuer = data.Metadata.Issuer,
                            TokenName = data.Metadata.TokenName,
                            Urls = data.Metadata.Urls.Select(u => new tokenUrlCarrier() { name = u.Name, url = u.Url, mimeType = u.MimeType }).ToList(),
                            UserData = new UserData4()
                            {
                                Meta = data.Metadata.UserData.Meta.Select(data => new Meta3()
                                {
                                    Key = data.Key,
                                    Value = data.Value,
                                    AdditionalProperties = new Dictionary<string, object>() { { "type", "String" } }
                                }).ToList()
                            }
                        }
                    };
                    var dto = new IssueTokenTxData()
                    {
                        Amount = (ulong)data.Amount,
                        IssuanceMetadata = meta,
                        SenderAddress = addr,
                        ReceiverAddress = data.Transfer?.FirstOrDefault()?.Address ?? addr
                    };

                    var transaction = await NeblioTransactionHelpers.IssueTokensAsync(dto, nutxos);
                    if (transaction != null)
                        return transaction;
                    else
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot create transaction from input data!");
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot create transaction from input data. Exception message: {ex.Message}!");
            }
        }

        public class ParseNTP1DataResponseDto
        {
            public string parsedMetadata { get; set; } = string.Empty;
            public List<NTP1Instructions> transferInstructions { get; set; } = new List<NTP1Instructions>();
            public TxType TxType { get; set; } = TxType.TxType_Transfer;
            public string TokenIssueSymbol { get; set; } = string.Empty;
            public ulong TokenIssueAmount { get; set; } = 0;
        }

        /// <summary>
        /// Parse NTP1 data
        /// </summary>
        /// <returns>ServerStatusDto</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("ParseNTP1Data/{opreturndata}")]
        public async Task<ParseNTP1DataResponseDto> ParseNTP1Data(string opreturndata)
        {
            try
            {
                var tx = new NTP1Transactions()
                {
                    ntp1_opreturn = opreturndata
                };

                NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata
                
                var customDecompressed1 = StringExt.Decompress(tx.metadata);
                var metadataString = Encoding.UTF8.GetString(customDecompressed1);

                var resp = new ParseNTP1DataResponseDto()
                {
                    parsedMetadata = metadataString,
                    TxType = tx.tx_type,
                    TokenIssueAmount = tx.tokenIssueAmount,
                    TokenIssueSymbol = tx.tokenSymbol,
                    transferInstructions = tx.ntp1_instruct_list
                };

                return resp;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get server status!");
            }
        }

        #endregion

        #region ServerStats

        /// <summary>
        /// Get server sync status
        /// </summary>
        /// <returns>ServerStatusDto</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetServerSyncStatus/")]
        public async Task<ServerStatusDto> GetServerSyncStatus()
        {
            try
            {
                var resp = new ServerStatusDto()
                {
                    LatestLoadedBlock = MainDataContext.LatestLoadedBlock,
                    ActualOldestLoadedBlock = MainDataContext.ActualOldestLoadedBlock,
                    AverageTimeToIndexBlockInMilliseconds = MainDataContext.AverageTimeToIndexBlock / 1000,
                    OldestBlockToLoad = MainDataContext.OldestBlockToLoad,
                    CountOfAddresses = MainDataContext.Node.Addresses.Count,
                    CountOfBlocks = MainDataContext.Node.Blocks.Count,
                    CountOfIndexedBlocks = MainDataContext.Node.Blocks.Where(b => b.Value.Indexed).Count(),
                    CountOfUtxos = MainDataContext.Node.Utxos.Count,
                    CountOfUsedUtxos = MainDataContext.Node.UsedUtxos.Count,
                    CountOfTransactions = MainDataContext.Node.Transactions.Count
                };

                return resp;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get server status!");
            }
        }

        /// <summary>
        /// Change latest loaded block server settings
        /// </summary>
        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("ChangeLatestLoadedBlock/{height}")]
        public async Task ChangeLatestLoadedBlock(double height)
        {
            if (height > 0 && height < MainDataContext.LatestLoadedBlock)
                MainDataContext.LatestLoadedBlock = height;
        }


        #endregion
    }
}
