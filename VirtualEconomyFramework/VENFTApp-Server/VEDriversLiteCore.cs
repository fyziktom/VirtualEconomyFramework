using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Admin.Dto;
using VEDriversLite.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;
using VEDriversLite.WooCommerce;

namespace VENFTApp_Server
{
    public class VEDriversLiteCore : BackgroundService
    {
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
            var addressForShop = string.Empty;

            MainDataContext.IpfsSecret = settings.GetValue<string>("IpfsSecret", string.Empty);
            MainDataContext.IpfsProjectID = settings.GetValue<string>("IpfsProjectID", string.Empty);
            MainDataContext.IsAPIWithCredentials = settings.GetValue<bool>("IsAPIWithCredentials", false);

            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keys.json");
                if (!FileHelpers.IsFileExists(path))
                {
                    Console.WriteLine("Missing keys.json file. Cannot continue without at least one root Neblio account.");
                    return;
                }
                Console.WriteLine("------------------------------------");
                Console.WriteLine("-----------Loading Accounts---------");
                Console.WriteLine("------------------------------------");
                var skeys = FileHelpers.ReadTextFromFile(path);
                var keys = JsonConvert.DeserializeObject<List<AccountExportDto>>(skeys);
                if (keys != null)
                {
                    foreach (var k in keys)
                    {
                        if (!string.IsNullOrEmpty(k.Address))
                        {
                            if (!k.IsDogeAccount)
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
                                await account.LoadAccount(k.Password, k.EKey, k.Address); // fill your password
                                if (bckp != null)
                                {
                                    var dadd = await ECDSAProvider.DecryptMessage(bckp.eadd, account.Secret);
                                    dbackup = await ECDSAProvider.DecryptMessage(bckp.edata, account.Secret);
                                    dpass = await ECDSAProvider.DecryptMessage(bckp.epass, account.Secret);
                                    if (dpass.Item1 && dadd.Item1 && dadd.Item1)
                                    {
                                        Console.WriteLine($"Loading Neblio address {k.Address} from VENFT Backup...");
                                        if(await account.LoadAccountFromVENFTBackup(dpass.Item2, dbackup.Item2))
                                            Console.WriteLine($"Neblio address {k.Address} initialized.");
                                        else
                                            Console.WriteLine($"Cannot load VENFT backup for address {k.Address}.");
                                    }
                                }
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
                                        Console.WriteLine("Canno init doge account" + ex.Message);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("");
                                Console.WriteLine("========Dogecoin Main Account=======");
                                Console.WriteLine($"Loading Dogecoin address {k.Address}...");
                                var dogeacc = new DogeAccount();
                                if(await dogeacc.LoadAccount(k.Password, k.EKey, k.Address))
                                    VEDLDataContext.DogeAccounts.TryAdd(dogeacc.Address, dogeacc);
                                Console.WriteLine($"Dogecoin address {k.Address} initialized.");
                                if (k.ConnectToMainShop)
                                    addressForShop = k.Address;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load the keys." + ex.Message);
            }

            await Task.Delay(2500);

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

                if (!string.IsNullOrEmpty(VEDLDataContext.WooCommerceStoreUrl) &&
                    !string.IsNullOrEmpty(VEDLDataContext.WooCommerceStoreAPIKey) &&
                    !string.IsNullOrEmpty(VEDLDataContext.WooCommerceStoreSecret))
                {
                    Console.WriteLine("Initializing WooCommerce Shop...");
                    Console.WriteLine("API Url: " + VEDLDataContext.WooCommerceStoreUrl);
                    if (await WooCommerceHelpers.InitStoreApiConnection(VEDLDataContext.WooCommerceStoreUrl,
                                                                    VEDLDataContext.WooCommerceStoreAPIKey,
                                                                    VEDLDataContext.WooCommerceStoreSecret))
                    {
                        Console.WriteLine("WooCommerce Shop Initialized correctly.");
                        Console.WriteLine($"- Number of Products: {WooCommerceHelpers.Shop.Products.Count}");
                        Console.WriteLine($"- Number of Orders: {WooCommerceHelpers.Shop.Orders.Count}");
                        Console.WriteLine("------------------------------------");
                        if (!string.IsNullOrEmpty(addressForShop))
                        {
                            await WooCommerceHelpers.Shop.ConnectDogeAccount(addressForShop);
                            Console.WriteLine($"Connecting Dogecoin address {addressForShop} to the WooCommerce Shop.");
                        }
                    }
                    else
                        Console.WriteLine("Cannot init the WooCommerce API.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot init the WooCommerce API." + ex.Message);
            }

            Console.WriteLine("------------------------------------");
            Console.WriteLine("--------Starting main cycle---------");
            Console.WriteLine("------------------------------------");
            try
            {
                var VENFTOwnersRefreshDefault = 3600;
                var VENFTOwnersRefresh = 100;
                var NFTHashsRefreshDefault = 15000;
                var NFTHashsRefresh = 10;

                _ = Task.Run(async () =>
                {
                    Console.WriteLine("Running...");
                    while (!stopToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000);
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
                                Console.WriteLine("Cannot refresh VENFT Owners");
                            }
                        }
                        else
                        {
                            VENFTOwnersRefresh--;
                        }

                        if (NFTHashsRefresh <= 0)
                        {
                            if(await AccountHandler.ReloadNFTHashes())
                                NFTHashsRefresh = NFTHashsRefreshDefault;
                        }
                        else
                        {
                            NFTHashsRefresh--;
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
