using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using VEDriversLite.NeblioAPI;
using Newtonsoft.Json;
using VEconomy.Common;
using VEDrivers.Bookmarks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Receipt;
using VEDrivers.Economy.Shops;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets;
using VEDrivers.Messages;
using VEDrivers.Messages.DTO;
using VEDrivers.Nodes;
using VEDrivers.Nodes.Dto;
using VEDrivers.Security;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace VEconomy.Controllers
{
    [Route("rpc")]
    [ApiController]
    public class RPCControler : Controller
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDbConnectorService dbService;
        private readonly DbEconomyContext _context;
        public RPCControler(DbEconomyContext context)
        {
            _context = context;
            dbService = new DbConnectorService(_context);
        }

        public class BroadcastRawTxData
        {
            public string txhex { get; set; } = string.Empty;
        }

        /// <summary>
        /// Broadcast Raw signed transaction through RPC node
        /// </summary>
        /// <param name="walletData"></param>
        /// <returns>tx id if success/returns>
        [HttpPost]
        [Route("BroadcastRawTx")]
        //[Authorize(Rights.Administration)]
        public async Task<string> BroadcastRawTx([FromBody] BroadcastRawTxData data)
        {
            var resp = "ERROR";

            try
            {
                if (EconomyMainContext.WorkWithQTRPC)
                {
                    if (EconomyMainContext.QTRPCClient.IsConnected)
                    {
                        var kr = await EconomyMainContext.QTRPCClient.RPCLocalCommandSplitedAsync("sendrawtransaction", new string[] { data.txhex });
                        var rr = JsonConvert.DeserializeObject<JObject>(kr);
                        var ores = rr["result"].ToString();
                        return ores;
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot broadcast raw tx.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot broadcast raw tx!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Broadcast raw tx!");
            }

            return resp;
        }

        /// <summary>
        /// Get Tx info
        /// </summary>
        /// <param name="tx"></param>
        /// <returns>returns tx info</returns>
        [HttpGet]
        [Route("GetTx/{tx}")]
        //[Authorize(Rights.Administration)]
        public async Task<string> GetTx(string tx)
        {
            var resp = "ERROR";

            try
            {
                if (EconomyMainContext.WorkWithQTRPC)
                {
                    if (EconomyMainContext.QTRPCClient.IsConnected)
                    {
                        var kr = await EconomyMainContext.QTRPCClient.RPCLocalCommandSplitedAsync("gettransaction", new string[] { tx });
                        var rr = JsonConvert.DeserializeObject<JObject>(kr);
                        var ores = rr["result"].ToString();
                        return ores;
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get tx.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get tx!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get tx!");
            }

            return resp;
        }

        /// <summary>
        /// Get Block info
        /// </summary>
        /// <param name="tx"></param>
        /// <returns>Returns block info</returns>
        [HttpGet]
        [Route("GetBlock/{hash}")]
        //[Authorize(Rights.Administration)]
        public async Task<string> GetBlock(string hash)
        {
            var resp = "ERROR";

            try
            {
                if (EconomyMainContext.WorkWithQTRPC)
                {
                    if (EconomyMainContext.QTRPCClient.IsConnected)
                    {
                        var kr = await EconomyMainContext.QTRPCClient.RPCLocalCommandSplitedAsync("getblock", new string[] { hash });
                        var rr = JsonConvert.DeserializeObject<JObject>(kr);
                        var ores = rr["result"].ToString();
                        return ores;
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot get block.");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot get block!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot get block!");
            }

            return resp;
        }

    }
}
