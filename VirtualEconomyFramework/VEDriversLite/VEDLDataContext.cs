using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Admin.Dto;
using VEDriversLite.Dto;
using VEDriversLite.NFT;
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
        /// <summary>
        /// NFT Hashes list. Usually it should contains list of all available NFTs in your app
        /// It should just speed up the testing
        /// </summary>
        public static ConcurrentDictionary<string, NFTHash> NFTHashs = new ConcurrentDictionary<string, NFTHash>();
        /// <summary>
        /// Public list of the NFT Cache
        /// </summary>
        public static ConcurrentDictionary<string, NFTCacheDto> NFTCache = new ConcurrentDictionary<string, NFTCacheDto>();
        /// <summary>
        /// If you will set this in you can control cache in you app. It is not implemented in the main logic of the App, just you can use it as common flag
        /// Example use is in VENFT App in omponent UnlockingAccount.razor
        /// </summary>
        public static bool AllowCache { get; set; } = false;
        /// <summary>
        /// Maximum items in the Cache dictionary
        /// </summary>
        public static int MaxCachedItems { get; set; } = 250;
        /// <summary>
        /// Woo Commerce URL of your store
        /// </summary>
        public static string WooCommerceStoreUrl { get; set; } = string.Empty;
        /// <summary>
        /// If you use credentials inside of the WooCommerce API requests it is created here automatically - usually do not work with cors
        /// </summary>
        public static string WooCommerceStoreUrlWithCred => WooCommerceStoreUrl.Replace("https://", $"https://{WooCommerceStoreAPIKey}:{WooCommerceStoreSecret}@");
        /// <summary>
        /// WooCommerce API key - you can obtain this key in the admin section of the WooCommerce
        /// Please install the WoC API plugin to enable this in the WoC
        /// </summary>
        public static string WooCommerceStoreAPIKey { get; set; } = string.Empty;
        /// <summary>
        /// WooCommerce API secret - you can obtain this key in the admin section of the WooCommerce
        /// Please install the WoC API plugin to enable this in the WoC
        /// </summary>
        public static string WooCommerceStoreSecret { get; set; } = string.Empty;
        /// <summary>
        /// If you will install the JWT plugin in the WordPress the JWT token can be stored here. 
        /// It should be obtained automatically after setup the login and pass
        /// </summary>
        public static string WooCommerceStoreJWTToken { get; set; } = string.Empty;
        /// <summary>
        /// This is the name of the Neblio Address field in the checkout form in WoC
        /// </summary>
        public static string WooCommerceStoreCheckoutFieldCustomerNeblioAddress { get; set; } = "_billing_neblio_address";
        public static bool WooCommerceStoreSendDogeToAuthor { get; set; } = false;

        public static bool AllowDispatchNFTOrders { get; set; } = false;
        public static Dictionary<string, DepositScheme> DepositSchemes { get; set; } = new Dictionary<string, DepositScheme>();
    }
}
