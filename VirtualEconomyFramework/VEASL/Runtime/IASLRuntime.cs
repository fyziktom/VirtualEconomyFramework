using ASL.Common;
using ASL.Functions.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VEASL.Runtime
{
    public enum ASLRuntimeType{
        Fake,
        Main
    }
    public interface IASLRuntime
    {
        ASLRuntimeType Type { get; set; }
        IFunctionController FunctionController { get; set; } 
        FunctionControllerTypes FunctionControllerType { get; set; } 
        bool IsRunning { get; set; }
        /// <summary>
        /// Cancelation token source for the cancel program
        /// </summary>
        CancellationTokenSource CancelTokenSource { get; set; }
        /// <summary>
        /// Cancelation token for the cancel program
        /// </summary>
        CancellationToken CancelToken { get; set; }

        Task<(bool, string)> InitRuntime(FunctionControllerTypes controllerType);
        Task<(bool, List<object>)> ExecuteProgram(ASLProgram program);
        Task<(bool, string)> StopProgram(ASLProgram program);

    }
}
