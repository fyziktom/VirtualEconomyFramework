using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public abstract class CommonNode : INode
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool? IsActivated { get; set; }
        public Guid AccountId { get; set; }
        public NodeTypes Type { get; set; }
        public string Parameters { get; set; }
        public NodeActionParameters ParsedParams { get; set; }
        public NodeActionTriggerTypes ActualTriggerType { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Version { get; set; }
        public bool? Deleted { get; set; } = false;
        public DateTime ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public abstract event EventHandler<NodeActionRequestArgs> ActionRequest;
        public abstract event EventHandler<NodeActionFinishedArgs> ActionFinished;
        public abstract void Activate();
        public abstract void DeActivate();
        public abstract void LoadParameters(NodeActionParameters parameters);
        public abstract void SetNodeTriggerType(NodeActionTriggerTypes type);
        public abstract Task<NodeActionFinishedArgs> InvokeNodeFunction(NodeActionTriggerTypes actionType, string[] otherData);
        public abstract object GetNodeParametersCarrier();
    }
}
