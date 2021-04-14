using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Bookmarks;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;
using VEDrivers.Nodes.Dto;
using VEDrivers.Security;

namespace VEDrivers.Database
{
    public class DbConnectorService : IDbConnectorService
    {
        public DbConnectorService(DbEconomyContext _context)
        {
            context = _context;
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private DbEconomyContext context;
        /// <summary>
        /// Function returns all Wallets stored in database
        /// </summary>
        /// <returns>List of IWallet object created based on specified type</returns>
        public List<IWallet> GetWallets()
        {
            try
            {
                ////using var context = new DbEconomyContext();

                var walllets = new List<IWallet>();

                foreach (var w in context.Wallets.Where(w => !w.Deleted))
                {
                    // todo: add RPC save!!!!!!
                    walllets.Add(w.Fill(WalletFactory.GetWallet(new Guid(), new Guid(), (WalletTypes)w.Type, w.Name, true, w.Host, w.Port)));
                }

                return walllets;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get wallets list from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function returns all Accounts stored in database
        /// </summary>
        /// <returns>List of IAccount object created based on specified type</returns>
        public List<IAccount> GetAccounts()
        {
            try
            {
                ////using var context = new DbEconomyContext();

                var accounts = new List<IAccount>();

                foreach (var a in context.Accounts.Where(a => !a.Deleted))
                {
                    var keys = context.Keys
                        .Where(k => !k.Deleted)
                        .Where(k => k.RelatedItemId == a.Id)
                        .ToList();

                    var bookmarks = context.Bookmarks
                        .Where(b => !b.Deleted)
                        .Where(b => b.RelatedItemId == a.Id)
                        .ToList();

                    var acc = AccountFactory.GetAccount(new Guid(a.Id), (AccountTypes)a.Type, Guid.Empty, new Guid(a.WalletId), string.Empty, string.Empty, 0);

                    // load keys and key if exist
                    if (keys != null)
                    {
                        // this will load main account key for signing transactions
                        var key = keys.FirstOrDefault(k => k.Id == a.AccountKeyId);
                        if (key != null)
                            acc.AccountKey = key.Fill(new EncryptionKey(""));

                        // fill other account keys - messages, etc.
                        foreach(var k in keys)
                        {
                            acc.AccountKeys.Add(k.Fill(new EncryptionKey("")));
                        }
                    }

                    // load bookmarks
                    if (bookmarks != null)
                    {
                        // fill account bookmarks
                        foreach (var b in bookmarks)
                        {
                            acc.Bookmarks.Add(b.Fill(BookmarkFactory.GetBookmark((BookmarkTypes)b.Type, new Guid(b.Id), b.Name, b.Address)));
                        }
                    }

                    accounts.Add(a.Fill(acc));
                }

                return accounts;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get accounts list from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function returns all Nodes stored in database
        /// </summary>
        /// <returns>List of INode object created based on specified type</returns>
        public List<INode> GetNodes()
        {
            try
            {
                ////using var context = new DbEconomyContext();

                var nodes = new List<INode>();
                NodeActionParameters parameters = new NodeActionParameters();

                foreach (var n in context.Nodes.Where(n => !n.Deleted))
                {
                    if (n != null)
                    {
                        if (!string.IsNullOrEmpty(n.Parameters))
                        {
                            try
                            {
                                parameters = JsonConvert.DeserializeObject<NodeActionParameters>(n.Parameters);
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Cannot get Node Parameters from Db. Continue with empty parameters", ex);
                            }
                        }
                    }

                    var actid = string.Empty;
                    if (!string.IsNullOrEmpty(n.AccountId))
                    {
                        actid = n.AccountId;
                    }
                    else
                    {
                        actid = Guid.Empty.ToString();
                    }

                    var node = n.Fill(NodeFactory.GetNode((NodeTypes)n.Type, new Guid(n.Id), new Guid(actid), n.Name, (bool)n.IsActivated, parameters));
                    node.SetNodeTriggerType(parameters.TriggerType);

                    nodes.Add(node);
                }

                return nodes;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Nodes from Db. Continue with empty list!", ex);
                return null;
            }
        }
        /// <summary>
        /// Function returns all Keys stored in database
        /// </summary>
        /// <returns>List of EncryptionKey object created based on specified type</returns>
        public List<EncryptionKey> GetKeys()
        {
            try
            {
                var keys = new List<EncryptionKey>();

                foreach (var k in context.Keys.Where(n => !n.Deleted))
                {
                    if (k != null)
                    {
                        var key = k.Fill(new EncryptionKey(k.StoredKey));
                        keys.Add(key);
                    }
                }

                return keys;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get keys list from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function returns all Bookmarks stored in database
        /// </summary>
        /// <returns>List of IBookmark object created based on specified type</returns>
        public List<IBookmark> GetBookmarks()
        {
            try
            {
                var bookmarks = new List<IBookmark>();

                foreach (var bkm in context.Bookmarks.Where(b => !b.Deleted))
                {
                    if (bkm != null)
                    {
                        var bookmark = bkm.Fill(BookmarkFactory.GetBookmark((BookmarkTypes)bkm.Type, new Guid(bkm.RelatedItemId), bkm.Name, bkm.Address));
                        bookmarks.Add(bookmark);
                    }
                }

                return bookmarks;
            }
            catch (Exception ex)
            {
                log.Error("Cannot get bookmarks list from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function returns Wallet stored in database based on id
        /// </summary>
        /// <returns>IWallet object created based on specified type</returns>
        public IWallet GetWallet(Guid id)
        {
            try
            {
                //using var context = new DbEconomyContext();
                var wallet = context.Wallets
                    .Where(w => !w.Deleted)
                    .Where(w => w.Id == id.ToString())
                    .FirstOrDefault();

                return wallet.Fill(WalletFactory.GetWallet(new Guid(), new Guid(), (WalletTypes)wallet.Type, string.Empty, false, string.Empty, 0));
            }
            catch (Exception ex)
            {
                log.Error("Cannot get wallet from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function save Wallet to the database
        /// </summary>
        /// <returns>true when success</returns>
        public bool SaveWallet(IWallet wallet)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var wall = context.Wallets
                    .Where(d => !d.Deleted)
                    .Where(d => d.Id == wallet.Id.ToString())
                    .FirstOrDefault();

                    if (wall != null)
                    {
                        wall.Update(wallet);

                        wall.ModifiedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        var w = new Models.Wallet();
                        w.Update(wallet);
                        w.CreatedOn = DateTime.UtcNow;
                        w.CreatedBy = "admin";
                        w.ModifiedBy = "admin";
                        context.Wallets.Add(w);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot save Wallet to Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will set specified Wallet as deleted
        /// </summary>
        /// <param name="walletId"></param>
        /// /// <param name="withAccounts">if true, function will set as deleted all related accounts</param>
        /// <returns></returns>
        public bool DeleteWallet(string walletId, bool withAccounts = false)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var w = context.Wallets
                    .Where(w => !w.Deleted)
                    .Where(w => w.Id == walletId)
                    .FirstOrDefault();

                    if (w != null)
                    {
                        w.Deleted = true;
                        context.Wallets.Update(w);
                    }

                    if (withAccounts)
                    {
                        var acs = context.Accounts
                            .Where(a => !a.Deleted)
                            .Where(a => a.WalletId == walletId)
                            .ToList();

                        foreach (var a in acs)
                        {
                            a.Deleted = true;
                            context.Accounts.Update(a);
                        }
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot set Wallet as deleted in Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will remove specified wallet from database
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="withAccounts">if true, function will remove all related accounts</param>
        /// <returns></returns>
        public bool RemoveWallet(string walletId, bool withAccounts = false)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var w = context.Wallets
                    .Where(w => w.Id == walletId)
                    .FirstOrDefault();

                    if (w != null)
                    {
                        context.Wallets.Remove(w);
                    }

                    if (withAccounts)
                    {
                        var acs = context.Accounts
                            .Where(a => !a.Deleted)
                            .Where(a => a.WalletId == walletId)
                            .ToList();

                        foreach (var a in acs)
                        {
                            context.Accounts.Remove(a);
                        }
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Wallet from Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function returns Account stored in database based on id
        /// </summary>
        /// <returns>IAccount object created based on specified type</returns>
        public IAccount GetAccount(Guid id)
        {
            try
            {
                //using var context = new DbEconomyContext();
                var a = context.Accounts
                    .Where(a => !a.Deleted)
                    .Where(a => a.Id == id.ToString())
                    .FirstOrDefault();

                return a.Fill(AccountFactory.GetAccount(new Guid(a.Id), (AccountTypes)a.Type, Guid.Empty, new Guid(a.WalletId), string.Empty, string.Empty, 0));
            }
            catch (Exception ex)
            {
                log.Error("Cannot get account from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function save Account to the database
        /// </summary>
        /// <returns>true when success</returns>
        public bool SaveAccount(IAccount account)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var acc = context.Accounts
                    .Where(d => !d.Deleted)
                    .Where(d => d.Id == account.Id.ToString())
                    .FirstOrDefault();

                    if (acc != null)
                    {
                        acc.Update(account);
                    }
                    else
                    {
                        var a = new Models.Account();
                        a.Update(account);
                        a.CreatedOn = DateTime.UtcNow;
                        a.CreatedBy = "admin";
                        a.ModifiedBy = "admin";
                        context.Accounts.Add(a);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot save Account to Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will set specified account as deleted
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool DeleteAccount(string accountId)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var a = context.Accounts
                    .Where(n => !n.Deleted)
                    .Where(n => n.Id == accountId)
                    .FirstOrDefault();

                    if (a != null)
                    {
                        a.Deleted = true;
                        context.Accounts.Update(a);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot set Account as deleted in Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will remove specified account from database
        /// </summary>
        /// <param name="accountId">Id of account</param>
        /// <returns></returns>
        public bool RemoveAccount(string accountId)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var a = context.Accounts
                    .Where(a => a.Id == accountId)
                    .FirstOrDefault();

                    if (a != null)
                    {
                        context.Accounts.Remove(a);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Account from Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function returns Node stored in database based on id
        /// </summary>
        /// <returns>INode object created based on specified type</returns>
        public INode GetNode(Guid id)
        {
            try
            {
                //using var context = new DbEconomyContext();
                var node = context.Nodes
                    .Where(n => !n.Deleted)
                    .Where(n => n.Id == id.ToString())
                    .FirstOrDefault();

                NodeActionParameters parameters = new NodeActionParameters();

                if (node != null)
                {
                    if (!string.IsNullOrEmpty(node.Parameters))
                    {
                        try
                        {
                            parameters = JsonConvert.DeserializeObject<NodeActionParameters>(node.Parameters);
                        }
                        catch(Exception ex)
                        {
                            log.Error("Cannot get Node Parameters from Db. Continue with empty parameters", ex);
                        }
                    }
                }

                return node.Fill(NodeFactory.GetNode((NodeTypes)node.Type, new Guid(node.Id), new Guid(node.AccountId), node.Name, (bool)node.IsActivated, parameters));
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Node from Db", ex);
                return null;
            }
        }

        /// <summary>
        /// Function save Node to the database
        /// </summary>
        /// <returns>true when success</returns>
        public bool SaveNode(INode node)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var nd = context.Nodes
                    .Where(n => !n.Deleted)
                    .Where(n => n.Id == node.Id.ToString())
                    .FirstOrDefault();

                    if (nd != null)
                    {
                        nd.Update(node);
                    }
                    else
                    {
                        var n = new Models.Node();
                        n.Update(node);
                        n.CreatedOn = DateTime.UtcNow;
                        n.CreatedBy = "admin";
                        n.ModifiedBy = "admin";
                        context.Nodes.Add(n);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot save Node to Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will set specified node as deleted
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool DeleteNode(string nodeId)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var n = context.Nodes
                    .Where(n => !n.Deleted)
                    .Where(n => n.Id == nodeId)
                    .FirstOrDefault();

                    if (n != null)
                    {
                        n.Deleted = true;
                        context.Nodes.Update(n);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot set Node as deleted", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will remove specified node from database
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public bool RemoveNode(string nodeId)
        {
            try
            {
                //using var context = new DbEconomyContext();

                using (var transaction = context.Database.BeginTransaction())
                {
                    var n = context.Nodes
                    .Where(n => n.Id == nodeId)
                    .FirstOrDefault();

                    if (n != null)
                    {
                        context.Nodes.Remove(n);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Node from Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function returns key stored in database based on id
        /// </summary>
        /// <returns>EncryptionKey object created based on specified type</returns>
        public EncryptionKey GetKey(Guid id)
        {
            try
            {
                var key = context.Keys
                    .Where(k => !k.Deleted)
                    .Where(k => k.Id == id.ToString())
                    .FirstOrDefault();

                return key.Fill(new EncryptionKey(key.StoredKey));
            }
            catch (Exception ex)
            {
                log.Error("Cannot get key from Db", ex);
                return null;
            }
        }
        /// <summary>
        /// Function save key to the database
        /// </summary>
        /// <returns>true when success</returns>
        public bool SaveKey(EncryptionKey key)
        {
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var keydb = context.Keys
                    .Where(k => !k.Deleted)
                    .Where(k => k.Id == key.Id.ToString())
                    .FirstOrDefault();

                    if (keydb != null)
                    {
                        keydb.Update(key);
                        keydb.ModifiedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        var k = new Models.Key();
                        k.Update(key);
                        k.CreatedOn = DateTime.UtcNow;
                        k.CreatedBy = "admin";
                        k.ModifiedBy = "admin";
                        context.Keys.Add(k);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot save Key to Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will set specified Key as deleted
        /// </summary>
        /// <returns></returns>
        public bool DeleteKey(string keyId)
        {
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var key = context.Keys
                    .Where(k => !k.Deleted)
                    .Where(k => k.Id == keyId)
                    .FirstOrDefault();

                    if (key != null)
                    {
                        key.Deleted = true;
                        context.Keys.Update(key);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot set Key as deleted in Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will remove specified key from database
        /// </summary>
        /// <returns></returns>
        public bool RemoveKey(string keyId)
        {
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var key = context.Keys
                    .Where(k => k.Id == keyId)
                    .FirstOrDefault();

                    if (key != null)
                    {
                        context.Keys.Remove(key);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Key from Db", ex);
                return false;
            }
        }


        /// <summary>
        /// Function returns Bookmark stored in database based on id
        /// </summary>
        /// <returns>IBookmark object created based on specified type</returns>
        public IBookmark GetBookmark(Guid id)
        {
            try
            {
                var bookmark = context.Bookmarks
                    .Where(b => !b.Deleted)
                    .Where(b => b.Id == id.ToString())
                    .FirstOrDefault();

                return bookmark.Fill(BookmarkFactory.GetBookmark((BookmarkTypes)bookmark.Type, new Guid(bookmark.RelatedItemId), bookmark.Name, bookmark.Address));
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Bookmark from Db", ex);
                return null;
            }
        }
        /// <summary>
        /// Function Bookmark to the database
        /// </summary>
        /// <returns>true when success</returns>
        public bool SaveBookmark(IBookmark bookmark)
        {
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var bkm = context.Bookmarks
                    .Where(b => !b.Deleted)
                    .Where(b => b.Id == bookmark.Id.ToString())
                    .FirstOrDefault();

                    if (bkm != null)
                    {
                        bkm.Update(bookmark);
                        bkm.ModifiedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        var bk = new Models.BookmarkEntity();
                        bk.Update(bookmark);
                        bk.CreatedOn = DateTime.UtcNow;
                        bk.CreatedBy = "admin";
                        bk.ModifiedBy = "admin";
                        context.Bookmarks.Add(bk);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot save Bookmark to Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will set specified Bookmark as deleted
        /// </summary>
        /// <returns></returns>
        public bool DeleteBookmark(string bookmarkId)
        {
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var bookmark = context.Bookmarks
                    .Where(b => !b.Deleted)
                    .Where(b => b.Id == bookmarkId)
                    .FirstOrDefault();

                    if (bookmark != null)
                    {
                        bookmark.Deleted = true;
                        context.Bookmarks.Update(bookmark);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot set Bookmark as deleted in Db", ex);
                return false;
            }
        }

        /// <summary>
        /// Function will remove specified Bookmark from database
        /// </summary>
        /// <returns></returns>
        public bool RemoveBookmark(string bookmarkId)
        {
            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    var bookmark = context.Bookmarks
                    .Where(b => b.Id == bookmarkId)
                    .FirstOrDefault();

                    if (bookmark != null)
                    {
                        context.Bookmarks.Remove(bookmark);
                    }

                    context.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot remove Bookmark from Db", ex);
                return false;
            }
        }
    }
}
