using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;

namespace VEFrameworkUnitTest.BlockEntities
{
    internal class EntitiesBlocksTestHelpers
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

        public IEntitiesHandler GetTestEnergyGridHandler(string ownname = "fyziktom", string srcName = "mainPVE")
        {
            var eGrid = new BaseEntitiesHandler();
            sourceName = srcName;
            owner = ownname;

            var entity = new BaseEntity() { Type = EntityType.Source, Name = sourceName, ParentId = owner };
            var res = eGrid.AddEntity(entity, sourceName, owner);
            sourceId = string.Empty;
            if (res.Item1)
                sourceId = res.Item2.Item2;

            entity = new BaseEntity() { Type = EntityType.Source, Name = "mainbattery", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            batteryId = string.Empty;
            if (res.Item1)
                batteryId = res.Item2.Item2;


            device1Id = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "device1", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
                device1Id = res.Item2.Item2;

            device2Id = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "device2", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
                device2Id = res.Item2.Item2;

            entity = new BaseEntity() { Type = EntityType.Source, Name = "devicegroupbattery", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            devicegroupbatteryId = string.Empty;
            if (res.Item1)
                devicegroupbatteryId = res.Item2.Item2;

            devicegroup = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "devicegroup", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
            {
                devicegroup = res.Item2.Item2;
                eGrid.AddSubEntityToEntity(devicegroup, device1Id);
                eGrid.AddSubEntityToEntity(devicegroup, device2Id);
                // add battery which is placed in this group as backup source to reduce spikes
                eGrid.AddSubEntityToEntity(devicegroup, devicegroupbatteryId);
            }

            device3Id = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "device3", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
                device3Id = res.Item2.Item2;

            device4Id = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "device4", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
                device4Id = res.Item2.Item2;

            device5Id = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "device5", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
                device5Id = res.Item2.Item2;

            devicegroup2 = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "devicegroup2", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
            if (res.Item1)
            {
                devicegroup2 = res.Item2.Item2;
                eGrid.AddSubEntityToEntity(devicegroup2, device3Id);
                eGrid.AddSubEntityToEntity(devicegroup2, device4Id);
                eGrid.AddSubEntityToEntity(devicegroup2, device5Id);
            }

            // create object for whole network
            networkId = string.Empty;
            entity = new BaseEntity() { Type = EntityType.Consumer, Name = "network", ParentId = owner };
            res = eGrid.AddEntity(entity, sourceName, owner);
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
