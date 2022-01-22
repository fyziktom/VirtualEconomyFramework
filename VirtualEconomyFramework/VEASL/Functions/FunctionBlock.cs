using ASL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASL.Functions
{
    public enum FunctionBlockTypes
    {
        OneLine,
        MultipleLine,
        Loop,
        If
    }
    public interface IFunctionBlock
    {
        FunctionBlockTypes Type { get; set; }
        bool OneLine { get; set; }
        int EndLocation { get; set; }
        int Location { get; set; }
        bool Done { get; set; }
        bool Success { get; set; }
        List<Function> Functions { get; set; }
    }
    public class FunctionBlock : FunctionBase, IFunctionBlock
    {
        public FunctionBlockTypes Type { get; set; } = FunctionBlockTypes.OneLine;
        public bool OneLine { get; set; } = true;
        public int EndLocation { get; set; } = 0;
        public List<Function> Functions { get; set; } = new List<Function>();

        public bool IsDone()
        {
            Done = true;
            foreach (var fnc in Functions)
                if (!fnc.Done)
                {
                    Done = false;
                    return false;
                }
            return true;
        }
    }

    public enum LoopType
    {
        ForLoop,
        FirstLoop,
        LastLoop,
    }
    public enum LoopSetupParameterType
    {
        Zero,
        Count,
        TxId,
        TxTime,
        Blockheigh,
        Exit
    }

    public class LoopFunctionBlock : FunctionBlock
    {
        public LoopFunctionBlock()
        {
            Type = FunctionBlockTypes.Loop;
        }
        public string LoopStart { get; set; } = string.Empty;
        public string LoopStartParam { get; set; } = string.Empty;
        public string LoopEnd { get; set; } = string.Empty;
        public string LoopEndParam { get; set; } = string.Empty;
        public LoopSetupParameterType LoopStartParameterType { get; set; } = LoopSetupParameterType.Zero;
        public LoopSetupParameterType LoopEndParameterType { get; set; } = LoopSetupParameterType.Zero;

        public bool SetLoopStart(string loopstart, string param)
        {
            if (!string.IsNullOrEmpty(loopstart))
            {
                if (loopstart.Contains("0"))
                {
                    LoopStart = loopstart;
                    LoopStartParameterType = LoopSetupParameterType.Zero;
                }
                else if (loopstart.Contains(Keywords.count))
                {
                    LoopStart = loopstart;
                    LoopStartParameterType = LoopSetupParameterType.Count;
                }
                else if (loopstart.Contains(Keywords.txid))
                {
                    LoopStart = loopstart;
                    LoopStartParameterType = LoopSetupParameterType.TxId;
                }
                else if (loopstart.Contains(Keywords.blockheight))
                {
                    LoopStart = loopstart;
                    LoopStartParameterType = LoopSetupParameterType.Blockheigh;
                }
                else if (loopstart.Contains(Keywords.time))
                {
                    LoopStart = loopstart;
                    LoopStartParameterType = LoopSetupParameterType.TxTime;
                }
                LoopStartParam = param;
                return true;
            }
            return false;
        }

        public bool SetLoopEnd(string loopend, string param)
        {
            if (!string.IsNullOrEmpty(loopend))
            {
                if (loopend.Contains("0"))
                {
                    LoopStart = loopend;
                    LoopEndParameterType = LoopSetupParameterType.Zero;
                }
                else if (loopend.Contains(Keywords.count))
                {
                    LoopStart = loopend;
                    LoopEndParameterType = LoopSetupParameterType.Count;
                }
                else if (loopend.Contains(Keywords.txid))
                {
                    LoopStart = loopend;
                    LoopEndParameterType = LoopSetupParameterType.TxId;
                }
                else if (loopend.Contains(Keywords.blockheight))
                {
                    LoopStart = loopend;
                    LoopEndParameterType = LoopSetupParameterType.Blockheigh;
                }
                else if (loopend.Contains(Keywords.time))
                {
                    LoopStart = loopend;
                    LoopEndParameterType = LoopSetupParameterType.TxTime;
                }
                else if (loopend.Contains(Keywords.exit))
                {
                    LoopStart = loopend;
                    LoopEndParameterType = LoopSetupParameterType.Exit;
                }
                LoopEndParam = param;
            }
            return false;
        }
    }
}
