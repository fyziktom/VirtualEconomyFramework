using ASL.Variables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASL.Functions.Controllers
{
    public enum FunctionControllerTypes
    {
        Common,
        Main,
        Fake
    }
    public interface IFunctionController
    {
        FunctionControllerTypes Type { get; set; }
        Task<List<object>> ProcessRequest(Opcodes command, string[] args, List<OVariable> InVariables);
    }
}
