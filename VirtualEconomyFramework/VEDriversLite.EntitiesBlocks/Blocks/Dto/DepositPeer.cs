using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Blocks.Dto
{
    public class DepositPeer
    {
        /// <summary>
        /// Name of the Deposit peer (IEntity)
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Deposit Peer Id, some IEntity
        /// </summary>
        public string PeerId { get; set; } = string.Empty;
        /// <summary>
        /// Percentage of the amount what should be send to the deposit peer from whole block amount
        /// </summary>
        public double Percentage { get; set; } = 0.0;
    }
}
