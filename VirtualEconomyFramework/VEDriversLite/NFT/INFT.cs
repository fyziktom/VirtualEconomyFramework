using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.NFT
{
    public enum NFTTypes
    {
        Image,
        Post,
        Profile,
        Music,
        YouTube,
        Spotify,
        Payment,
        Message,
        Ticket,
        Event,
        Receipt,
        Order,
        Invoice,
        Product,
        CoruzantArticle = 101,
        CoruzantPremiumArticle = 102,
        CoruzantPodcast = 103,
        CoruzantPremiumPodcast = 104,
        CoruzantProfile = 105,
        Device = 1001,
        IoTDevice = 1002,
        Protocol = 1003,
        HWSrc = 1004,
        FWSrc = 1005,
        SWSrc = 1006,
        MechSrc = 1007,
        IoTMessage = 1008,


    }
    public interface INFT
    {
        /// <summary>
        /// Text form of the NFT type like "NFT Image" or "NFT Post"
        /// The parsing is in the Common NFT
        /// </summary>
        string TypeText { get; set; }
        /// <summary>
        /// NFT Type by enum of NFTTypes
        /// </summary>
        NFTTypes Type { get; set; }
        /// <summary>
        /// Name of the NFT
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Author of the NFT
        /// </summary>
        string Author { get; set; }
        /// <summary>
        /// Description of the NFT - for longer text please use the "Text" property
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Text of the NFT - prepared for the longer texts
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Link to some webiste in the NFT
        /// </summary>
        string Link { get; set; }
        /// <summary>
        /// Link to the icon of the NFT
        /// </summary>
        string IconLink { get; set; }
        /// <summary>
        /// Link to the image in the NFT
        /// </summary>
        string ImageLink { get; set; }
        /// <summary>
        /// List of the tags separated by space
        /// </summary>
        string Tags { get; set; }
        /// <summary>
        /// Parsed tag list. It is parsed in Common NFT class
        /// </summary>
        List<string> TagsList { get; set; }
        /// <summary>
        /// NFT Utxo hash
        /// </summary>
        string Utxo { get; set; }
        /// <summary>
        /// NFT Utxo Index
        /// </summary>
        int UtxoIndex { get; set; }
        /// <summary>
        /// Shorten hash including index number
        /// </summary>
        string ShortHash { get; }
        /// <summary>
        /// NFT Origin transaction hash - minting transaction in the case of original NFTs (Image, Music, Ticket)
        /// </summary>
        string NFTOriginTxId { get; set; }
        /// <summary>
        /// Source tx where the input for the NFT Minting was taken
        /// </summary>
        string SourceTxId { get; set; }
        /// <summary>
        /// Id of the token on what the NFT is created
        /// </summary>
        string TokenId { get; set; }
        /// <summary>
        /// Price of the NFT in the Neblio
        /// </summary>
        double Price { get; set; }
        /// <summary>
        /// PriceActive is setted automatically when the price is setted up
        /// </summary>
        bool PriceActive { get; set; }
        /// <summary>
        /// Price of the NFT in the Dogecoin
        /// </summary>
        double DogePrice { get; set; }
        /// <summary>
        /// DogePriceActive is setted automatically when the price is setted up
        /// </summary>
        bool DogePriceActive { get; set; }
        /// <summary>
        /// Related Doge Address to this NFT. If it is created by VENFT App it is filled automatically during the minting request
        /// </summary>
        string DogeAddress { get; set; }
        /// <summary>
        /// Set that this NFT will be sold as just in coppies minted for the buyer
        /// </summary>
        bool SellJustCopy { get; set; }
        /// <summary>
        /// Info for publishing NFT to the Dogeft
        /// </summary>
        DogeftInfo DogeftInfo { get; set; }
        /// <summary>
        /// If the NFT is fully loaded this flag is set
        /// </summary>
        bool IsLoaded { get; set; }
        /// <summary>
        /// If the NFT is alredy saw in the payment this is set
        /// </summary>
        bool IsInThePayments { get; set; }
        /// <summary>
        /// If the NFT is sold this will be filled
        /// </summary>
        NFTSoldInfo SoldInfo { get; set; }
        /// <summary>
        /// DateTime stamp taken from the blockchain trnsaction
        /// </summary>
        DateTime Time { get; set; }
        /// <summary>
        /// History of this NFT
        /// </summary>
        List<INFT> History { get; set; }
        /// <summary>
        /// The transaction info details
        /// </summary>
        [JsonIgnore]
        GetTransactionInfoResponse TxDetails { get; set; }
        /// <summary>
        /// This event is fired when the transaction info is refreshed
        /// </summary>
        event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;

        /// <summary>
        /// Return info if the transaction is spendable
        /// </summary>
        /// <returns></returns>
        bool IsSpendable();
        /// <summary>
        /// Fill common properties for the NFT
        /// </summary>
        /// <param name="nft"></param>
        /// <returns></returns>
        Task FillCommon(INFT nft);
        /// <summary>
        /// Fill common and specific properties of the NFT
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        Task Fill(INFT NFT);
        /// <summary>
        /// Load last data of the NFT.
        /// It means that it will take just the last data and not tracking the origin for the orign data
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task LoadLastData(IDictionary<string, string> metadata);
        /// <summary>
        /// Parse the origin data of the NFT.
        /// It will track the NFT to its origin and use the data from the origin
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        Task ParseOriginData(IDictionary<string,string> lastmetadata);
        /// <summary>
        /// Parse info about the sellfrom the metadata of the NFT
        /// </summary>
        /// <param name="meta"></param>
        void ParseSoldInfo(IDictionary<string, string> meta);
        /// <summary>
        /// Parse price from the metadata of the NFT
        /// </summary>
        /// <param name="meta"></param>
        void ParsePrice(IDictionary<string, string> meta);
        /// <summary>
        /// Parse specific information related to the specific kind of the NFT. 
        /// This function must be overwritte in specific NFT class
        /// </summary>
        /// <param name="meta"></param>
        void ParseSpecific(IDictionary<string, string> meta);
        /// <summary>
        /// Parse dogeft info from the metadata
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        Task ParseDogeftInfo(IDictionary<string, string> meta);
        /// <summary>
        /// Clear the object with SoldInfo of NFT
        /// </summary>
        /// <returns></returns>
        Task ClearSoldInfo();
        /// <summary>
        /// Load NFT history.
        /// It will load fully all history steps of this NFT
        /// </summary>
        /// <returns></returns>
        Task LoadHistory();
        /// <summary>
        /// Clear all the prices inside of the NFT
        /// </summary>
        /// <returns></returns>
        Task ClearPrices();
        /// <summary>
        /// Stop the auto refreshin of the tx info data
        /// </summary>
        /// <returns></returns>
        Task StopRefreshingData();
        /// <summary>
        /// Start auto refreshing of the tx info data
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        Task StartRefreshingTxData(int interval = 5000);
        /// <summary>
        /// Retrive the Metadata of the actual NFT. 
        /// It will take the correct properties and translate them to the dictionary which can be add to the token transaction metdata
        /// If the NFT contains encrypted metadata with use of Shared Secret (EDCH) like NFT Message you must provide the parameters if you need to do encryption
        /// </summary>
        /// <param name="address">Address of the sender of the NFT</param>
        /// <param name="key">Private key of the sender of the NFT</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "");
    }
}
