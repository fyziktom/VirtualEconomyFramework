using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.DTO;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes.Handlers
{
    public interface INodeHandler
    {
        string UpdateNode(string accountAddress, string nodeId, Guid ownerid, string nodeName, NodeTypes type, bool isActivated, NodeActionParameters parameters);
        Task<string> SeNodeActivation(string nodeId, bool isActivated);
        Task<string> SetNodeTrigger(string nodeId, NodeActionTriggerTypes type);
        bool LoadNodesFromDb();
        Task<string> TriggerNodesActions(NodeActionTriggerTypes type, NewTransactionDTO txdata, object payload);

    }
}
