﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Bookmarks;
using VEDrivers.Database;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Security;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public abstract class CommonAccountHandler : IAccountHandler
    {
        public abstract Task<string> UpdateAccount(string accountAddress, Guid walletId, AccountTypes type, string name, IDbConnectorService dbservice, bool justInDb = true, string password = "");
        public abstract IDictionary<string, IToken> FindTokenByMetadata(string account, string key, string value = "");
        public abstract IDictionary<string, IToken> FindAllTokens(string account);
        public abstract LastTxSaveDto GetLastAccountProcessedTxs(string address);
        public abstract string LoadAccountKey(string wallet, string address, string key, IDbConnectorService dbservice, string pubkey = "", string password = "", string name = "", bool storeInDb = true, bool isItMainAccountKey = false, bool alreadyEncrypted = false, EncryptionKeyType type = EncryptionKeyType.BasicSecurity);
        public abstract string ChangeKeyName(string wallet, string address, string keyId, string newName, IDbConnectorService dbservice);
        public abstract EncryptionKey DeleteKey(string walletId, string address, string keyId, IDbConnectorService dbservice);
        public abstract string UnlockAccount(string wallet, string address, string password);
        public abstract string LockAccount(string wallet, string address);
        public abstract string UpdateBookmark(string wallet, string address, BookmarkTypes type, string bookmarkId, string name, string bookmarkAddress, IDbConnectorService dbservice);
        public abstract string DeleteBookmark(string wallet, string address, string bookmarkId, IDbConnectorService dbservice);
        public abstract string RemoveBookmark(string wallet, string address, string bookmarkId, IDbConnectorService dbservice);
        public abstract List<IBookmark> GetAccountBookmarks(string address);
    }
}
