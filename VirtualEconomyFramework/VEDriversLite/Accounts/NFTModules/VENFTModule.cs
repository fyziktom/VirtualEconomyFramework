using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.Accounts.NFTModules
{
    public class VENFTModule : CommonNFTModule
    {
        public VENFTModule()
        {
            TokenId = NFTHelpers.TokenId;
            Type = NFTModuleType.VENFT;
        }

        private static object _lock { get; set; } = new object();

        /// <summary>
        /// If address has some profile NFT, it is founded in Utxo list and in this object.
        /// </summary>
        [JsonIgnore]
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        public override event EventHandler<string> NFTsChanged;
        /// <summary>
        /// This event is fired whenever profile nft is updated or found
        /// </summary>
        public event EventHandler<INFT> ProfileUpdated;

        public override async Task Init(string address, string id = "")
        {
            await base.Init(address, id);
        }
        public override async Task ReLoadNFTs(ReloadNFTSetting settings)
        {
            if (settings.FirstLoad)
                base.FirstLoadNFTsChanged += VENFTModule_FirstLoadNFTsChanged;
            else
                base.NFTsChanged += VENFTModule_NFTsChanged;

            settings.LoadJustTypes = new List<NFTTypes>()
            {
                NFTTypes.Image,
                NFTTypes.Music,
                NFTTypes.Post,
                NFTTypes.Event,
                NFTTypes.Ticket,
                NFTTypes.Profile
            };

            await base.ReLoadNFTs(settings);

            if (settings.FirstLoad)
                base.FirstLoadNFTsChanged -= VENFTModule_FirstLoadNFTsChanged;
            else
                base.NFTsChanged -= VENFTModule_NFTsChanged;
        }

        private void VENFTModule_NFTsChanged(object sender, string e)
        {
            NFTsChanged?.Invoke(sender, e);
        }

        private void VENFTModule_FirstLoadNFTsChanged(object sender, INFT e)
        {
            if (e.Type == NFTTypes.Profile)
            {
                Profile = e as ProfileNFT;
                ProfileUpdated?.Invoke(this, e);
            }
        }

        public override async Task<(bool,INFT)> GetNFTIfExists(string utxo, int index)
        {
            return await base.GetNFTIfExists(utxo, index);
        }
    }
}
