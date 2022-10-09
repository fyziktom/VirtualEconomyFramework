using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.Tree;

namespace VEFramework.VEBlazor.EntitiesBlocks.Services
{
    public class AppData
    {
        public BaseEntitiesHandler EntitiesHandler { get; set; } = new BaseEntitiesHandler();
        public PVPanelsGroupsHandler PVEGrid { get; set; } = new PVPanelsGroupsHandler();
        public string RootItemId { get; set; } = string.Empty;
        public string RootItemName { get; set; } = string.Empty;
        public string StoredConfig { get; set; } = string.Empty;
        public TreeItem SelectedItem { get; set; } = new TreeItem();
        public bool PVESimulatorLoaded { get; set; }

    }
}
