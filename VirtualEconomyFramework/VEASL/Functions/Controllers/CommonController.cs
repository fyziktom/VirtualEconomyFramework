using ASL.Variables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASL.Functions.Controllers
{
    public abstract class CommonController : IFunctionController
    {
        public CommonController()
        {
            Type = FunctionControllerTypes.Common;
        }

        public FunctionControllerTypes Type { get; set; } = FunctionControllerTypes.Common;

        public Dictionary<Opcodes, Func<Opcodes, string[], List<OVariable>, List<object>, Task<List<object>>>> ASLFunctions = new Dictionary<Opcodes, Func<Opcodes, string[], List<OVariable>, List<object>, Task<List<object>>>>
        {
            { Opcodes.sortlist_1to9, CommonMultipleParameterRequest },
            { Opcodes.sortlist_9to1, CommonMultipleParameterRequest },
            { Opcodes.multiply_all, CommonMultipleParameterRequest },
            { Opcodes.add_all, CommonMultipleParameterRequest },
            { Opcodes.sub_all, CommonMultipleParameterRequest },
            { Opcodes.replace_all, CommonMultipleParameterRequest },
            { Opcodes.trim_all, CommonMultipleParameterRequest },
        };

        /// <summary>List<OVariable> InVariables, List<object> lastResult)
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

        private static async Task<List<object>> CommonMultipleParameterRequest(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
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

        private static async Task<List<object>> CommonNoParameterRequest(Opcodes command, string[] args, List<OVariable> InVariables, List<object> lastResult)
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
