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
    /// <summary>
    /// Message tab loads the messages between main address and the partner address
    /// </summary>
    public class MessageTab
    {
        private object _lock = new object();
        /// <summary>
        /// Main constructor. Input the address of the partner
        /// </summary>
        /// <param name="address"></param>
        public MessageTab(string address)
        {
            Address = address;
            ShortAddress = NeblioTransactionHelpers.ShortenAddress(address);
        }
        /// <summary>
        /// Message tab is selected
        /// </summary>
        public bool Selected { get; set; } = false;
        /// <summary>
        /// Address of the partner - loaded address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Address of the main account
        /// </summary>
        [JsonIgnore]
        public string AccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// Main Account Secret - it is used for decryption of the messages
        /// </summary>
        [JsonIgnore]
        public BitcoinSecret AccountSecret { get; set; } = null;
        /// <summary>
        /// Shorten address of the partner
        /// </summary>
        [JsonIgnore]
        public string ShortAddress { get; set; } = string.Empty;
        /// <summary>
        /// This flag is set when address of the partner is stored in the bookmark of the main account
        /// </summary>
        [JsonIgnore]
        public bool IsInBookmark { get; set; } = false;
        /// <summary>
        /// List of the NFTs of partner address
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// List of the NFTs related to the conversation between main account and partner address
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTMessages { get; set; } = new List<INFT>();
        /// <summary>
        /// Loaded bookaark form the main account if the bookmark exists
        /// Otherwise it is empty (not null)
        /// </summary>
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        /// <summary>
        /// Profile of the partner account - it is NFT profile
        /// </summary>
        [JsonIgnore]
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        /// <summary>
        /// Public key of the partner if it was already found
        /// </summary>
        [JsonIgnore]
        public PubKey PublicKey { get; set; }
        /// <summary>
        /// This is set when the public key has been already found and loaded
        /// </summary>
        public bool PublicKeyFound { get; set; } = false;

        /// <summary>
        /// Reload the NFTs 
        /// </summary>
        /// <param name="innfts"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Refresh the messages list
        /// </summary>
        /// <param name="innfts"></param>
        /// <returns></returns>
        public async Task RefreshMessages(List<INFT> innfts)
        {
            if (!PublicKeyFound)
            {
                var bobPubKey = await NFTHelpers.GetPubKeyFromLastFoundTx(Address);
                if (bobPubKey.Item1)
                {
                    PublicKey = bobPubKey.Item2;
                    PublicKeyFound = true;
                }
                else
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

        /// <summary>
        /// Add messages from the main account to the list
        /// This will combine list of received and sent messages to one list
        /// </summary>
        /// <param name="innfts"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Load the bookmark object
        /// </summary>
        /// <param name="bkm"></param>
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
        /// <summary>
        /// Clear the bookmark object
        /// </summary>
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }
    }
}
