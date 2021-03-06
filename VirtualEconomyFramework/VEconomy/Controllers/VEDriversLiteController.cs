﻿using log4net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using VEconomy.Common;
using VEDrivers.Common;
using VEDrivers.Economy.Wallets.Handlers;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;
using VEGameDrivers.Common;

namespace VEconomy.Controllers
{
    [Route("api")]
    [ApiController]
    public class VEDriversLiteController : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private BasicAccountHandler accountHandler = new BasicAccountHandler();

        /// <summary>
        /// Data carrier for Init Account API command
        /// </summary>
        public class InitVEDLAccountData
        {
            /// <summary>
            /// Guid format
            /// </summary>
            public string walletId { get; set; }
            /// <summary>
            /// Account address
            /// Set empty string or null if new address should be created
            /// </summary>
            public string accountAddress { get; set; }

        }

        [HttpPut]
        [Route("VEDLInitAccount")]
        public async Task<string> InitAccount([FromBody] InitVEDLAccountData accountData)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(accountData.walletId, out var wallet))
                {
                    // todo - remove address from real QT wallet
                    if (wallet.Accounts.TryGetValue(accountData.accountAddress, out var account))
                    {
                        var res = "Cannot Init Account.";
                        if (account.Type == VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                            res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).LoadVEDLTNeblioAccount();
                        return res;
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot init Account {accountData.accountAddress}, Account Not Found!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot init Account {accountData.accountAddress}, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot init Account!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot init Account {accountData.accountAddress}!");
            }

            return string.Empty;
        }

        public class AccountBalanceResponseDto
        {
            public double TotalBalance { get; set; } = 0.0;
            public double TotalSpendableBalance { get; set; } = 0.0;
            public double TotalUnconfirmedBalance { get; set; } = 0.0;
        }

        /// <summary>
        /// Get Neblio account balances. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("AccountBalance/{address}")]
        public AccountBalanceResponseDto AccountBalance(string address)
        {
            var dto = new AccountBalanceResponseDto();

            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    dto.TotalBalance = (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.TotalBalance;
                    dto.TotalSpendableBalance = (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.TotalSpendableBalance;
                    dto.TotalUnconfirmedBalance = (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.TotalUnconfirmedBalance;
                    return dto;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} balances, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} balances!");
            }
        }

        /// <summary>
        /// Get Neblio account utxos. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("AccountUtxos/{address}")]
        public List<(string,int)> AccountUtxos(string address)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var dto = new List<(string, int)>();
                    var utxos = (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Utxos;
                    foreach (var u in utxos)
                        dto.Add((u.Txid, (int)u.Index));
                    return dto;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} utxos, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} utxos!");
            }
        }

        #region NFTs

        /// <summary>
        /// Get Neblio account NFTs. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [HttpGet]
        [Route("AccountNFTs/{address}")]
        public List<INFT> AccountNFTs(string address)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nfts = (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.NFTs;
                    return nfts;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {address} NFTs!");
            }
        }

        /// <summary>
        /// Get NFT by id.
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>NFT</returns>
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
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get NFT {utxo}!");
            }
        }

        /// <summary>
        /// Get NFT by id and index
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>NFT</returns>
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
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get NFT {utxo}!");
            }
        }

        public class VEDLNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintNFT(data.NFT);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintImageNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new ImageNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Tags = data.tags;
                    nft.Price = data.price;
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintNFT(nft);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintPostNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new PostNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Price = data.price;
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintNFT(nft);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintMusicNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new MusicNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Tags = data.tags;
                    nft.Price = data.price;
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintNFT(nft);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintTicketNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new TicketNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Tags = data.tags;
                    nft.Price = data.price;
                    nft.EventId = data.eventId;
                    nft.EventDate = DateTime.Parse(data.eventDate);
                    nft.AuthorLink = data.authorLink;
                    nft.VideoLink = data.videoLink;
                    nft.Location = data.location;
                    nft.LocationCoordinates = data.locationCoordinates;
                    nft.Seat = data.seat;
                    nft.TicketClass = data.ticketClass;

                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintNFT(nft);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("MultiMintImageNFT")]
        public async Task<(bool, string)> MultiMintImageNFT([FromBody] VEDLMintImageNFTDto data)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new ImageNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Tags = data.tags;
                    nft.Price = data.price;
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintMultiNFT(nft, data.coppies);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("MultiMintMusicNFT")]
        public async Task<(bool, string)> MultiMintMusicNFT([FromBody] VEDLMintMusicNFTDto data)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new MusicNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Tags = data.tags;
                    nft.Price = data.price;
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintMultiNFT(nft, data.coppies);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("MultiMintTicketNFT")]
        public async Task<(bool,List<string>)> MultiMintTicketNFT([FromBody] VEDLMintTicketNFTDto data)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = new TicketNFT("");
                    nft.Name = data.name;
                    nft.Description = data.description;
                    nft.Author = data.author;
                    nft.ImageLink = data.image;
                    nft.Link = data.link;
                    nft.Tags = data.tags;
                    nft.Price = data.price;
                    nft.EventId = data.eventId;
                    nft.EventDate = DateTime.Parse(data.eventDate);
                    nft.AuthorLink = data.authorLink;
                    nft.VideoLink = data.videoLink;
                    nft.Location = data.location;
                    nft.LocationCoordinates = data.locationCoordinates;
                    nft.Seat = data.seat;
                    nft.TicketClass = data.ticketClass;

                    (bool, List<string>) result = (false, new List<string>());
                    if (data.coppies > 20)
                    {
                        var cps = data.coppies;
                        var lots = 0;
                        while(cps > 0)
                        {
                            lots += cps % 20;
                            cps /= 20;
                        }

                    }
                    else
                    {
                        var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintMultiNFT(nft, data.coppies);
                        result.Item1 = res.Item1;
                        result.Item2.Add(res.Item2);
                    }
                    return result;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        [HttpPut]
        [Route("ChangeNFT")]
        public async Task<(bool, string)> ChangeNFT([FromBody] VEDLNFTDto data)
        {
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.ChangeNFT(data.NFT);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLSendNFTDto
        {
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
            /// Input NFT Utxo
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, data.NFTUtox, data.NFTUtoxIndex, 0, true);
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.SendNFT(data.receiver, nft, data.PriceWrite, data.Price);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLSplitNeblioDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.SplitNeblioCoin(new List<string>(){ data.receiver}, data.count, data.splittedAmount);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        #endregion

        #region Coruzant

        public class VEDLMintCoruzantProfileNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

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
                    
                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintNFT(nft);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        public class VEDLMintCoruzantArticleNFTDto
        {
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
                if (EconomyMainContext.Accounts.TryGetValue(data.address, out var account))
                {
                    if (account.Type != VEDrivers.Economy.Wallets.AccountTypes.Neblio)
                        throw new HttpResponseException((HttpStatusCode)501, $"This is not Neblio Account");
                    if ((account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.Address == string.Empty)
                        throw new HttpResponseException((HttpStatusCode)501, $"VEDriversLite Account is not initialized.");

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

                    var res = await (account as VEDrivers.Economy.Wallets.NeblioAccount).VEDLNeblioAccount.MintMultiNFT(nft, data.coppies);
                    return res;
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs, Account Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get Account Balances!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get Account {data.address} NFTs!");
            }
        }

        #endregion

    }
}
