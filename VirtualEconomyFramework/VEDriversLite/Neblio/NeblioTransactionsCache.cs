using System;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace VEDriversLite.Neblio
{
    /// <summary>
    /// This class will query Neblio API with the input address, stores the BlockHeight from address's latest Utxo in Cache.
    /// Cache will expiry for every 10 seconds so when accessed system will check cache and return the latest blockheight 
    /// </summary>
    public static class NeblioTransactionsCache
    {
        static readonly MemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        /// <summary>
        /// Method will check if the data for current address is available in cache and returns the blockheight, IF not avaialble then retrievs from API.
        /// </summary>
        /// <param name="address">The address for which we need the latest block height</param>
        /// <param name="utxo"></param>
        /// <returns></returns>
        public async static Task<double> LatestBlockHeight(string utxo, string address)
        {
            double blockHeightValue = 0;

            object value = GetCacheValue(address);

            if (value != default)
            {
                blockHeightValue = (double)value;
            }
            else
            {
                var data = await NeblioTransactionHelpers.GetClient().GetTransactionInfoAsync(utxo);

                if (data != null && data.Blockheight != null && data.Confirmations != null)
                {
                    blockHeightValue = data.Blockheight.Value + data.Confirmations.Value;
                }
                SetChacheValue(address, blockHeightValue);
            }
            return blockHeightValue;
        }

        private static object GetCacheValue(string key)
        {
            if (key != null && cache.TryGetValue(key, out object val))
            {
                return val;
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Set value in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetChacheValue(string key, object value)
        {
            if (key != null)
            {
                cache.Set(key, value, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(10)
                });
            }
        }
    }
}
