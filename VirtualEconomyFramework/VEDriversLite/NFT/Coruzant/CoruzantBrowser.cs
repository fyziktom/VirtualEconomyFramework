using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT.Coruzant.Dto;

namespace VEDriversLite.NFT.Coruzant
{
    /// <summary>
    /// Class which loads just Coruzant NFTs on specific address
    /// It can handle multiple addresses like browser
    /// </summary>
    public class CoruzantBrowser
    {
        /// <summary>
        /// List of loaded tabs with NFTs
        /// </summary>
        public List<CoruzantTab> Tabs { get; set; } = new List<CoruzantTab>();
        /// <summary>
        /// Addresses to load NFTs from
        /// </summary>
        public List<CoruzantContentAddressDto> CoruzantContentAddresses { get; set; } = new List<CoruzantContentAddressDto>();

        /// <summary>
        /// Load tabs from previous serialized string.
        /// </summary>
        /// <param name="tabs">List of ActiveTabs as json string</param>
        /// <returns></returns>
        public async Task<string> LoadTabs(string tabs)
        {
            try
            {
                var tbs = JsonConvert.DeserializeObject<List<CoruzantTab>>(tabs);
                if (tbs != null)
                    Tabs = tbs;
                var firstAdd = string.Empty;
                if (Tabs.Count > 0)
                {
                    var first = true;
                    foreach (var t in Tabs)
                    {
                        t.Selected = false;
                        if (first)
                        {
                            await t.Reload();
                            first = false;
                            firstAdd = t.Address;
                        }
                        //else
                        //t.Reload();
                    }
                    Tabs.FirstOrDefault().Selected = true;
                }
                return firstAdd;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot deserialize the coruzant tabs. " + ex.Message);
            }
        }

        /// <summary>
        /// Add new tab based on some Neblio address
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> AddTab(string address)
        {
            if (!Tabs.Any(t => t.Address == address))
            {
                var tab = new CoruzantTab(address);
                tab.Selected = true;

                foreach (var t in Tabs)
                    t.Selected = false;

                await tab.Reload();
                Tabs.Add(tab);
            }
            else
            {
                return (false, "Already Exists.");
            }

            return (true, JsonConvert.SerializeObject(Tabs));
        }

        /// <summary>
        /// Remove tab by Neblio address if exists in the tabs
        /// </summary>
        /// <param name="address">Neblio Address which tab should be removed</param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> RemoveTab(string address)
        {
            var tab = Tabs.Find(t => t.Address == address);
            if (tab != null)
                Tabs.Remove(tab);
            else
            {
                return (false, "Tab not found");
            }

            foreach (var t in Tabs)
                t.Selected = false;
            Tabs.FirstOrDefault().Selected = true;

            return (true, JsonConvert.SerializeObject(Tabs));
        }
        /// <summary>
        /// Select the tab - it will set one selected tab to the Selected=true and others will set false
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task SelectTab(string address)
        {
            foreach (var t in Tabs)
                t.Selected = false;
            var tab = Tabs.Find(t => t.Address == address);
            if (tab != null)
                tab.Selected = true;
            if (tab.NFTs.Count == 0)
                await tab.Reload();
        }

        /// <summary>
        /// Return serialized list of ActiveTabs as Json stirng
        /// </summary>
        /// <returns></returns>
        public async Task<string> SerializeTabs()
        {
            return JsonConvert.SerializeObject(Tabs);
        }

        /// <summary>
        /// Return list of available Coruzant Content Addresses
        /// </summary>
        /// <returns></returns>
        public async Task LoadCoruzantContentAddresses()
        {
            try
            {
                string fileContent = new WebClient().DownloadString("https://ve-nft.com/coruzant.json");
                var res = JsonConvert.DeserializeObject<List<CoruzantContentAddressDto>>(fileContent);
                if (res != null)
                    CoruzantContentAddresses = res;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load Coruzant data from the server. " + ex.Message);
            }
        }
    }
}
