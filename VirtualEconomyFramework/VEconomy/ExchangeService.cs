using Binance.Net.Enums;
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

namespace VEconomy
{   
    public class ExchangeService : BackgroundService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;

        public ExchangeService(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            this.settings = settings; //startup configuration in appsettings.json
            this.lifetime = lifetime; 

        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            await Task.Delay(1);
            try
            {
                EconomyMainContext.CommonBinanceSocketClient = new Binance.Net.BinanceSocketClient();
                EconomyMainContext.ExchangeDataProvider = new VEDrivers.Economy.Exchanges.BinanceDataProvider(
                    EconomyMainContext.CommonBinanceSocketClient, 
                    "NEBLBTC", KlineInterval.OneMinute);

                await EconomyMainContext.ExchangeDataProvider.Start();
            }
            catch (Exception ex)
            {
                log.Fatal("Cannot start Exchange Service", ex);
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
