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
using VEDriversLite.Admin.Dto;
using VEDriversLite.Admin;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Extensions.WooCommerce.Dto;
using VEDriversLite.Extensions.WooCommerce;

namespace VENFTApp_Server.Controllers
{
    [Route("woocommerceapi")]
    [ApiController]
    public class WooCommerceController : ControllerBase
    {

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetAllOrders")]
        public async Task<List<Order>> GetAllOrders()
        {
            return WooCommerceHelpers.Shop.Orders.Values.ToList();
        }

        [AllowCrossSiteJsonAttribute]
        [HttpGet("GetOrderInfo/{orderkey}")]
        public async Task<Order> GetOrderInfo(string orderkey)
        {
            if (WooCommerceHelpers.Shop.Orders.TryGetValue(orderkey, out var order))
                return order;
            else
                return new Order();
        }

        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("ConnectDogeAccount")]
        public async Task<string> ConnectDogeAccount([FromBody] ConnectDogeAccountDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return "Wrong signature or missing action request.";
                }
                await WooCommerceHelpers.Shop.ConnectDogeAccount(data.dogeAccountAddress);
                return "OK";
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Connect doge account. " + ex.Message);
            }
        }
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("DisconnectDogeAccount")]
        public async Task<string> DisconnectDogeAccount([FromBody] ConnectDogeAccountDto data)
        {
            try
            {
                if (MainDataContext.IsAPIWithCredentials)
                {
                    var vres = await AccountHandler.VerifyAdminAction(data.adminCredentials.Admin, data.adminCredentials.Signature, data.adminCredentials.Message);
                    if (vres == null)
                        return "Wrong signature or missing action request.";
                }
                await WooCommerceHelpers.Shop.DisconnectDogeAccount();
                return "OK";
            }
            catch (Exception ex)
            {
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot Disconnect doge account. " + ex.Message);
            }
        }
    }
}
