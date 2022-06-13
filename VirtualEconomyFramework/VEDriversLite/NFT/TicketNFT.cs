using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Class of the ticket
    /// </summary>
    public enum ClassOfNFTTicket
    {
        /// <summary>
        /// Economy ticket
        /// </summary>
        Economy,
        /// <summary>
        /// General ticket
        /// </summary>
        General,
        /// <summary>
        /// Economy ticket
        /// </summary>
        Standard,
        /// <summary>
        /// VPI ticket
        /// </summary>
        VIP,
        /// <summary>
        /// VPIPlus ticket
        /// </summary>
        VIPPlus,
        /// <summary>
        /// Legendary ticket
        /// </summary>
        Legendary,
        /// <summary>
        /// Family ticket
        /// </summary>
        Family,
        /// <summary>
        /// Children ticket
        /// </summary>
        Children
    }
    /// <summary>
    /// Duration of the ticket
    /// </summary>
    public enum DurationOfNFTTicket
    {
        /// <summary>
        /// One Minute ticket
        /// </summary>
        OneMinute,
        /// <summary>
        /// One Hour ticket
        /// </summary>
        OneHour,
        /// <summary>
        /// One Day ticket
        /// </summary>
        Day,
        /// <summary>
        /// Two Days ticket
        /// </summary>
        TwoDays,
        /// <summary>
        /// Three Days ticket
        /// </summary>
        ThreeDays,
        /// <summary>
        /// Four Days ticket
        /// </summary>
        FourDays,
        /// <summary>
        /// Five Days ticket
        /// </summary>
        FiveDays,
        /// <summary>
        /// One Week ticket
        /// </summary>
        Week,
        /// <summary>
        /// One weekend ticket
        /// </summary>
        Weekend,
        /// <summary>
        /// One Month ticket
        /// </summary>
        Month,
        /// <summary>
        /// One Year ticket
        /// </summary>
        Year
    }
    /// <summary>
    /// Ticket NFT. Should related to the NFT Event
    /// </summary>
    public class TicketNFT : CommonNFT
    {
        /// <summary>
        /// Create empty NFT class
        /// </summary>
        public TicketNFT(string utxo = "")
        {
            Utxo = utxo;
            Type = NFTTypes.Ticket;
            TypeText = "NFT Ticket";
        }
        /// <summary>
        /// Fill basic parameters
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
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
            AddUsedTags();
        }
        /// <summary>
        /// Fill the NFT from the NFT Event data. Lots of fields are same
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task FillFromEvent(INFT NFT)
        {
            await FillCommon(NFT);
            
            var nft = NFT as EventNFT;
            EventId = nft.Utxo;
            Location = nft.Location;
            LocationCoordinates = nft.LocationCoordinates;
            LocationCoordinatesLat = nft.LocationCoordinatesLat;
            LocationCoordinatesLen = nft.LocationCoordinatesLen;
            MintAuthorAddress = nft.MintAuthorAddress;
            VideoLink = nft.VideoLink;
            AuthorLink = nft.AuthorLink;
            EventDate = nft.EventDate;
            MusicInLink = nft.MusicInLink;
            Used = nft.Used;
            AddUsedTags();
            Type = NFTTypes.Ticket;
            TypeText = "NFT Ticket";
            Utxo = string.Empty;
            UtxoIndex = 0;
        }
        /// <summary>
        /// Specify author address if different than the mint address
        /// </summary>
        public string MintAuthorAddress { get; set; } = string.Empty;
        /// <summary>
        /// Event Address - where the event NFT is stored
        /// </summary>
        public string EventAddress { get; set; } = string.Empty;
        /// <summary>
        /// Event NFT Utxo
        /// </summary>
        public string EventId { get; set; } = string.Empty;
        /// <summary>
        /// Event location name
        /// </summary>
        public string Location { get; set; } = string.Empty;
        /// <summary>
        /// Event coordinates "Lat,Len"
        /// </summary>
        public string LocationCoordinates { get; set; } = string.Empty;
        /// <summary>
        /// Location coordinate Latitude
        /// </summary>
        public double LocationCoordinatesLat { get; set; } = 0.0;
        /// <summary>
        /// Location coordinate Longitude
        /// </summary>
        public double LocationCoordinatesLen { get; set; } = 0.0;
        /// <summary>
        /// Seat on the event
        /// </summary>
        public string Seat { get; set; } = string.Empty;
        /// <summary>
        /// Indicate if the ticket was used. It goes through all history during load
        /// </summary>
        public bool Used { get; set; } = false;
        /// <summary>
        /// Music in the Link property
        /// </summary>
        public bool MusicInLink { get; set; } = false;
        /// <summary>
        /// Video link
        /// </summary>
        public string VideoLink { get; set; } = string.Empty;
        /// <summary>
        /// Author website page
        /// </summary>
        public string AuthorLink { get; set; } = string.Empty;
        /// <summary>
        /// Date of the Event
        /// </summary>
        public DateTime EventDate { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Class of the ticket
        /// </summary>
        public ClassOfNFTTicket TicketClass { get; set; } = ClassOfNFTTicket.Standard;
        /// <summary>
        /// Duration of the ticket
        /// </summary>
        public DurationOfNFTTicket TicketDuration { get; set; } = DurationOfNFTTicket.Day;
        /// <summary>
        /// Evenf NFT for the ticket
        /// </summary>
        [JsonIgnore]
        public EventNFT EventNFTForTheTicket { get; set; } = new EventNFT("");
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, object> metadata)
        {
            if (metadata.TryGetValue("EventId", out var ei))
                EventId = ei as string;
            if (metadata.TryGetValue("EventAddress", out var ea))
                EventAddress = ea as string;
            if (metadata.TryGetValue("Seat", out var seat))
                Seat = seat as string;
            if (metadata.TryGetValue("Used", out var used))
            {
                if (used as string == "true")
                    Used = true;
                else
                    Used = false;
            }
            if (metadata.TryGetValue("MusicInLink", out var mil))
            {
                if (mil as string == "true")
                    MusicInLink = true;
                else
                    MusicInLink = false;
            }
            if (metadata.TryGetValue("Location", out var location))
                Location = location as string;
            if (metadata.TryGetValue("LocationC", out var loc))
            {
                LocationCoordinates = loc as string;
                var split = (loc as string).Split(',');
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
                VideoLink = video as string;
            if (metadata.TryGetValue("AuthorLink", out var alink))
                AuthorLink = alink as string;
            if (metadata.TryGetValue("EventDate", out var date))
            {
                try
                {
                    EventDate = DateTime.Parse(date as string);
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
                    TicketClass = (ClassOfNFTTicket)Convert.ToInt32(tc as string);
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
            AddUsedTags();
            //LoadEventNFT();
        }
        /// <summary>
        /// Find and parse origin data
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public override async Task ParseOriginData(IDictionary<string, object> lastmetadata)
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
                AddUsedTags();
                
                MintAuthorAddress = await NeblioAPIHelpers.GetTransactionSender(NFTOriginTxId);

                IsLoaded = true;
            }
        }
        /// <summary>
        /// Get last data of this NFT
        /// </summary>
        /// <returns></returns>
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
                MintAuthorAddress = await NeblioAPIHelpers.GetTransactionSender(NFTOriginTxId);
                Used = nftData.Used;
                AddUsedTags();
                
                IsLoaded = true;
            }
        }

        /// <summary>
        /// Get last data for the Event NFT related to this NFT Ticket
        /// </summary>
        /// <returns></returns>
        public async Task LoadEventNFT()
        {
            if (string.IsNullOrEmpty(NFTOriginTxId))
                return;

            var nft = await NFTHelpers.FindEventOnTheAddress(EventAddress, EventId);
            if (nft != null)
                EventNFTForTheTicket = nft as EventNFT;
        }
        /// <summary>
        /// Get the NFT data for the NFT
        /// </summary>
        /// <param name="address">Address of the sender</param>
        /// <param name="key">Private key of the sender for encryption</param>
        /// <param name="receiver">receiver of the NFT</param>
        /// <returns></returns>
        public override async Task<IDictionary<string, object>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(EventId))
                throw new Exception("Cannot create NFT Ticket without Event Id = transaction hash of the NFT Event.");

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

        public override void ParseTags()
        {
            base.ParseTags();
            AddUsedTags();
        }
        
        public void AddUsedTags()
        {
            if (Used)
            {
                if (TagsList.Contains("FreeToUse"))
                    TagsList.Remove("FreeToUse");
                if (!TagsList.Contains("Used"))
                    TagsList.Insert(0,"Used");
            }
            else
            {
                if (TagsList.Contains("Used"))
                    TagsList.Remove("Used");
                if (!TagsList.Contains("FreeToUse"))
                    TagsList.Insert(0, "FreeToUse");
            }
        }
    }
}
