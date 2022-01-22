using ASL.Common;
using ASL.Parser;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using VEASL.Runtime;
using VEDriversLite;
using VEDriversLite.Bookmarks;

namespace VEASLConsole
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var parser = new ASLParser();
            IASLRuntime runtime = ASLRuntimeFactory.GetASLRuntime(ASLRuntimeType.Main);
            await runtime.InitRuntime(ASL.Functions.Controllers.FunctionControllerTypes.Main);

            var code = FileHelpers.ReadTextFromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aslprogram.txt"));
            var tab = new ActiveTab("NWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY");
            tab.MaxLoadedNFTItems = 20;

            Console.WriteLine("Loading the Active Tab with data... ");
            await tab.StartRefreshing(5000, false, true);

            Console.WriteLine("Tab loaded.");
            Console.WriteLine("Tab loaded. Initializing runtime...");
            await runtime.InitRuntime(ASL.Functions.Controllers.FunctionControllerTypes.Main);
            Console.WriteLine("Runtime Loaded.");
            if (!string.IsNullOrEmpty(code))
            {
                Console.WriteLine("Parsing the code...");
                var program = parser.ParseCode(code);
                if (program.IsLoaded)
                {
                    // clear values
                    program.RequestedRawVariables.ForEach(v => v.Values.Clear());

                    // load the new data from NFTs
                    tab.NFTs.ForEach(nft =>
                    {
                        Parallel.ForEach(program.RequestedRawVariables, v =>
                        {
                            var r = parser.GetVariableTypeValue(v.Represents, nft);
                            if (r.Item2 != null)
                                v.AddNewValues(new System.Collections.Generic.List<object>() { r.Item2 });
                        });
                    });

                    // Execution of the program

                    try { 
                        var result = await runtime.ExecuteProgram(program);
                        Console.WriteLine("Program executed with result: ");

                        if (result.Item1)
                        {
                            Console.WriteLine("\t Program is Done.");
                            Console.WriteLine($"\tResult:");
                            Console.WriteLine($"\t{JsonConvert.SerializeObject(result.Item2, Formatting.Indented)}");
                        }
                        else
                        {
                            Console.WriteLine("\t Program Failed.");
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("\t Program Failed.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Cannot find the program.");
            }

            Console.WriteLine("Press any key to Exit...");
            Console.ReadLine();
        }

    }
}
