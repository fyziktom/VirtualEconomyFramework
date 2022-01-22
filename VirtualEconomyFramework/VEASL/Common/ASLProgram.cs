using ASL.Functions;
using ASL.Variables;
using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace ASL.Common
{
    public class ASLProgram
    {
        public string NFTASLTxId { get; set; } = string.Empty;
        public INFT NFTASL { get; set; } = new VEDriversLite.NFT.DevicesNFTs.ProtocolNFT("");
        public string StringProgram { get; set; } = string.Empty;
        public bool IsRunning { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public bool IsDone { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
        public ProgramSettingsConstants ProgramSettingsConsts { get; set; } = new ProgramSettingsConstants();
        public List<OVariable> RequestedRawVariables { get; set; } = new List<OVariable>();
        public List<IFunctionBlock> FunctionBlocks { get; set; } = new List<IFunctionBlock>();
        public Result ResultOfProgram { get; set; } = new Result();
    }
}
