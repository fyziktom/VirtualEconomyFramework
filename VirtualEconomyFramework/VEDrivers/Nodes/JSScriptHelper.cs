using Jint;
using Jint.Native;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public static class JSScriptHelper
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static string RunJavaScript(string function, string functionName, string[] param)
        {
            JsValue[] parameters = new JsValue[param.Length];

            // remove spaces before and after
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = param[i].TrimStart().TrimEnd();
                parameters[i] = new JsValue(param[i]);
            }

            functionName = functionName.TrimStart().TrimEnd();

            var engine = new Engine()
                .SetValue("log", new Action<object>(Console.WriteLine))
                .Execute(function)
                .GetValue(functionName);

            var res = engine.Invoke(parameters);

            Console.WriteLine($"Result of JS script is: {res}");

            return res.ToString();
        }

        private static JSResultDto getErrorJSResult(string message)
        {
            return new JSResultDto() { done = false, payload = "{ \"error\": \"" + message + "\"" };
        }

        public static JSResultDto RunNodeJsScript(string script, List<string> nodeParams, string[] otherData)
        {
            if (otherData.Length < 2)
                return getErrorJSResult("ERROR IN SCRIPT - Not added payload!");

            var p = new List<string>();

            foreach (var o in otherData)
                p.Add(o);

            foreach (var o in nodeParams)
                p.Add(o);

            try
            {
                var scriptResult = JSScriptHelper.RunJavaScript(script, "nodeJSfunction", p.ToArray());

                if (string.IsNullOrEmpty(scriptResult))
                    return getErrorJSResult("ERROR IN SCRIPT - Script Result Not Received");

                var srParsed = JsonConvert.DeserializeObject<JSResultDto>(scriptResult);

                if (srParsed == null)
                    return getErrorJSResult("ERROR IN SCRIPT - wrong return format. Must match JSResultDto!");

                if (srParsed.done)
                    return srParsed;
                else
                    return srParsed;

            }
            catch (Exception ex)
            {
                log.Error("Error in running node custom JavaScript", ex);
                throw new Exception($"Cannot run node custom JavaScirpt: {ex}");
            }
        }
    }
}
