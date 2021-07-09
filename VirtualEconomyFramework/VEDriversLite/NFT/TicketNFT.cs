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
        Standard,
        VIP,
        VIPPlus
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
            PriceInDoge = nft.PriceInDoge;
            PriceInDogeActive = nft.PriceInDogeActive;
            Location = nft.Location;
            LocationCoordinates = nft.LocationCoordinates;
            LocationCoordinatesLat = nft.LocationCoordinatesLat;
            LocationCoordinatesLen = nft.LocationCoordinatesLen;
            VideoLink = nft.VideoLink;
            AuthorLink = nft.AuthorLink;
            EventDate = nft.EventDate;
            TicketClass = nft.TicketClass;
            EventId = nft.EventId;
            Seat = nft.Seat;
            MusicInLink = nft.MusicInLink;
        }

        public double PriceInDoge { get; set; } = 0;
        public bool PriceInDogeActive { get; set; } = false;
        public string MintAuthorAddress { get; set; } = string.Empty;
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

        private void ParseSpecific(IDictionary<string,string> meta)
        {
            if (meta.TryGetValue("EventId", out var ei))
                EventId = ei;
            if (meta.TryGetValue("Seat", out var seat))
                Seat = seat;
            if (meta.TryGetValue("Used", out var used))
            {
                if (used == "true")
                    Used = true;
                else
                    Used = false;
            }
            if (meta.TryGetValue("MusicInLink", out var mil))
            {
                if (mil == "true")
                    MusicInLink = true;
                else
                    MusicInLink = false;
            }
            if (meta.TryGetValue("Location", out var location))
                Location = location;
            if (meta.TryGetValue("LocationC", out var loc))
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
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot parse location coordinates in NFT Ticket.");
                    }
                }
            }
            if (meta.TryGetValue("VideoLink", out var video))
                VideoLink = video;
            if (meta.TryGetValue("AuthorLink", out var alink))
                AuthorLink = alink;
            if (meta.TryGetValue("EventDate", out var date))
            {
                try
                {
                    EventDate = DateTime.Parse(date);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot parse NFT Ticket Event Date");
                }
            }
            if (meta.TryGetValue("TicketClass", out var tc))
            {
                try
                {
                    TicketClass = (ClassOfNFTTicket)Convert.ToInt32(tc);
                }
                catch(Exception ex)
                {
                    TicketClass = ClassOfNFTTicket.Standard;
                }
            }
            if (meta.TryGetValue("PriceInDoge", out var priced))
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
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(lastmetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                ParseSpecific(nftData.NFTMetadata);

                Used = nftData.Used;
                MintAuthorAddress = await NeblioTransactionHelpers.GetTransactionSender(NFTOriginTxId);
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
            }
        }

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
            if (metadata != null)
            {
                ParseCommon(metadata);

                if (metadata.TryGetValue("SourceUtxo", out var su))
                {
                    SourceTxId = Utxo;
                    NFTOriginTxId = su;
                }
                else
                {
                    SourceTxId = Utxo;
                    NFTOriginTxId = Utxo;
                }

                ParsePrice(metadata);
                ParseSpecific(metadata);
            }
        }

        public override async Task<IDictionary<string,string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(ImageLink))
                throw new Exception("Cannot create NFT Ticket without image link.");
            if (string.IsNullOrEmpty(EventId))
                throw new Exception("Cannot create NFT Ticket without event Id.");
            if (string.IsNullOrEmpty(Author))
                throw new Exception("Cannot create NFT Ticket without author.");
            if (string.IsNullOrEmpty(Link))
                throw new Exception("Cannot create NFT Ticket without link.");
            if (string.IsNullOrEmpty(LocationCoordinates) || string.IsNullOrEmpty(Location))
                throw new Exception("Cannot create NFT Ticket without location.");
            if (Price == 0 && PriceInDoge == 0)
                throw new Exception("Cannot create NFT Ticket without price.");

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
            if (!string.IsNullOrEmpty(Seat))
                metadata.Add("Seat", Seat);
            metadata.Add("TicketClass", Convert.ToInt32(TicketClass).ToString());
            if (Used)
                metadata.Add("Used", "true");
            if (PriceInDoge > 0)
                metadata.Add("PriceInDoge", Price.ToString(CultureInfo.InvariantCulture));

            return metadata;
        }
    }
}
