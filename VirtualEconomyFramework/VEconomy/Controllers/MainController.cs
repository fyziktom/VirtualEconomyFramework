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
using Newtonsoft.Json;
using VEconomy.Common;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Receipt;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets;
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

        [HttpGet]
        [Route("GetServerParams")]
        public object GetServerParams()
        {
            return new
            {
                EconomyMainContext.MQTT
            };
        }

        [HttpGet]
        [Route("IsRPCAvailable")]
        public bool IsRPCAvailable()
        {
            return EconomyMainContext.WorkWithQTRPC;
        }

        [HttpGet]
        [Route("IsDbAvailable")]
        public bool IsDbAvailable()
        {
            return EconomyMainContext.WorkWithDb;
        }

        public class UpdateWalletData
        {
            public string owner { get; set; }
            public string walletId { get; set; }
            public string walletName { get; set; }
            public string walletBaseHost { get; set; }
            public int walletPort { get; set; }
            public WalletTypes walletType {get;set;}
        }

        #region Wallets

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
                    catch(Exception ex)
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

        [HttpGet]
        [Route("GetWalletsFromDb")]
        //[Authorize(Rights.Administration)]
        public async Task<List<IWallet>> GetWalletsFromDb() // todo carrier dto
        {
            try
            {
                var walls = dbService.GetWallets();
                return walls;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Wallets from Db", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Wallets from Db!");
            }
        }

        public class RemoveWalletData
        {
            public string walletId { get; set; }
            public bool withAccounts { get; set; }
        }

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

        [HttpGet]
        [Route("GetAccountTokens/{address}")]
        public async Task<IDictionary<string,IToken>> GetAccountTokens(string address)
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

        public class UpdateAccountData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public string accountName { get; set; }
            public string password { get; set; }
            public bool saveJustToDb { get; set; } = true;
            public AccountTypes accountType { get; set; }
        }

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

        [HttpGet]
        [Route("GetAccountsFromDb")]
        //[Authorize(Rights.Administration)]
        public async Task<List<IAccount>> GetAccountsFromDb() // todo carrier dto
        {
            try
            {
                var accs = dbService.GetAccounts();
                return accs;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Accounts from Db", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Accounts from Db!");
            }
        }

        public class GetAccountTransactionsData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public int maxItems { get; set; }
        }
        [HttpPut]
        [Route("GetAccountTransactions")]
        //[Authorize(Rights.Administration)]
        public async Task<IDictionary<string,ITransaction>> GetAccountTransactions([FromBody] GetAccountTransactionsData accountData)
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

        public class AccountKeyData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public string key { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public bool storeInDb { get; set; } = true;
        }
        [HttpPut]
        [Route("LoadAccountKey")]
        //[Authorize(Rights.Administration)]
        public async Task<string> LoadAccountKey([FromBody] AccountKeyData keyData)
        {
            try
            {
                return MainDataContext.AccountHandler.LoadAccountKey(keyData.walletId, keyData.accountAddress, keyData.key, dbService, keyData.password, keyData.name, keyData.storeInDb);
            }
            catch (Exception ex)
            {
                log.Error("Cannot load Account Key!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot load Account {keyData.accountAddress} Key!");
            }
        }

        public class UnlockAccountData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public string password { get; set; } = string.Empty;
        }
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

        public class LockAccountData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
        }
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

        public class AccountLockedData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
        }
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

        public class DeleteKeyData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public string keyId { get; set; } = string.Empty;
        }
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

        public class DeleteAccountData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public bool withNodes { get; set; }
        }
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

        public class GetTokenByMetadataData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public string metadataName { get; set; }
            public string metadataValue { get; set; }
        }
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
                        var resp = MainDataContext.AccountHandler.FindTokenByMetadata(metadataData.accountAddress, metadataData.metadataName, metadataData.metadataValue);
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

        public class UpdateNodeData
        {
            public string accountAddress { get; set; }
            public string nodeName { get; set; }
            public string nodeId { get; set; }
            public bool isActivated { get; set; }
            public NodeTypes nodeType { get; set; }
            public NodeActionParameters parameters { get; set; }
        }

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

        [HttpGet]
        [Route("GetNodesFromDb")]
        //[Authorize(Rights.Administration)]
        public async Task<List<INode>> GetNodesFromDb() // todo carrier dto
        {
            try
            {
                var nodes = dbService.GetNodes();
                return nodes;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Nodes from Db", ex);
                throw new HttpResponseException((HttpStatusCode)501, "Cannot get Nodes from Db!");
            }
        }

        public class SetActivateNodeData
        {
            public string nodeId { get; set; }
            public bool isActivated { get; set; }
        }

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

        public class SetNodeTriggerData
        {
            public string nodeId { get; set; }
            public NodeActionTriggerTypes type { get; set; }
        }

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

        public class DeleteNodeData
        {
            public string nodeId { get; set; }
        }
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

        public class InvokeNodeData
        {
            public NodeActionTriggerTypes triggerType { get; set; } = NodeActionTriggerTypes.UnconfirmedTokenTxArrived;
            public string nodeId { get; set; } = string.Empty;
            public string[] data { get; set; }
            public int NumberOfCalls { get; set; } = 1;
            public bool UseLastTokenTxData { get; set; } = false;
            public string altScript { get; set; } = string.Empty;
            public bool useAltScript { get; set; } = false;
        }
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

        public class TxReceiptData
        {
            public string walletId { get; set; }
            public string accountAddress { get; set; }
            public string txId { get; set; }
        }
        public class TxReceiptResponse
        {
            public IReceipt receipt { get; set; }
            public string htmlReceipt { get; set; }
        }
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


        #region ExchangeCommands

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
