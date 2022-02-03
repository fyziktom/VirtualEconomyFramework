using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Accounts.Dto
{
    public class AccountLoadData
    {
        public AccountType Type { get; set; } = AccountType.Neblio;
        public string Password { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string RecoveryPhrase { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool LoadAsSubAccount { get; set; } = false;
        public string ParentAddress { get; set; } = string.Empty;
        public bool LoadFromFile { get; set; } = false;
        public string Filename { get; set; } = "key.txt";
        public string VENFTBackupString { get; set; } = string.Empty;
        public bool LoadJustToObserve { get; set; } = false;
        public bool LoadJustForSignitures { get; set; } = false;
        public bool LoadWithoutNFTs { get; set; } = false;
        public bool LoadWithMessages { get; set; } = false;
        /// <summary>
        /// When creating new account do you want to save it to the file?
        /// </summary>
        public bool SaveToFile { get; set; } = false;
        public bool StartAutorefreshing { get; set; } = true;
        public int MaxLoadOfNFTs { get; set; } = 10000;
        public Dictionary<string, NFTModules.NFTModuleType> NFTModules { get; set; } = new Dictionary<string, NFTModules.NFTModuleType>();
    }
}
