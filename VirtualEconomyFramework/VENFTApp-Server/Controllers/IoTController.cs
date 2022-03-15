using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    [Route("iotapi")]
    [ApiController]
    public class IoTController : Controller
    {
        
        public class GetTemperaturesRequestDto
        {
            public string address { get; set; } = string.Empty;
            public int start { get; set; } = 0;
            public int loadmax { get; set; } = 0;
            
        }
        public class GetTemperaturesResponseDto
        {
            public string address { get; set; } = string.Empty;
            public List<TemperatureDataDto> data { get; set; } = new List<TemperatureDataDto>();
        }

        public class TemperatureDataDto
        {
            public string txid { get; set; } = string.Empty;
            public DateTime datetime { get; set; } = DateTime.UtcNow;
            public string description { get; set; } = string.Empty;
            public double temperature { get; set; } = 0.0;
            public double latitude { get; set; } = 0.0;
            public double longitude { get; set; } = 0.0;
        }

        /// <summary>
        /// Get Neblio account balances. 
        /// This data is loaded from VEDriversLite
        /// </summary>
        /// <returns>Account Balance Response Dto</returns>
        [AllowCrossSiteJsonAttribute]
        [HttpPost]
        [Route("GetTemperatures")]
        public async Task<GetTemperaturesResponseDto> GetTemperatures([FromBody] GetTemperaturesRequestDto data)
        {

            if (string.IsNullOrEmpty(data.address))
                return new GetTemperaturesResponseDto();

            var dto = new GetTemperaturesResponseDto();
            dto.address = data.address;

            if (MainDataContext.ObservedAccountsTabs.TryGetValue(data.address, out var tab))
            {
                if (data.start < tab.NFTs.Count)
                {
                    for (var i = data.start; i < tab.NFTs.Count && i < (data.start + data.loadmax); i++)
                    {
                        var nft = tab.NFTs[i];
                        if (nft.Type == NFTTypes.IoTMessage && !string.IsNullOrEmpty(nft.Description))
                        {
                            try
                            {
                                var parsed = JsonConvert.DeserializeObject<VEDriversLite.Devices.APIs.HARDWARIO.Dto.HWDto>(nft.Description);
                                if (parsed != null)
                                {
                                    dto.data.Add(new TemperatureDataDto()
                                    {
                                        txid = nft.Utxo,
                                        datetime = parsed.created_at,
                                        description = "IoT Device Temperature",
                                        temperature = parsed.data.sensor.thermometer.temperature,
                                        latitude = parsed.data.tracking.latitude,
                                        longitude = parsed.data.tracking.longitude
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot parse the value from IoT Message.");
                            }
                        }
                    }
                }
            }

            dto.data = dto.data.OrderBy(d => d.temperature).ToList();
            return dto;

        }

    }
}
