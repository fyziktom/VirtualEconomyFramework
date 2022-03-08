using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Tickets
{
    public static class TicketHelpers
    {
        public static async Task<List<(string,int)>> MintNFTTicketsFromTemplate(NeblioAccount account, string templateFileName)
        {
            if (!FileHelpers.IsFileExists(templateFileName))
                throw new Exception("Input file does not exists.");

            var templateData = FileHelpers.ReadTextFromFile(templateFileName);
            var ticketTemplate = new PlaneTicketsTemplate();
            try
            {
                ticketTemplate = JsonConvert.DeserializeObject<PlaneTicketsTemplate>(templateData);
            }
            catch(Exception ex)
            {
                throw new Exception("Cannot deserialize the template data." + ex.Message);
            }
            var output = new List<(string, int)>();

            foreach (var section in ticketTemplate.Sections)
            {
                foreach (var coulmn in section.ColumnsMarks)
                {
                    for (var i = 0; i < section.NumberOfRows; i++)
                    {
                        var nft = new TicketNFT("");
                        nft.Author = ticketTemplate.AerolinesName;
                        nft.Name = "Flight:" + ticketTemplate.FlightNumber;
                        nft.Description = $"Flight: {ticketTemplate.FlightNumber} to {ticketTemplate.Location}";
                        nft.Tags = "planeticket";
                        nft.Link = ticketTemplate.AerolinesWebsite;
                        nft.AuthorLink = ticketTemplate.AerolinesWebsite;
                        nft.EventId = ticketTemplate.FlightNumber;
                        nft.EventDate = ticketTemplate.StartOfFlight;
                        nft.Location = ticketTemplate.Location;
                        nft.LocationCoordinates = ticketTemplate.StartLocationCoordinates;
                        nft.Seat = $"Column: {coulmn}, Row: {i}";
                        nft.Price = section.PriceInNeblio;
                        nft.DogePrice = section.PriceInDoge;
                        nft.TicketClass = section.SectionClass;
                        nft.VideoLink = ticketTemplate.SafetyVideoLink;
                        nft.ImageLink = ticketTemplate.AerolinesLogo;
                        var done = false;
                        while (!done)
                        {
                            var res = await account.MintNFT(nft);
                            done = res.Item1;
                            if (!done)
                            {
                                Console.WriteLine("Waiting for spendable utxo...");
                                await Task.Delay(5000);
                            }
                            else
                            {
                                output.Add((res.Item2, 0));
                            }
                        }
                    }
                }
            }
            return output;
        }
    }
}
