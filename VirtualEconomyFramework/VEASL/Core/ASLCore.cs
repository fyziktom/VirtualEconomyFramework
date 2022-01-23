using ASL.Common;
using ASL.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEASL.Runtime;
using VEDriversLite;
using VEDriversLite.Bookmarks;
using VEDriversLite.NFT;
using VEDriversLite.NFT.ASL;

namespace VEASL.Core
{
    public class ASLCore
    {
        public ASLCore(string _processingAddress)
        {
            if (!string.IsNullOrEmpty(_processingAddress))
                processingAddress = _processingAddress;
            else
                throw new Exception("Cannot start without the processing Address");
        }
        
        public ConcurrentDictionary<string, ASLNFT> ASLNFTs { get; set; } = new ConcurrentDictionary<string, ASLNFT>();
        public ConcurrentDictionary<string, ASLPlanNFT> ASLPlanNFTs { get; set; } = new ConcurrentDictionary<string, ASLPlanNFT>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string,ASLProgram>> ASLPrograms { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string,ASLProgram>>();
        public ConcurrentDictionary<string, ActiveTab> Tabs { get; set; } = new ConcurrentDictionary<string, ActiveTab>();
        public bool IsRunning { get; set; } = false;
        public bool IsFullAccount { get; set; } = false;
        public bool WithConsoleOutputs { get; set; } = true;
        private string processingAddress = string.Empty;
        public string ProcessingAddress
        {
            get => processingAddress;
        }
        private NeblioAccount account = new NeblioAccount();
        private IASLRuntime runtime = null;
        private ASLParser parser = null;

        private void Log(string text)
        {
            if (WithConsoleOutputs) Console.WriteLine(text);
        }
        public async Task<bool> StartCore()
        {
            if (string.IsNullOrEmpty(processingAddress))
                return false;
            if (string.IsNullOrEmpty(account.Address))
            {
                Console.WriteLine("Please load processing the address first.");
                return false;
            }

            try
            {                
                Log("Initializing runtime...");
                runtime = ASLRuntimeFactory.GetASLRuntime(ASLRuntimeType.Main);
                await runtime.InitRuntime(ASL.Functions.Controllers.FunctionControllerTypes.Main);
                Log("Runtime Loaded.");

                // load NFTs
                if (!await ReloadNFTs())
                {
                    Log("Cannot reload NFTs.");
                    return false;
                }

                // load programs
                if (!await LoadPrograms())
                {
                    Log("Cannot Load Programs.");
                    return false;
                }

                // load tabs
                if (!await LoadTabs())
                {
                    Log("Cannot Load Tabs.");
                    return false;
                }

                IsRunning = true;
                // run execution
                if (!await ExecuteAllPrograms())
                {
                    Log("Cannot finish run of programs.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot start the core. " + ex.Message);
            }
            finally
            {
                IsRunning = false;
            }
            return false;
        }

        private async Task<bool> ExecuteAllPrograms()
        {
            try
            {
                foreach (var programs in ASLPrograms)
                {
                    foreach (var program in programs.Value)
                    {
                        // Execution of the program
                        try
                        {
                            Log($"Starting Program {program.Key} which observe address {programs.Key}");
                            var result = await runtime.ExecuteProgram(program.Value);
                            Log("Program executed with result: ");

                            if (result.Item1)
                            {
                                Log("\t Program is Done.");
                                Log($"\tResult:");
                                Log($"\t{JsonConvert.SerializeObject(result.Item2, Formatting.Indented)}");
                            }
                            else
                            {
                                Log("\t Program Failed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("\t Program Failed.");
                        }
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Log("cannot execute programs. " + ex.Message);
            }
            return false;
        }

        private async Task<bool> LoadTabs()
        {
            try
            {
                foreach(var programs in ASLPrograms)
                {
                    if (!Tabs.TryGetValue(programs.Key, out var tab))
                    {
                        tab = new ActiveTab(programs.Key);
                        tab.MaxLoadedNFTItems = 100; // todo const
                        await tab.StartRefreshing();
                        tab.NFTsChanged += Tab_NFTsChanged;
                        Tabs.TryAdd(programs.Key, tab);

                        foreach (var program in programs.Value)
                        {
                            program.Value.RequestedRawVariables.ForEach(v => v.Values.Clear());

                            tab.NFTs.ForEach(nft =>
                            {
                                Parallel.ForEach(program.Value.RequestedRawVariables, v =>
                                {
                                    if (v.Name == Keywords.txid)
                                        v.AddNewValues(new List<object>() { nft.Utxo });

                                    var r = parser.GetVariableTypeValue(v.Represents, nft);
                                    if (r.Item2 != null)
                                        v.AddNewValues(new List<object>() { r.Item2 });
                                });
                            });
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load tabs. " + ex.Message);
            }
            return false;
        }

        private void Tab_NFTsChanged(object sender, string e)
        {
            // TODO Add new NFTs to the Lists
        }

        private async Task<bool> LoadPrograms()
        {
            try
            {
                foreach (var nft in ASLNFTs.Values)
                {
                    if (!string.IsNullOrEmpty(nft.Code))
                    {
                        Console.WriteLine("Parsing the code...");
                        var program = parser.ParseCode(nft.Code);
                        if (program.IsLoaded)
                        {
                            if (string.IsNullOrEmpty(program.ProgramSettingsConsts.address))
                            {
                                Console.WriteLine("Program does not have filled blockchain address setting parameter. Exit!");
                                return false;
                            }
                            if (ASLPrograms.TryGetValue(program.ProgramSettingsConsts.address, out var prgdict))
                            {
                                var last = prgdict;
                                prgdict.TryAdd(nft.Utxo, program); // TODO...need better analysis of this part
                                ASLPrograms.TryUpdate(program.ProgramSettingsConsts.address, prgdict, last);
                            }
                            else
                            {
                                var dict = new ConcurrentDictionary<string, ASLProgram>();
                                dict.TryAdd(nft.Utxo, program);
                                ASLPrograms.TryAdd(program.ProgramSettingsConsts.address, dict);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log("Cannot Load programs. " + ex.Message);
            }
            return false;
        }

        private async Task<bool> ReloadNFTs()
        {
            try
            {
                // load ASL NFTs
                var aslplansnfts = account.NFTs.Where(n => n.Type == NFTTypes.ASLPlan)?.ToList();
                if (aslplansnfts.Count == 0)
                {
                    Console.WriteLine($"Acocunt does not have any NFT ASLPlan. Please create at least one on this address {processingAddress}.");
                    return false;
                }
                aslplansnfts.ForEach(asl =>
                {
                    if (ASLPlanNFTs.TryGetValue(asl.Utxo, out var nft))
                        ASLPlanNFTs.TryUpdate(asl.Utxo, (asl as ASLPlanNFT), nft);
                    else
                        ASLPlanNFTs.TryAdd(asl.Utxo, (asl as ASLPlanNFT));
                });

                var aslnfts = account.NFTs.Where(n => n.Type == NFTTypes.ASL)?.ToList();
                aslnfts.ForEach(asl =>
                {
                    if (ASLPlanNFTs.TryGetValue((asl as ASLNFT).PlanTxId, out var asplan))
                    {
                        if (ASLNFTs.TryGetValue(asl.Utxo, out var nft))
                            ASLNFTs.TryUpdate(asl.Utxo, (asl as ASLNFT), nft);
                        else
                            ASLNFTs.TryAdd(asl.Utxo, (asl as ASLNFT));
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                Log("Cannot reload ASLPlan and ASL NFTs. " + ex.Message);
            }
            return true;
        }

        private async Task<bool> LoadAccount(string address, string pass = "", string key = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(address) && string.IsNullOrEmpty(pass) && string.IsNullOrEmpty(key))
                {
                    Log("Loading Processing Address Account: " + processingAddress);
                    account.FirsLoadingStatus += Account_FirsLoadingStatus;
                    if (await account.LoadAccountWithDummyKey("", processingAddress))
                    {
                        IsFullAccount = false;
                        account.FirsLoadingStatus -= Account_FirsLoadingStatus;
                        Log("Processing Address Account: " + processingAddress + "Loaded.");
                        return true;
                    }
                    else
                    {
                        account.FirsLoadingStatus -= Account_FirsLoadingStatus;
                        Log("Cannot Load Processing Address Account: " + processingAddress);
                        account = new NeblioAccount();
                        return false;
                    }
                }
                else if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(key))
                {
                    Log("Loading Processing Address Account: " + processingAddress);
                    account.FirsLoadingStatus += Account_FirsLoadingStatus;
                    if (await account.LoadAccount(pass, key, processingAddress))
                    {
                        IsFullAccount = true;
                        account.FirsLoadingStatus -= Account_FirsLoadingStatus;
                        Log("Processing Address Account: " + processingAddress + "Loaded.");
                        return true;
                    }
                    else
                    {
                        account.FirsLoadingStatus -= Account_FirsLoadingStatus;
                        Log("Cannot Load Processing Address Account: " + processingAddress);
                        return false;
                    }
                }
            }
            catch(Exception ex)
            {
                account.FirsLoadingStatus -= Account_FirsLoadingStatus;
                Log("Cannot Load the account. " + ex.Message);
                account = new NeblioAccount();
            }
            return false;
        }

        private void Account_FirsLoadingStatus(object sender, string e)
        {
            Log(e);
        }
    }
}
