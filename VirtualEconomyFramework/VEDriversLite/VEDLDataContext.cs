using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Admin.Dto;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite
{
    public static class VEDLDataContext
    {
        public static ConcurrentDictionary<string, NeblioAccount> Accounts = new ConcurrentDictionary<string, NeblioAccount>();
        public static ConcurrentDictionary<string, DogeAccount> DogeAccounts = new ConcurrentDictionary<string, DogeAccount>();
        public static List<string> AdminAddresses = new List<string>();
        public static List<string> PublicAddresses = new List<string>();
        public static ConcurrentDictionary<string, IAdminAction> AdminActionsRequests = new ConcurrentDictionary<string, IAdminAction>();
        public static ConcurrentDictionary<string, NFTHash> NFTHashs = new ConcurrentDictionary<string, NFTHash>();

        public static string WooCommerceStoreUrl { get; set; } = string.Empty;
        public static string WooCommerceStoreUrlWithCred => WooCommerceStoreUrl.Replace("https://", $"https://{WooCommerceStoreAPIKey}:{WooCommerceStoreSecret}@");
        public static string WooCommerceStoreAPIKey { get; set; } = string.Empty;
        public static string WooCommerceStoreSecret { get; set; } = string.Empty;
        public static string WooCommerceStoreJWTToken { get; set; } = string.Empty;
    }
}
