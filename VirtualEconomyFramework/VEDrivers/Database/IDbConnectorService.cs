using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;
using VEDrivers.Security;

namespace VEDrivers.Database
{
    public interface IDbConnectorService
    {
        /// <summary>
        /// Function returns all Wallets stored in database
        /// </summary>
        /// <returns>List of IWallet object created based on specified type</returns>
        List<IWallet> GetWallets();
        /// <summary>
        /// Function returns all Accounts stored in database
        /// </summary>
        /// <returns>List of IAccount object created based on specified type</returns>
        List<IAccount> GetAccounts();
        /// <summary>
        /// Function returns all Nodes stored in database
        /// </summary>
        /// <returns>List of INode object created based on specified type</returns>
        List<INode> GetNodes();
        /// <summary>
        /// Function returns all Keys stored in database
        /// </summary>
        /// <returns>List of EncryptionKey object created based on specified type</returns>
        List<EncryptionKey> GetKeys();
        /// <summary>
        /// Function returns Wallet stored in database based on id
        /// </summary>
        /// <returns>IWallet object created based on specified type</returns>
        IWallet GetWallet(Guid id);
        /// <summary>
        /// Function save Wallet to the database
        /// </summary>
        /// <returns>true when success</returns>
        bool SaveWallet(IWallet wallet);
        /// <summary>
        /// Function will set specified Wallet as deleted
        /// </summary>
        /// <param name="walletId"></param>
        /// /// <param name="withAccounts">if true, function will set as deleted all related accounts</param>
        /// <returns></returns>
        bool DeleteWallet(string walletId, bool withAccounts = false);
        /// <summary>
        /// Function will remove specified wallet from database
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="withAccounts">if true, function will remove all related accounts</param>
        /// <returns></returns>
        bool RemoveWallet(string walletId, bool withAccounts = false);
        /// <summary>
        /// Function returns Account stored in database based on id
        /// </summary>
        /// <returns>IAccount object created based on specified type</returns>
        IAccount GetAccount(Guid id);
        /// <summary>
        /// Function save Account to the database
        /// </summary>
        /// <returns>true when success</returns>
        bool SaveAccount(IAccount account);
        /// <summary>
        /// Function will set specified account as deleted
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        bool DeleteAccount(string accountId);
        /// <summary>
        /// Function will remove specified account from database
        /// </summary>
        /// <param name="accountId">Id of account</param>
        /// <returns></returns>
        bool RemoveAccount(string accountId);
        /// <summary>
        /// Function returns Node stored in database based on id
        /// </summary>
        /// <returns>INode object created based on specified type</returns>
        INode GetNode(Guid id);
        /// <summary>
        /// Function save Node to the database
        /// </summary>
        /// <returns>true when success</returns>
        bool SaveNode(INode node);
        /// <summary>
        /// Function will set specified node as deleted
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        bool DeleteNode(string nodeId);
        /// <summary>
        /// Function will remove specified node from database
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        bool RemoveNode(string nodeId);

        /// <summary>
        /// Function returns key stored in database based on id
        /// </summary>
        /// <returns>EncryptionKey object created based on specified type</returns>
        EncryptionKey GetKey(Guid id);
        /// <summary>
        /// Function save key to the database
        /// </summary>
        /// <returns>true when success</returns>
        bool SaveKey(EncryptionKey key);
        /// <summary>
        /// Function will set specified Key as deleted
        /// </summary>
        /// <returns></returns>
        bool DeleteKey(string keyId);
        /// <summary>
        /// Function will remove specified key from database
        /// </summary>
        /// <returns></returns>
        bool RemoveKey(string keyId);
    }
}
