using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.NFT;

namespace VEDriversLite.Messaging
{
    public class MessageTab
    {
        private object _lock = new object();
        public MessageTab(string address)
        {
            Address = address;
            ShortAddress = NeblioTransactionHelpers.ShortenAddress(address);
        }

        public bool Selected { get; set; } = false;
        public string Address { get; set; } = string.Empty;
        [JsonIgnore]
        public string AccountAddress { get; set; } = string.Empty;
        [JsonIgnore]
        public BitcoinSecret AccountSecret { get; set; } = null;
        [JsonIgnore]
        public string ShortAddress { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsInBookmark { get; set; } = false;
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        [JsonIgnore]
        public List<INFT> NFTMessages { get; set; } = new List<INFT>();
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
        [JsonIgnore]
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        [JsonIgnore]
        public PubKey PublicKey { get; set; }
        public bool PublicKeyFound { get; set; } = false;

        public async Task Reload(List<INFT> innfts)
        {
            if (string.IsNullOrEmpty(Address))
                return;

            NFTs = await NFTHelpers.LoadAddressNFTs(Address, null, NFTs.ToList(), justMessages:true);
            if (NFTs == null)
                NFTs = new List<INFT>();

            //if (NFTs.Count > 0)
            //    Profile = await NFTHelpers.FindProfileNFT(NFTs);

            await RefreshMessages(innfts);
        }

        public async Task RefreshMessages(List<INFT> innfts)
        {
            var bobPubKey = await NFTHelpers.GetPubKeyFromLastFoundTx(Address);
            if (bobPubKey.Item1)
            {
                PublicKey = bobPubKey.Item2;
                PublicKeyFound = true;
            }
            else
            {
                PublicKeyFound = false;
            }
            
            var msgs = await NFTHelpers.LoadAddressNFTMessages(Address, AccountAddress, NFTs);
            lock (_lock)
            {
                NFTMessages = msgs;
            }
            // add related messages from main account
            await AddAccoundMessages(innfts);

            if (AccountSecret != null)
                await DecryptMessages();

            NFTMessages = NFTMessages.OrderBy(m => m.Time).Reverse().ToList();
        }

        public async Task AddAccoundMessages(List<INFT> innfts)
        {
            var msgs = await NFTHelpers.LoadAddressNFTMessages(Address, AccountAddress, innfts);
            lock (_lock)
            {
                foreach (var m in msgs)
                    NFTMessages.Add(m);
            }
        }

        private async Task DecryptMessages()
        {
            var dmsgst = new Task[NFTMessages.Count];

            for (int i = 0; i < NFTMessages.Count; i++)
                dmsgst[i] = (NFTMessages[i] as MessageNFT).Decrypt(AccountSecret);

            await Task.WhenAll(dmsgst);
        }

        public void LoadBookmark(Bookmark bkm)
        {
            if (!string.IsNullOrEmpty(bkm.Address) && !string.IsNullOrEmpty(bkm.Name))
            {
                IsInBookmark = true;
                BookmarkFromAccount = bkm;
            }
            else
                ClearBookmark();
        }
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }
    }
}
