using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VEDrivers.Common;

namespace VEQTWalletExecturo
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Program()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }

        public static void Main(string[] args)
        {
            log.Info($"QTWallet start");
            CreateHostBuilder(args).Build().Run();
            log.Info("QTWallet stop");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<QTWalletExecutor>();
                })
                /* uncomment when you want run reddcoin wallet too
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<QTReddCoinWalletExcecutor>();
                })
                */
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
               .UseWindowsService()
               .UseSystemd();
    }
}
