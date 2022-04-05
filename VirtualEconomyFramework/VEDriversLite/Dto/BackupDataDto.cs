using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// VENFT Backup Data Dto
    /// </summary>
    public class BackupDataDto
    {
        /// <summary>
        /// Serialized Bookmarks list
        /// </summary>
        public string Bookmarks { get; set; } = string.Empty;
        /// <summary>
        /// Serialized browser tabs list
        /// </summary>
        public string BrowserTabs { get; set; } = string.Empty;
        /// <summary>
        /// Serialized message tabs list
        /// </summary>
        public string MessageTabs { get; set; } = string.Empty;
        /// <summary>
        /// Serialized SubAccounts list
        /// </summary>
        public string SubAccounts { get; set; } = string.Empty;
        /// <summary>
        /// Main NeblioAddress
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Main NeblioAddress Key
        /// </summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// Main Dogecoin Address
        /// </summary>
        public string DogeAddress { get; set; } = string.Empty;
        /// <summary>
        /// Main Dogecoin Address Key
        /// </summary>
        public string DogeKey { get; set; } = string.Empty;
        /// <summary>
        /// Connection Url for WooCommerce eshop
        /// </summary>
        public string WoCAPIUrl { get; set; } = string.Empty;
        /// <summary>
        /// Connection API Key for the WooCommerce eshop
        /// </summary>
        public string WoCAPIKey { get; set; } = string.Empty;
        /// <summary>
        /// Connection API Secret for the WooCommerce eshop
        /// </summary>
        public string WoCAPISecret { get; set; } = string.Empty;
        /// <summary>
        /// Connection API JWT Token for the WordPress
        /// </summary>
        public string WoCAPIJWT { get; set; } = string.Empty;

    }
}
