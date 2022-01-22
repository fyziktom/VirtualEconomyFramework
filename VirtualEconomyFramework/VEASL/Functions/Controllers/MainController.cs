using ASL.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASL.Functions.Controllers
{
    public class MainController : CommonController
    {
        public MainController()
        {
            Type = FunctionControllerTypes.Main;
        }

        public Dictionary<Opcodes, Func<Opcodes, string[], List<OVariable>, Task<List<object>>>> ASLFunctions = new Dictionary<Opcodes, Func<Opcodes, string[], List<OVariable>, Task<List<object>>>>
        {
            { Opcodes.sortlist_1to9, SortList_1to9 },
            { Opcodes.sortlist_9to1, CommonNoParameterRequest },
            { Opcodes.multiply_all, CommonNoParameterRequest },
            { Opcodes.add_all, CommonNoParameterRequest },
            { Opcodes.sub_all, CommonNoParameterRequest },
            { Opcodes.replace_all, CommonNoParameterRequest },
            { Opcodes.trim_all, CommonNoParameterRequest },
        };

        private static async Task<List<object>> SortList_1to9(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {
                var count = InVariables.Count;
                if (count > 1) // one is description
                {
                    var vr = InVariables[0];
                    var itemcount = vr.Values.Count;
                    for (int i = 0; i < itemcount; i++)
                    {
                        JObject jo = new JObject();
                        for (int j = 1; j < count; j++)
                            jo.Add(InVariables[j].Name, new JObject(InVariables[j].Values[i]));

                        res.Add(jo);
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

                    var sorted = res.OrderBy(o => ((o as JObject)[args[0]].ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> SortList_9to1(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Multiply_all(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Add_all(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Sub_all(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Replace_all(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> Trim_all(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

        private static async Task<List<object>> CommonNoParameterRequest(Opcodes command, string[] args, List<OVariable> InVariables)
        {
            var res = new List<object>();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

    }
}
