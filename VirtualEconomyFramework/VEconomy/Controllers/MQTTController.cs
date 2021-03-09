using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEDrivers.Common;

namespace VEconomy.Controllers
{
    public class MQTTController
    {
        public static void RegisterTopics()
        {
            EconomyMainContext.MQTTTopics.Add("+/Started");
        }

        //Main mapping for API commands - MQTT Topics and functions

        public static Dictionary<string, Func<string, string, string, Task>> ApiFunctions = new Dictionary<string, Func<string, string, string, Task>>
        {
            { "Started", Started }
        };

        /// <summary>
        /// Find match for command in ApiDictionary and invoke function
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="command">Command to do - MQTT topic</param>
        /// <param name="payload">JSON or simple string</param>
        /// <returns></returns>
        public static async Task ProcessMessage(string id, string command, string payload)
        {
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(command))
            {
                // messages for this PL
                if (ApiFunctions.ContainsKey(command))
                {
                    await ApiFunctions[command].Invoke(id, command, payload);
                }
            }
        }

        /// <summary>
        /// Common Api Function - Started
        /// </summary>
        /// <param name="payload">bool TRUE or FALSE</param>
        /// <returns></returns>
        public static async Task Started(string id, string command, string payload)
        {
            if (payload == "TRUE")
            {
                // do something
                Console.WriteLine($"Just doing something usefull :) {command}, {payload}");
            }
        }
    }
}
