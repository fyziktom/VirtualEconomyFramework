using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using VEDriversLite.NeblioAPI;
using Newtonsoft.Json;
using VEconomy.Common;
using VEDrivers.Bookmarks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Receipt;
using VEDrivers.Economy.Shops;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets;
using VEDrivers.Messages;
using VEDrivers.Messages.DTO;
using VEDrivers.Nodes;
using VEDrivers.Nodes.Dto;
using VEDrivers.Security;

namespace VEconomy.Controllers
{
    [Route("api")]
    [ApiController]
    public class MainController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDbConnectorService dbService;
        private readonly DbEconomyContext _context;
        public MainController(DbEconomyContext context)
        {
            _context = context;
            dbService = new DbConnectorService(_context);
        }

        /// <summary>
        /// Get Server parameters. Now it returns just MQTT parameters. Refer to MQTTConfig.cs class
        /// </summary>
        /// <returns>Object with MQTTConfig class, item is called MQTT</returns>
        [HttpGet]
        [Route("GetServerParams")]
        public object GetServerParams()
        {
            return new
            {
                EconomyMainContext.MQTT
            };
        }

        /// <summary>
        /// Get status of RPC configuration. If the RPC is allowed returns true. Setting of RPC is in appsetting.json
        /// </summary>
        /// <returns>true if RPC available</returns>
        [HttpGet]
        [Route("IsRPCAvailable")]
        public bool IsRPCAvailable()
        {
            return EconomyMainContext.WorkWithQTRPC;
        }

        /// <summary>
        /// Get status of Db configuration. If the Db is allowed returns true. Setting of Db is in appsetting.json
        /// </summary>
        /// <returns>true if Db available</returns>
        [HttpGet]
        [Route("IsDbAvailable")]
        public bool IsDbAvailable()
        {
            return EconomyMainContext.WorkWithDb;
        }

        #region Wallets

        /// <summary>
        /// Data carrier for UpdateWallet API command
        /// </summary>
        public class UpdateWalletData
        {
            /// <summary>
            /// Not implemented now
            /// </summary>
            public string owner { get; set; }
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// readable name of wallet
            /// </summary>
            public string walletName { get; set; }
            /// <summary>
            /// This is for connection to RPC, not implemented yet for each wallet, but prepared
            /// </summary>
            public string walletBaseHost { get; set; }
            /// <summary>
            /// This is for connection to RPC, not implemented yet for each wallet, but prepared
            /// </summary>
            public int walletPort { get; set; }
            /// <summary>
            /// Wallet type. Now supported just Neblio
            /// </summary>
            public WalletTypes walletType { get; set; }
        }

        /// <summary>
        /// Create or update wallet. If wallet is not found it is created. 
        /// </summary>
        /// <param name="wallData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateWallet")]
        //[Authorize(Rights.Administration | Rights.RoleTrader)]
        public async Task<string> UpdateWallet([FromBody] UpdateWalletData wallData)
        {
            try
            {
                Guid id;
                Guid ownid;

                if (string.IsNullOrEmpty(wallData.owner))
                {
                    ownid = Guid.NewGuid();
                }
                else
                {
                    try
                    {
                        ownid = new Guid(wallData.owner);
                    }
                    catch (Exception ex)
                    {
                        log.Info(ex.ToString());
                        throw new HttpResponseException((HttpStatusCode)501, "Cannot create Guid for owner, wrong format!");
                    }
                }

                if (string.IsNullOrEmpty(wallData.walletId))
                {
                    id = Guid.NewGuid();
                }
                else
                {
                    try
                    {
                        id = new Guid(wallData.walletId);
                    }
                    catch (Exception ex)
                    {
                        log.Info(ex.ToString());
                        throw new HttpResponseException((HttpStatusCode)501, "Cannot create Guid for wallet, wrong format!");
                    }
                }

                return await MainDataContext.WalletHandler.UpdateWallet(id, ownid, wallData.walletName, wallData.walletType, wallData.walletBaseHost, wallData.walletPort, dbService);
            }
            catch (Exception ex)
            {
                log.Error("Cannot create wallet", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot create wallet!");
            }
        }

        /// <summary>
        /// Get all walet types. Usefull for creating dropbox in UI
        /// </summary>
        /// <returns>List of all wallet types</returns>
        [HttpGet]
        [Route("GetWalletTypes")]
        public async Task<List<string>> GetWalletTypes()
        {
            try
            {
                return Enum.GetValues(typeof(WalletTypes)).Cast<WalletTypes>().Select(t => t.ToString()).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Wallet Types", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Wallet types!");
            }
        }

        /// <summary>
        /// Returns info about QT wallet. Implemented just in Neblio wallet. Not recommended to use now
        /// </summary>
        /// <param name="walletName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetWalletInfo")]
        //[Authorize(Rights.Administration)]
        public async Task<object> GetWalletInfo(string walletName)
        {
            try
            {
                if (!EconomyMainContext.Wallets.TryGetValue(walletName, out var wallet))
                {
                    return new { info = null as string, ReadingError = "READER_NOT_FOUND" };
                }

                var res = await wallet.GetDetails();

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                log.Error("Cannot get Neblio Bitcoin price", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get wallet info!");
            }
        }

        /// <summary>
        /// Returns all stored wallets in Db. Works just when Db is available
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetWalletsFromDb")]
        //[Authorize(Rights.Administration)]
        public async Task<List<IWallet>> GetWalletsFromDb() // todo carrier dto
        {
            try
            {
                if (EconomyMainContext.WorkWithDb)
                {
                    var walls = dbService.GetWallets();
                    return walls;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot get Wallets from Db. Db is not setted up!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Wallets from Db", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Wallets from Db!");
            }
        }

        /// <summary>
        /// Remove wallet API command carrier
        /// </summary>
        public class RemoveWalletData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// if this is true all accounts of this wallet is deleted too
            /// </summary>
            public bool withAccounts { get; set; }
        }

        /// <summary>
        /// Delete wallet in Db. Delete just set the "Deleted" flag in the object. The record is kept in the Db, but it is not considered in queries
        /// </summary>
        /// <param name="walletData"></param>
        /// <returns>OK if wallet was deleted</returns>
        [HttpPut]
        [Route("DeleteWallet")]
        //[Authorize(Rights.Administration)]
        public async Task<string> DeleteWallet([FromBody] RemoveWalletData walletData)
        {
            var resp = "ERROR";

            try
            {
                // todo - remove all address from real QT wallet
                if (EconomyMainContext.Wallets.Remove(walletData.walletId, out var wallet))
                {
                    if (dbService.DeleteWallet(walletData.walletId, walletData.withAccounts))
                    {
                        resp = "OK";
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Wallet {walletData.walletId} from Db, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete wallet!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Wallet {walletData.walletId} from Db!");
            }

            return resp;
        }

        /// <summary>
        /// Remove wallet in Db. This will remove record from Db. 
        /// </summary>
        /// <param name="walletData"></param>
        /// <returns>OK if wallet was removed</returns>
        [HttpPut]
        [Route("RemoveWallet")]
        //[Authorize(Rights.Administration)]
        public async Task<string> RemoveWallet([FromBody] RemoveWalletData walletData)
        {
            var resp = "ERROR";

            try
            {
                // todo - remove all address from real QT wallet
                if (EconomyMainContext.Wallets.Remove(walletData.walletId, out var wallet))
                {
                    if (dbService.RemoveWallet(walletData.walletId, walletData.withAccounts))
                    {
                        resp = "OK";
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Wallet {walletData.walletId} from Db, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove wallet!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Wallet {walletData.walletId} from Db!");
            }

            return resp;
        }

        #endregion

        #region Accounts

        /// <summary>
        /// Returns list with all account types. Usefull for dropbox in UI
        /// </summary>
        /// <returns>List of account types</returns>
        [HttpGet]
        [Route("GetAccountTypes")]
        public async Task<List<string>> GetAccountTypes()
        {
            try
            {
                return Enum.GetValues(typeof(AccountTypes)).Cast<AccountTypes>().Select(t => t.ToString()).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Types", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Account types!");
            }
        }

        /// <summary>
        /// Returns dictionary of all token transactions of some address.
        /// Implemented just for the Neblio now
        /// Key is the txid of this token transaction
        /// </summary>
        /// <param name="address">Neblio address</param>
        /// <returns>Dictionary with key - txid, value - IToken object</returns>
        [HttpGet]
        [Route("GetAccountTokens/{address}")]
        public async Task<IDictionary<string, IToken>> GetAccountTokens(string address)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(address, out var account))
                {
                    var tokens = MainDataContext.AccountHandler.FindAllTokens(address);

                    return tokens;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot get Account Tokens, Account address not found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Types", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Account types!");
            }
        }

        /// <summary>
        /// Data carrier for UpdateAccount API command
        /// </summary>
        public class UpdateAccountData
        {
            /// <summary>
            /// Guid format
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Readable name of the account
            /// </summary>
            public string accountName { get; set; }
            /// <summary>
            /// if you want to encrypt the password this must be filled, in other case provide empty string ""
            /// </summary>
            public string password { get; set; } = string.Empty;
            /// <summary>
            /// if you dont want to create new account, but just store existing account in the database set this true. 
            /// Default is true to prevent creating new accounts accidentaly
            /// if you create account just to db you have to import the key separately
            /// </summary>
            public bool saveJustToDb { get; set; } = true;
            /// <summary>
            /// account type, now available just Neblio
            /// </summary>
            public AccountTypes accountType { get; set; }
        }


        /// <summary>
        /// Create or Update account
        /// if account does not exists the new will be created. In this case leave empty the address field in dto. 
        /// you can store existing address to Db or created new one. If you have connection with QT wallet, account is created in QT wallet
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateAccount")]
        //[Authorize(Rights.Administration)]
        public async Task<string> UpdateAccount([FromBody] UpdateAccountData accountData)
        {
            try
            {
                Guid walletId;

                if (string.IsNullOrEmpty(accountData.walletId))
                {
                    walletId = Guid.NewGuid();
                }
                else
                {
                    try
                    {
                        walletId = new Guid(accountData.walletId);
                    }
                    catch (Exception ex)
                    {
                        log.Info(ex.ToString());
                        throw new HttpResponseException((HttpStatusCode)501, "Cannot create Guid for account, wrong format!");
                    }
                }

                if (string.IsNullOrEmpty(accountData.accountAddress))
                    accountData.accountAddress = "";

                return await MainDataContext.AccountHandler.UpdateAccount(
                    accountData.accountAddress,
                    walletId,
                    accountData.accountType,
                    accountData.accountName,
                    dbService,
                    accountData.saveJustToDb,
                    accountData.password);
            }
            catch (Exception ex)
            {
                log.Error("Cannot create account", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot create account!");
            }
        }

        /// <summary>
        /// Returns
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAccountsFromDb")]
        //[Authorize(Rights.Administration)]
        public async Task<List<IAccount>> GetAccountsFromDb() // todo carrier dto
        {
            try
            {
                if (EconomyMainContext.WorkWithDb)
                {
                    var accs = dbService.GetAccounts();
                    return accs;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot get Accounts from Db!. Db is not setted up");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Accounts from Db", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Accounts from Db!");
            }
        }

        /// <summary>
        /// Data carrier for Get Account transaction API command
        /// </summary>
        public class GetAccountTransactionData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Maximum items received back - not implemented now
            /// </summary>
            public string txId { get; set; }
        }
        /// <summary>
        /// Get all account transaction by TxId,
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns>ITransaction</returns>
        [HttpPut]
        [Route("GetAccountTransaction")]
        //[Authorize(Rights.Administration)]
        public async Task<ITransaction> GetAccountTransaction([FromBody] GetAccountTransactionData accountData)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(accountData.walletId, out var wallet))
                {
                    // todo - remove address from real QT wallet
                    if (wallet.Accounts.TryGetValue(accountData.accountAddress, out var account))
                    {
                        if (account.Transactions.TryGetValue(accountData.txId, out var tx))
                            return tx;
                        else
                            return null;
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {accountData.accountAddress} transaction, Account Not Found!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {accountData.accountAddress} transaction, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Transactions!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {accountData.accountAddress} Transactions!");
            }

            return null;
        }

        /// <summary>
        /// Data carrier for Get Account transactions API command
        /// </summary>
        public class GetAccountTransactionsData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Maximum items received back - not implemented now
            /// </summary>
            public int maxItems { get; set; }
        }
        /// <summary>
        /// Get all account transactions, now it is not limited for maximum amount of tx items. 
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns>>Dictionary of account transactions, key - txid, value - ITransaction</returns>
        [HttpPut]
        [Route("GetAccountTransactions")]
        //[Authorize(Rights.Administration)]
        public async Task<IDictionary<string, ITransaction>> GetAccountTransactions([FromBody] GetAccountTransactionsData accountData)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(accountData.walletId, out var wallet))
                {
                    // todo - remove address from real QT wallet
                    if (wallet.Accounts.TryGetValue(accountData.accountAddress, out var account))
                    {
                        return account.Transactions;
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {accountData.accountAddress} transactions, Account Not Found!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {accountData.accountAddress} transactions, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Transactions!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {accountData.accountAddress} Transactions!");
            }

            return null;
        }

        /// <summary>
        /// Data carrier for Loading account key API command
        /// </summary>
        public class AccountKeyData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account addres, now just Neblio address
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Private key. It is unique code.
            /// For accounts you can obtain it from QT Wallet with console and compant "dumpprivatekey address"
            /// </summary>
            public string key { get; set; } = string.Empty;
            /// <summary>
            /// Public key. It is unique code.
            /// In Blockchain it is account address
            /// </summary>
            public string pubkey { get; set; } = string.Empty;
            /// <summary>
            /// If you want to encrypt the key in the database (strongly reccomended with) fill this field with some string, in other case leave empty string
            /// I reccomend to use password longer than 12 characters and including also numbers and special characters
            /// </summary>
            public string password { get; set; } = string.Empty;
            /// <summary>
            /// Optional name of the key - not implemented in ui now
            /// will be used later for storing another types of keys
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// if you want to store key in db set true
            /// This if for case when you want to load the key just for the instance without keep it after restart of application
            /// </summary>
            public bool storeInDb { get; set; } = true;
            /// <summary>
            /// if you want this key as primary key of your account set this true
            /// if you set this false function will generate new RSA private and public key stored in account keys list
            /// </summary>
            public bool isItMainAccountKey { get; set; } = false;
            /// <summary>
            /// if you load already encrypted key you must set this true.
            /// during loading without setting this to true the input key will be encrypted with the password.
            /// </summary>
            public bool alreadyEncrypted { get; set; } = false;
        }
        /// <summary>
        /// This command will load account private key important for signing the blockchain transactions
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("LoadAccountKey")]
        //[Authorize(Rights.Administration)]
        public async Task<string> LoadAccountKey([FromBody] AccountKeyData keyData)
        {
            try
            {
                return MainDataContext.AccountHandler.LoadAccountKey(keyData.walletId, 
                                                                     keyData.accountAddress, 
                                                                     keyData.key, 
                                                                     dbService, 
                                                                     keyData.pubkey, 
                                                                     keyData.password, 
                                                                     keyData.name, 
                                                                     keyData.storeInDb, 
                                                                     keyData.isItMainAccountKey,
                                                                     keyData.alreadyEncrypted);
            }
            catch (Exception ex)
            {
                log.Error("Cannot load Account Key!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot load Account {keyData.accountAddress} Key!");
            }
        }

        /// <summary>
        /// Data carrier for Loading account key API command
        /// </summary>
        public class KeyChangeNameData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account addres, now just Neblio address
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Guid format of Key Id
            /// </summary>
            public string keyId { get; set; } = string.Empty;
            /// <summary>
            /// New name for the key
            /// </summary>
            public string newName { get; set; } = string.Empty;
        }
        /// <summary>
        /// This command will change the key name
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("ChangeKeyName")]
        //[Authorize(Rights.Administration)]
        public async Task<string> ChangeKeyName([FromBody] KeyChangeNameData keyData)
        {
            try
            {
                return MainDataContext.AccountHandler.ChangeKeyName(keyData.walletId, keyData.accountAddress, keyData.keyId, keyData.newName, dbService);
            }
            catch (Exception ex)
            {
                log.Error("Cannot change key name!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot change key name!");
            }
        }


        /// <summary>
        /// Data carrier for Loading account key API command
        /// </summary>
        public class KeyDownloadData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account addres, now just Neblio address
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Guid format of Key Id
            /// </summary>
            public string keyId { get; set; } = string.Empty;
        }

        /// <summary>
        /// This command will find and download the key
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("DownloadKey")]
        //[Authorize(Rights.Administration)]
        public async Task<EncryptionKeyTransferDto> DownloadKey([FromBody] KeyDownloadData keyData)
        {
            try
            {
                EncryptionKey key = null;
                EncryptionKeyTransferDto keydto = new EncryptionKeyTransferDto();

                if (EconomyMainContext.WorkWithDb)
                {
                    key = dbService.GetKey(new Guid(keyData.keyId));
                }
                else
                {
                    foreach (var a in EconomyMainContext.Accounts)
                    {
                        var k = a.Value.AccountKeys.FirstOrDefault(k => k.Id.ToString() == keyData.keyId);
                        if (k != null)
                        {
                            key = k;
                        }
                    }
                }

                if (key != null)
                {
                    keydto.Update(key);
                    key = null;
                    return keydto;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot change key name!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot change key name!");
            }
        }

        /// <summary>
        /// Data carrier for Loading account key API command
        /// </summary>
        public class KeyDeleteData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account addres, now just Neblio address
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Guid format of Key Id
            /// </summary>
            public string keyId { get; set; } = string.Empty;
        }

        /// <summary>
        /// This command will delete the key
        /// This means just set the flag deleted to bool
        /// Full remove is not supported now for security reason. You have to remove the key permanently in the database
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("DeleteKey")]
        //[Authorize(Rights.Administration)]
        public async Task<EncryptionKeyTransferDto> DeleteKey([FromBody] KeyDeleteData keyData)
        {
            try
            {
                var keydto = new EncryptionKeyTransferDto();
                var key = MainDataContext.AccountHandler.DeleteKey(keyData.walletId, keyData.accountAddress, keyData.keyId, dbService);

                if (key != null)
                {
                    keydto.Update(key);
                    key = null;
                    return keydto;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete the key!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete the key!");
            }
        }

        /// <summary>
        /// Get all key types. Usefull for creating dropbox in UI
        /// </summary>
        /// <returns>List of all key types</returns>
        [HttpGet]
        [Route("GetKeyTypes")]
        public async Task<List<string>> GetKeyTypes()
        {
            try
            {
                return Enum.GetValues(typeof(EncryptionKeyType)).Cast<EncryptionKeyType>().Select(t => t.ToString()).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Keys Types", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Keys types!");
            }
        }

        /// <summary>
        /// Data carrier for unlock account API command
        /// </summary>
        public class UnlockAccountData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Password to decrypt the private key if it is encrypted. If not leave empty string
            /// </summary>
            public string password { get; set; } = string.Empty;
        }

        /// <summary>
        /// Unlock the account. 
        /// This will init load of the key from the database and load it with the password to the EncryptionKey class in the account.
        /// After that the account is ready for signing and sending payments
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UnlockAccount")]
        //[Authorize(Rights.Administration)]
        public async Task<string> UnlockAccount([FromBody] UnlockAccountData keyData)
        {
            try
            {
                return MainDataContext.AccountHandler.UnlockAccount(keyData.walletId, keyData.accountAddress, keyData.password);
            }
            catch (Exception ex)
            {
                log.Error("Cannot unlock Account!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot unlock Account {keyData.accountAddress}!");
            }
        }

        /// <summary>
        /// Data carrier for Loading account key API command
        /// </summary>
        public class LockAccountData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
        }
        /// <summary>
        /// Command will lock the account and drop the object with the key
        /// </summary>
        [HttpPut]
        [Route("LockAccount")]
        //[Authorize(Rights.Administration)]
        public async Task<string> LockAccount([FromBody] LockAccountData keyData)
        {
            try
            {
                return MainDataContext.AccountHandler.LockAccount(keyData.walletId, keyData.accountAddress);
            }
            catch (Exception ex)
            {
                log.Error("Cannot lock Account!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot lock Account {keyData.accountAddress}!");
            }
        }

        /// <summary>
        /// Data carrier for get account lock state API command
        /// </summary>
        public class AccountLockedData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
        }
        /// <summary>
        /// this command will return lock/unlock state of the account
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns>true if the account is locked</returns>
        [HttpPut]
        [Route("IsAccountLocked")]
        //[Authorize(Rights.Administration)]
        public async Task<bool> IsAccountLocked([FromBody] AccountLockedData keyData)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(keyData.walletId, out var wallet))
                {
                    if (wallet.Accounts.TryGetValue(keyData.accountAddress, out var account))
                    {
                        return account.IsLocked();
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot check account lock status for {keyData.accountAddress}. Account does not exists!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot check account lock status for {keyData.walletId}. Wallet does not exists!");
                }

            }
            catch (Exception ex)
            {
                log.Error("Cannot check account lock status!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot check account lock status!");
            }
        }

        /// <summary>
        /// Data carrier for deleting the key account data 
        /// </summary>
        public class DeleteKeyData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Guid format of the key
            /// </summary>
            public string keyId { get; set; } = string.Empty;
        }
        /// <summary>
        /// This command will delete the account key in the Db. 
        /// It means set the deleted flag to true and the reccords are kept in the Db but not considered int he queries
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("DeleteAccountKey")]
        //[Authorize(Rights.Administration)]
        public async Task<string> DeleteAccountKey([FromBody] DeleteKeyData keyData)
        {
            if (!EconomyMainContext.WorkWithDb)
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Key. App dont work with Db!");

            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(keyData.walletId, out var wallet))
                {
                    if (wallet.Accounts.TryGetValue(keyData.accountAddress, out var account))
                    {
                        account.AccountKey = null;
                        dbService.DeleteKey(keyData.keyId);
                        return "OK";
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete key for {keyData.accountAddress}. Account does not exists!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete key for {keyData.walletId}. Wallet does not exists!");
                }

            }
            catch (Exception ex)
            {
                log.Error("Cannot delete Key!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Key!");
            }
        }

        /// <summary>
        /// This command will remove the account key in the Db. 
        /// It means the record it is removed permanently from the Db.
        /// </summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("RemoveAccountKey")]
        //[Authorize(Rights.Administration)]
        public async Task<string> RemoveAccountKey([FromBody] DeleteKeyData keyData)
        {
            if (!EconomyMainContext.WorkWithDb)
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Key. App dont work with Db!");

            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(keyData.walletId, out var wallet))
                {
                    if (wallet.Accounts.TryGetValue(keyData.accountAddress, out var account))
                    {
                        account.AccountKey = null;
                        dbService.RemoveKey(keyData.keyId);
                        return "OK";
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove key for {keyData.accountAddress}. Account does not exists!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove key for {keyData.walletId}. Wallet does not exists!");
                }

            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Key!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Key!");
            }
        }

        /// <summary>
        /// Data carrier for deleting account API command
        /// </summary>
        public class DeleteAccountData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            public bool withNodes { get; set; }
        }
        /// <summary>
        /// This command will delete the account in the Db. 
        /// It means set the deleted flag to true and the reccords are kept in the Db but not considered int he queries
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("DeleteAccount")]
        //[Authorize(Rights.Administration)]
        public async Task<string> DeleteAccount([FromBody] DeleteAccountData accountData)
        {
            var resp = "ERROR";

            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(accountData.walletId, out var wallet))
                {
                    // todo - remove address from real QT wallet
                    if (wallet.Accounts.Remove(accountData.accountAddress, out var account))
                    {
                        if (dbService.DeleteAccount(account.Id.ToString()))
                        {
                            resp = "OK";
                        }
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Account {accountData.accountAddress} from Db, Account Not Found!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Account {accountData.accountAddress} from Db, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete Account!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Account {accountData.accountAddress} from Db!");
            }

            return resp;
        }

        /// <summary>
        /// This command will remove the account in the Db. 
        /// It means the record it is removed permanently from the Db.
        /// </summary>
        /// <param name="accountData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("RemoveAccount")]
        //[Authorize(Rights.Administration)]
        public async Task<string> RemoveAccount([FromBody] DeleteAccountData accountData)
        {
            var resp = "ERROR";

            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(accountData.walletId, out var wallet))
                {
                    // todo - remove address from real QT wallet
                    if (wallet.Accounts.Remove(accountData.accountAddress, out var account))
                    {
                        if (dbService.RemoveAccount(account.Id.ToString()))
                        {
                            resp = "OK";
                        }
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Account {accountData.accountAddress} from Db, Account Not Found!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Account {accountData.accountAddress} from Db, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Account!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Account {accountData.accountAddress} from Db!");
            }

            return resp;
        }

        /// <summary>
        /// Returns all accounts of the wallet, should be without tx details, but not implemented yet, 
        /// same asGetWalletAccountsWithTx
        /// </summary>
        /// <param name="walletName"></param>
        /// <returns>Dictionary of the accounts, key - address, value - IAccount</returns>
        [HttpGet]
        [Route("GetWalletAccounts")]
        //[Authorize(Rights.Administration)]
        public async Task<object> GetWalletAccounts(string walletName)
        {
            try
            {
                if (!EconomyMainContext.Wallets.TryGetValue(walletName, out var wallet))
                {
                    return new { info = null as string, ReadingError = "READER_NOT_FOUND" };
                }

                var res = await wallet.ListAccounts();

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                log.Error("Cannot get Neblio Bitcoin price", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get wallet info!");
            }
        }

        /// <summary>
        /// Now same as GetWalletAccounts
        /// </summary>
        /// <param name="walletName"></param>
        /// <returns>Dictionary of the accounts, key - address, value - IAccount include all tx data</returns>
        [HttpGet]
        [Route("GetWalletAccountsWithTx")]
        //[Authorize(Rights.Administration)]
        public async Task<object> GetWalletAccountsWithTx(string walletName)
        {
            try
            {
                if (!EconomyMainContext.Wallets.TryGetValue(walletName, out var wallet))
                {
                    return new { info = null as string, ReadingError = "READER_NOT_FOUND" };
                }

                var res = await wallet.ListAccounts(true);

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                log.Error("Cannot get Neblio Bitcoin price", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get wallet info!");
            }
        }

        [HttpGet]
        [Route("GetAccountMessages/{account}")]
        //[Authorize(Rights.Administration)]
        public async Task<IDictionary<string, IToken>> GetAccountMessages(string account)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(account, out var acc))
                {

                    // use default now, can be override
                    MessagesHelpers.TokenId = EconomyMainContext.MessagingToken.Id;
                    MessagesHelpers.TokenSymbol = EconomyMainContext.MessagingToken.Symbol;

                    var toks = MainDataContext.AccountHandler.FindTokenByMetadata(account, "MessageData");

                    var tmp = new List<IToken>();
                    var resp = new Dictionary<string, IToken>();

                    if (toks != null)
                    {
                        foreach (var t in toks.Values)
                        {
                            if (t.Symbol == EconomyMainContext.MessagingToken.Symbol)
                            {
                                tmp.Add(t);
                            }
                        }

                        tmp = tmp.OrderBy(p => p.TimeStamp)
                            .Reverse()
                            .ToList();

                        foreach (var t in tmp)
                        {
                            resp.Add(t.TxId, t);
                        }
                    }

                    return resp;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot find metadata of token, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot find metadata of token!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot find metadata of token!");
            }
        }

        public class AccountKeyDto
        {
            public string KeyName { get; set; }
            public string KeyId { get; set; }
            public int Type { get; set; }
        }
        [HttpGet]
        [Route("GetAccountKeys/{account}")]
        //[Authorize(Rights.Administration)]
        public async Task<IDictionary<string, AccountKeyDto>> GetAccountKeys(string account)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(account, out var acc))
                {

                    var resp = new Dictionary<string, AccountKeyDto>();

                    if (acc.AccountKeys != null)
                    {
                        foreach (var k in acc.AccountKeys)
                        {
                            resp.Add(k.Id.ToString(), new AccountKeyDto() { KeyId = k.Id.ToString(), KeyName = k.Name, Type = Convert.ToInt32(k.Type) });
                        }
                    }

                    return resp;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot find account keys - Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot find account keys!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot find account keys!");
            }
        }



        /// <summary>
        /// Returns decrypted message if the Sender Pub Key is found in the account keys and it can be unlocked by the password
        /// </summary>
        /// <param name="metadataData"></param>
        /// <returns>Decrypted mesage</returns>
        [HttpPut]
        [Route("DecryptMessage")]
        //[Authorize(Rights.Administration)]
        public async Task<DecryptedMessageResponseDto> DecryptMessage([FromBody] GetDecryptedMessageDto data)
        {
            try
            {
                return await MessagesHelpers.DecryptMessage(data);
            }
            catch (Exception ex)
            {
                log.Error("Cannot decrypt message!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot decrypt messagen!");
            }
        }

        /// <summary>
        /// Data carrier for get token by metadata API command
        /// </summary>
        public class GetTokenByMetadataData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Key which can be matched in the metadata field
            /// </summary>
            public string metadataName { get; set; }
            /// <summary>
            /// You can find also keys which includes some specific value, leave empty string if you need just special keys
            /// </summary>
            public string metadataValue { get; set; }
        }
        /// <summary>
        /// Returns dictionary of tokens where some specific key or value is matched. If there is more than one all match are returned
        /// </summary>
        /// <param name="metadataData"></param>
        /// <returns>Dictionary of matched results, key - txid, value - IToken</returns>
        [HttpPut]
        [Route("GetTokenByMetadata")]
        //[Authorize(Rights.Administration)]
        public async Task<IDictionary<string, IToken>> GetTokenByMetadata([FromBody] GetTokenByMetadataData metadataData)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(metadataData.walletId, out var wallet))
                {
                    if (string.IsNullOrEmpty(metadataData.accountAddress))
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot find metadata of token, account address cannot be empty!");

                    // todo - remove address from real QT wallet
                    if (!string.IsNullOrEmpty(metadataData.metadataName))
                    {
                        var toks = MainDataContext.AccountHandler.FindTokenByMetadata(metadataData.accountAddress, metadataData.metadataName, metadataData.metadataValue);

                        var tmp = new List<IToken>();
                        var resp = new Dictionary<string, IToken>();

                        if (toks != null)
                        {
                            foreach (var t in toks.Values)
                            {
                                tmp.Add(t);
                            }

                            tmp = tmp.OrderBy(p => p.TimeStamp)
                                .Reverse()
                                .ToList();

                            foreach (var t in tmp)
                            {
                                resp.Add(t.TxId, t);
                            }
                        }

                        return resp;
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot find metadata of token, you must fill Name of metadata and optional is Value!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot find metadata of token, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot find metadata of token!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot find metadata of token!");
            }
        }

        #endregion

        #region Nodes

        /// <summary>
        /// Get all node types. Useful for dropdown in UI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNodesTypes")]
        public async Task<List<string>> GetNodesTypes()
        {
            try
            {
                return Enum.GetValues(typeof(NodeTypes)).Cast<NodeTypes>().Select(t => t.ToString()).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Node Types", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Node types!");
            }
        }

        /// <summary>
        /// Get all trigger node types. Useful for dropdown in UI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNodeTriggersTypes")]
        public async Task<List<string>> GetNodeTriggersTypes()
        {
            try
            {
                return Enum.GetValues(typeof(NodeActionTriggerTypes)).Cast<NodeActionTriggerTypes>().Select(t => t.ToString()).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Node Triggers Types", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Node Triggers types!");
            }
        }

        /// <summary>
        /// Data carrier for update node API command
        /// </summary>
        public class UpdateNodeData
        {
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// Optional readable node name
            /// </summary>
            public string nodeName { get; set; }
            /// <summary>
            /// Guid formad of nodeId
            /// Leave empty string if new node should be created
            /// </summary>
            public string nodeId { get; set; }
            /// <summary>
            /// activate or disable the node
            /// </summary>
            public bool isActivated { get; set; }
            /// <summary>
            /// Set node type
            /// </summary>
            public NodeTypes nodeType { get; set; }
            /// <summary>
            /// add optional parameters for the node type
            /// please check call the API command GetNodeActionParametersCarrier for get the coorect structure
            /// </summary>
            public NodeActionParameters parameters { get; set; }
        }

        /// <summary>
        /// Create or update the node
        /// If the node does not exits new one is created
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateNode")]
        //[Authorize(Rights.Administration)]
        public async Task<string> UpdateNode([FromBody] UpdateNodeData nodeData)
        {
            try
            {
                return MainDataContext.NodeHandler.UpdateNode(nodeData.accountAddress,
                                nodeData.nodeId, Guid.Empty,
                                nodeData.nodeName,
                                nodeData.nodeType,
                                nodeData.isActivated,
                                nodeData.parameters, dbService);
            }
            catch (Exception ex)
            {
                log.Error("Cannot create node", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot create node!");
            }
        }

        /// <summary>
        /// Returns all nodes in the Db
        /// </summary>
        /// <returns>List of INode objects</returns>
        [HttpGet]
        [Route("GetNodesFromDb")]
        //[Authorize(Rights.Administration)]
        public async Task<List<INode>> GetNodesFromDb() // todo carrier dto
        {
            try
            {
                if (EconomyMainContext.WorkWithDb)
                {
                    var nodes = dbService.GetNodes();
                    return nodes;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot get Nodes from Db! Db is not setted up.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Nodes from Db", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Nodes from Db!");
            }
        }

        /// <summary>
        /// Data carrier for updating node actiation API command
        /// </summary>
        public class SetActivateNodeData
        {
            /// <summary>
            /// Guid format of node Id
            /// </summary>
            public string nodeId { get; set; }
            /// <summary>
            /// Set true if the node should be activated
            /// </summary>
            public bool isActivated { get; set; }
        }

        /// <summary>
        /// Activate or Deactivate the node 
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SetNodeActivateStatus")]
        //[Authorize(Rights.Administration)]
        public async Task<string> SetNodeActivateStatus([FromBody] SetActivateNodeData nodeData)
        {
            try
            {
                return await MainDataContext.NodeHandler.SeNodeActivation(nodeData.nodeId, nodeData.isActivated, dbService);
            }
            catch (Exception ex)
            {
                log.Error("Cannot set node", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot set node!");
            }
        }

        /// <summary>
        /// Data carrier for setting node trigger API command
        /// </summary>
        public class SetNodeTriggerData
        {
            /// <summary>
            /// Guid format of node Id
            /// </summary>
            public string nodeId { get; set; }
            public NodeActionTriggerTypes type { get; set; }
        }

        /// <summary>
        /// Set specific node trigger
        /// Without setting the trigger the node cannot invoke the action
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SetNodeTrigger")]
        //[Authorize(Rights.Administration)]
        public async Task<string> SetNodeTrigger([FromBody] SetNodeTriggerData nodeData)
        {
            try
            {
                return await MainDataContext.NodeHandler.SetNodeTrigger(nodeData.nodeId, nodeData.type, dbService);
            }
            catch (Exception ex)
            {
                log.Error("Cannot set node", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot set node!");
            }
        }

        /// <summary>
        /// Data carrier for deleting the node
        /// </summary>
        public class DeleteNodeData
        {
            /// <summary>
            /// Guid format of node Id
            /// </summary>
            public string nodeId { get; set; }
        }
        /// <summary>
        /// This command will delete the node in the Db. 
        /// It means set the deleted flag to true and the reccords are kept in the Db but not considered int he queries
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("DeleteNode")]
        //[Authorize(Rights.Administration)]
        public async Task<string> DeleteNode([FromBody] DeleteNodeData nodeData)
        {
            var resp = "ERROR";

            try
            {

                if (EconomyMainContext.Nodes.Remove(nodeData.nodeId, out var node))
                {
                    if (dbService.DeleteNode(nodeData.nodeId))
                    {
                        resp = "OK";
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Node {nodeData.nodeId} from Db, Node Not Found!");
                }

            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Node!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot delete Node {nodeData.nodeId} from Db!");
            }

            return resp;
        }

        /// <summary>
        /// This command will remove the node in the Db. 
        /// It means the record it is removed permanently from the Db.
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("RemoveNode")]
        //[Authorize(Rights.Administration)]
        public async Task<string> RemoveNode([FromBody] DeleteNodeData nodeData)
        {
            var resp = "ERROR";

            try
            {
                if (EconomyMainContext.Nodes.Remove(nodeData.nodeId, out var node))
                {
                    if (dbService.RemoveNode(nodeData.nodeId))
                    {
                        resp = "OK";
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Node {nodeData.nodeId} from Db, Node Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Node!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot remove Node {nodeData.nodeId} from Db!");
            }

            return resp;
        }

        /// <summary>
        /// Return shape of Node Specific Parameters
        /// These are related to specific node type
        /// </summary>
        /// <param name="nodeType">Node Type, please check api/GetNodeTypes </param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNodeSpecificParametersCarrier")]
        public object GetNodeSpecificParametersCarrier(NodeTypes nodeType)
        {
            try
            {
                var o = NodeFactory.GetNode(nodeType, Guid.Empty, Guid.Empty, string.Empty, false, null).GetNodeParametersCarrier();
                return o;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Node Specific Parameters Carrier!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Node Specific Parameters Carrier");
            }
        }

        /// <summary>
        /// Return common shape of Node Action Parameters
        /// Same for all Nodes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNodeActionParametersCarrier")]
        public object GetNodeActionParametersCarrier()
        {
            try
            {
                return new NodeActionParameters();
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Node Action Parameters Carrier!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Node Action Parameters Carrier");
            }
        }

        /// <summary>
        /// Data carrier for invoke node API command
        /// </summary>
        public class InvokeNodeData
        {
            /// <summary>
            /// Type of trigger in the simulated invoke
            /// </summary>
            public NodeActionTriggerTypes triggerType { get; set; } = NodeActionTriggerTypes.UnconfirmedTokenTxArrived;
            /// <summary>
            /// Guid format of node Id
            /// </summary>
            public string nodeId { get; set; } = string.Empty;
            /// <summary>
            /// Specific data for node actions
            /// </summary>
            public string[] data { get; set; }
            /// <summary>
            /// You can call this invoke multiple time automatically.
            /// </summary>
            public int NumberOfCalls { get; set; } = 1;
            /// <summary>
            /// If this is set to true, the node will take last token tx data and use it during invoke as source data
            /// </summary>
            public bool UseLastTokenTxData { get; set; } = false;
            /// <summary>
            /// if you want to just test the new JavaScript please fill it to this variable and set the useAltScript
            /// The main script stored in the Db will be ignored and node will do action with use of this script
            /// Good for testing scripts from UI without storing it
            /// </summary>
            public string altScript { get; set; } = string.Empty;
            /// <summary>
            /// set to true if the altScript should be used instead of stored script
            /// </summary>
            public bool useAltScript { get; set; } = false;
        }
        /// <summary>
        /// This command can invoke the node function via API immediately. 
        /// You can use it for the testing or as the function from API which is called in backend
        /// You can use it as partial JavaScript backend - just write your function to JS and run it in the node.
        /// You can use MQTT node to publish result back to UI
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("InvokeNodeAction")]
        //[Authorize(Rights.Administration)]
        public async Task<List<NodeActionFinishedArgs>> InvokeNodeAction([FromBody] InvokeNodeData nodeData)
        {
            var res = new List<NodeActionFinishedArgs>();
            try
            {
                if (EconomyMainContext.Nodes.TryGetValue(nodeData.nodeId, out var node))
                {
                    if (nodeData.UseLastTokenTxData)
                    {
                        if (node.LastOtherData == null)
                            nodeData.data = VEDrivers.Nodes.TestData.NodeTestData.GetTestData();
                        else
                            nodeData.data = node.LastOtherData;
                    }

                    if (nodeData.data == null)
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot invoke Node {nodeData.nodeId} action, data cannot be empty!");

                    NodeActionFinishedArgs r;

                    for (int j = 0; j < nodeData.NumberOfCalls; j++)
                    {
                        if (nodeData.useAltScript && !string.IsNullOrEmpty(nodeData.altScript))
                            r = await node.InvokeNodeFunction(nodeData.triggerType, nodeData.data, nodeData.altScript);
                        else
                            r = await node.InvokeNodeFunction(nodeData.triggerType, nodeData.data);

                        if (r != null)
                            res.Add(r);
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot invoke Node {nodeData.nodeId} action, Node Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Cannot invoke Node {nodeData.nodeId}!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot invoke Node {nodeData.nodeId} action!");
            }

            return res;
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Return object ITransaction with tx details
        /// </summary>
        /// <param name="txid"></param>
        /// <returns>ITransaction object</returns>
        [HttpGet]
        [Route("GetNeblioTransactionInfo")]
        public async Task<object> GetNeblioTransactionInfo(string txid)
        {
            try
            {
                var res = await NeblioTransactionHelpers.TransactionInfoAsync(null, TransactionTypes.Neblio, txid);

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                log.Error("Cannot get tx info", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get tx info!");
            }
        }

        /// <summary>
        /// Send specific amount Neblio NTP1 tokens to the addres
        /// It can contain custom metadata
        /// Please check SendTokenTxData dto for the details
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SendNTP1Token")]
        //[Authorize(Rights.Administration)]
        public async Task<object> SendNTP1Token([FromBody] SendTokenTxData data)
        {
            try
            {
                var res = await NeblioTransactionHelpers.SendNTP1TokenAPI(data);

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Token", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send Neblio Token - {ex.Message}!");
            }
        }

        /// <summary>
        /// Send Message with Neblio MSGT token to the addres
        /// Please check SendMessageDto class for the details
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SendMessageToken")]
        //[Authorize(Rights.Administration)]
        public async Task<object> SendMessageToken([FromBody] SendMessageDto data)
        {
            try
            {
                var res = await MessagesHelpers.SendMessage(data);

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Token", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send Neblio Token - {ex.Message}!");
            }
        }

        /// <summary>
        /// Send specific amount Neblio coins to the addres
        /// Please check SendTxData dto for the details
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SendNeblioTx")]
        //[Authorize(Rights.Administration)]
        public async Task<object> SendNeblioTx([FromBody] SendTxData data)
        {
            try
            {
                var res = await NeblioTransactionHelpers.SendNeblioTransactionAPI(data);

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send Transaction Token - {ex.Message}!");
            }
        }

        /// <summary>
        /// Send specific amount Neblio NFT to the addres
        /// Please check SendTxData dto for the details
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SendNeblioNFT")]
        //[Authorize(Rights.Administration)]
        public async Task<object> SendNeblioNFT([FromBody] SendTokenTxData data)
        {
            try
            {
                if (data.Metadata != null)
                {
                    if (data.Metadata.TryGetValue("SourceUtxo", out var sourceUtxo))
                    {
                        if (!string.IsNullOrEmpty(sourceUtxo))
                        {
                            data.Id = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
                            data.Symbol = "VENFT";
                            var res = await NeblioTransactionHelpers.SendNTP1TokenAPI(data, isNFTtx: true);

                            return new { info = res, ReadingError = "OK" };
                        }
                    }
                }

                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send NFT Transaction - you did not provided source of the NFT!");
            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send NFT Tx - {ex.Message}!");
            }
        }

        /// <summary>
        /// Mint Neblio NFT token to addres
        /// Please check MintNFTData dto for the details
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("MintNeblioNFT")]
        //[Authorize(Rights.Administration)]
        public async Task<object> MintNeblioNFT([FromBody] MintNFTData data)
        {
            try
            {
                var res = await NeblioTransactionHelpers.MintNFTToken(data);

                return new { info = res, ReadingError = "OK" }; ;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Mint NFT - {ex.Message}!");
            }
        }

        public class GetMintingStatusDto
        {
            public string accountAddress { get; set; } = string.Empty;
            public string tokenId { get; set; } = string.Empty;
        }

        /// <summary>
        /// Get actual balance for minting
        /// </summary>
        /// <param name="data"></param>
        /// <returns> list of utxos (key utxos, value List<Utxos>) and token data (key token, value IToken) </returns>
        [HttpPut]
        [Route("GetActualMintingTokenSupply")]
        //[Authorize(Rights.Administration)]
        public async Task<object> GetActualMintingTokenSupply([FromBody] GetMintingStatusDto data)
        {
            try
            {
                //var res = await NeblioTransactionHelpers.FindUtxoForMintNFT(data.accountAddress, data.tokenId);

                // this will get all utxos of the tokens biger than 1 token
                var res = await NeblioTransactionHelpers.GetAddressTokensUtxos(data.accountAddress);
                var utxos = new List<Utxos>();
                foreach (var r in res)
                {
                    var toks = r.Tokens.ToArray()?[0];
                    if (toks != null)
                    {
                        if (toks.Amount > 1)
                        {
                            if (toks.TokenId == "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8")
                                utxos.Add(r);
                        }
                    }
                }

                var token = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, data.tokenId, string.Empty);

                var totalAmount = 0.0;
                foreach (var u in utxos)
                    totalAmount += (double)u.Tokens.ToArray()?[0]?.Amount;

                var info = new
                {
                    totalAmount = totalAmount,
                    utxos = utxos,
                    token = token
                };

                return info;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Get Minting Token supply - {ex.Message}!");
            }
        }

        /// <summary>
        /// Send Message with Neblio VENFT token to the addres
        /// Please check SendMessageDto class for the details
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SendShopSettingToken")]
        //[Authorize(Rights.Administration)]
        public async Task<object> SendShopSettingToken([FromBody] SendNeblioShopSettingTokenTxData data)
        {
            try
            {
                data.Id = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
                data.TokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
                data.Symbol = "VENFT";
                data.Metadata.Add("ShopSettingToken", "true");
                data.Metadata.Add("Name", data.ShopName);
                data.Metadata.Add("Description", data.ShopDescription);

                data.Metadata.Add("ShopItems", JsonConvert.SerializeObject(data.ShopItems));

                var resp = await NeblioTransactionHelpers.SendNTP1TokenAPI(data, fee: 30000);

                return new { info = resp, ReadingError = "OK" };

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Token", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send Neblio Token - {ex.Message}!");
            }
        }

        public class StartShopDto
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            public string tokenId { get; set; } = string.Empty;
        }

        /// <summary>
        /// Start Account Shop
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("StartAccountShop")]
        //[Authorize(Rights.Administration)]
        public async Task<string> StartAccountShop([FromBody] StartShopDto data)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(data.walletId, out var w))
                {
                    if (w.Accounts.TryGetValue(data.accountAddress, out var account))
                    {
                        if (account.Shop == null)
                        {
                            account.Shop = ShopFactory.GetShop(ShopTypes.NeblioTokenShop, data.accountAddress, data.tokenId);
                        }

                        account.Shop.IsActive = true;
                        var resp = await account.Shop.StartShop();

                        return resp;
                    }
                }

                return string.Empty;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Get Minting Token supply - {ex.Message}!");
            }
        }

        public class SplitTokesData
        {
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            public string tokenId { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
            public int lotAmount { get; set; } = 100;
            public int numberOfLots { get; set; } = 10;
        }

        /// <summary>
        /// Split tokens to smaller lots
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SplitTokens")]
        //[Authorize(Rights.Administration)]
        public async Task<List<string>> SplitTokens([FromBody] SplitTokesData data)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(data.accountAddress, out var account))
                {
                    var resp = await NeblioTransactionHelpers.SplitTheTokens(data.accountAddress, data.password, data.tokenId, data.lotAmount, data.numberOfLots);

                    return resp;
                }

                return null;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Split Tokens - {ex.Message}!");
            }
        }

        /// <summary>
        /// Restart Account Shop
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("RestartAccountShop")]
        //[Authorize(Rights.Administration)]
        public async Task<string> RestartAccountShop([FromBody] StartShopDto data)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(data.walletId, out var w))
                {
                    if (w.Accounts.TryGetValue(data.accountAddress, out var account))
                    {
                        account.Shop = null; //todo cancel and dispose

                        account.Shop = ShopFactory.GetShop(ShopTypes.NeblioTokenShop, data.accountAddress, data.tokenId);

                        account.Shop.IsActive = true;
                        var resp = await account.Shop.StartShop();

                        return resp;
                    }
                }

                return string.Empty;

            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Get Minting Token supply - {ex.Message}!");
            }
        }

        /// <summary>
        /// Send 1 NEBL to VEF address to obtain the 100 VENFT tokens
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("OrderSourceTokens")]
        //[Authorize(Rights.Administration)]
        public async Task<object> OrderSourceTokens([FromBody] SendTxData data)
        {
            try
            {
                data.Amount = 1;
                data.Id = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
                data.Symbol = "VENFT";
                var res = await NeblioTransactionHelpers.SendNeblioTransactionAPI(data);

                return new { info = res, ReadingError = "OK" };
            }
            catch (Exception ex)
            {
                //log.Error("Cannot send Neblio Transaction", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot send Transaction Token - {ex.Message}!");
            }
        }

        /// <summary>
        /// Data carrier for tx receipt API command
        /// </summary>
        public class TxReceiptData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }
            /// <summary>
            /// tx id
            /// </summary>
            public string txId { get; set; }
        }
        /// <summary>
        /// Data carrier for tx response from receipt API command
        /// </summary>
        public class TxReceiptResponse
        {
            public IReceipt receipt { get; set; }
            public string htmlReceipt { get; set; }
        }
        /// <summary>
        /// return the html page with receipt and receipt object with details about the transaction and the currecny
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("GetTxReceipt")]
        //[Authorize(Rights.Administration)]
        public async Task<TxReceiptResponse> GetNeblioTxReceipt([FromBody] TxReceiptData data)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(data.accountAddress, out var account))
                {
                    var receipt = ReceiptFactory.GetReceipt(ReceiptTypes.Neblio, account.WalletId, account.Id, data.accountAddress, data.txId, true, true);
                    var str = await receipt.GetReceiptOutput();

                    var response = new TxReceiptResponse()
                    {
                        receipt = receipt,
                        htmlReceipt = str
                    };

                    return response;
                }
                else
                {
                    log.Error("Cannot get receipt, cannot find account!");
                    throw new HttpResponseException((HttpStatusCode)501, "Cannot get receipt, cannot find account!!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get receiopt", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get receipt!");
            }
        }

        #endregion


        #region bookmarks


        public class UpdateBookmarkData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }

            /// <summary>
            /// Guid format of bookmark Id
            /// Leave empty for new one
            /// </summary>
            public string bookmarkId { get; set; }
            /// <summary>
            /// Bookmark Name
            /// </summary>
            public string bookmarkName { get; set; }
            /// <summary>
            /// Address on bookmark
            /// </summary>
            public string bookmarkAddress { get; set; }

            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public BookmarkTypes type { get; set; }
        }
        /// <summary>
        /// add or update bookmark
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateBookmark")]
        //[Authorize(Rights.Administration)]
        public async Task<string> UpdateBookmark([FromBody] UpdateBookmarkData data)
        {
            try
            {
                var resp = MainDataContext.AccountHandler.UpdateBookmark(data.walletId,
                                                                         data.accountAddress,
                                                                         data.type,
                                                                         data.bookmarkId,
                                                                         data.bookmarkName,
                                                                         data.bookmarkAddress,
                                                                         dbService);
                return resp;
            }
            catch (Exception ex)
            {
                log.Error("Cannot add or update bookmark", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot add or update bookmark!");
            }
        }

        public class DeleteBookmarkData
        {
            /// <summary>
            /// Guid format of wallet Id
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address, now just Neblio addresses
            /// </summary>
            public string accountAddress { get; set; }

            /// <summary>
            /// Guid format of bookmark Id
            /// Leave empty for new one
            /// </summary>
            public string bookmarkId { get; set; }
        }
        /// <summary>
        /// add or update bookmark
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("DeleteBookmark")]
        //[Authorize(Rights.Administration)]
        public async Task<string> DeleteBookmark([FromBody] DeleteBookmarkData data)
        {
            try
            {
                var resp = MainDataContext.AccountHandler.DeleteBookmark(data.walletId,
                                                                         data.accountAddress,
                                                                         data.bookmarkId,
                                                                         dbService);
                return resp;
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete bookmark", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot delete bookmark!");
            }
        }

        /// <summary>
        /// remove bookmark
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("RemoveBookmark")]
        //[Authorize(Rights.Administration)]
        public async Task<string> RemoveBookmark([FromBody] DeleteBookmarkData data)
        {
            try
            {
                var resp = MainDataContext.AccountHandler.RemoveBookmark(data.walletId,
                                                                         data.accountAddress,
                                                                         data.bookmarkId,
                                                                         dbService);
                return resp;
            }
            catch (Exception ex)
            {
                log.Error("Cannot delete bookmark", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot delete bookmark!");
            }
        }

        /// <summary>
        /// remove bookmark
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAccountBookmarks/{address}")]
        //[Authorize(Rights.Administration)]
        public async Task<List<IBookmark>> GetAccountBookmarks(string address)
        {
            try
            {
                return MainDataContext.AccountHandler.GetAccountBookmarks(address);
            }
            catch (Exception ex)
            {
                log.Error("Cannot get bookmarks", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get bookmarks!");
            }
        }

        #endregion

        #region ExchangeCommands

        /// <summary>
        /// Get actual Nebl price in btc sat.
        /// Price is get from Binance API
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNeblBtcPrice")]
        public async Task<string> GetNeblBtcPrice()
        {
            try
            {
                var price = EconomyMainContext.ExchangeDataProvider.LastKline;

                if (price == null)
                {
                    return "Data not loaded yet, minimum time is one minute";
                }

                var res = "none";

                try
                {
                    res = price.Data.Close.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                }

                return res;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Neblio Bitcoin price", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot et Neblio Bitcoin price!");
            }
        }

        #endregion
    }
}
