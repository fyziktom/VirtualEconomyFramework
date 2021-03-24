using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEconomy.Common;
using VEDrivers.Common;
using VEDrivers.Economy.Wallets;

namespace VEconomy
{   
    public class VEconomyCore : BackgroundService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;

        public VEconomyCore(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            this.settings = settings; //startup configuration in appsettings.json
            this.lifetime = lifetime;

            EconomyMainContext.CommonConfig = settings;

            EconomyMainContext.MQTT = new MQTTConfig();
            settings.GetSection("MQTT").Bind(EconomyMainContext.MQTT);
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            await Task.Delay(1);
            var reconnect = true;

            try
            {
                EconomyMainContext.MQTTClient = new MQTTClient("VEconomy");
                
                _ = Task.Run(async () =>
                {
                    while (!stopToken.IsCancellationRequested)
                    {
                        /*
                        while(!EconomyMainContext.MQTTServerIsStarted && !stopToken.IsCancellationRequested)
                        {
                            // wait until broker is started
                            await Task.Delay(500);
                        }*/

                        if (reconnect)
                        {
                            try
                            {
                                // first wait until MQTT client exists and it is connected to the broker
                                if (EconomyMainContext.MQTTClient != null && !EconomyMainContext.MQTTClient.IsConnected)
                                {
                                    EconomyMainContext.MQTTClient.Dispose();
                                    await EconomyMainContext.MQTTClient.RunClient(stopToken, settings, new string[] { });
                                }
                            }
                            catch (Exception ex)
                            {
                                await Task.Delay(1000);
                                log.Info($"Cannot create first connection to MQTT, please check the MQTT Broker!: {ex}");
                                reconnect = true;
                            }

                        }

                        await Task.Delay(5000); // wait before checking connection
                        if (!EconomyMainContext.MQTTClient.IsConnected)
                            reconnect = true;
                    }

                    log.Info($"Virtual Economy Framework wallet handler task stopped");
                });
                
            }
            catch (Exception ex)
            {
                log.Fatal("Cannot start Virtual Economy server", ex);
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
