using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;
using VEDriversLite.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.Accounts.NFTModules
{
    public enum NFTModuleType
    {
        VENFT,
        Messages,
        Shop,
        Coruzant,
        IoT
    }
    public interface INFTModule
    {
        /// <summary>
        /// Address which runs this Module
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// Unique Id of the module
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// Type of the NFT Module
        /// </summary>
        NFTModuleType Type { get; set; }
        /// <summary>
        /// Token ID of the token used in this module
        /// </summary>
        string TokenId { get; set; }
        /// <summary>
        /// Total balance of tokens which can be used for minting purposes.
        /// </summary>
        double SourceTokensBalance { get; set; }
        /// <summary>
        /// Total number of NFT on the address. It counts also Profile NFT, etc.
        /// </summary>
        int NFTCount { get; set; }
        /// <summary>
        /// Limit the number of the loaded NFTs on the address
        /// if it is 0 it will load all of them
        /// </summary>
        int MaximumOfLoadedNFTs { get; set; }
        /// <summary>
        /// List of actual address NFTs. Based on Utxos list
        /// The list returns sorted NFTsDict based on blocktime
        /// </summary>
        [JsonIgnore]
        List<INFT> NFTs { get; }
        /// <summary>
        /// Dictionary of actual address NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        ConcurrentDictionary<string, INFT> NFTsDict { get; set; }
        /// <summary>
        /// Actual supply of token.
        /// </summary>
        [JsonIgnore]
        TokenSupplyDto TokensSupply { get; set; }
        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        event EventHandler<string> NFTsChanged;
        /// <summary>
        /// This event is fired whenever the list of NFTs is changed during the first load when all NFTs are loaded
        /// </summary>
        event EventHandler<INFT> FirstLoadNFTsChanged;
        /// <summary>
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        event EventHandler<string> FirsLoadingStatus;

        /// <summary>
        /// Init the NFT Module
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task Init(string address, string id = "");
        /// <summary>
        /// This function will reload changes in the NFTs list based on provided list of already loaded utxos.
        /// </summary>
        /// <returns></returns>
        Task ReLoadNFTs(ReloadNFTSetting settings);
        Task<(bool, INFT)> GetNFTIfExists(string utxo, int index);
    }
}
