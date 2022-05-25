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

            // test API response
            var nfthash = "4f8292ae54b09111e615ab554616929c83b84469732933d231657d3c21bec9a3";
            await GetNFTFromInstanceAsync(nfthash);
            
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
            var resp = await instancesController.Request(new VEDriversLite.FluxAPI.InstanceControler.Instances.Dto.TaskToRunRequestDto()
            {
                Topic = topic
            });

            Console.WriteLine("---------------------Topic------------------------");
            Console.WriteLine(topic);
            Console.WriteLine("---------------------Response------------------------");
            if (resp.Success)
            {
                Console.WriteLine($"Response: {resp.Success}, Topic: {(resp.Value as TaskToRunResponseDto).Topic}, ResponseData:");
                var datastring = Encoding.UTF8.GetString((resp.Value as TaskToRunResponseDto).TaskOutputData);
                Console.WriteLine($"{JsonConvert.SerializeObject(JsonConvert.DeserializeObject(datastring), Formatting.Indented)}");
            }
            else
                Console.WriteLine($"Cannot get response - Response status: {resp.Success}, Topic {topic}");

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("-------------------");
            Console.WriteLine("End.");
        }
    }
}
