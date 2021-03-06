using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public enum NodeTypes
    {
        Common,
        Function,
        MQTTApiPublish,
        HTTPAPIRequest
    }

    public class NodeActionFinishedArgs
    {
        public string result { get; set; }
        public string data { get; set; }
    }

    public enum NodeActionRequestTypes
    {
        MQTTPublishRetain,
        MQTTPublishNotRetain,
        PriceRequest
    }

    public class NodeActionRequestArgs
    {
        public NodeActionRequestTypes Type { get; set; }
        public string Topic { get; set; }
        public string Payload { get; set; }
    }

    public interface INode : ICommonDbObjectBase
    {
        Guid Id { get; set; }
        string Name { get; set; }
        Guid AccountId { get; set; }
        NodeTypes Type { get; set; }
        bool? IsActivated { get; set; }
        string Parameters { get; set; }
        NodeActionParameters ParsedParams { get; set; }
        string LastPayload { get; set; }
        string[] LastOtherData { get; set; }
        NodeActionTriggerTypes ActualTriggerType { get; set; }
        event EventHandler<NodeActionRequestArgs> ActionRequest;
        event EventHandler<NodeActionFinishedArgs> ActionFinished;
        void Activate();
        void DeActivate();
        void LoadParameters(NodeActionParameters parameters);
        void SetNodeTriggerType(NodeActionTriggerTypes type);
        Task<NodeActionFinishedArgs> InvokeNodeFunction(NodeActionTriggerTypes actionType, string[] otherData, string altFunction = "");

        object GetNodeParametersCarrier();
    }
}
