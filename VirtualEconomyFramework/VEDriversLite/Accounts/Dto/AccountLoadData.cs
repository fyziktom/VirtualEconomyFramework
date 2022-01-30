using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Accounts.Dto
{
    public class AccountLoadData
    {
        public string Password { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string RecoveryPhrase { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool LoadFromFile { get; set; } = false;
        public string Filename { get; set; } = "key.txt";
        public string VENFTBackupString { get; set; } = string.Empty;
        public bool LoadJustToObserve { get; set; } = false;
        public bool LoadJustForSignitures { get; set; } = false;
        public bool LoadWithoutNFTs { get; set; } = false;
        public bool LoadWithMessages { get; set; } = false;
        public bool StartAutorefreshing { get; set; } = true;
        public int MaxLoadOfNFTs { get; set; } = 10000;
    }
}
