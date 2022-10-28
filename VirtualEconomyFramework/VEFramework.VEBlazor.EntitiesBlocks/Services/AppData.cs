using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.StorageCalculations;
using VEDriversLite.EntitiesBlocks.Tree;

namespace VEFramework.VEBlazor.EntitiesBlocks.Services
{
    public class AppData
    {
        public BaseEntitiesHandler EntitiesHandler { get; set; } = new BaseEntitiesHandler();
        public PVPanelsGroupsHandler PVEGrid { get; set; } = new PVPanelsGroupsHandler();
        public BatteryBlockHandler BatteryStorage { get; set; } = new BatteryBlockHandler(null, "storagesimulator", BatteryBlockHandler.DefaultChargingFunction, BatteryBlockHandler.DefaultDischargingFunction);
        public string RootItemId { get; set; } = string.Empty;
        public string RootItemName { get; set; } = string.Empty;
        public string StoredConfig { get; set; } = string.Empty;
        public string StoredPVEConfig { get; set; } = string.Empty;
        public string StoredBatteryStorageConfig { get; set; } = string.Empty;
        public TreeItem SelectedItem { get; set; } = new TreeItem();
        public bool PVESimulatorLoaded { get; set; }
        public Coordinates DefaultCoordinates = new Coordinates(49.194103, 16.608998);

        public bool BatteryStorageSimulatorLoaded { get; set; }
        public bool TDDsLoaded { get; set; }
        public List<DataProfile> TDDs { get; set; } = new List<DataProfile>();

    }
}
