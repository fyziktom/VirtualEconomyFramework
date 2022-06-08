using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.FluxAPI.Models;

namespace VEDriversLite.FluxAPI
{
    /// <summary>
    /// Helper class for obtain and parse data from runonflux.io API
    /// </summary>
    public static class FluxAPIHelpers
    {
        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static string BaseURL = "https://api.runonflux.io/";
        private static IClient GetClient()
        {
            if (_client == null)
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            return _client;
        }
        /// <summary>
        /// Load the information about apps locations from API
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AppLocation>> GetListOfAppsLocations(string appname = "")
        {
            try
            {
                var resp = await GetClient().GetAppsAsync<List<AppLocation>>("locations");
                if (resp != null && resp.status.Contains("success") && resp.data != null)
                {                var list = resp.data;
                    if (list != null)
                    {
                        if (string.IsNullOrEmpty(appname))
                            return list;
                        if (appname != null)
                            return list.Where(a => a.Name.Contains(appname)).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FLUX API: Get List of Locations - Cannot parse the data response from Flux API.");
            }
            
            return null;
        }

        /// <summary>
        /// Load of the node flux transactions
        /// </summary>
        /// <param name="nodeIP">IP address of the node</param>
        /// <returns></returns>
        public static async Task<List<FluxTxBasicInfo>> GetListOfNodeFluxTransactions(string nodeIP = "")
        {
            try
            {
                var resp = await GetClient().GetExplorerAsync<List<FluxTxBasicInfo>>("fluxtxs", new string[] { nodeIP });
                if (resp != null && resp.status.Contains("success") && resp.data != null)
                {
                    var addressinfo = resp.data;
                    if (addressinfo != null)
                        return addressinfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FLUX API: Get List of Node Flux transactions - Cannot parse the data response from Flux API.");
            }
            return null;
        }

        /// <summary>
        /// Load of the address transactions
        /// </summary>
        /// <param name="address">Flux blockchain Address</param>
        /// <returns></returns>
        public static async Task<AddressInfo> GetListOfAddressTransactions(string address = "")
        {
            try
            {
                var resp = await GetClient().GetExplorerAsync<AddressInfo>("transactions", new string[] { address });
                if (resp != null && resp.status.Contains("success") && resp.data != null)
                {
                    var addressinfo = resp.data;
                    if (addressinfo != null)
                        return addressinfo;
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine("FLUX API: Get List of Node Flux transactions - Cannot parse the data response from Flux API.");
            }
            return null;
        }

        /// <summary>
        /// Load of the address Utxos
        /// </summary>
        /// <param name="address">Address you want to search</param>
        /// <param name="minValue">Setup minimal satoshi value of Utxo</param>
        /// <param name="maxValue">Setup maximal satoshi value of Utxo</param>
        /// <param name="height">Setup minimum height</param>
        /// <returns></returns>
        public static async Task<List<FluxUtxo>> GetListOfAddressUtxos(string address = "", long minValue = 0, long maxValue = 0, double height = 0)
        {
            try
            {
                var resp = await GetClient().GetExplorerAsync<List<FluxUtxo>>("utxo", new string[] { address });
                if (resp != null && resp.status.Contains("success") && resp.data != null)
                {
                    var addressinfo = resp.data;
                    if (addressinfo != null)
                    {
                        IEnumerable<FluxUtxo> list;
                        if (height > 0)
                            list = addressinfo.Where(u => u.Height >= height);
                        else
                            list = addressinfo;
                        
                        if (minValue > 0 && maxValue <= 0)
                            return addressinfo.Where(u => u.Value > minValue).ToList();
                        else if (minValue <= 0 && maxValue > 0)
                            return addressinfo.Where(u => u.Value > minValue && u.Value < maxValue).ToList();
                        else if (minValue <= 0 && maxValue <= 0)
                            return addressinfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FLUX API: Get List of Node Flux Utxos - Cannot parse the data response from Flux API.");
            }
            return null;
        }

        /// <summary>
        /// Load of the address balance
        /// </summary>
        /// <param name="address">Flux Blockchain address</param>
        /// <returns></returns>
        public static async Task<long> GetListOfAddressBalance(string address = "")
        {
            var resp = await GetClient().GetExplorerAsync<long>("balance", new string[] { address });
            if (resp != null && resp.status.Contains("success"))
                return resp.data;
            else
                return -1;
        }

    }
}
