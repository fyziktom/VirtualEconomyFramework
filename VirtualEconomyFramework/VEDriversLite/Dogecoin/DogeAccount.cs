using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.DogeAPI;
using VEDriversLite.Events;
using VEDriversLite.NFT;
using VEDriversLite.Security;

namespace VEDriversLite
{
    /// <summary>
    /// Main Dogecoin Account class
    /// </summary>
    public class DogeAccount
    {
        private static object _lock = new object();
        /// <summary>
        /// Doge Address hash
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Loaded Secret, NBitcoin Class which carry Public Key and Private Key
        /// </summary>
        public BitcoinSecret Secret { get; set; }
        /// <summary>
        /// Address in form of BitcoinAddress object
        /// </summary>
        public BitcoinAddress BAddress { get; set; }

        /// <summary>
        /// Total actual balance based on Utxos. This means sum of spendable and unconfirmed balances.
        /// </summary>
        public double TotalBalance { get; set; } = 0.0;
        /// <summary>
        /// Total spendable balance based on Utxos.
        /// </summary>
        public double TotalSpendableBalance { get; set; } = 0.0;
        /// <summary>
        /// Total balance which is now unconfirmed based on Utxos.
        /// </summary>
        public double TotalUnconfirmedBalance { get; set; } = 0.0;

        /// <summary>
        /// Actual list of all Utxos on this address.
        /// </summary>
        public List<Utxo> Utxos { get; set; } = new List<Utxo>();

        /// <summary>
        /// Actual list of last 100 Spended transactions on this address.
        /// </summary>
        public List<SpentTx> SentTransactions { get; set; } = new List<SpentTx>();

        /// <summary>
        /// Actual list of last 100 Received transactions on this address.
        /// </summary>
        public List<ReceivedTx> ReceivedTransactions { get; set; } = new List<ReceivedTx>();

        /// <summary>
        /// This event is called whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;

        /// <summary>
        /// This event is called whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is called whenever new doge transaction is received
        /// </summary>
        public event EventHandler<IEventInfo> NewDogeUtxos;

        /// <summary>
        /// Carrier for encrypted private key from storage and its password.
        /// </summary>
        [JsonIgnore]
        public EncryptionKey AccountKey { get; set; }

        /// <summary>
        /// This function will check if the account is locked or unlocked.
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            if (AccountKey != null)
            {
                if (AccountKey.IsEncrypted)
                {
                    if (AccountKey.IsPassLoaded)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (AccountKey.IsLoaded)
                        return false;
                    else
                        return true;
                }
            }
            else
                return true;
        }

        /// <summary>
        /// Invoke Success message info event
        /// </summary>
        /// <param name="txid">new tx id hash</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        private async Task InvokeSendPaymentSuccessEvent(string txid, string title = "Neblio Payment Sent")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Info,
                                                    title,
                                                    $"Successfull send. Please wait a while for enough confirmations.",
                                                    Address,
                                                    txid,
                                                    100));
        }

        /// <summary>
        /// Invoke Error message because account is locked
        /// </summary>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        private async Task InvokeAccountLockedEvent(string title = "Cannot send transaction")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Error,
                                                    title,
                                                    "Account is Locked. Please unlock it in account page.",
                                                    Address,
                                                    string.Empty,
                                                    100));
        }
        /// <summary>
        /// Invoke Error message which occured during sending of the transaction
        /// </summary>
        /// <param name="errorMessage">Error message content</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        private async Task InvokeErrorDuringSendEvent(string errorMessage, string title = "Cannot send transaction")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Error,
                                                    title,
                                                    errorMessage,
                                                    Address,
                                                    string.Empty,
                                                    100));
        }

        /// <summary>
        /// Invoke Common Error message
        /// </summary>
        /// <param name="errorMessage">Error message content</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        private async Task InvokeErrorEvent(string errorMessage, string title = "Error")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Error,
                                                    title,
                                                    errorMessage,
                                                    Address,
                                                    string.Empty,
                                                    100));
        }

        private async Task<(bool, string)> SignBroadcastAndInvokeSucessEvent(Transaction transaction, BitcoinSecret Secret, ICollection<Utxo> utxos, string message)
        {
            var rtxid = await DogeTransactionHelpers.SignAndBroadcastTransaction(transaction, Secret, utxos);

            if (rtxid != null)
            {
                await InvokeSendPaymentSuccessEvent(rtxid, message);
                return (true, rtxid);
            }
            return (false, "");
        }

        /// <summary>
        /// This function will create new account - Doge address and its Private key.
        /// </summary>
        /// <param name="password">Input password, which will encrypt the Private key</param>
        /// <param name="saveToFile">if you want to save it to the file (dont work in the WASM) set this. It will save to root exe path as key.txt</param>
        /// <returns></returns>
        public async Task<bool> CreateNewAccount(string password, bool saveToFile = false)
        {
            try
            {
                await Task.Run(async () =>
                {
                    Key privateKey = new Key(); // generate a random private key
                    PubKey publicKey = privateKey.PubKey;
                    BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(DogeTransactionHelpers.Network);
                    var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, DogeTransactionHelpers.Network);
                    Address = address.ToString();

                    // todo load already encrypted key
                    AccountKey = new Security.EncryptionKey(privateKeyFromNetwork.ToString(), password);
                    AccountKey.PublicKey = Address;
                    Secret = privateKeyFromNetwork;
                    BAddress = Secret.GetAddress(ScriptPubKeyType.Legacy);

                    if (saveToFile)
                    {
                        // save to file
                        var kdto = new KeyDto()
                        {
                            Address = Address,
                            Key = AccountKey.GetEncryptedKey(returnEncrypted: true)
                        };
                        FileHelpers.WriteTextToFile("dogekey.txt", JsonConvert.SerializeObject(kdto));
                    }
                });

                await StartRefreshingData();

                return true;
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot Create Account");
            }

            return false;
        }


        /// <summary>
        /// Load account from "key.txt" file placed in the root exe directory. Doesnt work in WABS
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <param name="filename">Filename with the key. Default name is dogekey.txt</param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password, string filename = "dogekey.txt")
        {
            if (FileHelpers.IsFileExists(filename))
            {
                try
                {
                    var k = FileHelpers.ReadTextFromFile(filename);
                    var kdto = JsonConvert.DeserializeObject<KeyDto>(k);

                    AccountKey = new EncryptionKey(kdto.Key, fromDb: true);
                    AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                    Address = kdto.Address;

                    Secret = new BitcoinSecret(AccountKey.GetEncryptedKey(), DogeTransactionHelpers.Network);
                    BAddress = Secret.GetAddress(ScriptPubKeyType.Legacy);

                    await StartRefreshingData();
                    return true;
                }
                catch
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
                }
            }
            else
            {
                await CreateNewAccount(password);
            }

            return false;
        }

        /// <summary>
        /// Load account from password, input encrypted private key and address.
        /// It expect the private key is encrypted by the password.
        /// It uses AES encryption
        /// </summary>
        /// <param name="password"></param>
        /// <param name="encryptedKey"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password, string encryptedKey, string address = "")
        {
            try
            {
                await Task.Run(async () =>
                {
                    AccountKey = new EncryptionKey(encryptedKey, fromDb: true);
                    //await AccountKey.LoadPassword(password);
                    
                    if (!string.IsNullOrEmpty(password))
                    {
                        AccountKey = new EncryptionKey(encryptedKey, fromDb: true);
                    }
                    else
                    {
                        AccountKey = new EncryptionKey(encryptedKey, fromDb: false);
                    }
                    if (!string.IsNullOrEmpty(password))
                    {
                        AccountKey.LoadPassword(password);
                        AccountKey.IsEncrypted = true;
                    }
                    Secret = new BitcoinSecret(AccountKey.GetEncryptedKey(), DogeTransactionHelpers.Network);
                    BAddress = Secret.GetAddress(ScriptPubKeyType.Legacy);

                    if (string.IsNullOrEmpty(address))
                    {
                        var add = DogeTransactionHelpers.GetAddressFromPrivateKey(Secret.ToString());
                        if (add.Success) Address = add.Value.ToString();
                    }
                    else
                    {
                        Address = address;
                    }

                });

                await StartRefreshingData();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load the Doge account. " + ex.Message);
                //await InvokeErrorEvent(ex.Message, "Cannot Load Account");
                //throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
            }

            return false;
        }

        /// <summary>
        /// Load account just for observation
        /// You cannot sign tx when you load address this way
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns></returns>
        public async Task<bool> LoadAccountWithDummyKey(string address)
        {
            try
            {
                Key k = new Key();
                BitcoinSecret privateKeyFromNetwork = k.GetBitcoinSecret(DogeTransactionHelpers.Network);
                AccountKey = new EncryptionKey(privateKeyFromNetwork.ToString(), fromDb: false);
                Secret = privateKeyFromNetwork;
                Address = address;//Secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

                await StartRefreshingData();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address! " + ex.Message);
            }
        }


        /// <summary>
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        public async Task<string> StartRefreshingData(int interval = 5000)
        {
            try
            {
                await ReloadUtxos();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Canont load dogecoin utxos. " + ex.Message);
            }
            var first = true;
            // todo cancelation token
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (!first)
                            await ReloadUtxos();
                        else
                            first = false;

                        await GetListOfReceivedTransactions();
                        await GetListOfSentTransactions();
                        Refreshed?.Invoke(this, null);
                    }
                    catch
                    {
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }
                    await Task.Delay(interval);
                }
            });

            return await Task.FromResult("RUNNING");
        }


        /// <summary>
        /// Reload address Utxos list. It will sort descending the utxos based on the utxos number of confirmations.
        /// Smallest number of confirmations leads to newest transations
        /// </summary>
        /// <returns></returns>
        public async Task ReloadUtxos()
        {
            var count = Utxos.Count;

            GetAddressUtxosResponse ux = null;
            try
            {
                ux = await DogeTransactionHelpers.AddressUtxosAsync(Address);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot get dogecoin address utxos. Please check the internet connection. " + ex.Message);
                return;
            }
            if (ux == null)
            {
                Console.WriteLine("Cannot get dogecoin address utxos. Please check the internet connection. ");
                return;
            }

            var ouxox = ux.Data.Utxos.OrderBy(u => u.Confirmations).ToList();

            if (ouxox.Count > 0)
            {
                lock (_lock)
                {
                    Utxos.Clear();
                }
                TotalBalance = 0.0;
                TotalUnconfirmedBalance = 0.0;
                TotalSpendableBalance = 0.0;
                // add new ones
                foreach (var u in ouxox)
                {
                    if (u.Confirmations <= DogeTransactionHelpers.MinimumConfirmations)
                        TotalUnconfirmedBalance += (Convert.ToDouble(u.Value, CultureInfo.InvariantCulture));
                    else
                    {
                        TotalSpendableBalance += (Convert.ToDouble(u.Value, CultureInfo.InvariantCulture));
                        lock (_lock)
                        {
                            Utxos.Add(u);
                        }
                    }
                }

                TotalBalance = TotalSpendableBalance + TotalUnconfirmedBalance;
            }

            if (count != Utxos.Count)
                NewDogeUtxos?.Invoke(this, await EventFactory.GetEvent(EventType.Info,
                                                "New Doge Transactions",
                                                "Received new dogecoin transactions."));
        }

        /// <summary>
        /// This function will get list of spended transaction
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<SpentTx>> GetListOfSentTransactions()
        {
            try
            {
                List<SpentTx> txs = null;
                try
                {
                    txs = await DogeTransactionHelpers.AddressSpendTxsAsync(Address);
                    txs.Reverse();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot get dogecoin address spent transactions. Please check the internet connection or API provider. " + ex.Message);
                    return null;
                }
                if (txs == null)
                {
                    Console.WriteLine("Cannot get dogecoin address spent transactions. Please check the internet  or API provider. ");
                    return null;
                }
                SentTransactions = txs;
                return txs;
            }
            catch
            {
                Console.WriteLine("Cannot load txs history");
            }
            return new List<SpentTx>();
        }

        /// <summary>
        /// This function will get list of received transaction
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<ReceivedTx>> GetListOfReceivedTransactions()
        {
            try
            {
                List<ReceivedTx> txs = null;
                try
                {
                    txs = await DogeTransactionHelpers.AddressReceivedTxsAsync(Address); 
                    txs.Reverse();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot get dogecoin address received transactions. Please check the internet connection or API provider. " + ex.Message);
                    return null;
                }
                if (txs == null)
                {
                    Console.WriteLine("Cannot get dogecoin address received transactions. Please check the internet  or API provider. ");
                    return null;
                }
                ReceivedTransactions = txs;
                return txs;
            }
            catch
            {
                Console.WriteLine("Cannot load txs history");
            }
            return new List<ReceivedTx>();
        }

        /// <summary>
        /// This function will check if there is some spendable doge of specific amount and returns list of the utxos for the transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(string, ICollection<Utxo>)> CheckSpendableDoge(double amount)
        {
            try
            {
                var utxos = await DogeTransactionHelpers.GetAddressSpendableUtxo(Address, 0.0002, amount);
                if (utxos == null || utxos.Count == 0)
                    return ($"You dont have enough Doge on the address. You need 5 Doge more than you want to send (for max fee). Probably waiting for more than {DogeTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                    return ("OK", utxos);
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Doge. " + ex.Message, null);
            }
        }

        /// <summary>
        /// Send Doge payment
        /// </summary>
        /// <param name="receiver">Receiver Doge Address</param>
        /// <param name="amount">Ammount in Doge</param>
        /// <param name="message">add message to OP_RETURN data. max 83 bytes</param>
        /// <param name="utxo">from specific utxo</param>
        /// <param name="N">with specific utxo index</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendPayment(string receiver, double amount, string message = "", string utxo = "", int N = 0)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableDoge(amount + 5);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Doge");
                return (false, res.Item1);
            }

            if (!string.IsNullOrEmpty(utxo))
            {
                res.Item2 = new List<Utxo>() { new Utxo()
                {
                     N = N,
                     TxId = utxo
                }};
            }
            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver,
                CustomMessage = message
            };

            try
            {
                // send tx
                var rtxid = string.Empty;
                Transaction transaction;

                if (string.IsNullOrEmpty(message))
                    transaction = DogeTransactionHelpers.GetDogeTransactionAsync(dto, BAddress, res.Item2);
                else
                    transaction = DogeTransactionHelpers.GetDogeTransactionWithMessageAsync(dto, BAddress, res.Item2);

                var result = await SignBroadcastAndInvokeSucessEvent(transaction, Secret, res.Item2, "Doge Payment Sent");
                if (result.Item1)
                {
                    return (true, result.Item2);
                }                
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Send Doge payment with multiple inputs
        /// </summary>
        /// <param name="receiver">Receiver Doge Address</param>
        /// <param name="amount">Ammount in Doge</param>
        /// <param name="utxos"></param>
        /// <param name="message"></param>
        /// <param name="fee"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendMultipleInputPayment(string receiver, double amount, List<Utxo> utxos, string message = "", UInt64 fee = 100000000)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableDoge(amount + 5);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            if (utxos.Count > 0)
            {
                res.Item2 = utxos;
            }

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver,
                CustomMessage = message
            };

            try
            {
                // send tx
                var rtxid = string.Empty;
                Transaction transaction;
                if (string.IsNullOrEmpty(message))
                    transaction = DogeTransactionHelpers.GetDogeTransactionAsync(dto, BAddress, res.Item2);
                else
                    transaction = DogeTransactionHelpers.GetDogeTransactionWithMessageAsync(dto, BAddress, res.Item2);

                var result = await SignBroadcastAndInvokeSucessEvent(transaction, Secret, res.Item2, "Doge Payment Sent");
                if (result.Item1)
                {
                    return (true, result.Item2);
                }                
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Send Doge payment with multiple outputs
        /// </summary>
        /// <param name="receiverAmounts"></param>
        /// <param name="utxos"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendMultipleOutputPayment(Dictionary<string,double> receiverAmounts, List<Utxo> utxos, string message = "")
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var totam = 0.0;
            foreach(var r in receiverAmounts)
                totam += r.Value;
            var res = await CheckSpendableDoge(totam + 5);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }
            if (utxos.Count > 0)
            {
                res.Item2 = utxos;
            }

            try
            {
                // send tx
                var transaction = DogeTransactionHelpers.SendDogeTransactionWithMessageMultipleOutputAsync(receiverAmounts, BAddress, res.Item2, message: message);

                var result = await SignBroadcastAndInvokeSucessEvent(transaction, Secret, res.Item2, "Doge Payment Sent");
                if (result.Item1)
                {
                    return (true, result.Item2);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        
        /// <summary>
        /// Buy the NFT based on the NFT and Neblio Address
        /// </summary>
        /// <param name="neblioAddress"></param>
        /// <param name="receiver"></param>
        /// <param name="nft"></param>
        /// <returns></returns>
        public async Task<(bool, string)> BuyNFT(string neblioAddress, string receiver, INFT nft)
        {
            if (!nft.PriceActive)
                return (false, "NFT does not have setted up the price. It is not sellable NFT.");

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableDoge(nft.Price);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            var nftid = nft.Utxo + ":" + nft.UtxoIndex.ToString();
            var msg = neblioAddress + "=" + String.Format("{0:X}", nftid.GetHashCode());

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTxData for another properties such as specify source UTXOs
            {
                Amount = nft.Price,
                SenderAddress = Address,
                ReceiverAddress = receiver,
                CustomMessage = msg
            };

            try
            {
                // send tx
                var transaction = DogeTransactionHelpers.GetDogeTransactionWithMessageAsync(dto, BAddress, res.Item2);

                var result = await SignBroadcastAndInvokeSucessEvent(transaction, Secret, res.Item2, "Doge Payment Sent");
                if (result.Item1)
                {
                    return (true, result.Item2);
                }               
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Sign custom message with use of account Private Key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SignMessage(string message)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var key = AccountKey.GetEncryptedKey();
            return await ECDSAProvider.SignMessage(message, Secret);
        }

        /// <summary>
        /// Verify message which was signed by some address.
        /// </summary>
        /// <param name="message">Input message</param>
        /// <param name="signature">Signature of this message created by owner of some doge address.</param>
        /// <param name="address">Doge address which should sign the message and should be verified.</param>
        /// <returns></returns>
        public async Task<(bool, string)> VerifyMessage(string message, string signature, string address)
        {
            return await ECDSAProvider.VerifyDogeMessage(message, signature, address);
        }

    }
}
