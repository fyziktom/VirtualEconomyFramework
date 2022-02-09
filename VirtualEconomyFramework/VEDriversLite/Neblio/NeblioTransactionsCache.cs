using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;
using System.Timers;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Neblio
{
    /// <summary>
    /// This class will query Neblio API with the input address, stores the BlockHeight from address's latest Utxo in Cache.
    /// Cache will expiry for every 10 seconds so when accessed system will check cache and return the latest blockheight 
    /// </summary>
    public static class NeblioTransactionsCache
    {
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Method will check if the data for current address is available in cache and returns the blockheight, IF not avaialble then retrievs from API.
        /// </summary>
        /// <param name="address">The address for which we need the latest block height</param>
        /// <returns></returns>
        public static double LatestBlockHeight(string address)
        {
            lock (_lockObject)
            {
                ObjectCache cache = MemoryCache.Default;
                double blockHeightValue;
                if (cache.Contains(address))
                {
                    blockHeightValue = (double)cache.Get(address);
                }
                else
                {
                    blockHeightValue = GetLatestTransaction(address);
                }
                return blockHeightValue;
            }
        }

        /// <summary>
        /// Method will get the Utxos list for the address and return the latest block height. The data will be added to cache with 10 seconds as expiration time.
        /// </summary>
        /// <param name="address">The address for which we need the latest block height</param>
        /// <returns></returns>
        private static double GetLatestTransaction(string address)
        {
            ObjectCache cache = MemoryCache.Default;

            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(10000)
            };

            GetAddressInfoResponse data = NeblioTransactionHelpers.GetClient().GetAddressInfoAsync(address).Result;
            if (data.Utxos.Count == 0)
            {
                return 0;
            }
            List<Utxos> utxos = (List<Utxos>)data.Utxos;
            double blockHeightValue = utxos[0].Blockheight.Value;

            if (cache.Contains(address))
            {
                cache.Remove(address);
            }
            cache.Add(address, blockHeightValue, cacheItemPolicy);
            return blockHeightValue;
        }
    }
}
