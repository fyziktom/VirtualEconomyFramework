using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NBitcoin;
using Newtonsoft.Json;
using VENFTApp_Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;

namespace VENFTApp_Server.Controllers
{
    [Route("ticketapi")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetNFT/{utxo}")]
        public async Task<INFT> GetNFT(string utxo)
        {
            if (!MainDataContext.NFTs.TryGetValue(utxo, out var nft))
                nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, 0, 0, true);
            return nft;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetNFT/{utxo}/{index}")]
        public async Task<INFT> GetNFT(string utxo, int index)
        {
            if (!MainDataContext.NFTs.TryGetValue(utxo, out var nft))
                nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, index, 0, true);
            return nft;
        }

        public class VerifyNFTTicketRequestDto
        {
            /// <summary>
            /// Event Id
            /// </summary>
            public string eventId { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Utxo
            /// </summary>
            public string utxo { get; set; } = string.Empty;
            /// <summary>
            /// Provided signature from owner
            /// </summary>
            public string signature { get; set; } = string.Empty;
            /// <summary>
            /// List of all addresses allowed to mint the NFT Ticket for this Event
            /// </summary>
            public List<string> allowedMintingAddresses { get; set; } = new List<string>();
        }

        public class VerifyNFTTicketDtoResponse
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string AuthorLink { get; set; }
            public string VideoLink { get; set; }
            public bool MusicInLink { get; set; }
            public bool Used { get; set; }
            public string Seat { get; set; }
            public double LocationCoordinatesLen { get; set; }
            public double LocationCoordinatesLat { get; set; }
            public string LocationCoordinates { get; set; }
            public DateTime EventDate { get; set; }
            public string Location { get; set; }
            public string MintAuthorAddress { get; set; }
            public string EventId { get; set; }
            public ClassOfNFTTicket TicketClass { get; set; }
            public bool IsSignatureValid { get; set; } = false;
            public bool IsUsedOnSameAddress { get; set; } = false;
            public bool IsMintedByAllowedAddress { get; set; } = false;
            public string MintAddress { get; set; } = string.Empty;
            public string TxId { get; set; } = string.Empty;
            public string OwnerAddress { get; set; } = string.Empty;
        }

        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("VerifyNFTTicketPost")]
        public async Task<VerifyNFTTicketDtoResponse> VerifyNFTTicketPost([FromBody] VerifyNFTTicketRequestDto data)
        {
            try
            {
                var resp = await NFTTicketVerifier.LoadNFTTicketToVerify(new VEDriversLite.NFT.OwnershipVerificationCodeDto()
                {
                    TxId = data.utxo,
                    Signature = data.signature
                },
                data.eventId, data.allowedMintingAddresses);

                //if (!MainDataContext.UsedAddressesWithTickets.TryGetValue(resp.Item2.OwnerAddress, out var add))
                //    MainDataContext.UsedAddressesWithTickets.TryAdd(resp.Item2.OwnerAddress, resp.Item2.TxId);

                var response = new VerifyNFTTicketDtoResponse();
                response.IsMintedByAllowedAddress = resp.IsMintedByAllowedAddress;
                response.IsSignatureValid = resp.IsSignatureValid;
                response.IsUsedOnSameAddress = resp.IsUsedOnSameAddress;
                response.TxId = resp.TxId;
                response.MintAddress = resp.MintAddress;

                response.Name = resp.NFT.Name;
                response.Description = resp.NFT.Description;
                response.AuthorLink = resp.NFT.AuthorLink;
                response.EventDate = resp.NFT.EventDate;
                response.EventId = resp.NFT.EventId;
                response.Location = resp.NFT.Location;
                response.LocationCoordinates = resp.NFT.LocationCoordinates;
                response.MintAuthorAddress = resp.NFT.MintAuthorAddress;
                response.MusicInLink = resp.NFT.MusicInLink;
                response.OwnerAddress = resp.OwnerAddress;
                response.Seat = resp.NFT.Seat;
                response.TicketClass = resp.NFT.TicketClass;
                response.Used = resp.NFT.Used;
                response.VideoLink = resp.NFT.VideoLink;
                return response;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Verify NFT Ticket {data.utxo}!" + ex.Message);
            }
        }

        [AllowCrossSiteJsonAttribute]
        [HttpPut]
        [Route("VerifyNFTTicket")]
        public async Task<VerifyNFTTicketDtoResponse> VerifyNFTTicket([FromBody] VerifyNFTTicketRequestDto data)
        {
            try
            {
                var resp = await NFTTicketVerifier.LoadNFTTicketToVerify(new VEDriversLite.NFT.OwnershipVerificationCodeDto()
                {
                    TxId = data.utxo,
                    Signature = data.signature
                },
                data.eventId, data.allowedMintingAddresses);
                 
                //if (!MainDataContext.UsedAddressesWithTickets.TryGetValue(resp.Item2.OwnerAddress, out var add))
                //    MainDataContext.UsedAddressesWithTickets.TryAdd(resp.Item2.OwnerAddress, resp.Item2.TxId);

                var response = new VerifyNFTTicketDtoResponse();
                response.IsMintedByAllowedAddress = resp.IsMintedByAllowedAddress;
                response.IsSignatureValid = resp.IsSignatureValid;
                response.IsUsedOnSameAddress = resp.IsUsedOnSameAddress;
                response.TxId = resp.TxId;
                response.MintAddress = resp.MintAddress;

                response.Name = resp.NFT.Name;
                response.Description = resp.NFT.Description;
                response.AuthorLink = resp.NFT.AuthorLink;
                response.EventDate = resp.NFT.EventDate;
                response.EventId = resp.NFT.EventId;
                response.Location = resp.NFT.Location;
                response.LocationCoordinates = resp.NFT.LocationCoordinates;
                response.MintAuthorAddress = resp.NFT.MintAuthorAddress;
                response.MusicInLink = resp.NFT.MusicInLink;
                response.OwnerAddress = resp.OwnerAddress;
                response.Seat = resp.NFT.Seat;
                response.TicketClass = resp.NFT.TicketClass;
                response.Used = resp.NFT.Used;
                response.VideoLink = resp.NFT.VideoLink;
                return response;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Verify NFT Ticket {data.utxo}!" + ex.Message);
            }
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet]
        [Route("GetVerifyNFTTicket/{utxo}/{signature}/{mintingAddress}/{eventId}")]
        public async Task<VerifyNFTTicketDtoResponse> GetVerifyNFTTicket(string utxo, string signature, string mintingAddress, string eventId)
        {
            try
            {
                signature = signature.Replace('-', '/');
                signature = signature.Replace('_', '+');
                var resp = await NFTTicketVerifier.LoadNFTTicketToVerify(new OwnershipVerificationCodeDto()
                {
                    TxId = utxo,
                    Signature = signature + "="
                },
                eventId, new List<string>() { mintingAddress });

                //if (!MainDataContext.UsedAddressesWithTickets.TryGetValue(resp.Item2.OwnerAddress, out var add))
                //    MainDataContext.UsedAddressesWithTickets.TryAdd(resp.Item2.OwnerAddress, resp.Item2.TxId);

                var response = new VerifyNFTTicketDtoResponse();
                response.IsMintedByAllowedAddress = resp.IsMintedByAllowedAddress;
                response.IsSignatureValid = resp.IsSignatureValid;
                response.IsUsedOnSameAddress = resp.IsUsedOnSameAddress;
                response.TxId = resp.TxId;
                response.MintAddress = resp.MintAddress;

                response.Name = resp.NFT.Name;
                response.Description = resp.NFT.Description;
                response.AuthorLink = resp.NFT.AuthorLink;
                response.EventDate = resp.NFT.EventDate;
                response.EventId = resp.NFT.EventId;
                response.Location = resp.NFT.Location;
                response.LocationCoordinates = resp.NFT.LocationCoordinates;
                response.MintAuthorAddress = resp.NFT.MintAuthorAddress;
                response.MusicInLink = resp.NFT.MusicInLink;
                response.OwnerAddress = resp.OwnerAddress;
                response.Seat = resp.NFT.Seat;
                response.TicketClass = resp.NFT.TicketClass;
                response.Used = resp.NFT.Used;
                response.VideoLink = resp.NFT.VideoLink;
                return response;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Verify NFT Ticket {utxo}!" + ex.Message);
            }
        }
    }
}
