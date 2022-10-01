using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Energy.Consumers.Dto;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.Energy.Consumers
{
    public static class ConsumerFactory
    {
        public static IConsumer GetConsumer(ConsumerType type, string name, string parentId, string id = null)
        {
            IConsumer consumer = null;
            switch(type)
            {
                case ConsumerType.Device:
                    consumer = new Device();
                    break;
                case ConsumerType.DevicesGroup:
                    consumer = new DeviceGroup();
                    break;
                case ConsumerType.GroupNetwork:
                    consumer = new GroupNetwork();
                    break;
            }
            if (consumer != null)
            {
                if (id != null)
                    consumer.Id = id;
                consumer.Name = name;
                consumer.ParentId = parentId;
            }
            return consumer;
        }

        public static IConsumer GetConsumerFromJson(string json)
        {
            ConsumerConfigDto loaded = null;
            try
            {
                loaded = JsonConvert.DeserializeObject<ConsumerConfigDto>(json);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot deserialize the consumer from the json. Exception: " + ex.Message);
                throw new Exception("Cannot deserialize the consumer from the json. Exception: " + ex.Message);
            }

            IConsumer consumer = null;
            if (loaded != null)
            {
                if (loaded.Type == EntityType.Consumer)
                {
                    try
                    {
                        switch (loaded.ConsumerType)
                        {
                            case ConsumerType.Device:
                                consumer = JsonConvert.DeserializeObject<Device>(json) ?? new Device();
                                break;
                            case ConsumerType.DevicesGroup:
                                consumer = JsonConvert.DeserializeObject<DeviceGroup>(json) ?? new DeviceGroup();
                                break;
                            case ConsumerType.GroupNetwork:
                                consumer = JsonConvert.DeserializeObject<GroupNetwork>(json) ?? new GroupNetwork();
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot deserialize the consumer from the json. Exception: " + ex.Message);
                        throw new Exception("Cannot deserialize the consumer from the json. Exception: " + ex.Message);
                    }
                }
            }

            return consumer;
        }
    }
}
