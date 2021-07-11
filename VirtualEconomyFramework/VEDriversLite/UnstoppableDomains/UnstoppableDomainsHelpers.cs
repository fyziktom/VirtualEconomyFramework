using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.UnstoppableDomains
{
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
        public static async Task<GetAddressDetailsResponse> GetAddressDetails(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);
            if (addinfo != null)
                return addinfo;
            else
                return new GetAddressDetailsResponse();
        }
        public static async Task<string> GetNeblioAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("NEBL", out var add))
                    return add;

            return string.Empty;
        }
        public static async Task<string> GetBTCAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("BTC", out var add))
                    return add;

            return string.Empty;
        }
        public static async Task<string> GetETHAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("ETH", out var add))
                    return add;

            return string.Empty;
        }
        public static async Task<string> GetLTCAddress(string uAddress)
        {
            var addinfo = await GetClient().GetAddressDetailsAsync(uAddress);

            if (addinfo != null)
                if (addinfo.Addresses.TryGetValue("LTC", out var add))
                    return add;

            return string.Empty;
        }
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
