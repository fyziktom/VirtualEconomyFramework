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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Common;

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
            MainDataContext.OldestBlockToLoad = settings.GetValue<int>("OldestBlockToLoad", 3000000);

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

            // load list of the blocks where are just PoS transactions if it exists
            var filecontent = FileHelpers.ReadTextFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "posblocks.json"));
            try
            {
                MainDataContext.PoSBlocks = JsonConvert.DeserializeObject<Dictionary<string, int>>(filecontent) ?? new Dictionary<string, int>();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot deserialize the list of the posblocks. Please check the file consitency.");
            }
            // register event for finding new block with just PoS Tx
            MainDataContext.Node.NewJustPoSBlockEvent += Node_NewJustPoSBlockEvent;

            await Console.Out.WriteLineAsync("Initial loading of data...");

            var latestBlock = await MainDataContext.Node.GetLatestBlockNumber();

            double oldestBlock = latestBlock - MainDataContext.NumberOfBlocksInHistory;
            int offset = latestBlock - MainDataContext.NumberOfBlocksInHistory;
            double start = 0 + offset;
            double end = MainDataContext.NumberOfBlocksInHistory + start;
            await MainDataContext.Node.GetIndexedBlocksByNumbersOffsetAndAmount(offset, 
                                                                                MainDataContext.NumberOfBlocksInHistory, 
                                                                                blocksToSkip: MainDataContext.PoSBlocks);
            MainDataContext.LatestLoadedBlock = latestBlock;

            await Console.Out.WriteLineAsync("Blocks are loaded :)");
            await Console.Out.WriteLineAsync("Loading transaction data...");

            await MainDataContext.Node.LoadAllBlocksTransactions();
            await Console.Out.WriteLineAsync("Initial loading of data done :)");

            var addresses = MainDataContext.Node.GetAllAddresses();
            await Console.Out.WriteLineAsync("All addresses with some token utxos:");
            var tokenAddress = new List<string>();
            foreach (var add in addresses)
            {
                if (MainDataContext.Node.Utxos.Values.Any(u => u.OwnerAddress == add && u.TokenUtxo))
                {
                    await Console.Out.WriteLineAsync($"\t{add}");
                    tokenAddress.Add(add);
                }
            }

            Parallel.ForEach(tokenAddress, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, add =>
            {
                MainDataContext.Node.UpdateAddressInfo(add, true);
            });

            storePoSBlocks();
            var storePoSBlocksDict = 10;

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
                                if (oldestBlock > 0 && oldestBlock > MainDataContext.OldestBlockToLoad)
                                {
                                    double numOfblcks = 250;
                                    var avg = MainDataContext.AverageTimeToIndexBlock;
                                    if (avg > 0)
                                        numOfblcks = Math.Round(5000000 / avg, 0);
                                    await Console.Out.WriteLineAsync($"\tInstead of waiting load {numOfblcks} blocks from history. It should fit into 5s...");

                                    if ((oldestBlock - numOfblcks) >= 0)
                                        offset = (int)(oldestBlock - numOfblcks);
                                    else
                                    {
                                        numOfblcks = oldestBlock;
                                        offset = 0;
                                    }
                                    await Console.Out.WriteLineAsync($"\tLoading blocks between {offset} and {offset + numOfblcks}...");

                                    var stopwatch = new Stopwatch();
                                    stopwatch.Start();
                                    await MainDataContext.Node.GetIndexedBlocksByNumbersOffsetAndAmount(offset, 
                                                                                                        (int)numOfblcks,
                                                                                                        blocksToSkip: MainDataContext.PoSBlocks);
                                    await MainDataContext.Node.LoadAllBlocksTransactions();
                                    stopwatch.Stop();
                                    var time = (long)(stopwatch.ElapsedMilliseconds * 1000) / (long)numOfblcks;
                                    MainDataContext.AverageTimeToIndexBlockHistory.Add(Convert.ToDouble(time));
                                    if (MainDataContext.AverageTimeToIndexBlockHistory.Count > 100)
                                        MainDataContext.AverageTimeToIndexBlockHistory.RemoveAt(0);

                                    oldestBlock = offset;
                                    MainDataContext.ActualOldestLoadedBlock = oldestBlock;
                                }
                                else
                                {
                                    await Task.Delay(1000);
                                }

                                // check for new blocks
                                var latestBlock = await MainDataContext.Node.GetLatestBlockNumber();

                                if (latestBlock > MainDataContext.LatestLoadedBlock)
                                {
                                    await Console.Out.WriteLineAsync($"Loading new blocks up to {latestBlock}...");
                                    var numOfb = latestBlock - (int)MainDataContext.LatestLoadedBlock;
                                    await MainDataContext.Node.GetIndexedBlocksByNumbersOffsetAndAmount((int)MainDataContext.LatestLoadedBlock, 
                                                                                                        numOfb,
                                                                                                        blocksToSkip: MainDataContext.PoSBlocks);
                                    MainDataContext.LatestLoadedBlock = latestBlock;
                                    await MainDataContext.Node.LoadAllBlocksTransactions();
                                }

                                if (storePoSBlocksDict <= 0)
                                {
                                    storePoSBlocks();
                                    storePoSBlocksDict = 10;
                                }
                                else
                                {
                                    storePoSBlocksDict--;
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

        private void storePoSBlocks()
        {
            // store the list of the blocks which contais just PoS transactions
            try
            {
                FileHelpers.WriteTextToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "posblocks.json"), JsonConvert.SerializeObject(MainDataContext.PoSBlocks, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot save list of PoSBlocks to the file.");
            }
        }

        private void Node_NewJustPoSBlockEvent(object? sender, (string, int) e)
        {
            if (!string.IsNullOrEmpty(e.Item1))
            {
                if (!MainDataContext.PoSBlocks.ContainsKey(e.Item1))
                    MainDataContext.PoSBlocks.Add(e.Item1, e.Item2);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
