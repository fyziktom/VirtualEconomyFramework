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
    EditXrayParams
}
public class MintingToolbarActionDto
{
    public MintingToolbarActionType Type { get; set; }
    public string[]? Args { get; set; }
}
public class AppData
{
    protected readonly ILocalStorageService localStorage;

    public AppData(ILocalStorageService LocalStorage)
    {
        localStorage = LocalStorage;
    }

    public string AppName { get; set; } = "VEBlazorApp";
    public string AppNick { get; set; } = "VEBA";
    public string AppHomeWebsiteUrl { get; set; } = "https://veframework.com/";
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
    public bool IsAccountLoaded { get; set; } = false;
    public Dictionary<string, XrayExposureParameters> ExposureParametersTemplates { get; set; } = new Dictionary<string, XrayExposureParameters>();
    public Dictionary<string, DetectorDataDto> DetectorParametersTemplates { get; set; } = new Dictionary<string, DetectorDataDto>();

    public List<GalleryTab> OpenedTabs { get; set; } = new List<GalleryTab>();
    public Dictionary<string, MintingTabData> MintingTabsData { get; set; } = new Dictionary<string, MintingTabData>()
    {
        {"default", new MintingTabData() }
    };

    public event EventHandler<bool> LockUnlockAccount;

    public async Task<(bool,string)> UnlockAccount(string password)
    {
        var ekey = await localStorage.GetItemAsync<string>("key");
        if (string.IsNullOrEmpty(ekey))
            return (false, string.Empty);

        Account.RunningAsVENFTBlazorApp = true; // block the start of the IoT NFTs, etc.
        VEDLDataContext.AllowCache = true; //turn on/off NFT cache

        await LoadChache();
        var address = string.Empty;
        if (await Account.LoadAccount(password, ekey, "", false))
        {
            address = Account.Address;
            IsAccountLoaded = true;

            await LoadBookmarks();
            await SaveCache();
        }
        else
        {
            IsAccountLoaded = false;
            Console.WriteLine("Cannot unlock the account.");
        }

        LockUnlockAccount?.Invoke(null, IsAccountLoaded);
        
        return (IsAccountLoaded,address);
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

