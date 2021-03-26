using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes.Handlers
{
    public abstract class CommonNodeHandler : INodeHandler
    {
        public abstract bool LoadNodesFromDb(IDbConnectorService dbservice);

        public abstract Task<string> SeNodeActivation(string nodeId, bool isActivated, IDbConnectorService dbservice);

        public abstract Task<string> SetNodeTrigger(string nodeId, NodeActionTriggerTypes type, IDbConnectorService dbservice);

        public abstract Task<string> TriggerNodesActions(NodeActionTriggerTypes type, NewTransactionDTO txdata, object payload);

        public abstract string UpdateNode(string accountAddress, string nodeId, Guid ownerid, string nodeName, NodeTypes type, bool isActivated, NodeActionParameters parameters, IDbConnectorService dbservice);
    }
}
