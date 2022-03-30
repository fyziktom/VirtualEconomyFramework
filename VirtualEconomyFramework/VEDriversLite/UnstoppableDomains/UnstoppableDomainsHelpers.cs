using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.UnstoppableDomains
{
    /// <summary>
    /// Helper class for obtain and parse data from UnstoppableDomains.com API
    /// </summary>
    public static class UnstoppableDomainsHelpers
    {
        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static string BaseURL = "https://unstoppabledomains.com/api/v1";
        private static IClient GetClient()
        {
            if (_client == null)
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            return _client;
        }
        /// <summary>
        /// Load the information from the Unstoppable Domains API
        /// </summary>
        /// <param name="uAddress">UnstoppableDomain address - like "something.crypto"</param>
        /// <returns></returns>
        public static async Task<GetAddressDetailsResponse> GetAddressDetails(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);
            if (addinfo != null)
                return addinfo;
            else
                return new GetAddressDetailsResponse();
        }
        /// <summary>
        /// Get Neblio Address if exists
        /// </summary>
        /// <param name="uAddress">UnstoppableDomain address - like "something.crypto"</param>
        /// <returns>Address if exists, string.Empty if not exists</returns>
        public static async Task<string> GetNeblioAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("NEBL", out var add))
                    return add;

            return string.Empty;
        }
        /// <summary>
        /// Get BTC Address if exists
        /// </summary>
        /// <param name="uAddress">UnstoppableDomain address - like "something.crypto"</param>
        /// <returns>Address if exists, string.Empty if not exists</returns>
        public static async Task<string> GetBTCAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("BTC", out var add))
                    return add;

            return string.Empty;
        }
        /// <summary>
        /// Get ETH Address if exists
        /// </summary>
        /// <param name="uAddress">UnstoppableDomain address - like "something.crypto"</param>
        /// <returns>Address if exists, string.Empty if not exists</returns>
        public static async Task<string> GetETHAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("ETH", out var add))
                    return add;

            return string.Empty;
        }
        /// <summary>
        /// Get LTC Address if exists
        /// </summary>
        /// <param name="uAddress">UnstoppableDomain address - like "something.crypto"</param>
        /// <returns>Address if exists, string.Empty if not exists</returns>
        public static async Task<string> GetLTCAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("LTC", out var add))
                    return add;

            return string.Empty;
        }
        /// <summary>
        /// Get DOGE Address if exists
        /// </summary>
        /// <param name="uAddress">UnstoppableDomain address - like "something.crypto"</param>
        /// <returns>Address if exists, string.Empty if not exists</returns>
        public static async Task<string> GetDOGEAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("DOGE", out var add))
                    return add;

            return string.Empty;
        }
    }
}
