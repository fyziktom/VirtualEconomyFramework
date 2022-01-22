using ASL.Common;
using ASL.Functions.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VEASL.Runtime
{
    public abstract class CommonASLRuntime : IASLRuntime
    {
        public bool IsRunning { get; set; } = false;
        /// <summary>
        /// Cancelation token source for the cancel of automatic loading of the messages
        /// </summary>
        public CancellationTokenSource CancelTokenSource { get; set; }
        /// <summary>
        /// Cancelation token for the cancel of automatic loading of the messages
        /// </summary>
        public CancellationToken CancelToken { get; set; }
        public ASLRuntimeType Type { get; set; }
        public IFunctionController FunctionController { get; set; }
        public FunctionControllerTypes FunctionControllerType { get; set; }

        public abstract Task<(bool, List<object>)> ExecuteProgram(ASLProgram program);
        public abstract Task<(bool, string)> InitRuntime(FunctionControllerTypes controllerType);
        public abstract Task<(bool, string)> StopProgram(ASLProgram program);
    }
}
