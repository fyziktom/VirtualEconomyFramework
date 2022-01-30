using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.Accounts.NFTModules
{
    public class ShopNFTModule : CommonNFTModule
    {
        public ShopNFTModule()
        {
            TokenId = NFTHelpers.TokenId;
            Type = NFTModuleType.Shop;
        }

        private static object _lock { get; set; } = new object();

        /// <summary>
        /// Received payments (means Payment NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments { get; set; } = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Returns List of received payments NFTs sorted by Time from newest to oldests
        /// </summary>
        public List<INFT> ReceivedPaymentsNFTs { get => ReceivedPayments.Values.OrderBy(n => n.Time)?.Reverse()?.ToList(); }
        /// <summary>
        /// Received receipts (means Receipt NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedReceipts { get; set; } = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Returns List of received payments NFTs sorted by Time from newest to oldests
        /// </summary>
        public List<INFT> ReceivedReceiptsNFTs { get => ReceivedReceipts.Values.OrderBy(n => n.Time)?.Reverse()?.ToList(); }
        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        public override event EventHandler<string> NFTsChanged;
        /// <summary>
        /// This event is fired whenever some NFT is in received payment too and it should be blocked for any further action.
        /// It provides Utxo and UtxoIndex as touple.
        /// Event is fired also for the SubAccounts when it is registred from Main Account
        /// </summary>
        public event EventHandler<INFT> NFTAddedToPayments;

        public override async Task Init(string address, string id = "")
        {
            await base.Init(address, id);
        }

        public override async Task ReLoadNFTs(ReloadNFTSetting settings)
        {
            if (!settings.FirstLoad)
                base.NFTsChanged += ShopNFTModule_NFTsChanged;

            settings.LoadJustTypes = new List<NFTTypes>() 
            {  
                NFTTypes.Payment, 
                NFTTypes.Receipt, 
                NFTTypes.Order, 
                NFTTypes.Invoice 
            };

            await base.ReLoadNFTs(settings);
            var tasks = new Task[2];
            tasks[0] = ReloadPayments();
            tasks[1] = ReloadReceipts();
            await Task.WhenAll(tasks);

            if (!settings.FirstLoad)
                base.NFTsChanged -= ShopNFTModule_NFTsChanged;
        }

        private void ShopNFTModule_NFTsChanged(object sender, string e)
        {
            NFTsChanged?.Invoke(sender, e);
        }

        public async Task ReloadPayments()
        {
            try
            {
                var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment && n.TxDetails.Confirmations > NeblioTransactionHelpers.MinimumConfirmations)?.ToList();
                if (pnfts == null) return;
                // remove old ones
                foreach(var opn in ReceivedPayments)
                    if (pnfts.FirstOrDefault(p => p.Utxo == opn.Key) == null)
                        ReceivedPayments.TryRemove(opn.Key, out var rn);

                // add new ones
                foreach(var pn in pnfts)
                {
                    if (!ReceivedPayments.TryGetValue(pn.Utxo, out var nft))
                    {
                        ReceivedPayments.TryAdd(pn.Utxo, pn);
                        NFTAddedToPayments?.Invoke(this, pn);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh address received payments. " + ex.Message);
            }
        }

        public async Task ReloadReceipts()
        {
            try
            {
                var rnfts = NFTs.Where(n => n.Type == NFTTypes.Receipt)?.ToList();
                if (rnfts == null) return;
                // remove old ones
                foreach (var opn in ReceivedReceipts)
                    if (rnfts.FirstOrDefault(p => p.Utxo == opn.Key) == null)
                        ReceivedReceipts.TryRemove(opn.Key, out var rn);

                // add new ones
                foreach (var pn in rnfts)
                    if (!ReceivedReceipts.TryGetValue(pn.Utxo, out var nft))
                        ReceivedReceipts.TryAdd(pn.Utxo, pn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh address received payments. " + ex.Message);
            }
        }
    }
}
