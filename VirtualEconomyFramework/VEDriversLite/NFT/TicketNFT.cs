using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public enum ClassOfNFTTicket
    {
        Economy,
        General,
        Standard,
        VIP,
        VIPPlus,
        Family,
        Children
    }
    public enum DurationOfNFTTicket
    {
        OneMinute,
        OneHour,
        Day,
        TwoDays,
        ThreeDays,
        FourDays,
        FiveDays,
        Week,
        Weekend,
        Month,
        Year
    }
    public class TicketNFT : CommonNFT
    {
        public TicketNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Ticket;
            TypeText = "NFT Ticket";
        }
        
        public override async Task Fill(INFT NFT) 
        {
            await FillCommon(NFT);

            var nft = NFT as TicketNFT;
            Location = nft.Location;
            LocationCoordinates = nft.LocationCoordinates;
            LocationCoordinatesLat = nft.LocationCoordinatesLat;
            LocationCoordinatesLen = nft.LocationCoordinatesLen;
            MintAuthorAddress = nft.MintAuthorAddress;
            VideoLink = nft.VideoLink;
            AuthorLink = nft.AuthorLink;
            EventDate = nft.EventDate;
            TicketClass = nft.TicketClass;
            TicketDuration = nft.TicketDuration;
            EventId = nft.EventId;
            EventAddress = nft.EventAddress;
            if (!string.IsNullOrEmpty(nft.EventNFTForTheTicket.Utxo))
                EventNFTForTheTicket = nft.EventNFTForTheTicket;
            Seat = nft.Seat;
            MusicInLink = nft.MusicInLink;
            Used = nft.Used;
        }

        public string MintAuthorAddress { get; set; } = string.Empty;
        public string EventAddress { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string LocationCoordinates { get; set; } = string.Empty;
        public double LocationCoordinatesLat { get; set; } = 0.0;
        public double LocationCoordinatesLen { get; set; } = 0.0;
        public string Seat { get; set; } = string.Empty;
        public bool Used { get; set; } = false;
        public bool MusicInLink { get; set; } = false;
        public string VideoLink { get; set; } = string.Empty;
        public string AuthorLink { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.UtcNow;
        public ClassOfNFTTicket TicketClass { get; set; } = ClassOfNFTTicket.Standard;
        public DurationOfNFTTicket TicketDuration { get; set; } = DurationOfNFTTicket.Day;
        [JsonIgnore]
        public EventNFT EventNFTForTheTicket { get; set; } = new EventNFT("");

        public async Task FillFromEventNFT(INFT nft)
        {
            await FillCommon(nft);

            EventId = (nft as EventNFT).NFTOriginTxId;
            EventDate = (nft as EventNFT).EventDate;
            Location = (nft as EventNFT).Location;
            LocationCoordinates = (nft as EventNFT).LocationCoordinates;
            LocationCoordinatesLat = (nft as EventNFT).LocationCoordinatesLat;
            LocationCoordinatesLen = (nft as EventNFT).LocationCoordinatesLen;
            VideoLink = (nft as EventNFT).VideoLink;
            AuthorLink = (nft as EventNFT).AuthorLink;
        }

        public override void ParseSpecific(IDictionary<string,string> metadata)
        {
            if (metadata.TryGetValue("EventId", out var ei))
                EventId = ei;
            if (metadata.TryGetValue("EventAddress", out var ea))
                EventAddress = ea;
            if (metadata.TryGetValue("Seat", out var seat))
                Seat = seat;
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
                catch
                {
                    Console.WriteLine("Cannot parse NFT Ticket Event Date");
                }
            }
            if (metadata.TryGetValue("TicketClass", out var tc))
            {
                try
                {
                    TicketClass = (ClassOfNFTTicket)Convert.ToInt32(tc);
                }
                catch
                {
                    TicketClass = ClassOfNFTTicket.Standard;
                }
            }
            if (metadata.TryGetValue("TicketDuration", out var td))
            {
                try
                {
                    TicketDuration = (DurationOfNFTTicket)Convert.ToInt32(td);
                }
                catch
                {
                    TicketDuration = DurationOfNFTTicket.Day;
                }
            }

            //LoadEventNFT();
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo, true);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(lastmetadata);
                await ParseDogeftInfo(lastmetadata);
                ParseSoldInfo(lastmetadata);
                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                ParseSpecific(nftData.NFTMetadata);

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
                MintAuthorAddress = await NeblioTransactionHelpers.GetTransactionSender(NFTOriginTxId);

                IsLoaded = true;
            }
        }

        public async Task LoadEventNFT()
        {
            if (string.IsNullOrEmpty(NFTOriginTxId))
                return;

            var nft = await NFTHelpers.FindEventOnTheAddress(EventAddress, EventId);
            if (nft != null)
                EventNFTForTheTicket = nft as EventNFT;
        }

        public override async Task<IDictionary<string,string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(ImageLink) && string.IsNullOrEmpty(Link))
                throw new Exception("Cannot create NFT Ticket without image link or music link.");
            if (string.IsNullOrEmpty(EventId))
                throw new Exception("Cannot create NFT Ticket without event Id.");
            //if (string.IsNullOrEmpty(EventAddress))
            //    throw new Exception("Cannot create NFT Ticket without event Address.");
            if (string.IsNullOrEmpty(Author))
                throw new Exception("Cannot create NFT Ticket without author.");
            //if (string.IsNullOrEmpty(LocationCoordinates) || string.IsNullOrEmpty(Location))
            //    throw new Exception("Cannot create NFT Ticket without location.");

            // create token metadata
            var metadata = await GetCommonMetadata();

            metadata.Add("EventId", EventId);
            metadata.Add("EventAddress", EventAddress);
            if (!string.IsNullOrEmpty(AuthorLink))
                metadata.Add("AuthorLink", AuthorLink);
            metadata.Add("EventDate", EventDate.ToString());
            if (!string.IsNullOrEmpty(VideoLink))
                metadata.Add("VideoLink", VideoLink);
            if (MusicInLink)
                metadata.Add("MusicInLink", "true");
            metadata.Add("Location", Location);
            metadata.Add("LocationC", LocationCoordinates);
            if (!string.IsNullOrEmpty(Seat))
                metadata.Add("Seat", Seat);
            metadata.Add("TicketClass", Convert.ToInt32(TicketClass).ToString());
            metadata.Add("TicketDuration", Convert.ToInt32(TicketDuration).ToString());
            if (Used)
                metadata.Add("Used", "true");
            
            return metadata;
        }
    }
}
