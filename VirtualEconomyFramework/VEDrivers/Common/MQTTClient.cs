using log4net;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VEDrivers.Common
{
    public class MQTTClient
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IMqttClient mqttClient;
        private CancellationToken stopToken;
        private string name; //only for logging
        public bool IsConnected
        {
            get
            {
                if (mqttClient != null)
                    return mqttClient.IsConnected;
                else
                    return false;
            }
        }

        public MQTTClient(string name)
        {
            this.name = name;
        }

        public Action<string, string> MessageReceived { set; private get; }

        public async Task PostObjectAsJSON<T>(string topic, T payload, bool retain = true) where T : class
        {
            try
            {
                var content = JsonConvert.SerializeObject(payload);

                if (mqttClient != null)
                {
                    if (mqttClient.IsConnected)
                        await mqttClient.PublishAsync($"{topic}", content, retain);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot send send object. Error: " + ex.Message);
            }
        }

        public async Task PostObjectAsJSONString(string topic, string payload, bool retain = true)
        {
            try
            {
                if (mqttClient != null)
                {
                    if (mqttClient.IsConnected)
                        await mqttClient.PublishAsync($"{topic}", payload, retain);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot send send object. Error: " + ex.Message);
            }
        }
        
        private IMqttClientOptions Initialize(IConfiguration settings)
        {
            if (mqttClient != null) throw new Exception($"{name} Duplicate start");

            var config = new MQTTConfig();
            settings.GetSection("MQTT").Bind(config);

            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            return new MqttClientOptionsBuilder()
                .WithTcpServer(config.Host, config.Port)
                .WithCredentials(config.User, config.Pass)
                .Build();
        }

        public IMqttClientOptions Initialize(string user, string password, string host = "127.0.0.1", int port = 1883)
        {
            if (mqttClient != null) throw new Exception($"{name} Duplicate start");

            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            return new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithCredentials(user, password)
                .Build();
        }

        public async Task RunClient(CancellationToken stopToken, IConfiguration settings, IEnumerable<string> topics)
        {
            this.stopToken = stopToken;
            var options = Initialize(settings);

            mqttClient.UseConnectedHandler(async e =>
            {
                log.Info($"{name} connected with server");
                // Subscribe to a topic
                foreach (var topic in topics)
                {
                    await mqttClient.SubscribeAsync(new MqttTopicFilter() { Topic = topic });
                    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
                }
                log.Debug($"{name} Subscribed to topics");
            });

            mqttClient.UseDisconnectedHandler(async e => { await DisconnectHandler(options); });

            mqttClient.UseApplicationMessageReceivedHandler(e => MessageHandler(e));

            await mqttClient.ConnectAsync(options, stopToken);

        }

        public async Task RunClient(CancellationToken stopToken, string user, string password, string host, int port, IEnumerable<string> topics)
        {
            this.stopToken = stopToken;
            var options = Initialize(user, password, host, port);

            mqttClient.UseConnectedHandler(async e =>
            {
                log.Info($"{name} connected with server");
                // Subscribe to a topic
                foreach (var topic in topics)
                {
                    await mqttClient.SubscribeAsync(new MqttTopicFilter() { Topic = topic });
                    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
                }
                log.Debug($"{name} Subscribed to topics");
            });

            mqttClient.UseDisconnectedHandler(async e => { await DisconnectHandler(options); });

            mqttClient.UseApplicationMessageReceivedHandler(e => MessageHandler(e));

            await mqttClient.ConnectAsync(options, stopToken);

        }

        private void MessageHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            if (log.IsDebugEnabled) log.Debug($"MessageReceived on Topic '{topic}'{Environment.NewLine}  Payload:'{payload}'{Environment.NewLine}QoS:  '{e.ApplicationMessage.QualityOfServiceLevel}'{Environment.NewLine}   Retain: '{e.ApplicationMessage.Retain}'");

            //MQTT client is not capable of parallel messsage processing https://github.com/chkr1011/MQTTnet/issues/732
            if (MessageReceived != null)
            {
                Task.Run(() => MessageReceived(topic, payload));
            }
        }

        public void UnsuscribeTopic(string[] topics)
        {
            if (mqttClient != null)
            {
                if (mqttClient.IsConnected)
                {
                    mqttClient.UnsubscribeAsync(topics);
                }
            }
        }

        private async Task DisconnectHandler(IMqttClientOptions options)
        {
            log.Error($"{name} disconnected from server");
            await Task.Delay(TimeSpan.FromSeconds(5), stopToken);

            try
            {
                await mqttClient.ConnectAsync(options, stopToken); // Since 3.0.5 with CancellationToken
            }
            catch
            {
                log.Warn($"{name} reconnecting failed");
            }
        }

        public void Dispose()
        {
            if (mqttClient != null)
            {
                try
                {
                    if (mqttClient.IsConnected)
                        mqttClient.DisconnectAsync().GetAwaiter().GetResult();
                }
                catch
                {
                    //todo, but probably not connected
                }

                mqttClient?.Dispose();
                mqttClient = null;
            }
        }
    }
}
