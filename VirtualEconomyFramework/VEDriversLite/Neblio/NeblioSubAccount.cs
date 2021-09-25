using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Bookmarks;
using VEDriversLite.Security;
using System.Threading.Tasks;
using System.Linq;
using VEDriversLite.Events;
using NBitcoin;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.Dto;
using System.Collections.Concurrent;

namespace VEDriversLite.Neblio
{
    public class NeblioSubAccount
    {
        private static object _lock { get; set; } = new object();
        /// <summary>
        /// Neblio Address hash
        /// </summary>
        public string Address { get; set; } = string.Empty;
        public string EKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// When this flag is set, account reload the Utxos state - inside autorefresh
        /// </summary>
        [JsonIgnore]
        public bool IsAutoRefreshActive { get; set; } = false;
        /// <summary>
        /// List of actual address NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();

        /// <summary>
        /// Received payments (means Payment NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// List of actual address Coruzant NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public List<INFT> CoruzantNFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// Actual list of all Utxos on this address.
        /// </summary>
        [JsonIgnore]
        public List<Utxos> Utxos { get; set; } = new List<Utxos>();
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        [JsonIgnore]
        public bool IsInBookmark { get; set; } = false;
        /// <summary>
        /// Actual loaded address info. It has inside list of all transactions.
        /// </summary>
        [JsonIgnore]
        public GetAddressResponse AddressInfo { get; set; } = new GetAddressResponse();
        /// <summary>
        /// Actual loaded address info with list of Utxos. When utxos are loaded first, this is just fill with it to prevent not necessary API request.
        /// </summary>
        [JsonIgnore]
        public GetAddressInfoResponse AddressInfoUtxos { get; set; } = new GetAddressInfoResponse();
        /// <summary>
        /// Carrier for encrypted private key from storage and its password.
        /// </summary>
        [JsonIgnore]
        public EncryptionKey AccountKey { get; set; }
        /// <summary>
        /// Loaded Secret, NBitcoin Class which carry Public Key and Private Key
        /// </summary>
        [JsonIgnore]
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
        /// Total balance of VENFT tokens which can be used for minting purposes.
        /// </summary>
        public double SourceTokensBalance { get; set; } = 0.0;
        /// <summary>
        /// Total balance of Coruzant tokens which can be used for minting purposes.
        /// </summary>
        public double CoruzantSourceTokensBalance { get; set; } = 0.0;

        /// <summary>
        /// If there is some Doge address in same project which should be searched for the payments triggers fill it here
        /// </summary>
        public string ConnectedDogeAccountAddress { get; set; } = string.Empty;

        /// <summary>
        /// Actual all token supplies. Consider also other tokens than VENFT.
        /// </summary>
        public Dictionary<string, TokenSupplyDto> TokensSupplies { get; set; } = new Dictionary<string, TokenSupplyDto>();

        /// <summary>
        /// This event is called whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is called whenever the list of NFTs is changed
        /// </summary>
        public event EventHandler<string> NFTsChanged;

        /// <summary>
        /// This event is called whenever some progress during multimint happens
        /// </summary>
        public event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public event EventHandler<string> FirsLoadingStatus;

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
                                                    $"Successful send. Please wait a while for enough confirmations.",
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

        public async Task<(bool,string)> CreateAddress(BitcoinSecret mainSecret, string name)
        {
            if (!string.IsNullOrEmpty(Address))
                return (false, "Account already contains address.");

            try
            {
                Key privateKey = new Key(); // generate a random private key
                PubKey publicKey = privateKey.PubKey;
                BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(NeblioTransactionHelpers.Network);
                var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
                Address = address.ToString();
                Secret = privateKeyFromNetwork;
                // todo load already encrypted key
                AccountKey = new Security.EncryptionKey(privateKeyFromNetwork.ToString());
                AccountKey.PublicKey = Address;
                EKey = mainSecret.PubKey.Encrypt(privateKeyFromNetwork.ToString());
                Name = name;
                return (true, Address);
            }
            catch(Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        public async Task<(bool,string)> LoadFromBackupString(BitcoinSecret mainSecret, string inputData)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<AccountExportDto>(inputData);
                if (dto == null)
                    return (false, "Cannot load SubAccount from backup. Wrong input data.");

                Address = dto.Address;
                EKey = dto.EKey;
                Name = dto.Name;
                var key = mainSecret.PrivateKey.Decrypt(dto.EKey);
                AccountKey = new EncryptionKey(key);
                AccountKey.PublicKey = Address;
                Secret = new BitcoinSecret(key, NeblioTransactionHelpers.Network);
                return (true, Address);
            }
            catch (Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> LoadFromBackupDto(BitcoinSecret mainSecret, AccountExportDto dto)
        {
            try
            {
                if (dto == null)
                    return (false, "Cannot load SubAccount from backup. Wrong input data.");

                Address = dto.Address;
                EKey = dto.EKey;
                Name = dto.Name;
                var key = mainSecret.PrivateKey.Decrypt(dto.EKey);
                AccountKey = new EncryptionKey(key);
                AccountKey.PublicKey = Address;
                Secret = new BitcoinSecret(key, NeblioTransactionHelpers.Network);
                return (true, Address);
            }
            catch (Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        public async Task<(bool,string)> BackupAddressToString()
        {
            if (string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(EKey))
                return (false, "Account is not loaded.");

            try
            {
                var dto = new AccountExportDto()
                {
                    Address = Address,
                    EKey = EKey,
                    Name = Name
                };
                var res = JsonConvert.SerializeObject(dto);
                return (true, res);
            }
            catch(Exception ex)
            {
                return (false, "Cannot backup address from SubAccount. " + ex.Message);
            }
        }

        public async Task<(bool, AccountExportDto)> BackupAddressToDto()
        {
            if (string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(EKey))
                return (false, null);

            try
            {
                var dto = new AccountExportDto()
                {
                    Address = Address,
                    EKey = EKey,
                    Name = Name
                };
                return (true, dto);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }

        public void LoadBookmark(Bookmark bkm)
        {
            if (!string.IsNullOrEmpty(bkm.Address) && !string.IsNullOrEmpty(bkm.Name))
            {
                IsInBookmark = true;
                BookmarkFromAccount = bkm;
            }
            else
                ClearBookmark();
        }
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }

        /// <summary>
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        public async Task<string> StartRefreshingData(int interval = 3000)
        {
            if (string.IsNullOrEmpty(Address) || !AccountKey.IsLoaded)
            {
                await InvokeErrorEvent("Please fill subaccount Address and Key first.", "Not loaded address and key.");
                return await Task.FromResult("Please fill subaccount Address and Key first.");
            }

            try
            {
                FirsLoadingStatus?.Invoke(this, "Loading of Sub Account data started.");
                AddressInfo = new GetAddressResponse();
                AddressInfo.Transactions = new List<string>();
                await ReloadUtxos();
                await ReLoadNFTs();
                await RefreshAddressReceivedPayments();
                await CheckPayments();
                await ReloadTokenSupply();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Problem during first load of subaccount. " + ex.Message);
            }

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                var minorRefresh = 5;
                var firstLoad = true;

                while (true)
                {
                    try
                    {
                        if (IsAutoRefreshActive && !firstLoad)
                        {
                            await ReloadUtxos();
                            await ReLoadNFTs();
                            await ReloadTokenSupply();
                            await RefreshAddressReceivedPayments();
                            await CheckPayments();
                        }
                        if (firstLoad)
                            firstLoad = false;
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
        /// Reload address Utxos list. It will sort descending the utxos based on the utxos time stamps.
        /// </summary>
        /// <returns></returns>
        public async Task ReloadUtxos()
        {
            var ux = await NeblioTransactionHelpers.GetAddressUtxosObjects(Address);
            var ouxox = ux.OrderBy(u => u.Blocktime).Reverse().ToList();
            AddressInfoUtxos = new GetAddressInfoResponse()
            {
                Utxos = ouxox
            };

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
                    if (u.Blockheight <= 0)
                        TotalUnconfirmedBalance += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);
                    else
                        TotalSpendableBalance += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);
                }

                TotalBalance = TotalSpendableBalance + TotalUnconfirmedBalance;
            }
        }

        /// <summary>
        /// This function will reload changes in the NFTs list based on provided list of already loaded utxos.
        /// </summary>
        /// <returns></returns>
        public async Task ReLoadNFTs()
        {
            try
            {
                if (!string.IsNullOrEmpty(Address))
                {
                    NFTHelpers.NFTLoadingStateChanged -= NFTHelpers_LoadingStateChangedHandler;
                    NFTHelpers.NFTLoadingStateChanged += NFTHelpers_LoadingStateChangedHandler;

                    var lastnft = NFTs.FirstOrDefault();
                    var lastcount = NFTs.Count;
                    NFTs = await NFTHelpers.LoadAddressNFTs(Address, Utxos.ToList(), NFTs.ToList());
                    if (lastnft != null)
                    {
                        if (NFTs.Count != lastcount)
                            NFTsChanged?.Invoke(this, "Changed");
                        var nft = NFTs.FirstOrDefault();
                        //Console.WriteLine("Last time: " + lastnft.Time.ToString());
                        //Console.WriteLine("Newest time: " + nft.Time.ToString());
                        if (nft != null)
                            if (nft.Time != lastnft.Time)
                                NFTsChanged.Invoke(this, "Changed");
                    }
                    else if (lastnft == null && NFTs.Count > 0)
                    {
                        NFTsChanged?.Invoke(this, "Changed");
                    }
                    CoruzantNFTs = await CoruzantNFTHelpers.GetCoruzantNFTs(NFTs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reload NFTs. " + ex.Message);
            }
            finally
            {
                NFTHelpers.NFTLoadingStateChanged -= NFTHelpers_LoadingStateChangedHandler;
            }
        }

        /// <summary>
        /// This function will search NFT Payments in the NFTs list and load them into ReceivedPayments list. 
        /// This list is cleared at the start of this function
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAddressReceivedPayments()
        {
            try
            {
                ReceivedPayments.Clear();
                var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                foreach (var p in pnfts)
                    ReceivedPayments.TryAdd(p.NFTOriginTxId, p);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh address received payments. " + ex.Message);
            }
        }

        /// <summary>
        /// This function will check payments and try to find them complementary NFT which was sold. 
        /// If there is price mathc and enough of confirmations it will try to send NFT to new owner.
        /// </summary>
        /// <returns></returns>
        public async Task CheckPayments()
        {
            try
            {
                List<INFT> pnfts = null;
                lock (_lock)
                {
                    pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                }
                if (pnfts != null && pnfts.Count > 0)
                {
                    foreach (var p in pnfts)
                    {
                        if (!((PaymentNFT)p).AlreadySoldItem)
                        {
                            var txinfo = await NeblioTransactionHelpers.GetTransactionInfo(p.Utxo);
                            if (txinfo != null && txinfo.Confirmations > NeblioTransactionHelpers.MinimumConfirmations)
                            {
                                INFT pn = null;
                                lock (_lock)
                                {
                                    pn = NFTs.FirstOrDefault(n => (n.Utxo == ((PaymentNFT)p).NFTUtxoTxId && n.UtxoIndex == ((PaymentNFT)p).UtxoIndex));
                                }

                                if (pn != null)
                                {
                                    Console.WriteLine($"Payment for NFT {pn.Name} received and it will be processed.");
                                    Console.WriteLine($"Received amount {p.Price} NEBL.");
                                    Console.WriteLine($"Requested amount {pn.Price} NEBL.");

                                    if (pn != null && pn.Price > 0 && p.Price >= pn.Price)
                                    {
                                        try
                                        {
                                            var res = await CheckSpendableNeblio(0.001);
                                            if (res.Item2 != null)
                                            {
                                                var rtxid = await NFTHelpers.SendOrderedNFT(Address, AccountKey, (PaymentNFT)p, pn, res.Item2);
                                                Console.WriteLine($"NFT sent to the buyer {((PaymentNFT)p).Sender} with txid: {rtxid}");
                                                //await Task.Delay(500);
                                                //await ReLoadNFTs();
                                            }
                                            else
                                            {
                                                Console.WriteLine("You do not have spendable utxo for the fee.");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //await InvokeErrorDuringSendEvent($"Cannot send ordered NFT. Payment TxId: {p.Utxo}, NFT TxId: {pn.Utxo}, error message: {ex.Message}", "Cannot send ordered NFT");
                                            Console.WriteLine("Cannot send ordered NFT, payment txid: " + p.Utxo + " - " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Price in the NFT Payment {p.NFTOriginTxId} does not match with the found NFT {pn.Utxo}.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Cannot find the NFT for the received Payment. Marking the NFT Payment as already sold item.");
                                    try
                                    {
                                        PaymentNFT pn2send = p as PaymentNFT;
                                        pn2send.AlreadySoldItem = true;
                                        pn2send.OriginalPaymentTxId = p.Utxo;
                                        var res = await SendNFT(Address, pn2send, priceWrite: true, price: p.Price);
                                        if (res.Item1)
                                        {
                                            Console.WriteLine($" NFT Payment {p.Name} was marked as already sold. Please inform sender about it and return payment.");
                                            Console.WriteLine($" NFT Payment processed in txid: {res.Item2}");
                                            Console.WriteLine(res);
                                            await ReLoadNFTs();
                                            await RefreshAddressReceivedPayments();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Cannot mark the NFT Payment {p.Name} with txid: {p.Utxo} with index: {p.UtxoIndex} as already sold.");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Cannot process the NFT Payment {p.Name} with txid: {p.Utxo} with index: {p.UtxoIndex}. Waiting for enough confirmations.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot Check the payments. " + ex.Message);
            }
        }

        private void NFTHelpers_LoadingStateChangedHandler(object sender, string e)
        {
            var add = sender as string;
            if (!string.IsNullOrEmpty(add) && add == Address)
            {
                FirsLoadingStatus?.Invoke(this, "SubAccount - " + e);
            }
        }

        /// <summary>
        /// Reload actual token supplies based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadTokenSupply()
        {
            TokensSupplies = await NeblioTransactionHelpers.CheckTokensSupplies(Address, AddressInfoUtxos);
            if (TokensSupplies.TryGetValue(CoruzantNFTHelpers.CoruzantTokenId, out var ts))
                CoruzantSourceTokensBalance = ts.Amount;
            if (TokensSupplies.TryGetValue(NFTHelpers.TokenId, out var ti))
                SourceTokensBalance = ti.Amount;
        }

        /// <summary>
        /// This function will check if there is some spendable neblio of specific amount and returns list of the utxos for the transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(string, ICollection<Utxos>)> CheckSpendableNeblio(double amount)
        {
            try
            {
                var nutxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0002, amount);
                if (nutxos == null || nutxos.Count == 0)
                    return ($"You dont have Neblio on the address. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                    return ("OK", nutxos);
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Neblio. " + ex.Message, null);
            }
        }

        /// <summary>
        /// This function will check if there is some spendable tokens of specific Id and amount and returns list of the utxos for the transaction.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(string, ICollection<Utxos>)> CheckSpendableNeblioTokens(string id, int amount)
        {
            try
            {
                var tutxos = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, id, amount);
                if (tutxos == null || tutxos.Count == 0)
                    return ($"You dont have Tokens on the address. You need at least 5 for minting. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                    return ("OK", tutxos);
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Neblio Tokens. " + ex.Message, null);
            }
        }

        /// <summary>
        /// Send classic neblio payment
        /// </summary>
        /// <param name="receiver">Receiver Neblio Address</param>
        /// <param name="amount">Ammount in Neblio</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNeblioPayment(string receiver, double amount)
        {
            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(amount);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNeblioTransactionAPIAsync(dto, AccountKey, res.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Payment Sent");
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
        /// Split neblio coin to smaller coins
        /// </summary>
        /// <param name="receiver">Receiver Neblio Address</param>
        /// <param name="splittedAmount">Ammount of new splitted coin in Neblio</param>
        /// <param name="count">Count of new splited couns</param>
        /// <returns></returns>
        public async Task<(bool, string)> SplitNeblioCoin(List<string> receivers, int lots, double amount)
        {
            if (amount < 0.0005)
                return (false, "Minimal output splitted coin amount is 0.0005 NEBL.");
            if (lots < 2 || lots > NeblioTransactionHelpers.MaximumNeblioOutpus)
                return (false, "Minimal count of output splitted coin amount is 2. Maximum is 25.");

            var totalAmount = amount * lots;

            if (totalAmount >= TotalSpendableBalance)
                return (false, $"Cannot send transaction. Total Amount is bigger than spendable neblio on this address. Total Amount {totalAmount}.");

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
                return (false, $"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableNeblio(totalAmount + 0.0002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SplitNeblioCoinTransactionAPIAsync(Address, receivers, lots, amount, AccountKey, res.Item2, 20000);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Split Sent");
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
        /// Send classic token payment. It must match same requirements as minting. It cannot use 1 token inputs (NFTs).
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="metadata">Custom metadata</param>
        /// <param name="receiver">Receiver Neblio address</param>
        /// <param name="amount">Amount of the tokens</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNeblioTokenPayment(string tokenId, IDictionary<string, string> metadata, string receiver, int amount)
        {
            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, amount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable token inputs");
                return (false, tres.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = Convert.ToDouble(amount),
                SenderAddress = Address,
                ReceiverAddress = receiver,
                Metadata = metadata,
                Id = tokenId
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendTokenLotAsync(dto, AccountKey, res.Item2, tres.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Token Payment Sent");
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
        /// Send split token payment. It will create multiple outputs with lots of tokens.
        /// It must match same requirements as minting. It cannot use 1 token inputs (NFTs).
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="metadata">Custom metadata</param>
        /// <param name="receivers">List Receiver Neblio address</param>
        /// <param name="lots">Amount of the tokens</param>
        /// <param name="amount">Amount of the tokens</param>
        /// <returns></returns>
        public async Task<(bool, string)> SplitTokens(string tokenId, IDictionary<string, string> metadata, List<string> receivers, int lots, int amount)
        {
            if (lots > NeblioTransactionHelpers.MaximumTokensOutpus)
                return (false, $"Cannot create more than {NeblioTransactionHelpers.MaximumTokensOutpus} lots.");

            var totalAmount = amount * lots;
            if (!TokensSupplies.TryGetValue(tokenId, out var tsdto))
                return (false, $"Cannot send transaction. You do not have this kind of tokens. Token Id {tokenId}.");

            if (totalAmount >= tsdto.Amount)
                return (false, $"Cannot send transaction. Total Amount is bigger than available source. Total Amount {totalAmount}, Token Id {tokenId}.");

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
                return (false, $"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, totalAmount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable token inputs");
                return (false, tres.Item1);
            }

            metadata.Add("VENFT App", "https://about.ve-nft.com/");
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SplitNTP1TokensAsync(receivers, lots, amount, tokenId, metadata, AccountKey, res.Item2, tres.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Split Token Payment Sent");
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
        /// Mint new NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintNFT(INFT NFT, string receiver = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "SubAccount is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(NFT.TokenId, 2);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.MintNFT(Address, AccountKey, nft, res.Item2, tres.Item2, receiver);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio NFT Minted");
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


        private async Task<(bool, (ICollection<Utxos>, ICollection<Utxos>))> MultimintSourceCheck(string tokenId, int coppies)
        {
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null || res.Item2.Count == 0)
            {
                //await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, (null, null));
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, 3 + coppies);
            if (tres.Item2 == null || tres.Item2.Count == 0)
            {
                //await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs. You need 3 tokens as minimum input.");
                return (false, (null, null));
            }
            return (true, (res.Item2, tres.Item2));

        }

        /// <summary>
        /// Mint new multi NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="coppies">Number of coppies. 1 coppy means 2 final NFTs</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintMultiNFTLargeAmount(INFT NFT, int coppies, string receiver = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);
            try
            {
                if (!AccountKey.IsLoaded)
                {
                    await InvokeAccountLockedEvent();
                    return (false, "Account is locked.");
                }

                int cps = coppies;

                Console.WriteLine("Start of minting.");
                int lots = 0;
                int rest = 0;
                rest += cps % NeblioTransactionHelpers.MaximumTokensOutpus;
                lots += (int)((cps - rest) / NeblioTransactionHelpers.MaximumTokensOutpus);
                (bool, string) res = (false, string.Empty);
                string txres = string.Empty;
                NewMintingProcessInfo.Invoke(this, $"Minting of {lots} lots started...");

                if (lots > 1 || (lots == 1 && rest > 0))
                {
                    var done = false;
                    for (int i = 0; i < lots; i++)
                    {
                        Console.WriteLine("-----------------------------");
                        Console.WriteLine($"Minting lot {i} from {lots}:");
                        done = false;
                        await Task.Run(async () =>
                        {
                            while (!done)
                            {
                                var sres = await MultimintSourceCheck(NFT.TokenId, NeblioTransactionHelpers.MaximumTokensOutpus);
                                if (sres.Item1)
                                {
                                    try
                                    {
                                        txres = await NFTHelpers.MintMultiNFT(Address, NeblioTransactionHelpers.MaximumTokensOutpus - 1, AccountKey, nft, sres.Item2.Item1, sres.Item2.Item2, receiver);
                                        if (string.IsNullOrEmpty(txres))
                                        {
                                            Console.WriteLine("Waiting for spendable utxo...");
                                            await Task.Delay(5000);
                                        }
                                        else
                                        {
                                            done = true;
                                            NewMintingProcessInfo.Invoke(this, $"New Lot Minted: {txres}, Waiting for processing next {i+1} of {lots} lots.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        await Task.Delay(5000);
                                        done = false;
                                    }
                                }
                                else
                                {
                                    await Task.Delay(5000);
                                    done = false;
                                }
                            }
                        });
                    }
                    if (rest > 0)
                    {
                        Console.WriteLine($"Minting rest {rest} tickets:");
                        done = false;
                        await Task.Run(async () =>
                        {
                            while (!done)
                            {
                                var sres = await MultimintSourceCheck(NFT.TokenId, rest);
                                if (sres.Item1)
                                {
                                    txres = await NFTHelpers.MintMultiNFT(Address, rest, AccountKey, nft, sres.Item2.Item1, sres.Item2.Item2, receiver);
                                    if (string.IsNullOrEmpty(txres))
                                    {
                                        Console.WriteLine("Waiting for spendable utxo...");
                                        await Task.Delay(5000);
                                    }
                                    else
                                    {
                                        done = true;
                                        NewMintingProcessInfo.Invoke(this, $"Rest of {rest} NFTs Minted: {txres}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Waiting for spendable utxo...");
                                    await Task.Delay(5000);
                                }
                            }
                        });
                    }
                }
                else
                {
                    var sres = await MultimintSourceCheck(NFT.TokenId, NeblioTransactionHelpers.MaximumTokensOutpus);
                    if (sres.Item1)
                        txres = await NFTHelpers.MintMultiNFT(Address, cps, AccountKey, nft, sres.Item2.Item1, sres.Item2.Item2, receiver);
                    else
                    {
                        await InvokeErrorDuringSendEvent("Cannot Mint NFTs", "Not enough spendable source.");
                        return (false, "Not enough spendable source.");
                    }
                }

                if (txres != null)
                {
                    await InvokeSendPaymentSuccessEvent(txres, "Neblio NFT Sent");
                    return (true, txres);
                }
                else
                {
                    NewMintingProcessInfo.Invoke(this, $"All NFTs minted...");
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
        /// This function will destroy provided NFTs. It means it will connect them again to one output/lot of tokens.
        /// Now it is possible to destroy just same kind of tokens. The first provided NFT will define TokenId. Different tokensIds will be skipped.
        /// Maximum to destroy in one transaction is 10 of NFTs
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="nfts">List of NFTs</param>
        /// <returns></returns>
        public async Task<(bool, string)> DestroyNFTs(ICollection<INFT> nfts, string receiver = "")
        {
            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "SubAccount is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                // send tx
                var rtxid = await NFTHelpers.DestroyNFTs(Address, AccountKey, nfts, res.Item2, receiver);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFTs Destroyed.");
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
        /// Change Post NFT. It requeires previous loadedPost NFT as input.
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> ChangeNFT(INFT NFT)
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot change Post NFT without provided Utxo TxId.", "Cannot change the Post NFT");
                return (false, "Cannot change NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.ChangeNFT(Address, AccountKey, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Post Changed");
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
        /// Send input NFT to new owner, or use it just for price write and resend it to yourself with new metadata about the price.
        /// </summary>
        /// <param name="receiver">If the pricewrite is set, this is filled with own address</param>
        /// <param name="NFT"></param>
        /// <param name="priceWrite">Set this if you need to just write the price to the NFT</param>
        /// <param name="price">Price must be bigger than 0.0002 NEBL</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNFT(string receiver, INFT NFT, bool priceWrite, double price, bool withDogePrice = false, double dogeprice = 0.0)
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "SubAccount is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot snd NFT without provided Utxo TxId.", "Cannot send NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            if (string.IsNullOrEmpty(receiver) || priceWrite)
                receiver = Address;
            if(string.IsNullOrEmpty(receiver))
                receiver = Address;

            try
            {
                var rtxid = await NFTHelpers.SendNFT(Address, receiver, AccountKey, nft, priceWrite, res.Item2, price, withDogePrice:withDogePrice, dogeprice:dogeprice);

                if (rtxid != null)
                {
                    if (!priceWrite)
                        await InvokeSendPaymentSuccessEvent(rtxid, "NFT Sent");
                    else
                        await InvokeSendPaymentSuccessEvent(rtxid, "Price written to NFT");
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
        /// Add comment to Coruzant Post NFT or add comment and send it to new owner
        /// </summary>
        /// <param name="receiver">Fill when you want to send to different address</param>
        /// <param name="NFT"></param>
        /// <param name="commentWrite">Set this if you need to just write the comment to the NFT</param>
        /// <param name="comment">Add your comment</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendCoruzantNFT(string receiver, INFT NFT, bool commentWrite, string comment = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "SubAccount is not loaded.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot snd NFT without provided Utxo TxId.", "Cannot send NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            if (string.IsNullOrEmpty(receiver))
                receiver = string.Empty;

            try
            {
                var rtxid = await CoruzantNFTHelpers.ChangeCoruzantPostNFT(Address, AccountKey, nft, res.Item2, receiver);

                if (rtxid != null)
                {
                    if (!commentWrite)
                        await InvokeSendPaymentSuccessEvent(rtxid, "NFT Sent");
                    else
                        await InvokeSendPaymentSuccessEvent(rtxid, "Comment written to NFT");
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
        /// Write Used flag into NFT Ticket
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> UseNFTTicket(INFT NFT)
        {
            if (NFT.Type != NFTTypes.Ticket)
                throw new Exception("This is not NFT ticket.");

            var nft = await NFTFactory.CloneNFT(NFT);

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "SubAccount is not loaded.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot snd NFT without provided Utxo TxId.", "Cannot send NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.UseNFTTicket(Address, AccountKey, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Ticket used.");
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
        /// This function will return the NFT Payment to the original sender.
        /// The receiver is taken from the Minting transaction of the PaymentNFT
        /// </summary>
        /// <param name="NFT">NFT Payment to return</param>
        /// <returns></returns>
        public async Task<(bool, string)> ReturnNFTPayment(string receiver, PaymentNFT NFT)
        {
            //var nft = await NFTFactory.CloneNFT(NFT);

            if (!AccountKey.IsLoaded)
            {
                await InvokeAccountLockedEvent();
                return (false, "SubAccount is not loaded.");
            }

            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.ReturnNFTPayment(Address, AccountKey, NFT, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Payment returned to the original sender");
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
        /// Obtain verify code of some transaction. This will combine txid and UTC time (rounded to minutes) and sign with the private key.
        /// It will create unique code, which can be verified and it is valid just one minute.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<OwnershipVerificationCodeDto> GetNFTVerifyCode(string txid)
        {
            var res = await OwnershipVerifier.GetCodeInDto(txid, Secret);
            if (res != null)
                return res;
            else
                return new OwnershipVerificationCodeDto();
        }
        /// <summary>
        /// Verification function for the NFT ownership code generated by GetNFTVerifyCode function.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<(OwnershipVerificationCodeDto, byte[])> GetNFTVerifyQRCode(string txid)
        {
            var res = await OwnershipVerifier.GetQRCode(txid, Secret);
            if (res.Item1)
                return (res.Item2.Item1, res.Item2.Item2);
            else
                return (new OwnershipVerificationCodeDto(), new byte[0]);
        }

    }
}
