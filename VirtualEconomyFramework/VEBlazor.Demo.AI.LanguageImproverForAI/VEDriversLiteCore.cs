
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
using VEDriversLite.NeblioAPI;
using VEDriversLite.StorageDriver.StorageDrivers;
using VEDriversLite.AI.OpenAI;

namespace VEBlazor.Demo.AI.LanguageImproverForAI
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

            var loadWithInWebAssemblyLimits = settings.GetValue<bool>("LoadWithInWebAssemblyLimits", false);
            var openAIApiKey = settings.GetValue<string>("OpenAIApiKey", "");

            await Console.Out.WriteLineAsync("AI Assistant Inicialization...");

            MainDataContext.Assistant = new VirtualAssistant(openAIApiKey);
            var init = await MainDataContext.Assistant.InitAssistant();
            if (init.Item1)
                await Console.Out.WriteLineAsync("AI Assistant Initialized.");

            VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GatewayURL = "https://ve-framework.com/ipfs/"; 
            var res = await VEDLDataContext.Storage.AddDriver(new VEDriversLite.StorageDriver.StorageDrivers.Dto.StorageDriverConfigDto()
            {
                Type = "IPFS",
                Name = "BDP",
                Location = "Cloud",
                ID = "BDP",
                IsPublicGateway = true,
                IsLocal = false,
                ConnectionParams = new StorageDriverConnectionParams()
                {
                    APIUrl = "https://ve-framework.com/",
                    APIPort = 443,
                    Secured = false,
                    GatewayURL = "https://ve-framework.com/ipfs/",
                    GatewayPort = 443,
                }
            });

            try
            {
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
                            var add = NeblioTransactionHelpers.ValidateNeblioAddress(k.Address);
                            if (!string.IsNullOrEmpty(add))
                            {
                                Console.WriteLine("");
                                Console.WriteLine("=========Neblio Main Account========");
                                Console.WriteLine($"Loading Neblio address {k.Address}...");
                                if (string.IsNullOrEmpty(k.Name))
                                    k.Name = NeblioAPIHelpers.ShortenAddress(k.Address);

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

                                    MainDataContext.MainAccount = k.Address;
                                    VEDLDataContext.Accounts.TryAdd(k.Address, account);
                                    VEDLDataContext.AdminAddresses.Add(k.Address);
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

            Console.WriteLine("------------------------------------");
            Console.WriteLine("--------Starting main cycle---------");
            Console.WriteLine("------------------------------------");
            try
            {
                var NFTHashsRefreshDefault = 100;
                var NFTHashsRefresh = 10;

                _ = Task.Run(async () =>
                {
                    
                    Console.WriteLine("Running...");
                    while (!stopToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000);
                        
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
