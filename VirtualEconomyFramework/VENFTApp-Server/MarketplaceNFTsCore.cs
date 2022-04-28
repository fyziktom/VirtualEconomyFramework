
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
using VEDriversLite.Bookmarks;
using VEDriversLite.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;
using VEDriversLite.Extensions.WooCommerce;

namespace VENFTApp_Server
{
    public class MarketplaceNFTsCore : BackgroundService
    {
        //private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;
        private static object _lock = new object();

        public MarketplaceNFTsCore(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            this.settings = settings; //startup configuration in appsettings.json
            this.lifetime = lifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            await Task.Delay(1);
            var tabRefreshInterval = 30000;
            var MainInterval = 60000;
            Console.WriteLine("------------------------------------");
            Console.WriteLine("--------Starting main cycle---------");
            Console.WriteLine("------------------------------------");
            try
            {
                _ = Task.Run(async () =>
                {
                    Console.WriteLine("Running...");
                    while (!stopToken.IsCancellationRequested)
                    {
                        try
                        {
                            var tokenholders = await NeblioTransactionHelpers.GetTokenOwnersList(NFTHelpers.TokenId);
                            var i = 0;

                            foreach (var th in tokenholders)
                            {
                                Console.WriteLine($"Start Loading {i} VENFT token owner of total {tokenholders.Count} owners.");

                                try
                                {
                                    if (MainDataContext.VENFTOwnersTabs.TryGetValue(th.Address, out var ownerTab))
                                    {
                                        //if (ownerTab.MaxLoadedNFTItems != 100) ownerTab.MaxLoadedNFTItems = 100;
                                        if (!ownerTab.IsRefreshingRunning)
                                            await ownerTab.StartRefreshing(tabRefreshInterval);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Adding New Address To the Tabs: " + th.Address);
                                        var tab = new ActiveTab(th.Address);
                                        tab.NFTsChanged += ((sender, e) =>
                                        {
                                            try
                                            {
                                                if (MainDataContext.VENFTOwnersTabs.TryGetValue(e, out var ownerTab))
                                                {
                                                    ownerTab.NFTs.ForEach(n =>
                                                    {
                                                        if (n.Type != NFTTypes.Payment && n.PriceActive)
                                                        {
                                                            if (!MainDataContext.PublicSellNFTs.TryGetValue($"{n.Utxo}:{n.UtxoIndex}", out var nft))
                                                            {
                                                                n.TxDetails = null;
                                                                MainDataContext.PublicSellNFTs.TryAdd($"{n.Utxo}:{n.UtxoIndex}", n);
                                                            }
                                                        }
                                                    });
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"Cannot load new NFTs of {e} token owner. " + ex.Message);
                                            }
                                        });

                                        MainDataContext.VENFTOwnersTabs.TryAdd(th.Address, tab);
                                        if (!tab.IsRefreshingRunning)
                                            await tab.StartRefreshing(tabRefreshInterval);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Cannot start loading tab {th}. " + ex.Message);
                                }
                                i++;
                            }

                            lock (_lock)
                            {
                                MainDataContext.PublicSellNFTsList.Clear();
                                MainDataContext.PublicSellNFTsList = MainDataContext.PublicSellNFTs.Values.ToList()?.OrderByDescending(n => n.Time).ToList();
                            }

                            Console.WriteLine("All VENFT Token Owners Loaded.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Cannot refresh VENFT Owners");
                        }

                        await Task.Delay(MainInterval);
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
