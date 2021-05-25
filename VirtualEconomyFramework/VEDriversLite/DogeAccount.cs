using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.DogeAPI;
using VEDriversLite.Events;
using VEDriversLite.Security;

namespace VEDriversLite
{
    public class DogeAccount
    {
        /// <summary>
        /// Doge Address hash
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Loaded Secret, NBitcoin Class which carry Public Key and Private Key
        /// </summary>
        public BitcoinSecret Secret { get; set; }

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
        /// This event is called whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;

        /// <summary>
        /// This event is called whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public event EventHandler<IEventInfo> NewEventInfo;

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
                    if (!string.IsNullOrEmpty(password))
                        AccountKey.PasswordHash = await Security.SecurityUtils.HashPassword(password);
                    SignMessage("init");
                    if (saveToFile)
                    {
                        // save to file
                        var kdto = new KeyDto()
                        {
                            Address = Address,
                            Key = await AccountKey.GetEncryptedKey(returnEncrypted: true)
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
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password)
        {
            if (FileHelpers.IsFileExists("dogekey.txt"))
            {
                try
                {
                    var k = FileHelpers.ReadTextFromFile("dogekey.txt");
                    var kdto = JsonConvert.DeserializeObject<KeyDto>(k);

                    AccountKey = new EncryptionKey(kdto.Key, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                    Address = kdto.Address;

                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), DogeTransactionHelpers.Network);
                    SignMessage("init");
                    await StartRefreshingData();
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
                }
            }
            else
            {
                CreateNewAccount(password);
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
        public async Task<bool> LoadAccount(string password, string encryptedKey, string address)
        {
            try
            {
                await Task.Run(async () =>
                {
                    AccountKey = new EncryptionKey(encryptedKey, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), DogeTransactionHelpers.Network);
                    Address = address;
                    SignMessage("init");
                });

                await StartRefreshingData();

            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot Load Account");
                //throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
            }

            return false;
        }


        /// <summary>
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        public async Task<string> StartRefreshingData(int interval = 3000)
        {
            try
            {
                await ReloadUtxos();
            }
            catch (Exception ex)
            {
                // todo
            }

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await ReloadUtxos();
                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
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
            var ux = await DogeTransactionHelpers.AddressUtxosAsync(Address);
            var ouxox = ux.Utxos.OrderBy(u => u.Confirmations).ToList();

            if (ouxox.Count > 0)
            {
                Utxos.Clear();
                TotalBalance = 0.0;
                TotalUnconfirmedBalance = 0.0;
                TotalSpendableBalance = 0.0;
                // add new ones
                foreach (var u in ouxox)
                {
                    Utxos.Add(u);

                    if (u.Confirmations == 0)
                        TotalUnconfirmedBalance += ((double)u.Value / DogeTransactionHelpers.FromSatToMainRatio);
                    else
                        TotalSpendableBalance += ((double)u.Value / DogeTransactionHelpers.FromSatToMainRatio);
                }

                TotalBalance = TotalSpendableBalance + TotalUnconfirmedBalance;
            }
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
                    return ($"You dont have Doge on the address. Probably waiting for more than {DogeTransactionHelpers.MinimumConfirmations} confirmations.", null);
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
        /// <returns></returns>
        public async Task<(bool, string)> SendPayment(string receiver, double amount)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableDoge(amount);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable inputs");
                return (false, res.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver
            };

            try
            {
                // send tx
                var rtxid = await DogeTransactionHelpers.SendDogeTransactionAsync(dto, AccountKey, res.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Doge Payment Sent");
                    return (true, rtxid);
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
            var key = await AccountKey.GetEncryptedKey();
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
