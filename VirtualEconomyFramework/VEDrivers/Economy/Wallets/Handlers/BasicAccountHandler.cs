using log4net;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public class BasicAccountHandler : CommonAccountHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override async Task<string> UpdateAccount(string accountAddress, Guid walletId, AccountTypes type, string name, IDbConnectorService dbservice, bool justInDb = true, string password = "")
        {
            //IDbConnectorService dbservice = new DbConnectorService();

            if (EconomyMainContext.Wallets.TryGetValue(walletId.ToString(), out var wallet))
            {
                if (wallet.Accounts.TryGetValue(accountAddress, out var accnt))
                {
                    accnt.Name = name;

                    if (EconomyMainContext.WorkWithDb)
                    {
                        if (!dbservice.SaveAccount(accnt))
                            return "Cannot save account to the Db!";
                    }

                    return "OK";
                }
                else
                {
                    Console.WriteLine("Account not found in actual wallet Accounts. New account will be created.");
                    // cannot be the same account name in one wallet
                    if (wallet.Accounts.Values.FirstOrDefault(a => a.Name == name) == null)
                    {
                        // QT RPC support requires Db too. It creates new addresses in QT and it must be saved somewhere
                        // todo: saving to file or something like that when Db is not on
                        if (EconomyMainContext.WorkWithQTRPC && EconomyMainContext.WorkWithDb)
                        {
                            if (EconomyMainContext.QTRPCClient.IsConnected)
                            {
                                // creating wallet in desktop QT Wallet

                                var accresp = new QTWalletResponseDto();
                                var keyresp = new QTWalletResponseDto();
                                var privateKey = string.Empty;

                                if (!justInDb)
                                {
                                    var acc = await EconomyMainContext.QTRPCClient.RPCLocalCommandSplitedAsync("getnewaddress", new string[] { name });
                                    accresp = JsonConvert.DeserializeObject<QTWalletResponseDto>(acc);

                                    var kr = await EconomyMainContext.QTRPCClient.RPCLocalCommandSplitedAsync("dumpprivatekey", new string[] { accresp.result });
                                    keyresp = JsonConvert.DeserializeObject<QTWalletResponseDto>(kr);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(accountAddress))
                                        accresp = new QTWalletResponseDto() { result = accountAddress };
                                    else
                                        accresp = new QTWalletResponseDto() { result = "No Address Filled" };
                                }

                                // if success add to list of Accounts of specified wallet
                                if (accresp != null)
                                {
                                    var account = AccountFactory.GetAccount(Guid.Empty, type, wallet.Owner, walletId, name, accresp.result, 0);

                                    // check if some files with last state already exists
                                    var ltxParsed = GetLastAccountProcessedTxs(account.Address);
                                    if (ltxParsed != null)
                                    {
                                        account.LastConfirmedTxId = ltxParsed.LastConfirmedTxId;
                                        account.LastProcessedTxId = ltxParsed.LastProcessedTxId;
                                    }

                                    account.WalletName = wallet.Name;
                                    account.StartRefreshingData(EconomyMainContext.WalletRefreshInterval);
                                    wallet.Accounts.TryAdd(account.Address, account);
                                    wallet.RegisterAccountEvents(account.Address);

                                    if (!string.IsNullOrEmpty(privateKey))
                                    {
                                        // load and save address private key if was dumped correctly
                                        LoadAccountKey(walletId.ToString(), account.Address, privateKey, dbservice, password, account.Name + "-key");
                                    }

                                    if (EconomyMainContext.WorkWithDb && account != null)
                                    {
                                        if (!dbservice.SaveAccount(account))
                                            return "Cannot save new account to the Db!";
                                    }

                                    return "OK";
                                }
                                else
                                {
                                    log.Error("Cannot create account - cannot get correct response from QTWalletRPC!");
                                    return "Cannot create account - cannot get correct response from QTWalletRPC!";
                                }
                            }
                            else
                            {
                                log.Error("Cannot create account - RPC is not connected, probably not configured!");
                                return "Cannot create account - RPC is not connected, probably not configured!";
                            }
                        }
                        else if (!EconomyMainContext.WorkWithQTRPC && justInDb)
                        {
                            // if not work with RPC you must fill address
                            if (!string.IsNullOrEmpty(accountAddress))
                            {
                                var account = AccountFactory.GetAccount(Guid.Empty, type, wallet.Owner, walletId, name, accountAddress, 0);

                                // check if some files with last state already exists
                                var ltxParsed = GetLastAccountProcessedTxs(account.Address);
                                if (ltxParsed != null)
                                {
                                    account.LastConfirmedTxId = ltxParsed.LastConfirmedTxId;
                                    account.LastProcessedTxId = ltxParsed.LastProcessedTxId;
                                }

                                account.WalletName = wallet.Name;
                                account.StartRefreshingData(EconomyMainContext.WalletRefreshInterval);
                                wallet.Accounts.TryAdd(account.Address, account);
                                wallet.RegisterAccountEvents(account.Address);
                                return "OK";
                            }
                            else
                            {
                                log.Error("Cannot create account - RPC is disabled and accountAddress is empty!");
                                return "Cannot create account - RPC is disabled and accountAddress is empty!";
                            }
                        }
                        else if (!EconomyMainContext.WorkWithQTRPC && EconomyMainContext.WorkWithDb && !justInDb)
                        {
                            try
                            {
                                // create new address with NBitcoin library
                                var network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
                                Key privateKey = new Key(); // generate a random private key
                                PubKey publicKey = privateKey.PubKey;
                                BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(network);
                                var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, network);


                                var account = AccountFactory.GetAccount(Guid.Empty, type, wallet.Owner, walletId, name, address.ToString(), 0);

                                // check if some files with last state already exists
                                var ltxParsed = GetLastAccountProcessedTxs(account.Address);
                                if (ltxParsed != null)
                                {
                                    account.LastConfirmedTxId = ltxParsed.LastConfirmedTxId;
                                    account.LastProcessedTxId = ltxParsed.LastProcessedTxId;
                                }

                                account.WalletName = wallet.Name;
                                account.StartRefreshingData(EconomyMainContext.WalletRefreshInterval);
                                wallet.Accounts.TryAdd(account.Address, account);
                                wallet.RegisterAccountEvents(account.Address);

                                // load and save address private key
                                LoadAccountKey(walletId.ToString(), address.ToString(), privateKeyFromNetwork.ToString(), dbservice, password, account.Name + "-key");

                                return "OK";
                            }
                            catch(Exception ex)
                            {
                                log.Error("Cannot create account - NBitcoin cannot create new address!");
                                return "Cannot create account - NBitcoin cannot create new address!";
                            }
                        }
                        else
                        {
                            log.Error("Cannot create account - RPC is disabled, accountAddress is empty!");
                            return "Cannot create account - RPC is disabled and accountAddress is empty!";
                        }
                    }
                    else
                    {

                        log.Error("Cannot create account - Name already exists!");
                        return "Cannot create account - Name already exists!";
                    }
                }
            }
            else
            {
                log.Error("Cannot create account - wallet not found");
                return "Cannot create account - wallet not found";
            }
        }
        public override IDictionary<string, IToken> FindTokenByMetadata(string account, string key, string value = "")
        {
            var result = new Dictionary<string, IToken>();
            var txss = new List<ITransaction>();
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(account, out var acc))
                {
                    var txs = acc.Transactions.ToList();

                    foreach (var t in txs)
                    {
                        var tx = t.Value;
                        if (tx.Loaded)
                        {
                            if (tx.VinTokens != null)
                            {
                                var found = false;
                                /*
                                foreach (var tok in tx.VinTokens)
                                {
                                    if (tok.Metadata.TryGetValue(key, out var v) && !found)
                                    {
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            if (v == value)
                                            {
                                                result.Add(t.Key, tok);
                                                found = true;
                                            }
                                        }
                                        else
                                        {
                                            result.Add(t.Key, tok);
                                            found = true;
                                        }
                                    }
                                }
                                */
                                if (!found)
                                {
                                    foreach (var tok in tx.VoutTokens)
                                    {
                                        if (tok.Metadata.TryGetValue(key, out var v))
                                        {
                                            if (!string.IsNullOrEmpty(value))
                                            {
                                                if (v == value)
                                                {
                                                    result.Add(t.Key, tok);
                                                    txss.Add(t.Value);
                                                }
                                            }
                                            else
                                            {
                                                result.Add(t.Key, tok);
                                                txss.Add(t.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ;
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot find token by metadata", ex);
            }

            return result;
        }

        public override IDictionary<string, IToken> FindAllTokens(string account)
        {
            var result = new Dictionary<string, IToken>();

            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(account, out var acc))
                {
                    var txs = acc.Transactions.ToList();
                    foreach (var t in txs)
                    {
                        var tx = t.Value;
                        if (tx.Loaded)
                        {
                            if (tx.VoutTokens != null)
                            {
                                var tok = tx.VoutTokens.FirstOrDefault();
                                if (tok != null)
                                {
                                    tok.Direction = tx.Direction;
                                    tok.TxId = tx.TxId;
                                    tok.TimeStamp = tx.TimeStamp;
                                    result.Add(tx.TxId, tok);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot find token by metadata", ex);
            }

            return result;
        }

        public override LastTxSaveDto GetLastAccountProcessedTxs(string address)
        {
            try
            {
                var ltx = FileHelpers.ReadTextFromFile(Path.Join(EconomyMainContext.CurrentLocation, $"Accounts/{address}.txt"));
                var ltxParsed = JsonConvert.DeserializeObject<LastTxSaveDto>(ltx);
                return ltxParsed;
            }
            catch (Exception ex)
            {
                log.Error("Wrong format of Last Tx saved Account data!", ex);
            }

            return null;
        }

        public override string LoadAccountKey(string wallet, string address, string key, IDbConnectorService dbservice, string password = "", string name = "", bool storeInDb = true)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        account.AccountKey = new Security.EncryptionKey(key, password);
                        account.AccountKey.RelatedItemId = account.Id;
                        account.AccountKey.Type = Security.EncryptionKeyType.AccountKey;

                        if (!string.IsNullOrEmpty(password))
                            account.AccountKey.PasswordHash = Security.SecurityUtil.HashPassword(password);

                        account.AccountKey.Name = name;

                        if (EconomyMainContext.WorkWithDb)
                        {
                            dbservice.SaveKey(account.AccountKey);
                        }

                        return "OK";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot load key to the account!", ex);
            }

            return "Load Account Key - ERROR";
        }


        public override string UnlockAccount(string wallet, string address, string password)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {

                        if (!string.IsNullOrEmpty(password))
                        {
                            if (Security.SecurityUtil.VerifyPassword(password, account.AccountKey.PasswordHash))
                            {
                                account.AccountKey.LoadPassword(password);
                            }
                        }
                        return "OK";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot unlock the account!", ex);
            }

            return "Unlock Account - ERROR";
        }

        public override string LockAccount(string wallet, string address)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        account.AccountKey.Lock();
                        return "OK";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot lock the account!", ex);
            }

            return "Lock Account - ERROR";
        }
    }
}
