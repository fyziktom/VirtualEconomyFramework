using NBitcoin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.DogeAPI;
using VEDriversLite.Security;

namespace VEDriversLite
{
    public static class DogeTransactionHelpers
    {
        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        public static double FromSatToMainRatio = 100000000;
        public static Network Network = NBitcoin.Altcoins.Dogecoin.Instance.Mainnet;
        public static int MinimumConfirmations = 2;

        /// <summary>
        /// Function converts EncryptionKey (optionaly with password if it is not already loaded in ekey)
        /// and returns BitcoinAddress and BitcoinSecret classes from NBitcoin
        /// </summary>
        /// <param name="ekey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<(BitcoinAddress, BitcoinSecret)> GetAddressAndKey(EncryptionKey ekey, string password = "")
        {
            var key = string.Empty;

            if (ekey != null)
            {
                if (ekey.IsLoaded)
                {
                    if (ekey.IsEncrypted && string.IsNullOrEmpty(password) && !ekey.IsPassLoaded)
                        throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
                    else if (!ekey.IsEncrypted)
                        key = await ekey.GetEncryptedKey();
                    else if (ekey.IsEncrypted && (!string.IsNullOrEmpty(password) || ekey.IsPassLoaded))
                        if (ekey.IsPassLoaded)
                            key = await ekey.GetEncryptedKey(string.Empty);
                        else
                            key = await ekey.GetEncryptedKey(password);

                    if (string.IsNullOrEmpty(key))
                        throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");

                }
            }

            BitcoinSecret loadedkey = null;
            BitcoinAddress addressForTx = null;
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    loadedkey = Network.CreateBitcoinSecret(key);
                    addressForTx = loadedkey.GetAddress(ScriptPubKeyType.Legacy);

                    return (addressForTx, loadedkey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot send token transaction!", ex);
                    //Console.WriteLine($"Cannot send token transaction, cannot create keys");
                    throw new Exception("Cannot send token transaction. cannot create keys!");
                }
            }
            else
                throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
        }

        /// <summary>
        /// Function will sign transaction with provided key and broadcast with Neblio API
        /// </summary>
        /// <param name="transaction">NBitcoin Transaction object</param>
        /// <param name="key">NBitcoin Key - must contain Private Key</param>
        /// <param name="address">NBitcoin address - must match with the provided key</param>
        /// <returns>New Transaction Hash - TxId</returns>
        private static async Task<string> SignAndBroadcast(Transaction transaction, BitcoinSecret key, BitcoinAddress address)
        {
            // add coins
            List<ICoin> coins = new List<ICoin>();
            try
            {
                var addrutxos = await AddressUtxosAsync(address.ToString());

                // add all spendable coins of this address
                foreach (var inp in addrutxos.Utxos)
                    coins.Add(new Coin(uint256.Parse(inp.TxId), (uint)inp.N, new Money((int)inp.Value), address.ScriptPubKey));

                // add signature to inputs before signing
                foreach (var inp in transaction.Inputs)
                    inp.ScriptSig = address.ScriptPubKey;
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            // sign
            try
            {
                var tx = transaction.ToString();

                transaction.Sign(key, coins);

                var sx = transaction.ToString();

                if (tx == sx)
                    throw new Exception("Transaction was not signed. Probably not spendable source.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during signing tx! " + ex.Message);
            }

            // broadcast
            try
            {
                var txhex = transaction.ToHex();
                var res = await BroadcastTxAsync(new BroadcastTxRequest() { TxHex = txhex });
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string SendDogeTransaction(SendTxData data, EncryptionKey ekey, ICollection<Utxo> utxos, double fee = 10000)
        {
            var res = SendDogeTransactionAsync(data, ekey, utxos, fee).GetAwaiter().GetResult();
            return res;
        }
        /// <summary>
        /// Function will send standard Neblio transaction
        /// </summary>
        /// <param name="data">Send data, please see SendTxData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="utxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 100000000sat = 1 DOGE minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SendDogeTransactionAsync(SendTxData data, EncryptionKey ekey, ICollection<Utxo> utxos, double fee = 100000000)
        {
            var res = "ERROR";

            if (data == null)
                throw new Exception("Data cannot be null!");

            if (ekey == null)
                throw new Exception("Account cannot be null!");

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey, data.Password);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create template for new tx from last one
            var transaction = Transaction.Create(Network); // new NBitcoin.Altcoins.Neblio.NeblioTransaction(network.Consensus.ConsensusFactory);//neblUtxo.Clone();

            try
            {
                // add inputs of tx
                foreach (var utxo in utxos)
                {
                    var txin = new TxIn(new OutPoint(uint256.Parse(utxo.TxId), utxo.N));
                    txin.ScriptSig = addressForTx.ScriptPubKey;
                    transaction.Inputs.Add(txin);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            try
            {
                var allNeblCoins = 0.0;
                foreach (var u in utxos)
                    allNeblCoins += (double)u.Value;

                var amountinSat = Convert.ToUInt64(data.Amount * FromSatToMainRatio);
                var diffinSat = Convert.ToUInt64(allNeblCoins) - amountinSat - Convert.ToUInt64(fee);

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during creating outputs. " + ex.Message);
            }

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static async Task<string> SendDogeTransactionWithMessageAsync(SendTxData data, EncryptionKey ekey, ICollection<Utxo> utxos, double fee = 100000000)
        {
            var res = "ERROR";

            if (data == null)
                throw new Exception("Data cannot be null!");

            if (ekey == null)
                throw new Exception("Account cannot be null!");

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey, data.Password);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create template for new tx from last one
            var transaction = Transaction.Create(Network); // new NBitcoin.Altcoins.Neblio.NeblioTransaction(network.Consensus.ConsensusFactory);//neblUtxo.Clone();

            try
            {
                // add inputs of tx
                foreach (var utxo in utxos)
                {
                    var txin = new TxIn(new OutPoint(uint256.Parse(utxo.TxId), utxo.N));
                    txin.ScriptSig = addressForTx.ScriptPubKey;
                    transaction.Inputs.Add(txin);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            try
            {
                var allNeblCoins = 0.0;
                foreach (var u in utxos)
                    allNeblCoins += (double)u.Value;

                var amountinSat = Convert.ToUInt64(data.Amount * FromSatToMainRatio);
                var diffinSat = Convert.ToUInt64(allNeblCoins) - amountinSat - Convert.ToUInt64(fee);

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                var bytes = Encoding.UTF8.GetBytes(data.CustomMessage); 
                transaction.Outputs.Add(new TxOut(){ 
                    Value = 0,    
                    ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
                });
                transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during creating outputs. " + ex.Message);
            }

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        ///////////////////////////////////////////
        // calls of Doge API and helpers

        /// <summary>
        /// Returns private client for Neblio API. If it is null, it will create new instance.
        /// </summary>
        /// <returns></returns>
        private static IClient GetClient()
        {
            if (_client == null)
                _client = (IClient)new Client(httpClient);
            return _client;
        }

        /// <summary>
        /// Broadcast of signed transaction.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> BroadcastTxAsync(BroadcastTxRequest data)
        {
            var info = await GetClient().BroadcastTxAsync(data);
            return info.TxId;
        }

        /// <summary>
        /// Return address balance.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<GetAddressBalanceResponse> AddressBalanceAsync(string addr)
        {
            var address = await GetClient().GetAddressBalanceAsync(addr);
            return address;
        }

        /// <summary>
        /// Return address info object. this object contains list of Utxos.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<GetAddressUtxosResponse> AddressUtxosAsync(string addr)
        {
            var address = await GetClient().GetAddressUtxosAsync(addr);
            return address;
        }

        /// <summary>
        /// Return transaction object
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<GetTransactionInfoResponse> TransactionInfoAsync(string txid)
        {
            var tx = await GetClient().GetTransactionInfoAsync(txid);
            return tx;
        }

        /// <summary>
        /// Returns list of spendable utxos which together match some input required amount for some transaction
        /// </summary>
        /// <param name="addr">address which has utxos for spend - sender in tx</param>
        /// <param name="minAmount">minimum amount of one utxo</param>
        /// <param name="requiredAmount">amount what must be collected even by multiple utxos</param>
        /// <returns></returns>
        public static async Task<ICollection<Utxo>> GetAddressSpendableUtxo(string addr, double minAmount = 0.0001, double requiredAmount = 0.0001)
        {
            var resp = new List<Utxo>();

            var addinfo = await GetClient().GetAddressUtxosAsync(addr);
            var utxos = addinfo.Utxos;
            if (utxos == null)
                return resp;
            utxos = utxos.OrderBy(u => u.Value).Reverse().ToList();

            var founded = 0.0;
            foreach (var utx in utxos)
                if (utx.Confirmations > MinimumConfirmations && utx.Value > 10000)
                    if (((double)utx.Value) > (minAmount * FromSatToMainRatio))
                    {
                        resp.Add(utx);
                        founded += ((double)utx.Value / FromSatToMainRatio);
                        if (founded > requiredAmount)
                            return resp;
                    }

            return resp;
        }

    }
}
