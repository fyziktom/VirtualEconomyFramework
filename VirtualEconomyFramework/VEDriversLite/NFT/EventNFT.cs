using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public enum ClassOfNFTEvent
    {
        PersonalEvent,
        OnlineMeeting,
        CorporateMeeting,
        Festival,
        Concert,
        Birthday,
        PlaneFlight
    }
    public class EventNFT : CommonNFT
    {
        public EventNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Event;
            TypeText = "NFT Event";
        }
        
        public override async Task Fill(INFT NFT) 
        {
            await FillCommon(NFT);

            var nft = NFT as EventNFT;
            PriceInDoge = nft.PriceInDoge;
            PriceInDogeActive = nft.PriceInDogeActive;
            MintAuthorAddress = nft.MintAuthorAddress;
            Location = nft.Location;
            LocationCoordinates = nft.LocationCoordinates;
            LocationCoordinatesLat = nft.LocationCoordinatesLat;
            LocationCoordinatesLen = nft.LocationCoordinatesLen;
            VideoLink = nft.VideoLink;
            AuthorLink = nft.AuthorLink;
            EventDate = nft.EventDate;
            EventClass = nft.EventClass;
            EventId = nft.EventId;
            MusicInLink = nft.MusicInLink;
            Used = nft.Used;
        }

        public double PriceInDoge { get; set; } = 0;
        public bool PriceInDogeActive { get; set; } = false;
        public string MintAuthorAddress { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string LocationCoordinates { get; set; } = string.Empty;
        public double LocationCoordinatesLat { get; set; } = 0.0;
        public double LocationCoordinatesLen { get; set; } = 0.0;
        public bool Used { get; set; } = false;
        public bool MusicInLink { get; set; } = false;
        public string VideoLink { get; set; } = string.Empty;
        public string AuthorLink { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.UtcNow;
        public ClassOfNFTEvent EventClass { get; set; } = ClassOfNFTEvent.PersonalEvent;

        public override void ParseSpecific(IDictionary<string,string> metadata)
        {
            if (metadata.TryGetValue("EventId", out var ei))
                EventId = ei;
            if (metadata.TryGetValue("Used", out var used))
            {
                if (used == "true")
                    Used = true;
                else
                    Used = false;
            }
            if (metadata.TryGetValue("MusicInLink", out var mil))
            {
                if (mil == "true")
                    MusicInLink = true;
                else
                    MusicInLink = false;
            }
            if (metadata.TryGetValue("Location", out var location))
                Location = location;
            if (metadata.TryGetValue("LocationC", out var loc))
            {
                LocationCoordinates = loc;
                var split = loc.Split(',');
                if (split.Length > 1)
                {
                    try
                    {
                        LocationCoordinatesLat = Convert.ToDouble(split[0], CultureInfo.InvariantCulture);
                        LocationCoordinatesLen = Convert.ToDouble(split[1], CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        Console.WriteLine("Cannot parse location coordinates in NFT Ticket.");
                    }
                }
            }
            if (metadata.TryGetValue("VideoLink", out var video))
                VideoLink = video;
            if (metadata.TryGetValue("AuthorLink", out var alink))
                AuthorLink = alink;
            if (metadata.TryGetValue("EventDate", out var date))
            {
                try
                {
                    EventDate = DateTime.Parse(date);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot parse NFT Ticket Event Date. " + ex.Message);
                }
            }
            if (metadata.TryGetValue("EventClass", out var tc))
            {
                try
                {
                    EventClass = (ClassOfNFTEvent)Convert.ToInt32(tc);
                }
                catch
                {
                    EventClass = ClassOfNFTEvent.PersonalEvent;
                }
            }
            if (metadata.TryGetValue("PriceInDoge", out var priced))
            {
                if (!string.IsNullOrEmpty(priced))
                {
                    priced = priced.Replace(',', '.');
                    PriceInDoge = double.Parse(priced, CultureInfo.InvariantCulture);
                    PriceInDogeActive = true;
                }
                else
                {
                    PriceInDogeActive = false;
                }
            }
            else
            {
                PriceInDogeActive = false;
            }
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo, true);
            if (nftData != null)
            {
                //ParseCommon(nftData.NFTMetadata);
                ParseCommon(lastmetadata);

                ParsePrice(lastmetadata);
                ParseDogeftInfo(lastmetadata);
                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                //ParseSpecific(nftData.NFTMetadata);
                ParseSpecific(lastmetadata);

                Used = nftData.Used;
                MintAuthorAddress = await NeblioTransactionHelpers.GetTransactionSender(NFTOriginTxId);
                IsLoaded = true;
            }
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                ParseSpecific(nftData.NFTMetadata);
                IsLoaded = true;
            }
        }

        public override async Task<IDictionary<string,string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(Name))
                throw new Exception("Cannot create NFT Event without name.");
            if (string.IsNullOrEmpty(ImageLink))
                throw new Exception("Cannot create NFT Event without image link.");
            if (string.IsNullOrEmpty(Author))
                throw new Exception("Cannot create NFT Event without author.");
            if (string.IsNullOrEmpty(LocationCoordinates) || string.IsNullOrEmpty(Location))
                throw new Exception("Cannot create NFT Event without location.");

            // create token metadata
            var metadata = await GetCommonMetadata();

            metadata.Add("EventId", EventId);
            if (!string.IsNullOrEmpty(AuthorLink))
                metadata.Add("AuthorLink", AuthorLink);
            metadata.Add("EventDate", EventDate.ToString());
            if (!string.IsNullOrEmpty(VideoLink))
                metadata.Add("VideoLink", VideoLink);
            if (MusicInLink)
                metadata.Add("MusicInLink", "true");
            metadata.Add("Location", Location);
            metadata.Add("LocationC", LocationCoordinates);
            metadata.Add("EventClass", Convert.ToInt32(EventClass).ToString());
            if (Used)
                metadata.Add("Used", "true");
            if (PriceInDoge > 0)
                metadata.Add("PriceInDoge", Price.ToString(CultureInfo.InvariantCulture));

            return metadata;
        }
    }
}
