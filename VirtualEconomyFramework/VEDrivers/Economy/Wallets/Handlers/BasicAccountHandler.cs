﻿using log4net;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Bookmarks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Security;

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
                                        LoadAccountKey(walletId.ToString(), account.Address, privateKey, dbservice, account.Address, password, account.Name + "-key", false, true, false, EncryptionKeyType.AccountKey);
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

                                if (EconomyMainContext.WorkWithDb && account != null)
                                {
                                    if (!dbservice.SaveAccount(account))
                                        return "Cannot save new account to the Db!";
                                }
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
                                LoadAccountKey(walletId.ToString(), address.ToString(), privateKeyFromNetwork.ToString(), dbservice, address.ToString(), password, account.Name + "-key", false, true, false, EncryptionKeyType.AccountKey);

                                if (EconomyMainContext.WorkWithDb && account != null)
                                {
                                    if (!dbservice.SaveAccount(account))
                                        return "Cannot save new account to the Db!";
                                }

                                return account.Address;
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
                            if (tx.VoutTokens != null)
                            {
                                foreach (var tok in tx.VoutTokens)
                                {
                                    if (tok.Metadata.TryGetValue(key, out var v))
                                    {
                                        if (tx.From[0] == account)
                                            tok.Direction = TransactionDirection.Outgoing;
                                        else
                                            tok.Direction = TransactionDirection.Incoming;

                                        tok.From = tx.From[0];
                                        tok.To = tx.To[0];

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

        public override string LoadAccountKey(string wallet, string address, string key, IDbConnectorService dbservice, string pubkey = "", string password = "", string name = "", bool storeInDb = true, bool isItMainAccountKey = false, bool alreadyEncrypted = false, EncryptionKeyType type = EncryptionKeyType.BasicSecurity)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        if (isItMainAccountKey)
                        {
                            // todo load already encrypted key
                            account.AccountKey = new Security.EncryptionKey(key, password);
                            account.AccountKey.RelatedItemId = account.Id;
                            account.AccountKey.Type = Security.EncryptionKeyType.AccountKey;
                            account.AccountKeyId = account.AccountKey.Id;
                            account.AccountKey.PublicKey = account.Address;

                            if (!string.IsNullOrEmpty(password))
                                account.AccountKey.PasswordHash = Security.SecurityUtil.HashPassword(password);

                            account.AccountKey.Name = name;

                            if (EconomyMainContext.WorkWithDb)
                            {
                                dbservice.SaveKey(account.AccountKey);
                            }

                            account.AccountKeyId = account.AccountKey.Id;

                            if (EconomyMainContext.WorkWithDb)
                            {
                                dbservice.SaveAccount(account);
                            }

                            return "OK";
                        }
                        else
                        {
                            
                            EncryptionKey k = null;
                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(pubkey))
                            {
                                // validate the key pair if it is correct combination of RSA keys
                                try
                                {
                                    if (type != EncryptionKeyType.AccountKey)
                                    {
                                        if (alreadyEncrypted)
                                        {
                                            var kd = SymetricProvider.DecryptString(password, key);
                                            if (kd != null)
                                            {
                                                key = kd;
                                            }
                                        }

                                        var m = AsymmetricProvider.EncryptString("test", pubkey);
                                        var r = AsymmetricProvider.DecryptString(m, key);
                                        if (r != "test")
                                        {
                                            throw new Exception("Key pair is not valid RSA key pair!");
                                        }
                                    }

                                    k = new EncryptionKey(key, password);
                                }
                                catch(Exception ex)
                                {
                                    throw new Exception("Key pair is not valid RSA key pair!");
                                }
                            }
                            else if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(pubkey))
                            {
                                if (alreadyEncrypted)
                                {
                                    var kd = SymetricProvider.DecryptString(password, key);
                                    if (kd != null)
                                    {
                                        k = new Security.EncryptionKey(kd, password);
                                    }
                                }
                                else
                                {
                                    k = new Security.EncryptionKey(key, password);
                                    k.LoadNewKey(key, fromDb: true);
                                }
                            }
                            else if (!string.IsNullOrEmpty(pubkey) && string.IsNullOrEmpty(key))
                            {
                                // create enc object
                                k = new Security.EncryptionKey("passtest", password, true); // this can be used for testing the password
                                k.PublicKey = pubkey;
                            }
                            else if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(pubkey))
                            {
                                // obtain new RSA key pair
                                var keypair = Security.AsymmetricProvider.GenerateNewKeyPair();
                                // create enc object
                                k = new EncryptionKey(keypair.PrivateKey, password);
                                k.PublicKey = keypair.PublicKey;
                            }
                            else
                            {
                                throw new Exception("Strange input!");
                            }

                            k.RelatedItemId = account.Id;
                            k.Type = Security.EncryptionKeyType.BasicSecurity;
                                                                                    
                            if (!string.IsNullOrEmpty(password))
                                k.PasswordHash = Security.SecurityUtil.HashPassword(password);

                            k.Name = name;
                            account.AccountKeys.Add(k);

                            if (EconomyMainContext.WorkWithDb)
                            {
                                dbservice.SaveKey(k);
                            }

                            if (isItMainAccountKey)
                            {
                                account.AccountKeyId = account.AccountKey.Id;

                                if (EconomyMainContext.WorkWithDb)
                                {
                                    dbservice.SaveAccount(account);
                                }
                            }

                            return "OK";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot load key to the account!", ex);
            }

            return "Load Account Key - ERROR";
        }

        public override string ChangeKeyName(string wallet, string address, string keyId, string newName, IDbConnectorService dbservice)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        var key = account.AccountKeys.FirstOrDefault(k => k.Id.ToString() == keyId);
                        if (key != null)
                        {
                            key.Name = newName;

                            if (EconomyMainContext.WorkWithDb)
                            {
                                dbservice.SaveKey(key);
                            }

                            return "OK";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot change key name!", ex);
            }

            return "Change Key Name - ERROR";
        }

        public override EncryptionKey DeleteKey(string walletId, string address, string keyId, IDbConnectorService dbservice)
        {
            EncryptionKey respkey = null;
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(walletId, out var wallet))
                {
                    if (wallet.Accounts.TryGetValue(address, out var account))
                    {
                        var key = account.AccountKeys.FirstOrDefault(k => k.Id.ToString() == keyId);
                        if (key != null)
                        {
                            respkey = key;
                            account.AccountKeys.Remove(key);
                        }
                        if (account.AccountKey != null)
                        {
                            if (account.AccountKey.Id.ToString() == keyId)
                            {
                                account.AccountKey = null;
                            }
                        }
                    }
                }

                if (EconomyMainContext.WorkWithDb)
                {
                    var k = dbservice.GetKey(new Guid(keyId));
                    if (k != null)
                    {
                        respkey = k;
                        dbservice.DeleteKey(keyId);
                    }
                }

                return respkey;
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete the key!", ex);
                throw new Exception($"Cannot delete the key!");
            }
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

        public override string UpdateBookmark(string wallet, string address, BookmarkTypes type, string bookmarkId, string name, string bookmarkAddress, IDbConnectorService dbservice)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        IBookmark bkm = null;
                        bkm = account.Bookmarks.FirstOrDefault(b => b.Id.ToString() == bookmarkId);
                        if (bkm != null)
                        {
                            if (!string.IsNullOrEmpty(name))
                                bkm.Name = name;

                            if (!string.IsNullOrEmpty(bookmarkAddress))
                                bkm.Address = bookmarkAddress;

                            if (type != bkm.Type)
                                bkm.Type = type;

                        }
                        else
                        {
                            bkm = BookmarkFactory.GetBookmark(type, account.Id, name, bookmarkAddress);

                            bkm.Id = Guid.NewGuid();
                            account.Bookmarks.Add(bkm);
                        }

                        if (EconomyMainContext.WorkWithDb)
                        {
                            dbservice.SaveBookmark(bkm);
                        }

                        return "OK";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot add bookmark!", ex);
            }

            return "Add bookmark - ERROR";
        }

        public override string DeleteBookmark(string wallet, string address, string bookmarkId, IDbConnectorService dbservice)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        var bkm = account.Bookmarks.FirstOrDefault(b => b.Id.ToString() == bookmarkId);
                        if (bkm != null)
                        {
                            account.Bookmarks.Remove(bkm);
                            if (EconomyMainContext.WorkWithDb)
                            {
                                dbservice.DeleteBookmark(bkm.Id.ToString());
                            }

                            return "OK";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete bookmark!", ex);
            }

            return "Delete Bookmark - ERROR";
        }

        public override string RemoveBookmark(string wallet, string address, string bookmarkId, IDbConnectorService dbservice)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(wallet, out var w))
                {
                    if (w.Accounts.TryGetValue(address, out var account))
                    {
                        var bkm = account.Bookmarks.FirstOrDefault(b => b.Id.ToString() == bookmarkId);
                        if (bkm != null)
                        {
                            account.Bookmarks.Remove(bkm);
                            if (EconomyMainContext.WorkWithDb)
                            {
                                dbservice.RemoveBookmark(bkm.Id.ToString());
                            }

                            return "OK";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove bookmark!", ex);
            }

            return "Remove Bookmark - ERROR";
        }

        public override List<IBookmark> GetAccountBookmarks(string address)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(address, out var account))
                {
                    return account.Bookmarks;
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get bookmarks!", ex);
            }

            return null;
        }
    }
}
