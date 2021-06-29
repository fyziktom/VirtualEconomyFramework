using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Tickets
{
    public class SectionTemplate
    {
        public ClassOfNFTTicket SectionClass { get; set; } = ClassOfNFTTicket.Economy;
        public List<string> ColumnsMarks { get; set; } = new List<string>();
        public int NumberOfRows { get; set; } = 1;
        public double PriceInNeblio { get; set; } = 1.0;
        public double PriceInDoge { get; set; } = 1.0;
    }
    public class PlaneTicketsTemplate
    {
        public string PlaneType { get; set; } = string.Empty;
        public string AerolinesName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string StartLocationCoordinates { get; set; } = string.Empty;
        public string EndLocationCoordinates { get; set; } = string.Empty;
        public string AerolinesWebsite { get; set; } = string.Empty;
        public string AerolinesLogo { get; set; } = string.Empty;
        public string SafetyVideoLink { get; set; } = string.Empty;
        public DateTime StartOfFlight { get; set; } = DateTime.UtcNow;
        public double ExpectedFlightDuration { get; set; } = 1.0;
        public List<SectionTemplate> Sections { get; set; } = new List<SectionTemplate>();

    }
}
