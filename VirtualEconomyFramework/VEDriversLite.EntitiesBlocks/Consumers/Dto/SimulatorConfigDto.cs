using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Consumers.Dto
{
    public class SimulatorConfigDto
    {
        public SimulatorTypes? Type { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ParentId { get; set; }
        public double[]? DayProfileData { get; set; }
        public double? DevicePowerConsumption { get; set; }


        public void Load(ISimulator simulator)
        {
            if (simulator == null) return;

            Type = simulator.Type;

            if (simulator.Id != null)
                Id = simulator.Id;
            if (simulator.Name != null)
                Name = simulator.Name;
            if (simulator.ParentId != null)
                ParentId = simulator.ParentId;
            if (simulator.Type == SimulatorTypes.Device)
            {
                DevicePowerConsumption = (simulator as DeviceSimulator).DevicePowerConsumption;
                DayProfileData = (simulator as DeviceSimulator).DayProfileData;
            }
        }

        public DeviceSimulator Fill()
        {
            if (Type == SimulatorTypes.Device)
            {
                var result = new DeviceSimulator();

                if (Name != null)
                    result.Name = Name;

                if (Id != null)
                    result.Id = Id;

                if (ParentId != null)
                    result.ParentId = ParentId;

                if (DevicePowerConsumption != null)
                    result.DevicePowerConsumption = (double)DevicePowerConsumption;

                if (DayProfileData != null)
                    result.DayProfileData = DayProfileData;

                return result;
            }
            return new DeviceSimulator();
        }

    }
}
