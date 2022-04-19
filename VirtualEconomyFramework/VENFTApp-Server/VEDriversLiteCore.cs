
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Admin.Dto;
using VEDriversLite.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;
using VEDriversLite.Extensions.WooCommerce;

namespace VENFTApp_Server
{
    public class VEDriversLiteCore : BackgroundService
    {
        //private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;

        public VEDriversLiteCore(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            this.settings = settings; //startup configuration in appsettings.json
            this.lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            await Task.Delay(1);
            var neblioAddressForShop = string.Empty;
            var neblioDepositAddressForShop = string.Empty;
            var dogeAddressForShop = string.Empty;
            var dogeDepositAddressForShop = string.Empty;

            MainDataContext.IsAPIWithCredentials = settings.GetValue<bool>("IsAPIWithCredentials", true);
            MainDataContext.LoadAllVENFTOwnersWithAllNFTs = settings.GetValue<bool>("LoadAllVENFTOwnersWithAllNFTs", false);
            var loadWithInWebAssemblyLimits = settings.GetValue<bool>("LoadWithInWebAssemblyLimits", false);

            MainDataContext.IpfsSecret = settings.GetValue<string>("IpfsSecret", string.Empty);
            MainDataContext.IpfsProjectID = settings.GetValue<string>("IpfsProjectID", string.Empty);
            if (!string.IsNullOrEmpty(MainDataContext.IpfsSecret) && !string.IsNullOrEmpty(MainDataContext.IpfsProjectID))
                NFTHelpers.LoadConnectionInfo(MainDataContext.IpfsProjectID, MainDataContext.IpfsSecret);

            var acc = new List<string>();
            settings.GetSection("ObservedAccounts").Bind(acc);
            if (acc != null)
                MainDataContext.ObservedAccounts = acc;

            try
            {
                /*
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keys.json");
                if (!FileHelpers.IsFileExists(path))
                {
                    Console.WriteLine("Missing keys.json file. Cannot continue without at least one root Neblio account.");
                    return;
                }
                var skeys = FileHelpers.ReadTextFromFile(path);
                var keys = JsonConvert.DeserializeObject<List<AccountExportDto>>(skeys);
                */
                var keys = new List<AccountExportDto>();
                settings.GetSection("keys").Bind(keys);
                if (keys == null || keys.Count == 0)
                {
                    //log.Error("Missing keys in settigns. Cannot continue without at least one root Neblio account.");
                    Console.WriteLine("Missing keys in settigns. Cannot continue without at least one root Neblio account.");
                    return;
                }

                Console.WriteLine("------------------------------------");
                Console.WriteLine("-----------Loading Accounts---------");
                Console.WriteLine("------------------------------------");
                
                if (keys != null)
                {
                    foreach (var k in keys)
                    {
                        if (!string.IsNullOrEmpty(k.Address))
                        {
                            if (!k.IsDogeAccount)
                            {
                                var add = NeblioTransactionHelpers.ValidateNeblioAddress(k.Address);
                                if (!string.IsNullOrEmpty(add))
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine("=========Neblio Main Account========");
                                    Console.WriteLine($"Loading Neblio address {k.Address}...");
                                    if (string.IsNullOrEmpty(k.Name))
                                        k.Name = NeblioTransactionHelpers.ShortenAddress(k.Address);

                                    var account = new NeblioAccount();
                                    EncryptedBackupDto bckp = null;
                                    if (FileHelpers.IsFileExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, k.Address + "-backup.json")))
                                    {
                                        var b = FileHelpers.ReadTextFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, k.Address + "-backup.json"));
                                        if (!string.IsNullOrEmpty(b))
                                        {
                                            bckp = JsonConvert.DeserializeObject<EncryptedBackupDto>(b);
                                            Console.WriteLine($"Backup data found for this account. It will be loaded based on VENFT backup.");
                                        }
                                    }

                                    (bool, string) dbackup = (false, string.Empty);
                                    (bool, string) dpass = (false, string.Empty);
                                    if (await account.LoadAccount(k.Password, k.EKey, k.Address)) // fill your password
                                    {
                                        if (k.ConnectToMainShop && k.IsReceivingAccount)
                                            neblioAddressForShop = k.Address;
                                        else if (k.ConnectToMainShop && k.IsDepositAccount)
                                            neblioDepositAddressForShop = k.Address;

                                        if (bckp != null)
                                        {
                                            var dadd = await ECDSAProvider.DecryptMessage(bckp.eadd, account.Secret);
                                            dbackup = await ECDSAProvider.DecryptMessage(bckp.edata, account.Secret);
                                            dpass = await ECDSAProvider.DecryptMessage(bckp.epass, account.Secret);
                                            if (dpass.Item1 && dadd.Item1 && dadd.Item1)
                                            {
                                                Console.WriteLine($"Loading Neblio address {k.Address} from VENFT Backup...");
                                                if (await account.LoadAccountFromVENFTBackup(dpass.Item2, dbackup.Item2))
                                                    Console.WriteLine($"Neblio address {k.Address} initialized.");
                                                else
                                                    Console.WriteLine($"Cannot load VENFT backup for address {k.Address}.");
                                            }
                                        }

                                        // this is block autostart of the IoT Devices NFTs...add this to appsettings.json   "LoadWithInWebAssemblyLimits": true,
                                        account.RunningAsVENFTBlazorApp = loadWithInWebAssemblyLimits;

                                        VEDLDataContext.Accounts.TryAdd(k.Address, account);
                                        VEDLDataContext.AdminAddresses.Add(k.Address);

                                        if (dbackup.Item1 && dpass.Item1)
                                        {
                                            try
                                            {
                                                var bdto = JsonConvert.DeserializeObject<BackupDataDto>(dbackup.Item2);
                                                if (bdto != null && !string.IsNullOrEmpty(bdto.DogeAddress))
                                                {
                                                    Console.WriteLine($"Backup for main address {k.Address} contains also Dogecoin address {bdto.DogeAddress}. It will be imported.");
                                                    var dogeAccount = new DogeAccount();
                                                    var res = await dogeAccount.LoadAccount(dpass.Item2, bdto.DogeKey, bdto.DogeAddress);
                                                    VEDLDataContext.DogeAccounts.TryAdd(bdto.DogeAddress, dogeAccount);
                                                    Console.WriteLine($"Dogecoin address {bdto.DogeAddress} initialized.");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                //log.Error("Canno init doge account" + ex.Message);
                                                Console.WriteLine("Cannot init doge account" + ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var dadd = DogeTransactionHelpers.ValidateDogeAddress(k.Address);
                                if (dadd.Success)
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine("========Dogecoin Main Account=======");
                                    Console.WriteLine($"Loading Dogecoin address {k.Address}...");
                                    var dogeacc = new DogeAccount();
                                    if (await dogeacc.LoadAccount(k.Password, k.EKey, k.Address))
                                        VEDLDataContext.DogeAccounts.TryAdd(dogeacc.Address, dogeacc);
                                    Console.WriteLine($"Dogecoin address {k.Address} initialized.");
                                    if (k.ConnectToMainShop && k.IsReceivingAccount)
                                        dogeAddressForShop = k.Address;
                                    else if (k.ConnectToMainShop && k.IsDepositAccount)
                                        dogeDepositAddressForShop = k.Address;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //log.Error("Canno load the keys. " + ex.Message);
                Console.WriteLine("Cannot load the keys. " + ex.Message);
            }

            await Task.Delay(5000);

            Console.WriteLine("------------------------------------");
            Console.WriteLine("Loading NFT Hashes...");
            await AccountHandler.ReloadNFTHashes();
            Console.WriteLine($"Count of NFTHashes: {VEDLDataContext.NFTHashs.Count}.");

            Console.WriteLine("");  
            Console.WriteLine("------------------------------------");
            Console.WriteLine("---------WooCommerce Shop-----------");
            Console.WriteLine("------------------------------------");

            try
            {
                VEDLDataContext.WooCommerceStoreUrl = settings.GetValue<string>("WooCommerceStoreUrl");
                VEDLDataContext.WooCommerceStoreAPIKey = settings.GetValue<string>("WooCommerceStoreAPIKey");
                VEDLDataContext.WooCommerceStoreSecret = settings.GetValue<string>("WooCommerceStoreSecret");
                VEDLDataContext.WooCommerceStoreJWTToken = settings.GetValue<string>("WooCommerceStoreJWT");
                VEDLDataContext.WooCommerceStoreCheckoutFieldCustomerNeblioAddress = settings.GetValue<string>("WooCommerceStoreCheckoutFieldCustomerNeblioAddress");
                VEDLDataContext.WooCommerceStoreSendDogeToAuthor = settings.GetValue<bool>("WooCommerceStoreSendDogeToAuthor", false);
                VEDLDataContext.AllowDispatchNFTOrders = settings.GetValue<bool>("AllowDispatchNFTOrders", false);
                try
                {
                    settings.GetSection("DepositSchemes").Bind(VEDLDataContext.DepositSchemes);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot load the deposit schemes. " + ex.Message);
                }

                if (VEDLDataContext.DepositSchemes.Count == 0)
                {
                    Console.WriteLine("!!!Cannot load the deposit schemes!!!");
                }

                if (!string.IsNullOrEmpty(VEDLDataContext.WooCommerceStoreUrl) &&
                    !string.IsNullOrEmpty(VEDLDataContext.WooCommerceStoreAPIKey) &&
                    !string.IsNullOrEmpty(VEDLDataContext.WooCommerceStoreSecret))
                {
                    Console.WriteLine("Initializing WooCommerce Shop...");
                    Console.WriteLine("API Url: " + VEDLDataContext.WooCommerceStoreUrl);
                    if (await WooCommerceHelpers.InitStoreApiConnection(VEDLDataContext.WooCommerceStoreUrl,
                                                                    VEDLDataContext.WooCommerceStoreAPIKey,
                                                                    VEDLDataContext.WooCommerceStoreSecret,
                                                                    VEDLDataContext.WooCommerceStoreJWTToken, true))
                    {
                        Console.WriteLine("WooCommerce Shop Initialized correctly.");
                        Console.WriteLine($"- Number of Products: {WooCommerceHelpers.Shop.Products.Count}");
                        Console.WriteLine($"- Number of Orders: {WooCommerceHelpers.Shop.Orders.Count}");
                        Console.WriteLine("------------------------------------");
                        if (!string.IsNullOrEmpty(dogeAddressForShop))
                        {
                            await WooCommerceHelpers.Shop.ConnectDogeAccount(dogeAddressForShop);
                            Console.WriteLine($"Connecting Dogecoin address {dogeAddressForShop} to the WooCommerce Shop main input account.");
                        }
                        if (!string.IsNullOrEmpty(dogeDepositAddressForShop) && dogeAddressForShop != dogeDepositAddressForShop)
                        {
                            WooCommerceHelpers.Shop.ConnectedDepositDogeAccountAddress = dogeDepositAddressForShop;
                            Console.WriteLine($"Connecting Dogecoin address {dogeDepositAddressForShop} to the WooCommerce Shop as deposit account.");
                        }
                        if (!string.IsNullOrEmpty(dogeDepositAddressForShop) && !string.IsNullOrEmpty(dogeAddressForShop) && dogeAddressForShop == dogeDepositAddressForShop)
                            Console.WriteLine($"Input doge address and deposit address cannot be same.");

                        if (string.IsNullOrEmpty(WooCommerceHelpers.Shop.ConnectedDogeAccountAddress))
                        {
                            if (!string.IsNullOrEmpty(neblioAddressForShop))
                            {
                                await WooCommerceHelpers.Shop.ConnectNeblioAccount(neblioAddressForShop);
                                Console.WriteLine($"Connecting Neblio address {neblioAddressForShop} to the WooCommerce Shop main input account.");
                            }
                            if (!string.IsNullOrEmpty(neblioDepositAddressForShop) && neblioAddressForShop != dogeDepositAddressForShop)
                            {
                                await WooCommerceHelpers.Shop.ConnectDepositNeblioAccount(neblioDepositAddressForShop);
                                Console.WriteLine($"Connecting Neblio address {neblioDepositAddressForShop} to the WooCommerce Shop as deposit account.");
                            }
                            if (!string.IsNullOrEmpty(neblioDepositAddressForShop) && !string.IsNullOrEmpty(neblioAddressForShop) && neblioAddressForShop == neblioDepositAddressForShop)
                                Console.WriteLine($"Input Neblio address and deposit address cannot be same.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Cannot init the WooCommerce API.");
                        //log.Warn("Canno init WooCommerce API.");
                    }
                }
            }
            catch(Exception ex)
            {
                //log.Error("Canno init WooCommerce API. " + ex.Message);
                Console.WriteLine("Cannot init the WooCommerce API." + ex.Message);
            }

            Console.WriteLine("------------------------------------");
            Console.WriteLine("--------Starting main cycle---------");
            Console.WriteLine("------------------------------------");
            try
            {
                var VENFTOwnersRefreshDefault = 3600;
                var VENFTOwnersRefresh = 100;
                var NFTHashsRefreshDefault = 100;
                var NFTHashsRefresh = 10;

                
                _ = Task.Run(async () =>
                {
                    Console.WriteLine("Loading Observing addresses...");
                    // load observed addresses
                    if (MainDataContext.ObservedAccounts.Count > 0)
                    {
                        for (var i = 0; i < MainDataContext.ObservedAccounts.Count; i++)
                        {
                            try
                            {
                                var tab = new VEDriversLite.Bookmarks.ActiveTab(MainDataContext.ObservedAccounts[i]);
                                tab.MaxLoadedNFTItems = 1500;
                                await tab.StartRefreshing(10000, false, true);
                                MainDataContext.ObservedAccountsTabs.TryAdd(tab.Address, tab);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot start observing of account: " + MainDataContext.ObservedAccounts[i]);
                            }
                        }
                    }

                    Console.WriteLine("Running...");
                    while (!stopToken.IsCancellationRequested)
                    {
                        
                        await Task.Delay(1000);
                        /*
                        if (VENFTOwnersRefresh <= 0)
                        {
                            try
                            {
                                var owns = await NeblioTransactionHelpers.GetTokenOwners(NFTHelpers.TokenId);
                                if (owns != null)
                                {
                                    var ow = new Dictionary<string, TokenOwnerDto>();
                                    foreach (var o in owns)
                                        ow.Add(o.Address, o);
                                    MainDataContext.VENFTTokenOwners = ow;
                                    VENFTOwnersRefresh = VENFTOwnersRefreshDefault;
                                }
                            }
                            catch(Exception ex)
                            {
                                log.Error("Cannot refresh VENFT Owners. " + ex.Message);
                                Console.WriteLine("Cannot refresh VENFT Owners");
                            }
                        }
                        else
                        {
                            VENFTOwnersRefresh--;
                        }*/

                        try
                        {
                            if (NFTHashsRefresh <= 0)
                            {
                                if (await AccountHandler.ReloadNFTHashes())
                                    NFTHashsRefresh = NFTHashsRefreshDefault;
                            }
                            else
                            {
                                NFTHashsRefresh--;
                            }
                        }
                        catch(Exception ex)
                        {
                            //log.Error("Cannot reload the NFT Hashes. " + ex.Message);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                //log.Error("Fatal error in main loop. " + ex.Message);
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
