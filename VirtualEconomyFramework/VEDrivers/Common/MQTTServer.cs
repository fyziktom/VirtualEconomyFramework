using log4net;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VEDrivers.Common
{
    public class MQTTServer
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CancellationToken stopToken;
        private IMqttServer mqttServer;
        private string name = string.Empty;
        public bool IsConnected
        {
            get
            {
                if (mqttServer != null)
                    return mqttServer.IsStarted;
                else
                    return false;
            }
        }

        public MQTTServer(string _name)
        {
            name = _name;
        }
            
        private IMqttServerOptions Initialize(IConfiguration settings)
        {
            //if (mqttServer != null) throw new Exception($"{name} Duplicate start");

            var config = new MQTTConfig();
            settings.GetSection("MQTT").Bind(config);

            var factory = new MqttFactory();
            mqttServer = factory.CreateMqttServer();

            return new MqttServerOptionsBuilder()
                        .WithConnectionBacklog(100)
                        .WithConnectionValidator(c => { if (c.Username != config.User) return; if (c.Password != config.Pass) return; c.ReasonCode = MqttConnectReasonCode.Success; })
                        .WithDefaultEndpointPort(config.Port).Build();
        }

        public async Task RunServer(CancellationToken stopToken, IConfiguration settings)
        {
            this.stopToken = stopToken;
            var options = Initialize(settings);

            await mqttServer.StartAsync(options);
        }
    }
}
