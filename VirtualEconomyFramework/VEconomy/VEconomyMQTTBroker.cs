using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    public class VEconomyMQTTBroker : BackgroundService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;

        public VEconomyMQTTBroker(IConfiguration settings, IHostApplicationLifetime lifetime)
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
                EconomyMainContext.MQTTServer = new MQTTServer("VEconomyBroker");
                
                _ = Task.Run(async () =>
                {
                    while (!stopToken.IsCancellationRequested)
                    {
                        if (reconnect)
                        {
                            try
                            {
                                // first wait until MQTT client exists and it is connected to the broker
                                if (EconomyMainContext.MQTTServer != null && !EconomyMainContext.MQTTServer.IsConnected)
                                {
                                    await EconomyMainContext.MQTTServer.RunServer(stopToken, settings);
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Info("Cannot create MQTT Broker, please check the MQTT configuration!");
                                reconnect = true;
                            }

                        }

                        await Task.Delay(5000); // wait before checking connection
                        if (!EconomyMainContext.MQTTServer.IsConnected)
                            reconnect = true;
                    }

                    log.Info($"Virtual Economy Framework MQTT Broker handler task stopped");
                });
                
            }
            catch (Exception ex)
            {
                log.Fatal("Cannot start Virtual Economy MQTT Broker", ex);
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
