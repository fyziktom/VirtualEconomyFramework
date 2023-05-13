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
using VEDriversLite.Neblio;
using System.Security.Cryptography.X509Certificates;
using Ipfs.Http;
using Org.BouncyCastle.Bcpg;

namespace VEDriversLite.Indexer
{
    public class VirtualNode
    {
        public ConcurrentDictionary<string, IndexedAddress> Addresses { get; set; } = new ConcurrentDictionary<string, IndexedAddress>();
        public ConcurrentDictionary<string, IndexedBlock> Blocks { get; set; } = new ConcurrentDictionary<string, IndexedBlock>();
        public ConcurrentDictionary<string, IndexedTransaction> Transactions { get; set; } = new ConcurrentDictionary<string, IndexedTransaction>();
        public ConcurrentDictionary<string, IndexedUtxo> Utxos { get; set; } = new ConcurrentDictionary<string, IndexedUtxo>();
        /// <summary>
        /// Contains Utxo:N and TXID of where utxo has been used
        /// </summary>
        public ConcurrentDictionary<string, string> UsedUtxos { get; set; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, GetTokenMetadataResponse> TokenMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>();
        public ConcurrentDictionary<string, TokenSupplyDto> TokenInfoCache = new ConcurrentDictionary<string, TokenSupplyDto>();

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
        public void UpdateAddressInfo(string address, bool force = false)
        {
            if (!Addresses.ContainsKey(address))
                Addresses.TryAdd(address, new IndexedAddress());

            if (Addresses.TryGetValue(address, out var add))
            {
                if ((DateTime.UtcNow - add.LastUpdated) >= new TimeSpan(0, 0, 10) || force)
                {
                    var utxos = Utxos.Values.Where(u => u.OwnerAddress == address);

                    if (utxos != null)
                    {
                        foreach (var utxo in utxos)
                        {
                            add.AddTransaction(utxo.TransactionHashAndN.Split(':')[0], utxo.Time);
                            if (!utxo.Used)
                                add.AddUtxo(utxo);

                            if (utxo.TokenUtxo && TokenInfoCache.TryGetValue(utxo.TokenId, out var token))
                                add.UpdateTokenSupply(utxo, token);

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
        public IEnumerable<IndexedUtxo> GetAddressUtxosObjects(string address, bool takeJustNotUsed = true)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.Utxos.Where(u => u.Used == !takeJustNotUsed).ToList();
            
            return new List<IndexedUtxo>();
        }

        /// <summary>
        /// Get just not used token outputs for this address.
        /// This function returns whole object of IndexedUtxo
        /// These outputs can be used in the new transactions.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IEnumerable<IndexedUtxo> GetAddressTokenUtxosObjects(string address, bool takeJustNotUsed = true)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.Utxos.Where(u => u.Used == !takeJustNotUsed && u.TokenUtxo).ToList();
            
            return new List<IndexedUtxo>();
        }

        /// <summary>
        /// Get all transactions where this address has some input or outputs
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAddressTransactions(string address, int skip = 0, int take = 0)
        {
            UpdateAddressInfo(address);
            if (Addresses.TryGetValue(address, out var add))
                return add.GetTransactions(skip, take);
            
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
                    if (!ores.ToLower().Contains("error"))
                    {
                        await ProcessBroadcastedTransaction(txhex);
                    }

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
        /// This function will take the broadcasted transaction and update the cache
        /// </summary>
        /// <param name="txhex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ProcessBroadcastedTransaction(string txhex)
        {
            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(txhex, NeblioTransactionHelpers.Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }
            var txid = transaction.GetHash().ToString();

            var tokensFromInputs = new List<Tokens3>();
            var totalTokensSent = 0.0;
            var totalTokens = 0.0;
            var totalNeblioInput = 0.0;
            var tokenSymbol = string.Empty;
            var tokenId = string.Empty;

            var addressesToUpdate = new List<string>();

            foreach (var input in transaction.Inputs)
            {
                var utxoId = $"{input.PrevOut.Hash}:{input.PrevOut.N}";
                if (Utxos.TryGetValue(utxoId, out var utxo))
                {
                    utxo.Used = true;
                    utxo.UsedInTxHash = txid;
                    if (!UsedUtxos.ContainsKey(utxoId))
                        UsedUtxos.TryAdd(utxoId, txid);

                    if (utxo.TokenUtxo)
                    {
                        totalTokens += utxo.TokenAmount;
                        tokenId = utxo.TokenId;
                        tokenSymbol = utxo.TokenSymbol;
                    }

                    totalNeblioInput += utxo.Value;
                }
                else
                {
                    var prevUtxo = input.PrevOut.Hash.ToString();
                    var prevUtxoIndex = input.PrevOut.N;
                    var prevTx = await GetTx(prevUtxo);
                    if (prevTx != null)
                    {
                        if (prevTx.Vout.Count > prevUtxoIndex)
                        {
                            var prevVout = prevTx.Vout.Where(v => v.N == prevUtxoIndex)?.FirstOrDefault();
                            if (prevVout != null)
                            {
                                totalNeblioInput += (prevVout.Value ?? 0.0);

                                var prevTokenVout = prevVout.Tokens.FirstOrDefault();
                                if (prevTokenVout != null)
                                {
                                    var ti = ProcessTokensMetadata(prevTokenVout);
                                    if (ti != null)
                                    {
                                        totalTokens += prevTokenVout.Amount ?? 0.0;
                                        tokenId = prevTokenVout.TokenId;
                                        tokenSymbol = ti.TokenName;
                                    }

                                    tokensFromInputs.Add(prevTokenVout);
                                }
                            }
                        }
                    }
                }

                var split = input.ScriptSig.ToString().Split(' ');
                if (split.Length >= 2)
                {
                    var pubkey = split[split.Length - 1];
                    BitcoinAddress add = null;
                    try
                    {
                        PubKey pk = new PubKey(pubkey);
                        add = pk.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Wrong public key input during parsing the Tx data. PubKey input:" + pubkey);
                    }
                    if (add != null)
                    {
                        var ads = add.ToString();
                        if (!addressesToUpdate.Contains(ads))
                            addressesToUpdate.Add(ads);
                    }
                }
            }

            var tx = new NTP1Transactions() { ntp1_instruct_list = new List<NTP1Instructions>() };
            var isTokenTx = false;
            var opReturnOutput = transaction.Outputs.Where(o => o.ScriptPubKey.IsUnspendable)?.FirstOrDefault();
            if (opReturnOutput != null)
            {
                isTokenTx = true;
                var parsedInfo = opReturnOutput.ScriptPubKey.ToString();
                if (!string.IsNullOrEmpty(parsedInfo) && parsedInfo.Contains("OP_RETURN"))
                {
                    var data = parsedInfo.Replace("OP_RETURN ", string.Empty);

                    if (!string.IsNullOrEmpty(data))
                    {
                        tx = new NTP1Transactions()
                        {
                            ntp1_opreturn = data,
                            tx_type = 1
                        };
                        NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

                        var customDecompressed = StringExt.Decompress(tx.metadata);
                        var metadataString = Encoding.UTF8.GetString(customDecompressed);

                        foreach (var inst in tx.ntp1_instruct_list)
                            await Console.Out.WriteLineAsync($"Token amount in tx is: {inst.amount} on {inst.vout_num} output");
                    }
                }
            }

            for(var i = 0; i < transaction.Outputs.Count; i++)
            {
                var output = transaction.Outputs[i];
                var utxoId = $"{txid}:{i}";
                if (!output.ScriptPubKey.IsUnspendable)
                {
                    if (!Utxos.TryGetValue(utxoId, out var utxo))
                    {
                        var time = DateTime.UtcNow;
                        var ux = new IndexedUtxo()
                        {
                            TransactionHash = txid,
                            Index = i,
                            Value = (double)output.Value.Satoshi / NeblioTransactionHelpers.FromSatToMainRatio,
                            OwnerAddress = output.ScriptPubKey.GetDestinationAddress(NeblioTransactionHelpers.Network).ToString(),
                            Blockheight = -1,
                            Blocktime = TimeHelpers.DateTimeToUnixTimestamp(time),
                            Time = time
                        };

                        if (isTokenTx)
                        {
                            var tokenVout = tx.ntp1_instruct_list.Where(o => o.vout_num == i).FirstOrDefault();
                            if (tokenVout != null)
                            {
                                ux.TokenUtxo = true;
                                ux.TokenAmount = tokenVout.amount;
                                totalTokensSent += tokenVout.amount;
                                ux.TokenSymbol = tokenSymbol;
                                ux.TokenId = tokenId;
                            }
                        }

                        if (i == transaction.Outputs.Count - 1 && 
                            totalTokensSent > 0 && 
                            transaction.Outputs[i].Value.Satoshi == NeblioTransactionHelpers.MinimumAmount)
                        {
                            ux.TokenAmount = totalTokens - totalTokensSent;
                            ux.TokenUtxo = true;
                            ux.TokenId = tokenId;
                            ux.TokenSymbol = tokenSymbol;
                        }

                        if (Utxos.TryGetValue(utxoId, out var eu))
                            eu = ux;
                        else
                            Utxos.TryAdd(utxoId, ux);
                    }
                }

                var add = output.ScriptPubKey.GetDestinationAddress(NeblioTransactionHelpers.Network);
                if (add != null)
                {
                    var ads = add.ToString();
                    if (!addressesToUpdate.Contains(ads))
                        addressesToUpdate.Add(ads);
                }
            }

            Parallel.ForEach(addressesToUpdate, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, address =>
            {
                UpdateAddressInfo(address, true);
            });
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
                if (Transactions.TryGetValue(tx, out var it))
                {
                    if (it.TxInfo != null)
                        return it.TxInfo;
                }

                if (QTRPCClient.IsConnected)
                {
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("gettransaction", new string[] { tx });
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    try
                    {
                        var txr = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(ores);
                        if (IsTxPOS(txr))
                            return null;

                        if (txr.Hex == null)
                            txr.Hex = string.Empty;

                        var block = new IndexedBlock();
                        if (Blocks.TryGetValue(txr.Blockhash, out var blk))
                        {
                            block = blk;
                        }
                        else
                        {
                            var blck = await GetBlock(txr.Blockhash);
                            if (blck != null)
                            {
                                block.Height = blck.Height ?? 0.0;
                                block.Time = TimeHelpers.UnixTimestampToDateTime((blck.Time ?? 0.0) * 1000);
                                block.Hash = blck.Hash;
                                if (blck.Hash != null)
                                    Blocks.TryAdd(blck.Hash, GetIndexedBlockFromResponse(blck));
                            }
                        }

                        foreach (var vout in txr.Vout)
                        {
                            if (Utxos.TryGetValue($"{tx}:{vout.N}", out var ux))
                            {
                                if (ux.Used)
                                    vout.Used = true;
                                vout.UsedTxid = ux.UsedInTxHash;
                            }

                            if (vout.Used == null)
                                vout.Used = false;
                            if (vout.UsedTxid == null)
                                vout.UsedTxid = string.Empty;
                            if (vout.UsedBlockheight == null)
                                vout.UsedBlockheight = 0.0;
                            if (vout.Blockheight == null)
                                vout.Blockheight = 0.0;
                        }

                        foreach (var vin in txr.Vin)
                        {
                            vin.Addr = string.Empty;
                            vin.PreviousOutput = new PreviousOutput()
                            {
                                Addresses = new List<string>(),
                                Asm = string.Empty,
                                Hex = string.Empty,
                                ReqSigs = 0.0,
                                Type = string.Empty
                            };

                            if (Utxos.TryGetValue($"{vin.Txid}:{vin.Vout}", out var ux))
                            {
                                vin.Addr = ux.OwnerAddress;
                                vin.PreviousOutput.Addresses.Add(ux.OwnerAddress);
                                vin.Value = ux.Value;
                                vin.ValueSat = ux.Value * NeblioTransactionHelpers.FromSatToMainRatio;
                            }
                            else
                            {
                                var add = NeblioTransactionHelpers.GetAddressStringFromSignedScriptPubKey(vin.ScriptSig);
                                if (add != null)
                                {
                                    vin.Addr = add.ToString();
                                    vin.PreviousOutput.Addresses.Add(vin.Addr);
                                }
                                if (vin.Value == null)
                                    vin.Value = 0.0;
                                if (vin.ValueSat == null)
                                    vin.ValueSat = 0.0;
                            }
                            txr.Blocktime = TimeHelpers.DateTimeToUnixTimestamp((block.Time));
                            txr.Blockheight = block.Height;
                        }

                        if (txr.Blockheight == null)
                            txr.Blockheight = 0.0;
                        if (txr.Blocktime == null)
                            txr.Blocktime = 0.0;

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
                    var kr = await QTRPCClient.RPCLocalCommandSplitedAsync("getblockcount", Array.Empty<string>());
                    var rr = JsonConvert.DeserializeObject<JObject>(kr);
                    var ores = rr["result"].ToString();
                    try
                    {
                        var block = JsonConvert.DeserializeObject<int>(ores);
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
        public void ProcessInput(Vin input, string txid)
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
                        TransactionHash = input.Txid,
                        Index = (int)input.Vout
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
                    else
                    {
                        Utxos.TryAdd(iname, ix);
                    }

                    if (!UsedUtxos.ContainsKey(iname))
                        UsedUtxos.TryAdd(iname, txid);
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
        public GetTokenMetadataResponse ProcessTokensMetadata(Tokens3 tokens)
        {
            var tokeninfo = new GetTokenMetadataResponse();

            if (TokenMetadataCache.TryGetValue(tokens.TokenId, out var ti))
            {
                tokeninfo = ti;
            }
            else
            {
                if (tokens.MetadataOfIssuance != null)
                {
                    var tkm = tokens.MetadataOfIssuance; //tokens.AdditionalProperties.FirstOrDefault().Value.ToString().Replace("\r\n", string.Empty);

                    var tus = new List<tokenUrlCarrier>();
                    try
                    {
                        tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(tkm.Data.Urls));
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot parse token image for tokenId: " + tokens.TokenId);
                    }
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
                    tokeninfo.Divisibility = tokens.Divisibility;
                    tokeninfo.IssuanceTxid = tokens.IssueTxid;
                    tokeninfo.AggregationPolicy = tokens.AggregationPolicy;
                    tokeninfo.LockStatus = tokens.LockStatus;

                    if (!TokenMetadataCache.ContainsKey(tokens.TokenId))
                        TokenMetadataCache.TryAdd(tokens.TokenId, tokeninfo);
                    if (!TokenInfoCache.ContainsKey(tokens.TokenId))
                        TokenInfoCache.TryAdd(tokens.TokenId, tt);
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
        public void ProcessOutput(Vout output, string txid, double blocktime, DateTime time, string metadata)
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
                    catch(Exception ex) 
                    {
                        Console.WriteLine($"Cannot parse output {output.N} address in the tx {txid}.");
                    }

                    
                    var ux = new IndexedUtxo()
                    {
                        Indexed = true,
                        TransactionHash = txid,
                        Index = (int)output.N,
                        Value = (double)output.Value,
                        OwnerAddress = a,
                        Blockheight = output.Blockheight ?? 0.0,
                        Blocktime = blocktime,
                        Time = time
                    };

                    var tokeninfo = new GetTokenMetadataResponse();
                    var tokens = output.Tokens.FirstOrDefault();
                    if (tokens != null)
                    {
                        var metadataDict = NeblioTransactionHelpers.ParseCustomMetadata(metadata);
                        var metadataString = JsonConvert.SerializeObject(metadataDict);

                        ux.Metadata = metadataString;

                        tokeninfo = ProcessTokensMetadata(tokens);

                        ux.TokenId = tokens.TokenId;
                        ux.TokenUtxo = true;
                        ux.TokenAmount = tokens.Amount ?? 0.0;
                        ux.TokenSymbol = tokeninfo.TokenName;
                    }

                    if (UsedUtxos.TryGetValue(u, out var utxid))
                    {
                        ux.Used = true;
                        ux.UsedInTxHash = utxid;
                    }

                    Utxos.TryRemove(u, out var eu);
                    
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
        public static bool IsTxPOS(GetTransactionInfoResponse tx)
        {
            try
            {
                var vin = tx.Vin.FirstOrDefault();
                if (vin != null)
                {
                    var adp = vin.AdditionalProperties?.FirstOrDefault().ToString()?.Contains("coinbase") ?? false;
                    if (adp)
                        return true;
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Cannot check if tx is PoS tx. " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Load tx info from the node and analyze inputs and outputs
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="blocknumber"></param>
        /// <returns></returns>
        public async Task<GetTransactionInfoResponse?> ParseTransaction(string tx, double blockheight, bool includePOS = false )
        {
            var t = await GetTx(tx);
                
            if (t != null)
            {
                if (!includePOS && IsTxPOS(t))
                        return null;

                var utxos = new List<string>();
                var time = TimeHelpers.UnixTimestampToDateTime((t?.Time ?? 0.0) * 1000);

                var it = new IndexedTransaction()
                {
                    Indexed = true,
                    Hash = t.Txid,
                    BlockHash = t.Blockhash,
                    Blockheight = blockheight,
                    Time = time,
                    Blocktime = t?.Time ?? 0.0,
                    TxInfo = t
                };

                var cancelToken = new CancellationToken();
                var options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancelToken };
                Parallel.ForEach(t.Vin, options, item =>
                {
                    ProcessInput(item, t.Txid);
                });

                var metadata = string.Empty;
                var voutsWithMeta = new List<int>();

                if (t.Vout.Any(o => o.ScriptPubKey.Type == "nulldata"))
                {
                    // Token transaction with metadata
                    metadata = t.Vout.FirstOrDefault(o => o.ScriptPubKey.Type == "nulldata")?.ScriptPubKey.Asm ?? string.Empty;
                    var ntp1 = new NTP1Transactions() { tx_type = 1, ntp1_opreturn = string.Empty };
                    if (!string.IsNullOrEmpty(metadata))
                    {
                        var meta = metadata.Replace("OP_RETURN ", string.Empty);
                        ntp1.ntp1_opreturn = meta;
                        try
                        {
                            NTP1ScriptHelpers._NTP1ParseScript(ntp1);
                        }
                        catch (Exception ex)
                        {
                        }
                        if (ntp1.ntp1_instruct_list != null && ntp1.ntp1_instruct_list.Count >= 1)
                        {
                            foreach (var inst in ntp1.ntp1_instruct_list)
                                voutsWithMeta.Add(inst.vout_num);
                        }
                    }
                }
                
                Parallel.ForEach(t.Vout, options, item =>
                {
                    if (item.ScriptPubKey.Type != "nulldata")
                    {
                        item.Blockheight = blockheight;
                        if (voutsWithMeta.Count > 0 && voutsWithMeta.Contains((int)item.N))
                            ProcessOutput(item, t.Txid, t?.Time ?? 0.0, time, metadata);
                        else if (voutsWithMeta.Count > 0 && !voutsWithMeta.Contains((int)item.N))
                            ProcessOutput(item, t.Txid, t?.Time ?? 0.0, time, string.Empty);
                        else if (voutsWithMeta.Count == 0 && item.Value == 0.0001)
                            ProcessOutput(item, t.Txid, t?.Time ?? 0.0, time, metadata);
                        else
                            ProcessOutput(item, t.Txid, t?.Time ?? 0.0, time, string.Empty);

                        var u = $"{t.Txid}:{item.N}";
                        utxos.Add(u);
                    }
                });

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
        public async Task<List<GetTransactionInfoResponse>?> GetBlockTransactions(List<string> txs, double blockheight, bool includePOS = false)
        {
            if (txs == null)
                return null;

            var result = new List<GetTransactionInfoResponse>();

            foreach (var tx in txs)
            {
                var t = await ParseTransaction(tx, blockheight, includePOS);
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
            var blocks = Blocks.Values.Where(b => !b.Indexed).OrderByDescending(b => b.Height).ToList();

            await blocks.ParallelForEachAsync(async block =>
            {
                _ = await GetBlockTransactions(block.Transactions, block.Height, false);
                if (Blocks.TryGetValue(block.Hash, out var blk))
                    blk.Indexed = true;

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
                    var add = true;

                    if (!string.IsNullOrEmpty(blkn.Flags) && blkn.Flags == "proof-of-stake" && blkn.Tx.Count == 2 && (blkn.Size >= 400 && blkn.Size <= 450))
                        add = false;

                    if (add)
                        Blocks.TryAdd(blkn.Hash, GetIndexedBlockFromResponse(blkn));
                }
            }, maxDegreeOfParallelism: maxDegreeOfParallelism);
        }

        public IndexedBlock GetIndexedBlockFromResponse(GetBlockResponse blkn)
        {
            var b = new IndexedBlock()
            {
                Hash = blkn.Hash,
                Height = blkn.Height ?? -1,
                Time = TimeHelpers.UnixTimestampToDateTime((blkn.Time ?? 0.0) * 1000),
                Transactions = blkn.Tx.ToList()
            };
            return b;
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

                if (b != null && b.Hash != null)
                    tasks[i] = GetIndexedBlocksOptimized(b.Hash, (actualDate2 - actualDate1).TotalHours);
            }

            await Task.WhenAll(tasks);

        }


    }
}
