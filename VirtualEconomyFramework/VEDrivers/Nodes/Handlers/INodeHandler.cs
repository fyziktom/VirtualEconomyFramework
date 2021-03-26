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
    public interface INodeHandler
    {
        string UpdateNode(string accountAddress, string nodeId, Guid ownerid, string nodeName, NodeTypes type, bool isActivated, NodeActionParameters parameters, IDbConnectorService dbservice);
        Task<string> SeNodeActivation(string nodeId, bool isActivated, IDbConnectorService dbservice);
        Task<string> SetNodeTrigger(string nodeId, NodeActionTriggerTypes type, IDbConnectorService dbservice);
        bool LoadNodesFromDb(IDbConnectorService dbservice);
        Task<string> TriggerNodesActions(NodeActionTriggerTypes type, NewTransactionDTO txdata, object payload);

    }
}
