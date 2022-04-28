using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Admin.Dto;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;
using VENFTApp_Server.Common;
using static VEDriversLite.AccountHandler;

namespace VENFTApp_Server.Controllers
{
    [Route("vedlapi")]
    [ApiController]
    public class VEDriversLiteController : Controller
    {
        
        public class GetBalanceRequestDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            public string address { get; set; } = string.Empty;
        }
        public class AccountBalanceResponseDto
        {
            public double TotalBalance { get; set; } = 0.0;
            public double TotalSpendableBalance { get; set; } = 0.0;
            public double TotalUnconfirmedBalance { get; set; } = 0.0;
            public bool IsSubAccount { get; set; } = false;
            public string MainAccountAddress { get; set; } = string.Empty;
        }

        /// <summary>
        /// Get Neblio account balances. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpPost]
        [Route("AccountBalance")]
        public async Task<AccountBalanceResponseDto> AccountBalance([FromBody] GetBalanceRequestDto data)
        {
            if (MainDataContext.IsAPIWithCredentials)
            {
                var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                if (vres == null)
                    return null;
            }

            if (string.IsNullOrEmpty(data.address))
                return new AccountBalanceResponseDto();
            if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
            {
                var dto = new AccountBalanceResponseDto();
                dto.TotalBalance = account.TotalBalance;
                dto.TotalSpendableBalance = account.TotalSpendableBalance;
                dto.TotalUnconfirmedBalance = account.TotalUnconfirmedBalance;
                return dto;
            }
            else
            {
                foreach (var a in VEDLDataContext.Accounts.Values)
                {
                    if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                    {
                        var dto = new AccountBalanceResponseDto();
                        dto.TotalBalance = sacc.TotalBalance;
                        dto.TotalSpendableBalance = sacc.TotalSpendableBalance;
                        dto.TotalUnconfirmedBalance = sacc.TotalUnconfirmedBalance;
                        return dto;
                    }
                }
            }
            return null;
        }

        public class GetAccountUtxosRequestDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            public string address { get; set; } = string.Empty;
        }
        /// <summary>
        /// Get Neblio account utxos. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpPost]
        [Route("AccountUtxos")]
        public async Task<List<(string,int)>> AccountUtxos([FromBody] GetAccountUtxosRequestDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return null;
                }

                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    var dto = new List<(string, int)>();
                    var utxos = account.Utxos;
                    foreach (var u in utxos)
                        dto.Add((u.Txid, (int)u.Index));
                    return dto;
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                        {
                            var dto = new List<(string, int)>();
                            var utxos = account.Utxos;
                            foreach (var u in utxos)
                                dto.Add((u.Txid, (int)u.Index));
                            return dto;
                        }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} utxos!");
            }
        }

        #region dogecoin


        public class BroadcastTransactionRequestDto
        {
            public string network { get; set; } = "neblio";
            public string tx_hex { get; set; } = string.Empty;
        }
        public class BroadcastTransactionResponseDto
        {
            public BroadcastDataResponseDto data { get; set; } = new BroadcastDataResponseDto();
        }
        public class BroadcastDataResponseDto
        {
            public string network { get; set; } = "neblio";
            public string txid { get; set; } = string.Empty;
        }
        /// <summary>
        /// Get Neblio account NFTs. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("BroadcastTransaction")]
        public async Task<BroadcastTransactionResponseDto> BroadcastTransaction([FromBody] BroadcastTransactionRequestDto data)
        {
            try
            {
                if (string.IsNullOrEmpty(data.tx_hex))
                    return (new BroadcastTransactionResponseDto());
                var resp = new BroadcastTransactionResponseDto();
                if (data.network == "neblio")
                {
                    var txid = await NeblioTransactionHelpers.BroadcastSignedTransaction(data.tx_hex);
                    if (!string.IsNullOrEmpty(txid))
                    {
                        resp.data = new BroadcastDataResponseDto() { network = "neblio", txid = txid };
                        return resp;
                    }
                    else
                        return (new BroadcastTransactionResponseDto());
                }
                else if (data.network == "dogecoin")
                {
                    var txid = await DogeTransactionHelpers.ChainSoBroadcastTxAsync(
                        new VEDriversLite.DogeAPI.ChainSoBroadcastTxRequest() 
                        { 
                            tx_hex = data.tx_hex 
                        });

                    if (!string.IsNullOrEmpty(txid))
                    {
                        resp.data = new BroadcastDataResponseDto() { network = "dogecoin", txid = txid };
                        return resp;
                    }
                    else
                        return (new BroadcastTransactionResponseDto());
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Broadcast Dogecoin transaction.! Exception: " + ex.Message);
            }

            return (new BroadcastTransactionResponseDto());
        }

        #endregion


        #region NFTs

        public class GetAccountNFTsRequestDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            public string address { get; set; } = string.Empty;
        }
        /// <summary>
        /// Get Neblio account NFTs. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpPost]
        [Route("AccountNFTs")]
        public async Task<List<INFT>> AccountNFTs([FromBody] GetAccountNFTsRequestDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return null;
                }
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    var nfts = account.NFTs;
                    return nfts;
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            return sacc.NFTs;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        /// <summary>
        /// Get Neblio account NFTs. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("AccountNFTs/{address}")]
        public async Task<List<INFT>> AccountNFTs(string address)
        {
            try
            {
                if (!VEDLDataContext.PublicAddresses.Contains(address))
                    return null;

                if (VEDLDataContext.Accounts.TryGetValue(address, out var account))
                {
                    var nfts = account.NFTs;
                    return nfts;
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(address, out var sacc))
                            return sacc.NFTs;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} NFTs!");
            }
        }

        /// <summary>
        /// Get NFT by utxo and index
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("GetNFT/{utxo}/{index}")]
        public async Task<INFT> GetNFT(string utxo, int index)
        {
            try
            {
                if (!string.IsNullOrEmpty(utxo))
                {
                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, index, 0, true);
                    return nft;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get NFT {utxo}!");
            }
        }

        /// <summary>
        /// Get NFT by utxo (0 index)
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("GetNFT/{utxo}")]
        public async Task<INFT> GetNFT(string utxo)
        {
            try
            {
                if(!string.IsNullOrEmpty(utxo))
                {
                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, utxo, 0, 0, true);
                    return nft;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get NFT {utxo}!");
            }
        }

        public class VEDLNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT
            /// </summary>
            public INFT NFT { get; set; }
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintNFT")]
        public async Task<(bool,string)> MintNFT([FromBody] VEDLNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    var res = await account.MintNFT(data.NFT);
                    return res;
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                        {
                            var res = await sacc.MintNFT(data.NFT);
                            return res;
                        }
                }
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintImageNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Name
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Link
            /// </summary>
            public string link { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Tags
            /// </summary>
            public string tags { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Image Link
            /// </summary>
            public string image { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Price
            /// </summary>
            public double price { get; set; } = 0.0;
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintImageNFT")]
        public async Task<(bool, string)> MintImageNFT([FromBody] VEDLMintImageNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                var nft = new ImageNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Tags = data.tags;
                nft.Price = data.price;
                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                    res = await account.MintNFT(nft);
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await sacc.MintNFT(nft);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintPostNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Name
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Link
            /// </summary>
            public string link { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Image Link
            /// </summary>
            public string image { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Price
            /// </summary>
            public double price { get; set; } = 0.0;
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintPostNFT")]
        public async Task<(bool, string)> MintPostNFT([FromBody] VEDLMintPostNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                var nft = new PostNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Price = data.price;
                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.MintNFT(nft);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await sacc.MintNFT(nft);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintMusicNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Name
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Link to music file
            /// </summary>
            public string link { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Image Link
            /// </summary>
            public string image { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Tags
            /// </summary>
            public string tags { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Price
            /// </summary>
            public double price { get; set; } = 0.0;
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintMusicNFT")]
        public async Task<(bool, string)> MintMusicNFT([FromBody] VEDLMintMusicNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                var nft = new MusicNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Tags = data.tags;
                nft.Price = data.price;
                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.MintNFT(nft);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await sacc.MintNFT(nft);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintTicketNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Name
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Link to music file
            /// </summary>
            public string link { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Image Link
            /// </summary>
            public string image { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Tags
            /// </summary>
            public string tags { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Event Id
            /// </summary>
            public string eventId { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Event Address
            /// </summary>
            public string eventAddress { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT AuthorLink
            /// </summary>
            public string authorLink { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT EventDate
            /// </summary>
            public string eventDate { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT VideoLink
            /// </summary>
            public string videoLink { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Location
            /// </summary>
            public string location { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Location Coordinates in lat and len
            /// </summary>
            public string locationCoordinates { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Seat (row and seat, etc).
            /// </summary>
            public string seat { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Class of ticket (Economy, Standard, VIP, VIPPlus)
            /// </summary>
            public ClassOfNFTTicket ticketClass { get; set; } = ClassOfNFTTicket.Standard;
            /// <summary>
            /// Input NFT Class of ticket (Economy, Standard, VIP, VIPPlus)
            /// </summary>
            public DurationOfNFTTicket ticketDuration { get; set; } = DurationOfNFTTicket.Day;
            /// <summary>
            /// Input NFT Price
            /// </summary>
            public double price { get; set; } = 0.0;
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintTicketNFT")]
        public async Task<(bool, string)> MintTicketNFT([FromBody] VEDLMintTicketNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                var nft = new TicketNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Tags = data.tags;
                nft.Price = data.price;
                nft.EventId = data.eventId;
                nft.EventAddress = data.eventAddress;
                nft.EventDate = DateTime.Parse(data.eventDate);
                nft.AuthorLink = data.authorLink;
                nft.VideoLink = data.videoLink;
                nft.Location = data.location;
                nft.LocationCoordinates = data.locationCoordinates;
                nft.Seat = data.seat;
                nft.TicketClass = data.ticketClass;
                nft.TicketDuration = data.ticketDuration;

                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    
                    res = await account.MintNFT(nft);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await account.MintNFT(nft);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("MultiMintImageNFT")]
        public async Task<(bool, string)> MultiMintImageNFT([FromBody] VEDLMintImageNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }
                var nft = new ImageNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Tags = data.tags;
                nft.Price = data.price;
                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.MintMultiNFT(nft, data.coppies);
                }
                else
                {
                    var rs = (false, new Dictionary<string, string>());
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            rs = await sacc.MintMultiNFTLargeAmount(nft, data.coppies);
                    res = (rs.Item1, Newtonsoft.Json.JsonConvert.SerializeObject(rs.Item2, Newtonsoft.Json.Formatting.Indented));
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("MultiMintMusicNFT")]
        public async Task<(bool, string)> MultiMintMusicNFT([FromBody] VEDLMintMusicNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                var nft = new MusicNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Tags = data.tags;
                nft.Price = data.price;
                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.MintMultiNFT(nft, data.coppies);
                }
                else
                {
                    var rs = (false, new Dictionary<string, string>());
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            rs = await sacc.MintMultiNFTLargeAmount(nft, data.coppies);
                    res = (rs.Item1, Newtonsoft.Json.JsonConvert.SerializeObject(rs.Item2, Newtonsoft.Json.Formatting.Indented));
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("MultiMintTicketNFT")]
        public async Task<(bool,string)> MultiMintTicketNFT([FromBody] VEDLMintTicketNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                var nft = new TicketNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.Link = data.link;
                nft.Tags = data.tags;
                nft.Price = data.price;
                nft.EventId = data.eventId;
                nft.EventAddress = data.eventAddress;
                nft.EventDate = DateTime.Parse(data.eventDate);
                nft.AuthorLink = data.authorLink;
                nft.VideoLink = data.videoLink;
                nft.Location = data.location;
                nft.LocationCoordinates = data.locationCoordinates;
                nft.Seat = data.seat;
                nft.TicketClass = data.ticketClass;
                nft.TicketDuration = data.ticketDuration;

                (bool, string) res = (false, string.Empty);
                var rs = (false, new Dictionary<string, string>());
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    rs = await account.MintMultiNFTLargeAmount(nft, data.coppies);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            rs = await account.MintMultiNFTLargeAmount(nft, data.coppies);
                }

                res = (rs.Item1, Newtonsoft.Json.JsonConvert.SerializeObject(rs.Item2, Newtonsoft.Json.Formatting.Indented));
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("ChangeNFT")]
        public async Task<(bool, string)> ChangeNFT([FromBody] VEDLNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    var res = await account.ChangeNFT(data.NFT);
                    return res;
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                        {
                            var res = await sacc.ChangeNFT(data.NFT);
                            return res;
                        }
                }
                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLSendNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Receiver of the NFT
            /// </summary>
            public string receiver { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Utxo
            /// </summary>
            public string NFTUtox { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Utxo Index
            /// </summary>
            public int NFTUtoxIndex { get; set; } = 0;
            /// <summary>
            /// Set if you want to write price.
            /// </summary>
            public bool PriceWrite { get; set; } = false;
            /// <summary>
            /// Price must be bigger than 0.0002 NEBL
            /// </summary>
            public double Price { get; set; } = 0.0;
        }

        [HttpPut]
        [Route("SendNFT")]
        public async Task<(bool, string)> SendNFT([FromBody] VEDLSendNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                (bool, string) res = (false, string.Empty);
                var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, data.NFTUtox, data.NFTUtoxIndex, 0, true);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.SendNFT(data.receiver, nft, data.PriceWrite, data.Price);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await sacc.SendNFT(data.receiver, nft, data.PriceWrite, data.Price);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLSplitNeblioDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Receiver of the NFT
            /// </summary>
            public string receiver { get; set; } = string.Empty;
            /// <summary>
            /// Amount of new splitted coin
            /// </summary>
            public double splittedAmount { get; set; } = 0.0;
            /// <summary>
            /// Number of new splitted coin
            /// </summary>
            public int count { get; set; } = 2;
        }

        [HttpPut]
        [Route("SplitNeblioCoin")]
        public async Task<(bool, string)> SplitNeblioCoin([FromBody] VEDLSplitNeblioDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.SplitNeblioCoin(new List<string>() { data.receiver }, data.count, data.splittedAmount);
                    
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await sacc.SplitNeblioCoin(new List<string>() { data.receiver }, data.count, data.splittedAmount);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class AirdropDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            public string address { get; set; } = string.Empty;
            public string tokenId { get; set; } = NFTHelpers.TokenId;
            public int amount { get; set; } = 100;
            public double amountNeblio { get; set; } = 0.05;
            public List<string> receivers { get; set; } = new List<string>();
        }
        [HttpPut]
        [Route("SendAirdrop")]
        public async Task<(bool, string)> SendAirdrop([FromBody] AirdropDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                (bool, string) res = (false, string.Empty);

                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    foreach (var rec in data.receivers)
                    {
                        var done = false;
                        while (!done)
                        {
                            try
                            {
                                res = await account.SendAirdrop(rec, data.tokenId, data.amount, data.amountNeblio);
                                Console.WriteLine($"Airdrop {rec}: {res.Item2}");
                                if (!res.Item1)
                                {
                                    await Task.Delay(5000);
                                    Console.WriteLine("Probably waiting for the spendable coins and tokens...");
                                }
                                else
                                {
                                    done = true;
                                }
                            }
                            catch(Exception ex)
                            {
                                await Task.Delay(5000);
                                Console.WriteLine("Probably waiting for the spendable coins and tokens...");
                            }
                        }
                    }
                    return res;
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        #endregion

        #region Coruzant

        public class VEDLMintCoruzantProfileNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input Name of Person
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input Surname of Person
            /// </summary>
            public string surname { get; set; } = string.Empty;
            /// <summary>
            /// Input Person Nickname
            /// </summary>
            public string nickname { get; set; } = string.Empty;
            /// <summary>
            /// Input Person Age
            /// </summary>
            public int age { get; set; } = 0;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Link
            /// </summary>
            public string link { get; set; } = string.Empty;
            /// <summary>
            /// Input Company Icon Link
            /// </summary>
            public string iconLink { get; set; } = string.Empty;
            /// <summary>
            /// Input link to podcast
            /// </summary>
            public string podcastLink { get; set; } = string.Empty;
            /// <summary>
            /// Input Personal Page Link
            /// </summary>
            public string personalPageLink { get; set; } = string.Empty;
            /// <summary>
            /// Input Twitter nick
            /// </summary>
            public string twitter { get; set; } = string.Empty;
            /// <summary>
            /// Input Linkedin nick
            /// </summary>
            public string linkedin { get; set; } = string.Empty;
            /// <summary>
            /// Input Company Name
            /// </summary>
            public string companyName { get; set; } = string.Empty;
            /// <summary>
            /// Input Company Link
            /// </summary>
            public string companyLink { get; set; } = string.Empty;
            /// <summary>
            /// Input Person working position in the company
            /// </summary>
            public string workingPosition { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Image Link
            /// </summary>
            public string image { get; set; } = string.Empty;
            /// <summary>
            /// Input Profile tags
            /// </summary>
            public string tags { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Price
            /// </summary>
            public double price { get; set; } = 0.0;
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintCoruzantProfileNFT")]
        public async Task<(bool, string)> MintCoruzantProfileNFT([FromBody] VEDLMintCoruzantProfileNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }

                (bool, string) res = (false, string.Empty);
                var nft = new CoruzantProfileNFT("");
                nft.Name = data.name;
                nft.Surname = data.surname;
                nft.Nickname = data.nickname;
                nft.Age = data.age;
                nft.Description = data.description;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.IconLink = data.iconLink;
                nft.Link = data.link;
                nft.PodcastLink = data.podcastLink;
                nft.PersonalPageLink = data.personalPageLink;
                nft.Twitter = data.twitter;
                nft.Linkedin = data.linkedin;
                nft.CompanyLink = data.companyLink;
                nft.CompanyName = data.companyName;
                nft.WorkingPosition = data.workingPosition;
                nft.Tags = data.tags;
                nft.TokenId = CoruzantNFTHelpers.CoruzantTokenId;

                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.MintNFT(nft);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await account.MintNFT(nft);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintCoruzantArticleNFTDto
        {
            public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string address { get; set; } = string.Empty;
            /// <summary>
            /// Input Name of Person
            /// </summary>
            public string name { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Description
            /// </summary>
            public string description { get; set; } = string.Empty;
            /// <summary>
            /// Input Text
            /// </summary>
            public string text { get; set; } = string.Empty;
            /// <summary>
            /// Input Author profile Utxo (meant Utxo/txid of her/his NFT CoruzantProfile
            /// </summary>
            public string authorProfileUtxo { get; set; } = string.Empty;
            /// <summary>
            /// Input Link to full post
            /// </summary>
            public string fullPostLink { get; set; } = string.Empty;
            /// <summary>
            /// Input link to podcast
            /// </summary>
            public string podcastLink { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Author
            /// </summary>
            public string author { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Link
            /// </summary>
            public string link { get; set; } = string.Empty;
            /// <summary>
            /// Input Company Icon Link
            /// </summary>
            public string iconLink { get; set; } = string.Empty;
            /// <summary>
            /// Input Author of last comment
            /// </summary>
            public string lastCommentBy { get; set; } = string.Empty;
            /// <summary>
            /// Input last comment
            /// </summary>
            public string lastComment { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Image Link
            /// </summary>
            public string image { get; set; } = string.Empty;
            /// <summary>
            /// Input Profile tags
            /// </summary>
            public string tags { get; set; } = string.Empty;
            /// <summary>
            /// Input NFT Price
            /// </summary>
            public double price { get; set; } = 0.0;
            /// <summary>
            /// Just for the case of multimint
            /// </summary>
            public int coppies { get; set; } = 0;
        }

        [HttpPut]
        [Route("MintCoruzantArticleNFT")]
        public async Task<(bool, string)> MintCoruzantArticleNFT([FromBody] VEDLMintCoruzantArticleNFTDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return (false, string.Empty);
                }
                var nft = new CoruzantArticleNFT("");
                nft.Name = data.name;
                nft.Description = data.description;
                nft.Text = data.text;
                nft.Author = data.author;
                nft.ImageLink = data.image;
                nft.IconLink = data.iconLink;
                nft.Link = data.link;
                nft.FullPostLink = data.fullPostLink;
                nft.PodcastLink = data.podcastLink;
                nft.LastComment = data.lastComment;
                nft.LastCommentBy = data.lastCommentBy;
                nft.AuthorProfileUtxo = data.authorProfileUtxo;
                nft.Price = data.price;
                nft.Tags = data.tags;
                nft.TokenId = CoruzantNFTHelpers.CoruzantTokenId;

                (bool, string) res = (false, string.Empty);
                if (VEDLDataContext.Accounts.TryGetValue(data.address, out var account))
                {
                    res = await account.MintMultiNFT(nft, data.coppies);
                }
                else
                {
                    foreach (var a in VEDLDataContext.Accounts.Values)
                        if (a.SubAccounts.TryGetValue(data.address, out var sacc))
                            res = await account.MintNFT(nft);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        #endregion

    }
}
