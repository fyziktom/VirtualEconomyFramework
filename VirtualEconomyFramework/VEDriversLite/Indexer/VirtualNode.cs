using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite;
using System.Collections.Concurrent;
using Dasync.Collections;
using NBitcoin;
using VEDriversLite.NFT;
using NBitcoin.Crypto;
using System.Threading;
using VEDriversLite.Indexer.Dto;
using VEDriversLite.Common;

namespace VEDriversLite.Indexer
{
    public class VirtualNode
    {
        public ConcurrentDictionary<string, IndexedAddress> Addresses { get; set; } = new ConcurrentDictionary<string, IndexedAddress>();
        public ConcurrentDictionary<string, IndexedBlock> Blocks { get; set; } = new ConcurrentDictionary<string, IndexedBlock>();
        public ConcurrentDictionary<string, IndexedTransaction> Transactions { get; set; } = new ConcurrentDictionary<string, IndexedTransaction>();
        public ConcurrentDictionary<string, IndexedUtxo> Utxos { get; set; } = new ConcurrentDictionary<string, IndexedUtxo>();
        public ConcurrentDictionary<string, IndexedUtxo> UsedUtxos { get; set; } = new ConcurrentDictionary<string, IndexedUtxo>();

        public static ConcurrentDictionary<string, GetTokenMetadataResponse> TokenMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>();
        public static ConcurrentDictionary<string, TokenSupplyDto> TokenInfoCache = new ConcurrentDictionary<string, TokenSupplyDto>();

        /// <summary>
        /// QT Wallet RPC parameters
        /// </summary>
        public QTRPCConfig QTRPConfig { get; set; } = new QTRPCConfig();
        /// <summary>
        /// Global RPC client for communication with QT Wallet
        /// </summary>
        public QTWalletRPCClient QTRPCClient { get; set; } = new QTWalletRPCClient();

        public bool InitClient(QTRPCConfig config)
        {
            QTRPConfig = config ?? new QTRPCConfig();

            try
            {
                if (!string.IsNullOrEmpty(QTRPConfig.Host))
                {
                    QTRPCClient = new QTWalletRPCClient(QTRPConfig);
                    QTRPCClient.InitClients();
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot init RPC client: " + ex.Message.ToString());
            }

            return false;
        }

        /// <summary>
        /// Get all listed transactions
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllAddresses()
        {
            var res = new List<string>();
            foreach (var tx in Utxos.Values)
            {
                if (tx.OwnerAddress != null && !res.Contains(tx.OwnerAddress))
                    res.Add(tx.OwnerAddress);
            }
            return res;
        }

        /// <summary>
        /// Update address info. 
        /// </summary>
        /// <param name="address"></param>
        public void UpdateAddressInfo(string address)
        {
            if (!Addresses.ContainsKey(address))
                Addresses.TryAdd(address, new IndexedAddress());

            if (Addresses.TryGetValue(address, out var add))
            {
                if ((DateTime.UtcNow - add.LastUpdated) >= new TimeSpan(0, 0, 30))
                {
                    var utxos = Utxos.Values.Where(u => u.OwnerAddress == address);

                    if (utxos != null)
                    {
                        foreach (var utxo in utxos)
                        {
                            if (!add.Transactions.Contains(utxo.TransactionHashAndN))
                                add.Transactions.Add(utxo.TransactionHashAndN);
                            if (!utxo.Used)
                                add.AddUtxo(utxo);

                            if (utxo.TokenUtxo)
                            {
                                if (TokenInfoCache.TryGetValue(utxo.TokenId, out var token))
                                {
                                    if (add.TokenSupplies.TryGetValue(utxo.TokenId, out var addtoken))
                                    {
                                        if (!utxo.Used)
                                            addtoken.Amount += utxo.TokenAmount;
                                        else
                                            addtoken.Amount -= utxo.TokenAmount;
                                    }
                                    else
                                    {
                                        add.TokenSupplies.TryAdd(utxo.TokenId, new TokenSupplyDto()
                                        {
                                            Amount = utxo.TokenAmount,
                                            ImageUrl = token.ImageUrl,
                                            TokenId = token.TokenId,
                                            TokenSymbol = token.TokenSymbol,
                                        });
                                    }
                                }
                            }

                            add.LastUpdated = DateTime.UtcNow;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get cached object of address info
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IndexedAddress GetAddressInfo(string address)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add;
            
            return new IndexedAddress();
        }

        /// <summary>
        /// Get token supplies of some specific address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IDictionary<string, TokenSupplyDto> GetAddressTokenSupplies(string address)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.TokenSupplies;
            
            return new Dictionary<string, TokenSupplyDto>();
        }

        /// <summary>
        /// Get just not used outputs for this address.
        /// This function returs just the "TXID:N" of Utxos as result
        /// These outputs can be used in the new transactions.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public List<string> GetAddressUtxos(string address)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.Utxos.Where(u => u.Value > 0).Select(u => u.TransactionHashAndN).ToList();
            
            return new List<string>();
        }

        /// <summary>
        /// Get just not used outputs for this address.
        /// This function returns whole object of IndexedUtxo
        /// These outputs can be used in the new transactions.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IEnumerable<IndexedUtxo> GetAddressUtxosObjects(string address)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.Utxos.ToList();
            
            return new List<IndexedUtxo>();
        }

        /// <summary>
        /// Get just not used token outputs for this address.
        /// This function returns whole object of IndexedUtxo
        /// These outputs can be used in the new transactions.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IEnumerable<IndexedUtxo> GetAddressTokenUtxosObjects(string address)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.Utxos.Where(u => u.TokenUtxo).ToList();
            
            return new List<IndexedUtxo>();
        }

        /// <summary>
        /// Get all transactions where this address has some input or outputs
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public List<string> GetAddressTransactions(string address)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.Transactions;
            
            return new List<string>();
        }

        //////////////////////////////////
        // RPC Commands for get info from the node
        //////////////////////////////////
        #region RPCCommands

        /// <summary>
        /// Broadcast raw transaction through node
        /// </summary>
        /// <param name="txhex"></param>
        /// <returns></returns>
        public async Task<string> BroadcastRawTx(string txhex)
        {
            var resp = "ERROR";

            try
            {
                if (QTRPCClient.IsConnected)
                {
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("sendrawtransaction", new string[] { txhex });
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    return ores;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error: " + ex.Message);
            }

            return resp;
        }

        /// <summary>
        /// Get transaction info from the node
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        public async Task<GetTransactionInfoResponse?> GetTx(string tx)
        {
            try
            {
                if (QTRPCClient.IsConnected)
                {
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("gettransaction", new string[] { tx });
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    try
                    {
                        var txr = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(ores);
                        if (txr != null)
                            return txr;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync("Error: " + ex.Message);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Get latest block number from the node
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetLatestBlockNumber()
        {
            try
            {
                if (QTRPCClient.IsConnected)
                {
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("getblockcount", new string[] { });
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    try
                    {
                        var block = JsonConvert.DeserializeObject<int>(ores);
                        if (block != null)
                            return block;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync("Error: " + ex.Message);
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error: " + ex.Message);
            }

            return -1;
        }
        /// <summary>
        /// Get Block info based on the block hash from the node
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<GetBlockResponse?> GetBlock(string hash)
        {
            try
            {
                if (QTRPCClient.IsConnected)
                {
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("getblock", new string[] { hash });
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    try
                    {
                        var block = JsonConvert.DeserializeObject<VEDriversLite.NeblioAPI.GetBlockResponse>(ores);
                        if (block != null)
                            return block;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync("Error: " + ex.Message);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error: " + ex.Message);
            }

            return null;
        }
        /// <summary>
        /// Get block info based on the block number from the node
        /// </summary>
        /// <param name="blockNumber"></param>
        /// <returns></returns>
        public async Task<GetBlockResponse?> GetBlockByNumber(double blockNumber)
        {
            try
            {
                if (QTRPCClient.IsConnected)
                {
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("getblockbynumber", new string[] { blockNumber.ToString() });
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    try
                    {
                        var block = JsonConvert.DeserializeObject<VEDriversLite.NeblioAPI.GetBlockResponse>(ores);
                        if (block != null)
                            return block;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync("Error: " + ex.Message);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error: " + ex.Message);
            }

            return null;
        }

        #endregion

        //////////////////////////////////
        // END: RPC Commands for get info from the node
        //////////////////////////////////



        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }

        /// <summary>
        /// Process input of the transaction.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task ProcessInput(Vin input, string txid)
        {
            try
            {
                if (!string.IsNullOrEmpty(input.Txid))
                {
                    var iname = $"{input.Txid}:{input.Vout}";
                    var ix = new IndexedUtxo()
                    {
                        Indexed = true,
                        Used = true,
                        UsedInTxHash = txid,
                        TransactionHashAndN = iname
                    };

                    var tokens = input.Tokens.FirstOrDefault();
                    if (tokens != null)
                    {
                        ix.TokenId = tokens.TokenId;
                        ix.TokenUtxo = true;
                        ix.TokenAmount = tokens.Amount ?? 0.0;
                    }

                    if (Utxos.TryGetValue(iname, out var ixo))
                    {
                        ix.Value = ixo.Value;
                        ix.OwnerAddress = ixo.OwnerAddress;
                        ixo.Used = true;
                        ixo.UsedInTxHash = txid;
                    }

                    if (!UsedUtxos.ContainsKey(iname))
                        UsedUtxos.TryAdd(iname, ix);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot process input: {input.Txid}:{input.Vout}, message: {ex.Message}");
            }
        }

        /// <summary>
        /// Parsing of the token metadata in the transaction
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public async Task<GetTokenMetadataResponse> ProcessTokensMetadata(Tokens3 tokens)
        {
            var tokeninfo = new GetTokenMetadataResponse();

            if (TokenMetadataCache.TryGetValue(tokens.TokenId, out var ti))
            {
                tokeninfo = ti;
            }
            else
            {
                if (tokens.AdditionalProperties != null && tokens.AdditionalProperties.Count > 0)
                {
                    var info = tokens.AdditionalProperties.FirstOrDefault().Value.ToString().Replace("\r\n", string.Empty);

                    if (info != null)
                    {
                        var tkm = JsonConvert.DeserializeObject<MetadataOfIssuance>(info);
                        if (tkm != null)
                        {
                            var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(tkm.Data.Urls));

                            var tt = new TokenSupplyDto
                            {
                                TokenId = tokens.TokenId,
                                TokenSymbol = tkm.Data.TokenName
                            };
                            var tu = tus.FirstOrDefault();
                            if (tu != null)
                                tt.ImageUrl = tu.url;

                            tokeninfo.TokenId = tokens.TokenId;
                            tokeninfo.TokenName = tkm.Data.TokenName;

                            tokeninfo.MetadataOfIssuance = tkm;

                            TokenMetadataCache.TryAdd(tokens.TokenId, tokeninfo);
                            if (!TokenInfoCache.ContainsKey(tokens.TokenId))
                                TokenInfoCache.TryAdd(tokens.TokenId, tt);

                        }
                    }
                }
            }

            return tokeninfo;
        }

        /// <summary>
        /// Parse the outputs of the transaction. 
        /// Contains processing of tokens also.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task ProcessOutput(Vout output, string txid, DateTime blocktime)
        {
            try
            {
                if (output.Value > 0)
                {
                    var u = $"{txid}:{output.N}";
                    var a = string.Empty;
                    try
                    {
                        var ao = output.ScriptPubKey.Addresses.FirstOrDefault();
                        if (ao != null)
                            a = ao;
                    }
                    catch { }
                    var ux = new IndexedUtxo()
                    {
                        Indexed = true,
                        TransactionHashAndN = u,
                        Value = (double)output.Value,
                        OwnerAddress = a,
                        Blockheight = output.Blockheight ?? 0.0,
                        Blocktime = blocktime
                    };

                    var tokeninfo = new GetTokenMetadataResponse();
                    var tokens = output.Tokens.FirstOrDefault();
                    if (tokens != null)
                    {
                        tokeninfo = await ProcessTokensMetadata(tokens);

                        ux.TokenId = tokens.TokenId;
                        ux.TokenUtxo = true;
                        ux.TokenAmount = tokens.Amount ?? 0.0;
                        ux.TokenSymbol = tokeninfo.TokenName;
                    }

                    if (UsedUtxos.TryGetValue(u, out var uux))
                    {
                        ux.Used = true;
                        ux.UsedInTxHash = uux.UsedInTxHash;
                    }

                    Utxos.TryAdd(u, ux);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot process input: {txid}:{output.N}, message: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if the transaction output comes from the staking reward (POS proof of stake)
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        public bool IsTxPOS(GetTransactionInfoResponse tx)
        {
            try
            {
                var vin = tx.Vin.FirstOrDefault();
                if (vin != null)
                {
                    var adp = vin.AdditionalProperties.FirstOrDefault().ToString()?.Contains("coinbase") ?? false;
                    if (adp)
                        return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Load tx info from the node and analyze inputs and outputs
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="blocknumber"></param>
        /// <returns></returns>
        public async Task<GetTransactionInfoResponse?> ParseTransaction(string tx, int blocknumber, bool includePOS = false)
        {
            var t = await GetTx(tx);
            if (t != null)
            {
                if (!includePOS && IsTxPOS(t))
                        return null;

                var utxos = new List<string>();
                var blocktime = TimeHelpers.UnixTimestampToDateTime((t?.Time ?? 0.0) * 1000);

                var it = new IndexedTransaction()
                {
                    Hash = t.Txid,
                    BlockHash = t.Blockhash,
                    BlockNumber = blocknumber,
                    Blocktime = blocktime
                };

                var cancelToken = new CancellationToken();

                await t.Vin.ParallelForEachAsync(async item =>
                {
                    await ProcessInput(item, t.Txid);

                }, cancellationToken: cancelToken, maxDegreeOfParallelism: Environment.ProcessorCount);

                await t.Vout.ParallelForEachAsync(async item =>
                {
                    await ProcessOutput(item, t.Txid, blocktime);
                    var u = $"{t.Txid}:{item.N}";
                    utxos.Add(u);

                }, cancellationToken: cancelToken, maxDegreeOfParallelism: Environment.ProcessorCount);

                it.Utxos = utxos;

                Transactions.TryAdd(tx, it);

                return t;
            }

            return null;
        }

        /// <summary>
        /// Load and analyze all transactions in the block
        /// </summary>
        /// <param name="txs"></param>
        /// <param name="blocknumber"></param>
        /// <returns></returns>
        public async Task<List<GetTransactionInfoResponse>?> GetBlockTransactions(List<string> txs, int blocknumber, bool includePOS = false)
        {
            if (txs == null)
                return null;

            var result = new List<GetTransactionInfoResponse>();

            foreach (var tx in txs)
            {
                var t = await ParseTransaction(tx, blocknumber, includePOS);
                if (t != null)
                    result.Add(t);
            }

            return result;
        }

        /// <summary>
        /// Load all blocks transactions
        /// </summary>
        /// <returns></returns>
        public async Task LoadAllBlocksTransactions()
        {
            var blocks = Blocks.Values.OrderByDescending(b => b.Number).ToList();

            await blocks.ParallelForEachAsync(async block =>
            {
                var txs = await GetBlockTransactions(block.Transactions, (int)block.Number, false);
            }, maxDegreeOfParallelism: Environment.ProcessorCount);
        }

        /// <summary>
        /// Get Indexed blocks based on their number in row
        /// In this overload you can setup some offset from the 0 (block number 0) and number of blocks to load
        /// </summary>
        /// <param name="offset">Offset from the 0</param>
        /// <param name="numberOfBlocks">number of blocks to load</param>
        /// <param name="reverse">Load from end to start</param>
        /// <returns></returns>
        public async Task GetIndexedBlocksByNumbersOffsetAndAmount(int offset, int numberOfBlocks, bool reverse = false)
        {
            await GetIndexedBlocksByNumbersStartToEnd(offset, offset + numberOfBlocks, reverse);
        }

        /// <summary>
        /// Get Indexed blocks based on their number. 
        /// You can setup start and end of the blocks.
        /// </summary>
        /// <param name="start">Start number of block</param>
        /// <param name="end">End number of block</param>
        /// <param name="reverse">Load from end to start</param>
        /// <returns></returns>
        public async Task GetIndexedBlocksByNumbersStartToEnd(int start, int end, bool reverse = false)
        {
            var maxDegreeOfParallelism = Environment.ProcessorCount;
            var blocksIds = new int[end - start];
            if (reverse)
            {
                for (var i = end - start - 1; i >= 0; i--)
                    blocksIds[i] = i + start;
            }
            else
            {
                for (var i = 0; i < end - start; i++)
                    blocksIds[i] = i + start;
            }

            await blocksIds.ParallelForEachAsync(async item =>
            {
                var blkn = await GetBlockByNumber(item);
                if (blkn != null)
                {
                    Blocks.TryAdd(blkn.Hash, new IndexedBlock()
                    {
                        Hash = blkn.Hash,
                        Number = item,
                        Height = blkn.Height ?? -1,
                        Time = TimeHelpers.UnixTimestampToDateTime((blkn.Time ?? 0.0) * 1000),
                        Transactions = blkn.Tx.ToList()
                    });
                }
            }, maxDegreeOfParallelism: maxDegreeOfParallelism);
        }




        ////////////////////////////////////
        ///Obsolete
        ////////////////////////////////////

        public async Task<List<IndexedBlock>> GetIndexedBlocks(string latestBlock, double numberOfHoursInHistory = 3)
        {
            var records = new List<IndexedBlock>();

            var lastLoadedBlock = "";
            var nextBlock = latestBlock;
            var actualDate = DateTime.UtcNow;
            var printedDate = DateTime.UtcNow;
            // set the date which should be 
            var requestedLatestDate = actualDate.AddHours(-numberOfHoursInHistory);
            var firstRun = true;

            if (lastLoadedBlock != null)
            {
                while (actualDate > requestedLatestDate)
                {
                    if (!string.IsNullOrEmpty(nextBlock))
                    {
                        var resb = await GetBlock(nextBlock);
                        if (resb != null)
                        {
                            lastLoadedBlock = nextBlock;
                            nextBlock = resb.Previousblockhash;
                            actualDate = TimeHelpers.UnixTimestampToDateTime((resb.Time ?? 0.0) * 1000);
                            if (firstRun)
                            {
                                requestedLatestDate = actualDate.AddHours(-numberOfHoursInHistory);

                                Console.WriteLine("I will load all transactions between dates:!");
                                Console.WriteLine($"StartDate:{requestedLatestDate}");
                                Console.WriteLine($"EndDate:{actualDate}");
                                Console.WriteLine("Loading, it can take a while...");
                                Console.WriteLine($"Processing date {actualDate}...");

                                firstRun = false;
                            }

                            if (actualDate.Year == printedDate.Year && actualDate.Month == printedDate.Month && actualDate.Day == printedDate.Day && actualDate.Hour == printedDate.Hour)
                                Console.Write(".");
                            else
                                Console.WriteLine($"Processing date {actualDate.Month}.{actualDate.Day}.{actualDate.Year}:{actualDate.Hour} hour...");

                            printedDate = actualDate;

                            records.Add(new IndexedBlock()
                            {
                                Hash = lastLoadedBlock,
                                Time = actualDate,
                                Transactions = resb.Tx.ToList()
                            });
                        }

                        //await Task.Delay(10); // to do not overload the API with requests
                    }
                }

            }

            return records;
        }

        public async Task GetIndexedBlocksOptimized(string latestBlock, double numberOfHoursInHistory = 3)
        {
            var lastLoadedBlock = "";
            var nextBlock = latestBlock;
            var actualDate = DateTime.UtcNow;
            // set the date which should be 
            var requestedLatestDate = actualDate.AddHours(-numberOfHoursInHistory);
            var firstRun = true;

            if (lastLoadedBlock != null)
            {
                while (actualDate > requestedLatestDate)
                {
                    if (!string.IsNullOrEmpty(nextBlock))
                    {
                        var resb = await GetBlock(nextBlock);
                        if (resb != null)
                        {
                            lastLoadedBlock = nextBlock;
                            nextBlock = resb.Previousblockhash;
                            actualDate = TimeHelpers.UnixTimestampToDateTime((resb.Time ?? 00) * 1000);
                            if (firstRun)
                            {
                                requestedLatestDate = actualDate.AddHours(-numberOfHoursInHistory);
                                firstRun = false;
                            }
                            Blocks.TryAdd(lastLoadedBlock, new IndexedBlock()
                            {
                                Hash = lastLoadedBlock,
                                Height = resb.Height ?? -1,
                                Time = actualDate,
                                Transactions = resb.Tx.ToList()
                            });
                        }
                    }
                }
            }
        }

        public async Task GetIndexedBlocksOptimized(string latestBlock, int numberOfHoursInHistory = 3)
        {

            var lastLoadedBlock = "";
            var nextBlock = latestBlock;
            var actualDate = DateTime.UtcNow;
            var printedDate = DateTime.UtcNow;
            // set the date which should be 
            var requestedLatestDate = actualDate.AddHours(-numberOfHoursInHistory);

            double startBlockHeight = 0;
            double avgTimeOfOneBlock = 0;
            var firstAnalysisData = new List<IndexedBlock>();
            var numOfBlockForFirstAnalysis = 3;

            for (var i = 0; i < numOfBlockForFirstAnalysis; i++)
            {
                var r = await GetBlock(nextBlock);
                if (r != null)
                {
                    nextBlock = r.Previousblockhash;
                    actualDate = TimeHelpers.UnixTimestampToDateTime((double)r.Time * 1000);

                    startBlockHeight = r.Height ?? -1;

                    firstAnalysisData.Add(new IndexedBlock()
                    {
                        Hash = lastLoadedBlock,
                        Height = r.Height ?? -1,
                        Time = actualDate,
                        Transactions = r.Tx.ToList()
                    });
                }
            }

            // calc average timespan between blocks
            var sum = 0.0;
            for (var i = 1; i < firstAnalysisData.Count; i++)
                sum += (firstAnalysisData[i].Time - firstAnalysisData[i - 1].Time).TotalMilliseconds;

            sum /= (double)firstAnalysisData.Count;
            avgTimeOfOneBlock = sum / 1000; // get time in seconds

            double farawayBlock = Math.Round(firstAnalysisData[0].Height - (int)((actualDate - requestedLatestDate).TotalSeconds / avgTimeOfOneBlock), 0);

            var block = GetBlockByNumber(farawayBlock);

            var cores = 4;
            double quater = (firstAnalysisData[0].Height - farawayBlock) / cores;

            var quaters = new double[cores];

            for (var i = 1; i <= cores; i++)
                quaters[i - 1] = Math.Round(farawayBlock + quater * i, 0);

            quaters[cores - 1] = firstAnalysisData[0].Height;

            var tasks = new Task[cores];

            var blocks = new GetBlockResponse[cores];

            for (var i = 0; i < cores; i++)
            {
                var b = await GetBlockByNumber(quaters[i]);
                var bl = await GetBlockByNumber(Math.Round(quaters[i] - quater, 0));
                var actualDate1 = TimeHelpers.UnixTimestampToDateTime((b?.Time ?? 0.0) * 1000);
                var actualDate2 = TimeHelpers.UnixTimestampToDateTime((bl?.Time ?? 0.0) * 1000);

                tasks[i] = GetIndexedBlocksOptimized(b.Hash, (actualDate2 - actualDate1).TotalHours);
            }

            await Task.WhenAll(tasks);

        }


    }
}
