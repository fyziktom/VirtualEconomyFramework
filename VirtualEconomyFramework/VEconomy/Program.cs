using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.AspNetCore.Extensions;
using VEconomy.Controllers;

namespace VEconomy
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static Program()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }
        static void Main(string[] args)
        {
            log.Info($"Virtual Economy Server start");
            CreateHostBuilder(args).Build().Run();
            log.Info("Virtual Economy Server stop");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(
                    (bcont, o) =>
                    {
                        var config = new VEDrivers.Common.MQTTConfig();
                        bcont.Configuration.GetSection("MQTT").Bind(config);
                        var mainport = bcont.Configuration.GetValue<int>("MainPort");
                        if (mainport == 0)
                            mainport = 8080;
                        o.ListenAnyIP(config.Port, l => l.UseMqtt()); // MQTT pipeline
                        o.ListenAnyIP(config.WSPort); // Default HTTP pipeline
                        o.ListenAnyIP(mainport); // Default HTTP pipeline
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<VEconomyCore>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ExchangeService>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<WalletHandlerCore>();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: true);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    //logging.AddProvider(new Log4NetProvider()); //not needed, using log4net directly
                })
                .UseWindowsService()
                .UseSystemd();
    }
}
