using System;
using System.Collections.Generic;
using System.Linq;
using VEDriversLite;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.NFT.Imaging.Xray.Dto;
using VEDriversLite.Bookmarks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using VEDriversLite.AI.OpenAI;
using VEDriversLite.Security;
using System.Text;
using VEFramework.VEBlazor.Models;
using IndexedDB.Blazor;
using VEDriversLite.Dto;
using Newtonsoft.Json;

public enum TabType
{
    Main,
    WorkTab,
    ActiveTab
}

public class WorkTab
{
    public string Name { get; set; } = "Empty WT";
    public List<INFT> NFTs { get; set; } = new List<INFT>();
}

public class GalleryTab
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public TabType Type { get; set; } = TabType.WorkTab;
    public object? Tab { get; set; }
    public bool IsActive { get; set; } = false;
}
public enum MintingToolbarActionType
{
    None,
    PreviousStep,
    NextStep,
    LoadFromTemplate,
    Save,
    Finish,
    Mint,
    Cancel,
    ClearAll,
    ClearActualForm,
    Share,
    Send,
    SendCopy,
    Delete,
    AddMarker,
    EditProps,
    EditXrayParams,
}
public class MintingToolbarActionDto
{
    public MintingToolbarActionType Type { get; set; }
    public string[]? Args { get; set; }
}
public class AppData
{
    protected readonly ILocalStorageService localStorage;
    protected readonly IIndexedDbFactory DbFactory;

    public AppData(ILocalStorageService LocalStorage, IIndexedDbFactory dbFactory)
    {
        localStorage = LocalStorage;
        DbFactory = dbFactory;
    }

    public const string NeblioImageLink = "https://ve-framework.com/ipfs/QmPUvBN4qKvGyKKhADBJKSmNC7JGnr3Rwf5ndENGMfpX54";
    public const string DogecoinImageLink = "https://ve-framework.com/ipfs/QmRp3eyUeqctcgBFcRuBa7uRWiABTXmLBeYuhLp8xLX1sy";
    public const string VENFTImageLink = "https://ve-framework.com/ipfs/QmZSdjuLTihuPzVwUKaHLtivw1HYhsyCdQFnVLLCjWoVBk";
    public const string BDPImageLink = "https://ve-framework.com/ipfs/QmYMVuotTTpW24eJftpbUFgK7Ln8B4ox3ydbKCB6gaVwVB";
    public const string WDOGEImageLink = "https://ve-framework.com/ipfs/Qmc9xS9a8TnWmU7AN4dtsbu4vU6hpEXpMNAeUdshFfg1wT";
    
    public bool Development { get; set; } = false;
    public static string AppName { get; set; } = "VEBlazorApp";
    public static string AppNick { get; set; } = "VEBA";
    public static string AppTokenId { get; set; } = NFTHelpers.BDPTokenId;
    public static string AppHomeWebsiteUrl { get; set; } = "https://veframework.com/";
    private string _appshareNFTUrl = "https://veframework.com/";
    public string AppShareNFTUrl 
    {
        get => _appshareNFTUrl.Trim('/');
        set => _appshareNFTUrl = value;
    }
    public bool AllowWorkTabs { get; set; } = true;
    public bool AllowDestroy { get; set; } = true;
    public bool AllowSend { get; set; } = true;
    public bool AllowSell { get; set; } = false;
    public bool DisplayGettingStartedMenuItem { get; set; } = false;
    public string GettingStartedPageName { get; set; } = "gettingstarted";
    public List<NFTTypes> AllowedNFTTypes { get; set; } = new List<NFTTypes>()
    {
        NFTTypes.Image,
        NFTTypes.Post,
        NFTTypes.Message,
        NFTTypes.Profile,
        NFTTypes.Message,
        NFTTypes.IoTDevice,
        NFTTypes.IoTMessage,
        NFTTypes.Order,
        NFTTypes.Invoice,
        NFTTypes.Payment,
        NFTTypes.Receipt,
        NFTTypes.Event,
        NFTTypes.Ticket,
        NFTTypes.Xray,
        NFTTypes.XrayImage
    };
    public List<NFTTypes> RestrictedInGalleryNFTTypes { get; set; } = new List<NFTTypes>()
    {
        NFTTypes.Profile,
        NFTTypes.Message,
        NFTTypes.IoTDevice,
        NFTTypes.IoTMessage,
        NFTTypes.Order,
        NFTTypes.Invoice,
        NFTTypes.Payment,
        NFTTypes.Receipt,
    };

    public NeblioAccount Account { get; set; } = new NeblioAccount();
    public DogeAccount DogeAccount { get; set; } = new DogeAccount();
    public VirtualAssistant? Assistant { get; set; } = null;
    public bool IsAccountLoaded { get; set; } = false;
    public List<GalleryTab> OpenedTabs { get; set; } = new List<GalleryTab>();
    public Dictionary<string, VEDriversLite.NFT.Tags.Tag> DefaultTags { get; set; } = new Dictionary<string, VEDriversLite.NFT.Tags.Tag>();
    public Dictionary<string, MintingTabData> MintingTabsData { get; set; } = new Dictionary<string, MintingTabData>()
    {
        {"default", new MintingTabData() }
    };

    public event EventHandler<bool> LockUnlockAccount;

    public event EventHandler<bool> AccountLoadedOrImported;
    
    public void AccountLoadedOrImportedEcho()
    {
        AccountLoadedOrImported.Invoke(null, true);
    }

    public async Task<bool> DoesAccountExist()
    {
        var res = await GetAccountInfoFromDb();
        if (res.Item1)
            return true;
        else
        {
            var ekey = await localStorage.GetItemAsync<string>("key");
            if (!string.IsNullOrEmpty(ekey))
                return true;
        }

        return false;
    }

    public async Task<(bool,string)> UnlockAccount(string password, bool withoutNFTs = false)
    {
        var ekey = string.Empty;
        var address = string.Empty;
        var migrateToDb = false;
        var res = await GetAccountInfoFromDb();
        if (res.Item1)
        {
            address = res.Item2.Item1;
            ekey = res.Item2.Item2;
            var m = SymetricProvider.ContainsIV(ekey);
            if (!m)
                migrateToDb = true;
        }
        else
        {
            // for the case that someone was not migrated yet
            ekey = await localStorage.GetItemAsync<string>("key");
            address = await localStorage.GetItemAsync<string>("address");
            migrateToDb = true;
        }

        if (string.IsNullOrEmpty(ekey))
            return (false, string.Empty);

        Account.RunningAsVENFTBlazorApp = true; // block the start of the IoT NFTs, etc.
        VEDLDataContext.AllowCache = true; //turn on/off NFT cache

        await LoadChache();
        if (await Account.LoadAccount(password, ekey, "", withoutNFTs))
        {
            address = Account.Address;
                        
            if (migrateToDb)
            {
                await Console.Out.WriteLineAsync("Migrating the keys to IndexedDb");
                var migres = await MigrateToIndexedDb(address, Account.Secret.ToString(), password);
                if (migres)
                {
                    migrateToDb = false;
                    await Console.Out.WriteLineAsync("Migration was successful.");
                }
            }

            var migSA = false;
            var migSuc = false;
            if (await localStorage.ContainKeyAsync("subAccounts"))
                migSA = true;
            else
                migSuc = true;

            if (migSA)
                if (await MigrateSubAccountsToDb())
                    migSuc = true;

            if (migSuc)
            {
                var sas = await GetSubAccountsFromDb();
                if (sas.Item1)
                {
                    var acnts = JsonConvert.SerializeObject(sas.Item2);
                    if (!string.IsNullOrEmpty(acnts))
                    {
                        await Account.LoadSubAccounts(acnts);
                        //if (migSA)
                        //  if (await localStorage.ContainKeyAsync("subAccounts"))
                        //     await localStorage.RemoveItemAsync("subAccounts");
                    }
                }
            }

            await LoadBookmarks();

            await SaveCache();
            // try init assistant if there is stored OpenAI api key in the browser local memory
            await InitAssistant();

            IsAccountLoaded = true;
        }
        else
        {
            IsAccountLoaded = false;
            Console.WriteLine("Cannot unlock the account.");
        }

        LockUnlockAccount?.Invoke(null, IsAccountLoaded);
        
        return (IsAccountLoaded,address);
    }

    /// <summary>
    /// Returns Account Address and Encrypted Key if exists in the Db
    /// </summary>
    /// <returns>True if account exists in the Db. First returned string is Address, second is the Key</returns>
    public async Task<(bool,(string,string))> GetAccountInfoFromDb()
    {
        try
        {
            using (var db = await this.DbFactory.Create<VEFDb>())
            {
                var account = db.Accounts.Single(x => x.Id == 1);
                if (account != null)
                    return (true, (account.Address, account.Key));
            }
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return (false, (string.Empty, string.Empty));
    }
    /// <summary>
    /// Get encrypted OpenAI API Key if exists in the Db
    /// </summary>
    /// <returns>True if exists in the Db. String is the encrypted OpenAI API Key</returns>
    public async Task<(bool, string)> GetOpenAPIKeyFromDb()
    {
        try
        {
            using (var db = await this.DbFactory.Create<VEFDb>())
            {
                var account = db.Accounts.Single(x => x.Id == 1);
                return (true, account.OpenAPIKey);
            }
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return (false, string.Empty);
    }

    /// <summary>
    /// Save account in the Db
    /// </summary>
    /// <param name="address">Address of the account, if not necessary to save fill null</param>
    /// <param name="key">Encrypted key of the account, if not necessary to save fill null</param>
    /// <param name="openAPIKey">Encrypted OpenAI API key, if not necessary to save fill null</param>
    /// <returns></returns>
    public async Task<bool> SaveAccountInfoToDb(string address = null, string key = null, string openAPIKey = null)
    {
        try
        {
            using (var db = await this.DbFactory.Create<VEFDb>())
            {
                var done = false;
                try
                {
                    if (db.Accounts.Count > 0)
                    {
                        var account = db.Accounts.FirstOrDefault();//.Single(x => x.Id == 1);
                        if (account != null)
                        {
                            if (address != null)
                                account.Address = address;
                            if (key != null)
                                account.Key = key;
                            if (openAPIKey != null)
                                account.OpenAPIKey = openAPIKey;
                            done = true;
                        }
                    }
                }
                catch { }

                if (!done)
                {
                    var acc = new AccountInfo();

                    if (address != null)
                        acc.Address = address;
                    if (key != null)
                        acc.Key = key;
                    if (openAPIKey != null)
                        acc.OpenAPIKey = openAPIKey;

                    db.Accounts.Add(acc);
                }

                await db.SaveChanges();
            }
            return true;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return false;
    }

    public async Task<bool> SaveSubAccountToDb(SubAccountInfo subAccount)
    {
        try
        {
            using (var db = await this.DbFactory.Create<VEFDb>())
            {
                var done = false;
                try
                {
                    if (db.Accounts.Count > 0)
                    {
                        var account = db.SubAccounts.Single(x => x.Address == subAccount.Address);
                        if (account != null)
                        {
                            if (subAccount.EKey != null)
                                account.EKey = subAccount.EKey;
                            if (subAccount.Address != null)
                                account.Address = subAccount.Address;
                            if (subAccount.Name != null)
                                account.Name = subAccount.Name;
                            if (subAccount.ESKey != null)
                                account.ESKey = subAccount.ESKey;
                            account.ConnectToMainShop = subAccount.ConnectToMainShop;
                            account.IsDogeAccount = subAccount.IsDogeAccount;
                            account.IsDepositAccount = subAccount.IsDepositAccount;
                            account.IsReceivingAccount = subAccount.IsReceivingAccount;

                            done = true;
                        }
                    }
                }
                catch { }

                if (!done)
                {
                    var account = new SubAccountInfo();

                    if (subAccount.EKey != null)
                        account.EKey = subAccount.EKey;
                    if (subAccount.Address != null)
                        account.Address = subAccount.Address;
                    if (subAccount.Name != null)
                        account.Name = subAccount.Name;
                    if (subAccount.ESKey != null)
                        account.ESKey = subAccount.ESKey;
                    account.ConnectToMainShop = subAccount.ConnectToMainShop;
                    account.IsDogeAccount = subAccount.IsDogeAccount;
                    account.IsDepositAccount = subAccount.IsDepositAccount;
                    account.IsReceivingAccount = subAccount.IsReceivingAccount;

                    db.SubAccounts.Add(account);
                }

                await db.SaveChanges();
            }
            return true;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return false;
    }

    public async Task<bool> SaveSubAccountsToDb(List<SubAccountInfo> subAccounts)
    {
        try
        {
            using (var db = await this.DbFactory.Create<VEFDb>())
            {
                foreach (var subAccount in subAccounts)
                {
                    var done = false;
                    try
                    {
                        if (db.Accounts.Count > 0)
                        {
                            var account = db.SubAccounts.Single(x => x.Address == subAccount.Address);
                            if (account != null)
                            {
                                if (subAccount.EKey != null)
                                    account.EKey = subAccount.EKey;
                                if (subAccount.Address != null)
                                    account.Address = subAccount.Address;
                                if (subAccount.Name != null)
                                    account.Name = subAccount.Name;
                                if (subAccount.ESKey != null)
                                    account.ESKey = subAccount.ESKey;
                                account.ConnectToMainShop = subAccount.ConnectToMainShop;
                                account.IsDogeAccount = subAccount.IsDogeAccount;
                                account.IsDepositAccount = subAccount.IsDepositAccount;
                                account.IsReceivingAccount = subAccount.IsReceivingAccount;

                                done = true;
                            }
                        }
                    }
                    catch { }

                    if (!done)
                    {
                        var account = new SubAccountInfo();

                        if (subAccount.EKey != null)
                            account.EKey = subAccount.EKey;
                        if (subAccount.Address != null)
                            account.Address = subAccount.Address;
                        if (subAccount.Name != null)
                            account.Name = subAccount.Name;
                        if (subAccount.ESKey != null)
                            account.ESKey = subAccount.ESKey;
                        account.ConnectToMainShop = subAccount.ConnectToMainShop;
                        account.IsDogeAccount = subAccount.IsDogeAccount;
                        account.IsDepositAccount = subAccount.IsDepositAccount;
                        account.IsReceivingAccount = subAccount.IsReceivingAccount;

                        db.SubAccounts.Add(account);
                    }
                }

                await db.SaveChanges();
            }
            return true;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Get SubAccounts List if exists in the Db
    /// </summary>
    /// <returns>True if exists in the Db.</returns>
    public async Task<(bool, List<SubAccountInfo>)> GetSubAccountsFromDb()
    {
        try
        {
            using (var db = await this.DbFactory.Create<VEFDb>())
            {
                var accounts = db.SubAccounts.Where(s => !string.IsNullOrEmpty(s.EKey)).ToList();
                return (true, accounts);
            }
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return (false, new List<SubAccountInfo>());
    }

    private async Task<bool> MigrateSubAccountsToDb()
    {
        try
        {
            var sas = await localStorage.GetItemAsync<string>("subAccounts");
            if (!string.IsNullOrEmpty(sas))
            {
                var accnts = JsonConvert.DeserializeObject<List<SubAccountInfo>>(sas);
                foreach (var a in accnts)
                    a.Id = -1;

                if (accnts == null)
                    return false;

                var res = await SaveSubAccountsToDb(accnts);
                return res;
            }
            return true;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Function will migrate the Account info and OpenAI API Key to the IndexedDb. 
    /// It will use new encryption step to use PasswordHash as pass and it uses IV.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="key"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private async Task<bool> MigrateToIndexedDb(string address, string key, string password)
    {
        try
        {
            var openAPIKey = await GetAndDecryptOpenAIApiKey();
            if (openAPIKey == null)
                openAPIKey = string.Empty;

            var ackey = new EncryptionKey();
            ackey.LoadPassword(password);

            var ekeyIV = SymetricProvider.GetIV();
            var ekey = SymetricProvider.EncryptString(ackey.PasswordHashString, key, ekeyIV);
            var keyToStore = SymetricProvider.JoinIVToString(ekey, ekeyIV);

            string? eoaiToStore = null;
            if (!string.IsNullOrEmpty(openAPIKey))
            {
                var oaiIV = SymetricProvider.GetIV();
                var eoai = SymetricProvider.EncryptString(ackey.PasswordHashString, openAPIKey, oaiIV);
                eoaiToStore = SymetricProvider.JoinIVToString(eoai, oaiIV);
            }

            var checkAcc = await GetAccountInfoFromDb();
            if (checkAcc.Item1)
            {
                await SaveAccountInfoToDb(address, keyToStore, eoaiToStore);
                return true;
            }
            else
            {
                using (var db = await this.DbFactory.Create<VEFDb>())
                {
                    var ai = new AccountInfo()
                    {
                        Address = address,
                        Key = keyToStore
                    };

                    if (eoaiToStore != null)
                        ai.OpenAPIKey = eoaiToStore;

                    db.Accounts.Add(ai);
                    await db.SaveChanges();

                    var mresCheck = await GetAccountInfoFromDb();
                    if (mresCheck.Item1)
                    {
                        //await localStorage.RemoveItemAsync("key");
                        //await localStorage.RemoveItemAsync("address");
                    }

                    return true;
                }
            }
        }
        catch(Exception ex)
        {
            await Console.Out.WriteLineAsync("Cannot save the record to the Db. Ex: " + ex.Message);
        }
        return false;
    }

    /// <summary>
    /// Initialize OpenAI assistant. If you will provide apikey it will take it and save into localStorage in browser.
    /// If you will not provide apikey it will try to load it from the localStorage in browser.
    /// work just with the loaded account because account private key is used to encrypt the apikey
    /// </summary>
    /// <param name="apikey"></param>
    /// <returns></returns>
    public async Task<bool> InitAssistant(string apikey = "")
    {
        if (!IsAccountLoaded)
            return false;

        if (string.IsNullOrEmpty(apikey))
            apikey = await GetAndDecryptOpenAIApiKey();
        
        if (!string.IsNullOrEmpty(apikey))
        {
            Assistant = new VirtualAssistant(apikey);
            if ((await Assistant.InitAssistant()).Item1)
            {
                if (await EncryptAndStoreOpenAIApiKey(apikey))
                    return true;
            }
            else
                Assistant = null;
        }

        return false;
    }


    public async Task<string?> GetOpenAIApiKey()
    {
        var accInfo = await GetOpenAPIKeyFromDb();
        if (accInfo.Item1)
        {
            //await localStorage.RemoveItemAsync("OpenAIapiKey");
            return accInfo.Item2;
        }
        else
        {
            var OAIapikey = await localStorage.GetItemAsync<string>("OpenAIapiKey");
            if (!string.IsNullOrEmpty(OAIapikey))
                return OAIapikey;
        }
        return null;
    }
    public async Task<bool> SaveOpenAIApiKey(string key)
    {
        return await SaveAccountInfoToDb(null, null, key);
    }

    public async Task<string?> GetAndDecryptOpenAIApiKey()
    {
        if (!IsAccountLoaded)
            return null;

        var OAIapikey = await GetOpenAIApiKey();
        if (string.IsNullOrEmpty(OAIapikey))
            return null;
        else
        {
            var done = false;
            if (SymetricProvider.ContainsIV(OAIapikey))
            {
                try
                {
                    var parse = SymetricProvider.ParseIVFromString(OAIapikey);
                    var res = SymetricProvider.DecryptString(Account.AccountKey.PasswordHashString, parse.etext, parse.iv);
                    if (!string.IsNullOrEmpty(res))
                    {
                        done = true;
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("Cannot decrypt OpenAPIKey with pass hash, probably older format of encrypted key.");
                }
            }

            if (!done)
            {
                try
                {
                    if (OAIapikey != string.Empty)
                    {
                        var res = SymetricProvider.DecryptString(Account.Secret.ToString(), OAIapikey);
                        if (!string.IsNullOrEmpty(res))
                            return res;
                    }
                }
                catch { }
            }
        }

        return null;
    }

    public async Task<bool> EncryptAndStoreOpenAIApiKey(string apikey = "")
    {
        if (!IsAccountLoaded)
            return false;

        if (!string.IsNullOrEmpty(apikey))
        {
            var iv = SymetricProvider.GetIV();
            var res = SymetricProvider.EncryptString(Account.AccountKey.PasswordHashString, apikey, iv);
            var store = SymetricProvider.JoinIVToString(res, iv);
            if (!string.IsNullOrEmpty(store))
                return await SaveOpenAIApiKey(store);
        }

        return false;
    }

    public bool LockAccount(bool clearAll = false)
    {
        Account.AccountKey.Lock();
        IsAccountLoaded = false;
        if (clearAll)
        {
            Account = new NeblioAccount();
            OpenedTabs = new List<GalleryTab>();
        }
        LockUnlockAccount?.Invoke(null, IsAccountLoaded);
        return true;
    }

    public async Task LoadBookmarks()
    {
        try
        {
            if (Account.Bookmarks.Count == 0)
            {
                var bookmarks = await localStorage.GetItemAsync<string>("bookmarks");
                if (bookmarks == null || (bookmarks != null && (bookmarks == "" || bookmarks == "{}" || bookmarks == "[]")))
                {
                    bookmarks = string.Empty;
                    return;
                }
                else
                {
                    await Account.LoadBookmarks(bookmarks);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot load bookmarks!" + ex.Message);
        }
    }

    public async Task StoreBookmarks()
    {
        try
        {
            if (Account.Bookmarks.Count == 0)
            {
                var bks = Account.SerializeBookmarks();
                if (!string.IsNullOrEmpty(bks))
                    await localStorage.SetItemAsStringAsync("bookmarks", bks);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot store bookmarks!" + ex.Message);
        }
    }

    public async Task<(bool,string)> AddBookmark(Bookmark bkm)
    {
        var res = (false, string.Empty);
        try
        {
            if (!string.IsNullOrEmpty(bkm.Name) && !string.IsNullOrEmpty(bkm.Address))
            {
                if (string.IsNullOrEmpty(bkm.Note))
                    bkm.Note = string.Empty;
                
                res = await Account.AddBookmark(bkm.Name, bkm.Address, bkm.Note);
                if (res.Item1)
                {
                    await localStorage.SetItemAsStringAsync("bookmarks", res.Item2);
                    return (true, "Bookmark added and stored to memory.");
                }                    
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot add bookmarks!" + ex.Message);
            return (false, ex.Message);
        }
        return res;
    }

    public async Task<(bool,string)> RemoveBookmark(string address)
    {
        var res = (false, string.Empty);
        try
        {
            if (!string.IsNullOrEmpty(address))
            {
                res = await Account.RemoveBookmark(address);
                if (res.Item1)
                {
                    await localStorage.SetItemAsStringAsync("bookmarks", res.Item2);
                    return (true,"Bookmarked removed");
                }                    
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot remove bookmarks!" + ex.Message);
            return (false, ex.Message);            
        }
        return res;
    }    
    
    public async Task<(bool,string)> LoadChache()
    {
        if (VEDLDataContext.AllowCache)
        {
            try
            {
                var cch = await localStorage.GetItemAsync<IDictionary<string, VEDriversLite.NFT.Dto.NFTCacheDto>>("nftcache");
                var resload = Account.LoadCacheNFTsFromString(cch);
                if (!resload)
                {
                    Console.WriteLine("Cannot load data from cache.");
                    await localStorage.SetItemAsStringAsync("nftcache", "");                    
                }
                else
                    return (true, "Data loaded from cache.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load data from cache." + ex.Message);
                await localStorage.SetItemAsStringAsync("nftcache", "");
                return (false, ex.Message);                
            }           
        }
        return (false, string.Empty);        
    }
    public async Task<(bool,string)> SaveCache()
    {
        if (VEDLDataContext.AllowCache)
        {
            // save cached NFTs
            try
            {
                var nftcache = Account.CacheNFTs();
                if (!string.IsNullOrEmpty(nftcache))
                {
                    await localStorage.SetItemAsStringAsync("nftcache", nftcache);
                    return (true, "NFTs cached");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot save data to cache." + ex.Message);
                await localStorage.SetItemAsStringAsync("nftcache", "");
                return (false, ex.Message);
            }
        }
        return (false, string.Empty);
    }

    public async Task<(bool, string)> LoadTags()
    {
        try
        {
            var tags = await localStorage.GetItemAsync<ConcurrentDictionary<string, VEDriversLite.NFT.Tags.Tag>>("tags");
            if (tags == null)
            {
                Console.WriteLine("Cannot load data from cache.");
                await localStorage.SetItemAsStringAsync("tags", "");
            }
            else
            {
                NFTDataContext.Tags = tags;
                return (true, "Data loaded from cache.");
            }                
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot load data from cache." + ex.Message);
            await localStorage.SetItemAsStringAsync("tags", "");
            return (false, ex.Message);
        }
        return (false, string.Empty);
    }
    public async Task<(bool, string)> SaveTags()
    {
        try
        {
            var tags = Newtonsoft.Json.JsonConvert.SerializeObject(NFTDataContext.Tags);
            if (!string.IsNullOrEmpty(tags))
            {
                await localStorage.SetItemAsStringAsync("tags", tags);
                return (true, "NFTs cached");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot save data to cache." + ex.Message);
            await localStorage.SetItemAsStringAsync("tags", "");
            return (false, ex.Message);
        }
        return (false, string.Empty);
    }

    public string GetAddressKnownAsName(string address)
    {
        if (!string.IsNullOrEmpty(address))
        {
            if (address == Account.Address)
                return "My Address";
            if (Account.SubAccounts.TryGetValue(address, out var sacc))
            {
                if (!string.IsNullOrEmpty(sacc.Name))
                    return sacc.Name;
                else
                    return sacc.BookmarkFromAccount.Name;
            }
            var bk = Account.IsInTheBookmarks(address);
            if (bk.Item1)
                return bk.Item2.Name;

            return address;
        }
        return address;
    }

    public INFT GetMintingNFTTabNFT(string name = "default")
    {
        if (MintingTabsData.TryGetValue(name, out var tab))
            return tab.NFT;
        return new ImageNFT("");
    }
    public void SetMintingNFTTabNFT(INFT nft, string name = "default")
    {
        if (MintingTabsData.TryGetValue(name, out var tab))
            tab.NFT = nft;
    }
    public INFT GetMintingNFTTabTemplateNFT(string name = "default")
    {
        if (MintingTabsData.TryGetValue(name, out var tab))
            return tab.NFTTemplate;
        return new ImageNFT("");
    }
    public MintingTabData GetMintingNFTTab(string name = "default")
    {
        if (MintingTabsData.TryGetValue(name, out var tab))
            return tab;
        return new MintingTabData();
    }
}

