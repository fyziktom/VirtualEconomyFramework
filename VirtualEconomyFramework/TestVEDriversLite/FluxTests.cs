using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.FluxAPI.InstanceControler;
using VEDriversLite.FluxAPI.InstanceControler.Instances.Dto;

namespace TestVEDriversLite
{
    public static  class FluxTests
    {
        private static IInstancesControler instancesController;
        
        [TestEntry]
        public static void InitializeInstances(string param)
        {
            InitializeInstancesAsync(param);
        }
        public static async Task InitializeInstancesAsync(string param)
        {
            instancesController = InstancesControlerFactory.GetInstanceControler(InstancesControlerType.Basic);

            await instancesController.Initialize("MainController", "Veframe", "Veframe", true);

            foreach (var instance in instancesController.Instances.Values)
                Console.WriteLine($"Average Ping: {instance.Name} - {instance.AveragePingTime} ms.");

            if (!string.IsNullOrEmpty(param) && param.ToLower().Contains("test"))
            {
                // test API response
                var nfthash = "4f8292ae54b09111e615ab554616929c83b84469732933d231657d3c21bec9a3";
                await GetNFTFromInstanceAsync(nfthash);
            }
            
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("-------------------");
            Console.WriteLine("End.");
        }

        [TestEntry]
        public static void GetNFTFromInstance(string param)
        {
            GetNFTFromInstanceAsync(param);
        }
        public static async Task GetNFTFromInstanceAsync(string param)
        {
            if (instancesController == null)
                await InitializeInstancesAsync(string.Empty);
            
            var topic = $"api/GetNFT/{param}";
            await SendRequest(topic, "default");
            Console.WriteLine("End.");
        }

        [TestEntry]
        public static void MultipleClientsGetNFTFromInstance(string param)
        {
            MultipleClientsGetNFTFromInstanceAsync(param);
        }
        public static async Task MultipleClientsGetNFTFromInstanceAsync(string param)
        {
            if (instancesController == null)
                await InitializeInstancesAsync(string.Empty);

            var topic = $"api/GetNFT/{param}";
            
            var numoftasks = 10;
            var tasks = new Task[numoftasks];
            for (var i = 0; i < numoftasks; i++)
            {
                tasks[i] = SendRequest(topic, $"Task-{i}", false, true);
            }
            await Task.WhenAll(tasks);
            
            Console.WriteLine("End.");
        }

        [TestEntry]
        public static void MultipleClientsGetMultipleNFTFromInstance(string param)
        {
            MultipleClientsGetMultipleNFTFromInstanceAsync(param);
        }
        public static async Task MultipleClientsGetMultipleNFTFromInstanceAsync(string param)
        {
            if (instancesController == null)
                await InitializeInstancesAsync(string.Empty);
            var split = param.Split(',');
            if (split == null) return;
            if (split.Length == 0 || split.Length == 1) return;
            var topic = $"api/GetNFT/{split[0]}";
            var topic1 = $"api/GetNFT/{split[1]}";

            var numoftasks = 10;
            var tasks = new Task[numoftasks];
            for (var i = 0; i < numoftasks; i++)
            {
                if (i < 5)
                    tasks[i] = SendRequest(topic, $"Task-{i}-firstNFT", false, true);
                else
                    tasks[i] = SendRequest(topic1, $"Task-{i}-secondNFT", false, true);
            }
            await Task.WhenAll(tasks);

            Console.WriteLine("End.");
        }

        private static async Task SendRequest(string topic, string clientId, bool printWholeObject = true, bool printSimpleResult = false)
        {
            Console.WriteLine("---------------------ClientId------------------------");
            Console.WriteLine("");
            Console.WriteLine(clientId);
            Console.WriteLine($"---------------------Topic - {clientId}------------------------");
            Console.WriteLine("");
            Console.WriteLine(topic);
            
            var resp = await instancesController.Request(new VEDriversLite.FluxAPI.InstanceControler.Instances.Dto.TaskToRunRequestDto()
            {
                Topic = topic,
                ClientId = clientId,
            });
            
            if (resp.Success)
            {
                Console.WriteLine($"---------------------Response - {clientId}------------------------");
                Console.WriteLine("");                
                Console.WriteLine($"Status: {resp.Success}, Topic: {(resp.Value as TaskToRunResponseDto).Topic}");
                var datastring = Encoding.UTF8.GetString((resp.Value as TaskToRunResponseDto).TaskOutputData);
                if (printSimpleResult || printWholeObject)
                {
                    Console.WriteLine($"---------------------Data - {clientId}------------------------");
                    Console.WriteLine("");
                }
                if (printWholeObject)
                    Console.WriteLine($"{JsonConvert.SerializeObject(JsonConvert.DeserializeObject(datastring), Formatting.Indented)}");
                else if (printSimpleResult)
                {
                    var data = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(datastring), Formatting.Indented);
                    if (data.Length > 100) 
                        data = data.Substring(0, 100) + "...";
                    Console.WriteLine($"{data}");
                }
            }
            else
                Console.WriteLine($"Cannot get response - Response status: {resp.Success}, Topic {topic}");

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("-----------------------------------------------");
        }
    }
}
