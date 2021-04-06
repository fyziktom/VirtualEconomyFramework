using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Security;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public interface IAccountHandler
    {
        Task<string> UpdateAccount(string accountAddress, Guid walletId, AccountTypes type, string name, IDbConnectorService dbservice, bool justInDb = true, string password = "");
        IDictionary<string, IToken> FindTokenByMetadata(string account, string key, string value = "");
        IDictionary<string, IToken> FindAllTokens(string account);
        LastTxSaveDto GetLastAccountProcessedTxs(string address);
        string LoadAccountKey(string wallet, string address, string key, IDbConnectorService dbservice, string pubkey = "", string password = "", string name = "", bool storeInDb = true, bool isItMainAccountKey = false, bool alreadyEncrypted = false, EncryptionKeyType type = EncryptionKeyType.BasicSecurity);
        string ChangeKeyName(string wallet, string address, string keyId, string newName, IDbConnectorService dbservice);
        EncryptionKey DeleteKey(string walletId, string address, string keyId, IDbConnectorService dbservice);
        string UnlockAccount(string wallet, string address, string password);
        string LockAccount(string wallet, string address);
    }
}
