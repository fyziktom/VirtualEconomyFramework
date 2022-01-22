using ASL.Variables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASL.Functions.Controllers
{
    public class MainController : IFunctionController
    {
        public MainController()
        {
            Type = FunctionControllerTypes.Main;
        }

        public Dictionary<Opcodes, Func<Opcodes, string[], List<OVariable>, List<object>, Task<List<object>>>> ASLFunctions = new Dictionary<Opcodes, Func<Opcodes, string[], List<OVariable>, List<object>, Task<List<object>>>>
        {
            { Opcodes.sortlist_1to9, SortList_1to9 },
            { Opcodes.sortlist_9to1, SortList_9to1 },
            { Opcodes.multiply_all, CommonNoParameterRequest },
            { Opcodes.add_all, CommonNoParameterRequest },
            { Opcodes.sub_all, CommonNoParameterRequest },
            { Opcodes.replace_all, CommonNoParameterRequest },
            { Opcodes.trim_all, CommonNoParameterRequest },
            { Opcodes.print_result, Print_Result }
        };

        public FunctionControllerTypes Type { get; set; } = FunctionControllerTypes.Main;
        /// <summary>
        /// Find match for command in ASLFunctions Dictionary and invoke function
        /// </summary>
        /// <param name="uid">Request UID</param>
        /// <param name="command">Command to do</param>
        /// <param name="args">set of arguments</param>
        /// <returns></returns>
        public async Task<List<object>> ProcessRequest(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();

            if (ASLFunctions.ContainsKey(command))
                res = await ASLFunctions[command].Invoke(command, args, InVariables, lastResult);

            return res;
        }

        private static async Task<List<object>> SortList_1to9(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                var count = InVariables.Count;
                if (count > 1) // one is description
                {
                    var vr = InVariables[0];
                    var itemcount = vr.Values.Count;
                    for (int i = lastResult.Count; i < itemcount; i++)
                    {
                        JObject jo = new JObject();
                        Parallel.ForEach(InVariables, v =>
                        {
                            jo.Add(v.Name, new JValue(v.Values[i]));
                        });  

                        lastResult.Add(jo);
                    }

                    Type sorttype = null;
                    for (int i = 1; i < count; i++)
                    {
                        if (InVariables[i].Name == args[0])
                        {
                            sorttype = InVariables[i].VarType;
                            break;
                        }
                    }

                    res = lastResult.OrderBy(o => (o as JObject)[args[0]]?.ToString())?.ToList();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during sorting the list {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> SortList_9to1(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                var count = InVariables.Count;
                if (count > 1) // one is description
                {
                    var vr = InVariables[0];
                    var itemcount = vr.Values.Count;
                    for (int i = lastResult.Count; i < itemcount; i++)
                    {
                        JObject jo = new JObject();
                        Parallel.ForEach(InVariables, v =>
                        {
                            jo.Add(v.Name, new JValue(v.Values[i]));
                        });

                        lastResult.Add(jo);
                    }

                    Type sorttype = null;
                    for (int i = 1; i < count; i++)
                    {
                        if (InVariables[i].Name == args[0])
                        {
                            sorttype = InVariables[i].VarType;
                            break;
                        }
                    }

                    res = lastResult.OrderBy(o => (o as JObject)[args[0]]?.ToString())?.Reverse().ToList();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during sorting the list {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Multiply_all(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                res = lastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Add_all(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                res = lastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Sub_all(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                res = lastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Replace_all(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                res = lastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Trim_all(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                res = lastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> CommonNoParameterRequest(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            var res = new List<object>();
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                res = lastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Print_Result(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
        {
            if (lastResult == null)
                lastResult = new List<object>();

            try
            {
                Console.WriteLine("Printing Result:");
                //Console.WriteLine($"{JsonConvert.SerializeObject(lastResult, Formatting.Indented)}");
                lastResult.ForEach(r => Console.WriteLine($"\t{JsonConvert.SerializeObject(r)}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during processing print_result command {command}: {ex}");
            }

            return lastResult;
        }
    }
}
