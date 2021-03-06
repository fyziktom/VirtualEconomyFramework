﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public class HTTPApiRequestNode : CommonNode
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public HTTPApiRequestNode(Guid id, Guid accId, string name, bool isActivated, NodeActionParameters parameters)
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
        }

        public override event EventHandler<NodeActionRequestArgs> ActionRequest;
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

        public override async Task<NodeActionFinishedArgs> InvokeNodeFunction(NodeActionTriggerTypes actionType, string[] otherData, string altFunction = "")
        {
            if (!(bool)IsActivated)
                return await Task.FromResult(new NodeActionFinishedArgs() { result = "NOT_ACTIVATED", data = "HTTP API Node - Node is not activated. You cannot invoke action!" });

            if (actionType != ActualTriggerType)
                return await Task.FromResult(new NodeActionFinishedArgs() { result = "NOT_INVOKED", data = $"HTTP API Node - Node is not set to this {Enum.GetName(actionType)} of trigger. It is set to {Enum.GetName(ActualTriggerType)}!" });

            JSResultDto jsRes = new JSResultDto();
            // Node Custom JavaScript call
            if (ParsedParams.IsScriptActive)
            {
                if (!string.IsNullOrEmpty(ParsedParams.Script) && (otherData.Length > 0))
                {
                    if (!string.IsNullOrEmpty(altFunction))
                        jsRes = JSScriptHelper.RunNodeJsScript(altFunction, ParsedParams.ScriptParametersList, otherData);

                    jsRes = JSScriptHelper.RunNodeJsScript(ParsedParams.Script, ParsedParams.ScriptParametersList, otherData);

                    if (!jsRes.done)
                        return await Task.FromResult(new NodeActionFinishedArgs() { result = "NOT_INVOKED", data = $"HTTP API Node - Custom JS script runned with error {jsRes.payload}!" });
                }
            }

            try
            {
                var p = JsonConvert.DeserializeObject<HTTPApiRequestNodeParams>(ParsedParams.Parameters);

                if (p != null)
                {
                    var url = string.Empty;
                    if (!p.UseHttps)
                    {
                        url = $"http://{p.Host}";
                    }
                    else
                    {
                        url = $"https://{p.Host}";
                    }

                    if (!string.IsNullOrEmpty(p.Port))
                        url += $":{p.Port}/";
                    else
                        url += "/";

                    if (!string.IsNullOrEmpty(p.ApiCommand))
                        url += p.ApiCommand;

                    if (ParsedParams.TimeDelay > 0)
                        await Task.Delay(ParsedParams.TimeDelay);

                    var payload = string.Empty;
                    if (ParsedParams.IsScriptActive && !string.IsNullOrEmpty(jsRes.payload))
                        payload = jsRes.payload;
                    else
                        payload = p.Data;

                    LastPayload = payload;
                    LastOtherData = otherData;

                    var res = string.Empty;

                    if (ParsedParams.Command != null) {

                        switch (ParsedParams.Command)
                        {
                            case "GET":
                                res = await SendGETRequest(url, payload);
                                break;
                            case "PUT":
                                res = await SendPURequest(url, payload);
                                break;
                            case "":
                                res = "HTTP API Node - Command cannot be empty. Please fill GET, POST or PUT!";
                                log.Warn($"Node {Name} cannot send HTTP Request. Command is empty. Please fill GET, POST or PUT!");
                                break;
                        }
                    }
                    else
                    {
                        res = "HTTP API Node - Command cannot be null. Please fill GET, POST or PUT!";
                        log.Warn($"Node {Name} cannot send HTTP Request. Command is null. Please fill GET, POST or PUT!");
                    }

                    ActionFinished?.Invoke(this, new NodeActionFinishedArgs() { result = "OK", data = res });

                    return await Task.FromResult(new NodeActionFinishedArgs() { result = "OK", data = res });
                }
                else
                {
                    return await Task.FromResult(new NodeActionFinishedArgs() { result = "ERROR", data = "HTTP API Node - Cannot parse the parameters!" });
                    log.Warn($"Node {Name} cannot send HTTP Request. Cannot parse the parameters!");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Node {Name} cannot send HTTP Request. ", ex);
            }

            return await Task.FromResult(new NodeActionFinishedArgs() { result = "ERROR", data = "Unexpected error!" });
        }

        private async Task<string> SendGETRequest(string url, string payload)
        {
            string html = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (WebResponse response = await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                Console.WriteLine(html);
                return html;
            }
            catch(Exception ex)
            {
                log.Error($"Node {Name} cannot send HTTP Request to the: {url} ", ex);
                return $"ERROR when sending HTTP Request to: {url}";
            }
        }

        private async Task<string> SendPURequest(string url, string payload)
        {
            string output = string.Empty;
            string Error = string.Empty;
            WebRequest req = WebRequest.Create(url);

            try
            {
                
                req.Method = "PUT";
                req.Timeout = 100000;
                req.ContentType = "application/json";
                byte[] sentData = Encoding.UTF8.GetBytes(payload);
                req.ContentLength = sentData.Length;

                using (System.IO.Stream sendStream = req.GetRequestStream())
                {
                    sendStream.Write(sentData, 0, sentData.Length);
                    sendStream.Close();

                }

                WebResponse res = req.GetResponse();
                Stream ReceiveStream = res.GetResponseStream();
                using (StreamReader sr = new
                StreamReader(ReceiveStream, Encoding.UTF8))
                {
                    char[] read = new char[256];
                    int count = sr.Read(read, 0, 256);

                    while (count > 0)
                    {
                        string str = new string(read, 0, count);
                        output += str;
                        count = sr.Read(read, 0, 256);
                    }
                }

                Console.WriteLine(output);
                return output;
            }
            catch (Exception ex)
            {
                log.Error($"Node {Name} cannot send HTTP Request to the: {url} ", ex);
                return $"ERROR when sending HTTP Request to: {url}";
            }
        }

        public override object GetNodeParametersCarrier()
        {
            return (object)new HTTPApiRequestNodeParams();
        }

    }
}
