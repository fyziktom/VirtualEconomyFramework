using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;
using System.Timers;
using VEDriversLite.NeblioAPI;
using System.Threading.Tasks;

namespace VEDriversLite.Neblio
{
    /// <summary>
    /// This class will query Neblio API with the input address, stores the BlockHeight from address's latest Utxo in Cache.
    /// Cache will expiry for every 10 seconds so when accessed system will check cache and return the latest blockheight 
    /// </summary>
    public static class NeblioTransactionsCache
    {
        private static readonly object _lockObject = new object();
        private static double latestblock = 0.0;
        private static DateTime time = DateTime.MinValue;
        private static TimeSpan timeSpanToRefresh = new TimeSpan(0,0,5);
        /// <summary>
        /// Method will check if the data for current address is available in cache and returns the blockheight, IF not avaialble then retrievs from API.
        /// </summary>
        /// <param name="address">The address for which we need the latest block height</param>
        /// <returns></returns>
        public static async Task<double> LatestBlockHeight(string utxo)
        {
            var newtime = DateTime.UtcNow;
            if ((newtime - time) >= timeSpanToRefresh)
            {
                var data = await NeblioTransactionHelpers.GetClient().GetTransactionInfoAsync(utxo);
                time = newtime;
                if (data != null && data.Blockheight != null && data.Confirmations != null)
                {
                    latestblock = data.Blockheight.Value + data.Confirmations.Value;
                }                
            }

            return latestblock;

        }
    }
}
