using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace VEDriversLite.Accounts.Dto
{
    public class ReloadNFTSetting
    {
        public List<Utxo> Utxos { get; set; } = new List<Utxo>();
        public string Address { get; set; } = string.Empty;
        public int MaxItems { get; set; } = 0;
        public bool FirstLoad { get; set; } = false;
        public bool FireProfileEvent { get; set; } = false;
        public List<NFTTypes> LoadJustTypes { get; set; } = null;
        public List<NFTTypes> SkipTypes { get; set; } = null;
    }
}
