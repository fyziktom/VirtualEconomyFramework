using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks.Dto
{
    public class AlocationScheme
    {
        /// <summary>
        /// Name of the alocation scheme
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Deposit Scheme Id
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Main deposit peer Id (if there is no others the amount goes there)
        /// </summary>
        public string MainDepositPeerId { get; set; } = string.Empty;
        /// <summary>
        /// Is the deposit scheme active
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Dictionary of the peers for the deposit of the block amount
        /// If there are more then one the amount is split based on the percentage in DepositPeer class
        /// </summary>
        public Dictionary<string, DepositPeer> DepositPeers { get; set; } = new Dictionary<string, DepositPeer>();
    }
}
