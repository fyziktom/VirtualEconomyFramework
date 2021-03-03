using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes
{
    public static class NodeFactory
    {
        public static INode GetNode(NodeTypes type, Guid id, Guid accId, string name, bool isActivated, NodeActionParameters parameters)
        {
            switch (type)
            {
                case NodeTypes.Common:
                    return new BasicNode(id, accId, name, parameters);
                case NodeTypes.MQTTApiPublish:
                    if (id == Guid.Empty)
                        id = Guid.NewGuid();
                    return new MQTTPublishNode(id, accId, name, isActivated, parameters);
                case NodeTypes.HTTPAPIRequest:
                    if (id == Guid.Empty)
                        id = Guid.NewGuid();
                    return new HTTPApiRequestNode(id, accId, name, isActivated, parameters);
            }

            return new BasicNode(id, accId, name, parameters); ;
        }

    }
}
