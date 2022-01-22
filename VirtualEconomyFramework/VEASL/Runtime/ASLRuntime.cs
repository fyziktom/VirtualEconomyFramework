using ASL.Common;
using ASL.Functions;
using ASL.Functions.Controllers;
using ASL.Variables;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEASL.Runtime
{
    public class ASLRuntime : CommonASLRuntime
    {
        public override async Task<(bool, List<object>)> ExecuteProgram(ASLProgram program)
        {
            CancelToken = CancelTokenSource.Token;
           
            IsRunning = true;

            List<object> resultList = null;

            foreach (var fncb in program.FunctionBlocks)
            {
                if (CancelToken.IsCancellationRequested)
                {
                    IsRunning = false;
                    return (false, null);
                }

                try
                {
                    if (!fncb.Done)
                    {
                        var result = await ProcessFunctionBlock(fncb, program.RequestedRawVariables, resultList);
                        if (result.Item1)
                        {
                            resultList = result.Item2;
                            fncb.Done = true;
                            fncb.Success = true;
                        }
                        else
                        {
                            IsRunning = false;
                            fncb.Done = false;
                            fncb.Failed = true;
                            fncb.Success = false;
                            return (false, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot process the function in program {program.NFTASLTxId}. " + ex.Message);
                    IsRunning = false;
                    fncb.Done = false;
                    fncb.Failed = true;
                    fncb.Success = false;
                    return (false, null);
                }
                await Task.Delay(1);
                
            }

            IsRunning = false;

            if (resultList != null && resultList.Count > 0)
            {
                return (true, resultList);
            }
            return (false, null);
        }

        public override async Task<(bool, string)> StopProgram(ASLProgram program)
        {
            if (IsRunning)
            {
                if (CancelToken != null)
                    CancelTokenSource.Cancel();

                return (true, $"Program {program.NFTASLTxId} was stopped.");
            }
            else
                return (false, string.Empty);
        }

        public async Task<(bool,List<object>)> ProcessFunctionBlock(IFunctionBlock functionBlock, List<OVariable> vars, List<object> resultList)
        {
            if (functionBlock != null && FunctionController != null)
            {
                foreach (var fnc in functionBlock.Functions)
                {
                    var result = await FunctionController.ProcessRequest(fnc.OpCode, fnc.Paramseters, vars, resultList);
                    if (result != null && result.Count > 0)
                    {
                        fnc.Done = true;
                        fnc.Success = true;
                        return (true, result);
                    }
                    else
                    {
                        fnc.Done = false;
                        fnc.Failed = true;
                        fnc.Success = false;
                    }
                }
            }    
            return (false, null);
        }

        public override async Task<(bool, string)> InitRuntime(FunctionControllerTypes controllerType)
        {
            CancelTokenSource = new System.Threading.CancellationTokenSource();
            FunctionControllerType = controllerType;
            FunctionController = FunctionControllerFactory.GetController(FunctionControllerType);
            if (FunctionController != null)
                return (true, "OK");
            else
                return (false, $"Wrong Controller Type.");
        }
    }
}
