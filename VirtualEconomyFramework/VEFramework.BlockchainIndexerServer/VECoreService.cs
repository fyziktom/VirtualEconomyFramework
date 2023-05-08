using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBitcoin.Protocol;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VEFramework.BlockchainIndexerServer
{
    public class VECoreService : BackgroundService
    {
        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;

        public VECoreService(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            this.settings = settings; //startup configuration in appsettings.json
            this.lifetime = lifetime;

            //if the RPC is setted up try to get the setting
            // if (EconomyMainContext.WorkWithQTRPC)
            //{
            settings.GetSection("QTRPC").Bind(MainDataContext.Node.QTRPConfig);

            MainDataContext.NumberOfBlocksInHistory = settings.GetValue<int>("NumberOfBlocksInHistory", 10000);

            // start loading from the latest block
            MainDataContext.StartFromTheLatestBlock = Convert.ToBoolean(settings.GetValue<bool>("StartFromTheLatestBlock"));
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            await Task.Delay(1);

            try
            {
                if (MainDataContext.Node.QTRPConfig != null)
                {
                    if (MainDataContext.Node.InitClient(MainDataContext.Node.QTRPConfig))
                        await Console.Out.WriteLineAsync("RPC Initialized.");
                    else
                        throw new Exception("Cannot init the node. Application exit.");
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Cannot init QTRPC Client! Please check settings in appsetting.json" + ex);
            }

            await Console.Out.WriteLineAsync("Initial loading of data...");

            var latestBlock = await MainDataContext.Node.GetLatestBlockNumber();

            var oldestBlock = latestBlock - MainDataContext.NumberOfBlocksInHistory;
            var offset = latestBlock - MainDataContext.NumberOfBlocksInHistory;
            var start = 0 + offset;
            var end = MainDataContext.NumberOfBlocksInHistory + start;
            await MainDataContext.Node.GetIndexedBlocksByNumbersOffsetAndAmount(offset, MainDataContext.NumberOfBlocksInHistory);
            MainDataContext.LatestLoadedBlock = latestBlock;

            await Console.Out.WriteLineAsync("Blocks are loaded :)");
            await Console.Out.WriteLineAsync("Loading transaction data...");

            await MainDataContext.Node.LoadAllBlocksTransactions();
            await Console.Out.WriteLineAsync("Initial loading of data done :)");

            var addresses = MainDataContext.Node.GetAllAddresses();
            await Console.Out.WriteLineAsync("All addresses with some utxos:");
            foreach (var add in addresses)
            {
                if (MainDataContext.Node.Utxos.Values.Any(u => u.OwnerAddress == add && u.TokenUtxo))
                    await Console.Out.WriteLineAsync($"\t{add}");
            }

            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("Starting main loop...");
            try
            {
                _ = Task.Run(async () =>
                {
                    while (!stopToken.IsCancellationRequested)
                    {
                        try
                        {
                            if (MainDataContext.Node != null)
                            {
                                //await Task.Delay(2000);
                                
                                await Console.Out.WriteLineAsync("\tInstead of waiting load some few blocks from history...");
                                if (oldestBlock > 0)
                                {
                                    var numOfblcks = 2000;
                                    if ((oldestBlock - 2000) >= 0)
                                        offset = oldestBlock - numOfblcks;
                                    else
                                    {
                                        numOfblcks = oldestBlock;
                                        offset = 0;
                                    }
                                    await Console.Out.WriteLineAsync($"\tLoading blocks between {offset} and {offset + numOfblcks}...");
                                    await MainDataContext.Node.GetIndexedBlocksByNumbersOffsetAndAmount(offset, numOfblcks);
                                    await MainDataContext.Node.LoadAllBlocksTransactions();
                                    oldestBlock = offset;
                                }

                                // check for new blocks
                                var latestBlock = await MainDataContext.Node.GetLatestBlockNumber();

                                if (latestBlock > MainDataContext.LatestLoadedBlock)
                                {
                                    await Console.Out.WriteLineAsync($"Loading new blocks up to {latestBlock}...");
                                    var numOfb = latestBlock - (int)MainDataContext.LatestLoadedBlock;
                                    await MainDataContext.Node.GetIndexedBlocksByNumbersOffsetAndAmount((int)MainDataContext.LatestLoadedBlock, numOfb);
                                    MainDataContext.LatestLoadedBlock = latestBlock;
                                    await MainDataContext.Node.LoadAllBlocksTransactions();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync("Error occured in VECoreService." + ex.Message);
                            await Task.Delay(1000);
                        }
                    }
                    await Console.Out.WriteLineAsync($"Virtual Economy Framework wallet handler task stopped");
                });

            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Cannot start Virtual Economy server wallet handler" + ex.Message);
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
