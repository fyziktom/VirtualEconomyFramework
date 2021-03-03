using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public class BasicNode : CommonNode
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public BasicNode(Guid id, Guid accId, string name, NodeActionParameters parameters)
        {
            Type = NodeTypes.Common;
            Id = id;
            AccountId = accId;
            Name = name;
            LoadParameters(parameters);
            IsActivated = false;
            ActualTriggerType = NodeActionTriggerTypes.None;
        }

        public override event EventHandler<NodeActionFinishedArgs> ActionFinished;

        public override void Activate()
        {
            IsActivated = true;
        }

        public override void DeActivate()
        {
            IsActivated = false;
        }

        public override Task<NodeActionFinishedArgs> InvokeNodeFunction(NodeActionTriggerTypes actionType, string[] otherData)
        {
            return Task.FromResult(new NodeActionFinishedArgs() { result = "ERROR", data = "Not Implemented!" });
        }

        public override void LoadParameters(NodeActionParameters parameters)
        {
            Parameters = JsonConvert.SerializeObject(parameters);
            var o = new object();
            lock (o)
            {
                ParsedParams = parameters;
            }
        }

        public override void SetNodeTriggerType(NodeActionTriggerTypes type)
        {
            ActualTriggerType = type;
            ParsedParams.TriggerType = type;
            Parameters = JsonConvert.SerializeObject(ParsedParams);
        }

        public override object GetNodeParametersCarrier()
        {
            return new object();
        }
    }
}
