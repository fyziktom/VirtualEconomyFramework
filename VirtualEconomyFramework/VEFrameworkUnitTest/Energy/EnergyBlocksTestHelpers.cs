using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Sources;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.Sources;

namespace VEFrameworkUnitTest.Energy
{
    public class EnergyBlocksTestHelpers
    {
        public string sourceId { get; set; } = string.Empty;
        public string batteryId { get; set; } = string.Empty;
        public string device1Id { get; set; } = string.Empty;
        public string device2Id { get; set; } = string.Empty;
        public string device3Id { get; set; } = string.Empty;
        public string device4Id { get; set; } = string.Empty;
        public string device5Id { get; set; } = string.Empty;
        public string devicegroupbatteryId { get; set; } = string.Empty;
        public string devicegroup { get; set; } = string.Empty;
        public string devicegroup2 { get; set; } = string.Empty;
        public string networkId { get; set; } = string.Empty;
        public string owner { get; set; } = string.Empty;
        public string sourceName { get; set; } = string.Empty;

        public BaseEntitiesHandler GetTestEnergyGridHandler(string ownname = "fyziktom", string srcName = "mainPVE")
        {
            var eGrid = new BaseEntitiesHandler();
            sourceName = srcName;
            owner = ownname;

            var id = Guid.NewGuid().ToString();
            var source = SourceFactory.GetSource(SourceType.PVE, sourceName, "", id);
            var res = eGrid.AddEntity(source, sourceName, owner);
            sourceId = string.Empty;
            if (res.Item1)
                sourceId = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            source = SourceFactory.GetSource(SourceType.Battery, "mainbattery", "", id);
            res = eGrid.AddEntity(source, "mainbattery", owner);
            batteryId = string.Empty;
            if (res.Item1)
                batteryId = res.Item2.Item2;

            var battery = eGrid.GetEntity(batteryId, EntityType.Source) as BatteryStorage;
            if (battery != null)
            {
                battery.Capacity = 500;
                battery.MaximumDischargePower = 5;
            }

            id = Guid.NewGuid().ToString();
            var consumer = ConsumerFactory.GetConsumer(ConsumerType.Device, "device1", "", id);
            res = eGrid.AddEntity(consumer, "device1", owner);
            device1Id = string.Empty;
            if (res.Item1)
                device1Id = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.Device, "device2", "", id);
            res = eGrid.AddEntity(consumer, "device2", owner);
            device2Id = string.Empty;
            if (res.Item1)
                device2Id = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            source = SourceFactory.GetSource(SourceType.Battery, "devicegroupbattery", "", id);
            res = eGrid.AddEntity(source, "devicegroupbattery", owner);
            devicegroupbatteryId = string.Empty;
            if (res.Item1)
                devicegroupbatteryId = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.DevicesGroup, "devicegroup", "", id);
            res = eGrid.AddEntity(consumer, "devicegroup", owner);
            devicegroup = string.Empty;
            if (res.Item1)
            {
                devicegroup = res.Item2.Item2;
                eGrid.AddSubEntityToEntity(devicegroup, device1Id);
                eGrid.AddSubEntityToEntity(devicegroup, device2Id);
                // add battery which is placed in this group as backup source to reduce spikes
                eGrid.AddSubEntityToEntity(devicegroup, devicegroupbatteryId);
            }

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.Device, "device3", "", id);
            res = eGrid.AddEntity(consumer, "device3", owner);
            device3Id = string.Empty;
            if (res.Item1)
                device3Id = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.Device, "device4", "", id);
            res = eGrid.AddEntity(consumer, "device4", owner);
            device4Id = string.Empty;
            if (res.Item1)
                device4Id = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.Device, "device5", "", id);
            res = eGrid.AddEntity(consumer, "device5", owner);
            device5Id = string.Empty;
            if (res.Item1)
                device5Id = res.Item2.Item2;

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.DevicesGroup, "devicegroup2", "", id);
            res = eGrid.AddEntity(consumer, "devicegroup2", owner);
            devicegroup2 = string.Empty;
            if (res.Item1)
            {
                devicegroup2 = res.Item2.Item2;
                eGrid.AddSubEntityToEntity(devicegroup2, device3Id);
                eGrid.AddSubEntityToEntity(devicegroup2, device4Id);
                eGrid.AddSubEntityToEntity(devicegroup2, device5Id);
            }

            id = Guid.NewGuid().ToString();
            consumer = ConsumerFactory.GetConsumer(ConsumerType.GroupNetwork, "network", "", id);
            res = eGrid.AddEntity(consumer, "network", owner);
            // create object for whole network
            networkId = string.Empty;
            if (res.Item1)
            {
                networkId = res.Item2.Item2;
                eGrid.AddSubEntityToEntity(networkId, devicegroup);
                eGrid.AddSubEntityToEntity(networkId, devicegroup2);
                // add common sources in network
                eGrid.AddSubEntityToEntity(networkId, sourceId);
                eGrid.AddSubEntityToEntity(networkId, batteryId);
            }

            return eGrid;
        }
    }
}
