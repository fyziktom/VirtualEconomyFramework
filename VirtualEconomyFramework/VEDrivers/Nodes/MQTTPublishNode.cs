using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public class MQTTPublishNode : CommonNode
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static MQTTClient mqttClient;
        public MQTTPublishNode(Guid id, Guid accId, string name, bool isActivated, NodeActionParameters parameters)
        {
            Type = NodeTypes.HTTPAPIRequest;
            Id = id;
            AccountId = accId;
            Name = name;
            if (parameters != null)
            {
                LoadParameters(parameters);
                SetNodeTriggerType(parameters.TriggerType);
            }
            IsActivated = isActivated;
            ActualTriggerType = NodeActionTriggerTypes.None;
            mqttClient = new MQTTClient("node-" + id.ToString());
        }

        public override event EventHandler<NodeActionFinishedArgs> ActionFinished;

        public override void Activate()
        {
            IsActivated = true;
        }
        public override void DeActivate()
        {
            IsActivated = false;
        }

        public override void LoadParameters(NodeActionParameters parameters)
        {
            Parameters = JsonConvert.SerializeObject(parameters);
            var o = new object();
            lock (o)
            {
                ParsedParams = parameters;
            }
        }
        
        public override void SetNodeTriggerType(NodeActionTriggerTypes type)
        {
            ActualTriggerType = type;
            ParsedParams.TriggerType = type;
            Parameters = JsonConvert.SerializeObject(ParsedParams);
        }

        public override async Task<NodeActionFinishedArgs> InvokeNodeFunction(NodeActionTriggerTypes actionType, string[] otherData)
        {
            if (!(bool)IsActivated)
                return await Task.FromResult(new NodeActionFinishedArgs() { result = "NOT_ACTIVATED", data = "MQTT Publish Node - Node is not activated. You cannot invoke action!" });

            if (actionType != ActualTriggerType)
                return await Task.FromResult(new NodeActionFinishedArgs() { result = "NOT_INVOKED", data = $"MQTT Publish Node - Node is not set to this {Enum.GetName(actionType)} of trigger. It is set to {Enum.GetName(ActualTriggerType)}!" });

            // Node Custom JavaScript call
            if (ParsedParams.IsScriptActive)
            {
                if (!string.IsNullOrEmpty(ParsedParams.Script) && (otherData.Length > 0))
                {
                    var res = JSScriptHelper.RunNodeJsScript(ParsedParams.Script, ParsedParams.ScriptParametersList, otherData);

                    if (!res.done)
                        return await Task.FromResult(new NodeActionFinishedArgs() { result = "NOT_INVOKED", data = $"MQTT Publish Node - Custom JS script runned with error {res.payload}!" });
                }
            }

            try
            {
                var p = JsonConvert.DeserializeObject<MQTTNodeParams>(ParsedParams.Parameters);

                if (p != null)
                {
                    if (ParsedParams.TimeDelay > 0)
                        await Task.Delay(ParsedParams.TimeDelay);

                    var res = string.Empty;

                    if (ParsedParams.Command != null) 
                    {
                        ////////////////////////////////////////////////
                        // TODO publish ParsedParams.Command as topic //
                        ////////////////////////////////////////////////
                    }
                    else
                    {
                        res = "MQTT Publish Node - Command cannot be null. Please fill full Topic name!";
                        log.Warn($"Node {Name} MQTT Publish Node - Command cannot be null. Please fill full Topic name!!");
                    }

                    ActionFinished?.Invoke(this, new NodeActionFinishedArgs() { result = "OK", data = res });

                    return await Task.FromResult(new NodeActionFinishedArgs() { result = "OK", data = res });
                }
                else
                {
                    log.Warn($"Node {Name} cannot send MQTT Publish. Cannot parse the parameters!");
                    return await Task.FromResult(new NodeActionFinishedArgs() { result = "ERROR", data = "MQTT Publish Node - Cannot parse the parameters!" });
                }
            }
            catch (Exception ex)
            {
                log.Error($"Node {Name} cannot send MQTT Publish. ", ex);
            }

            return await Task.FromResult(new NodeActionFinishedArgs() { result = "ERROR", data = "Unexpected error!" });
        }

        public override object GetNodeParametersCarrier()
        {
            return (object)new MQTTNodeParams();
        }
    }
}
