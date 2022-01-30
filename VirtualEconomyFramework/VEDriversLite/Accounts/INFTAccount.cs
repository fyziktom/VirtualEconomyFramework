using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.Accounts
{
    public interface INFTAccount
    {
        /// <summary>
        /// Total balance of VENFT tokens which can be used for minting purposes.
        /// </summary>
        double SourceTokensBalance { get; set; }
        /// <summary>
        /// Total number of NFT on the address. It counts also Profile NFT, etc.
        /// </summary>
        int AddressNFTCount { get; set; }
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
        ConcurrentDictionary<string,INFT> NFTsDict { get; set; }

        /// <summary>
        /// Received payments (means Payment NFT) of this address.
        /// </summary>
        [JsonIgnore]
        ConcurrentDictionary<string, INFT> ReceivedPayments { get; set; }
        /// <summary>
        /// Received receipts (means Receipt NFT) of this address.
        /// </summary>
        [JsonIgnore]
        ConcurrentDictionary<string, INFT> ReceivedReceipts { get; set; }
        /// <summary>
        /// If address has some profile NFT, it is founded in Utxo list and in this object.
        /// </summary>
        [JsonIgnore]
        ProfileNFT Profile { get; set; }
        /// <summary>
        /// Actual all token supplies. Consider also other tokens than VENFT.
        /// </summary>
        [JsonIgnore]
        Dictionary<string, TokenSupplyDto> TokensSupplies { get; set; }
        /// <summary>
        /// This function will reload changes in the NFTs list based on provided list of already loaded utxos.
        /// </summary>
        /// <returns></returns>
        Task ReLoadNFTs(bool fireProfileEvent = false, bool withoutMessages = false, bool justMessages = false, int maxItems = 0, bool firstLoad = false);
        /// <summary>
        /// Serialize the NFTCache dictionary
        /// </summary>
        /// <returns></returns>
        Task<string> CacheNFTs();
        /// <summary>
        /// Load the data from the stirng to the Dictionary of the NFTs cache
        /// The input string must be serialized NFTCache dictionary from VEDriversLite with use of the function CacheNFTs from this class
        /// </summary>
        /// <param name="cacheString">Input serialized NFTCache dictionary as string</param>
        /// <returns></returns>
        Task<bool> LoadCacheNFTsFromString(string cacheString);
        /// <summary>
        /// Load the NFTCache data from the input dictionary to the Dictionary of the NFTs cache
        /// The input must be dictionary which contains NFTCacheDto as value with cache data
        /// </summary>
        /// <param name="cacheString">Input NFTCache dictionary</param>
        /// <returns></returns>
        Task<bool> LoadCacheNFTsFromString(IDictionary<string, NFTCacheDto> nfts);
    }
}
