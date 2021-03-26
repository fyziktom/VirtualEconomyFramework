using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Nodes;
using VEDrivers.Nodes.Dto;

namespace VEDrivers.Nodes.Handlers
{
    public class BasicNodeHandler : CommonNodeHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string UpdateNode(string accountAddress, string nodeId, Guid ownerid, string nodeName, NodeTypes type, bool isActivated, NodeActionParameters parameters, IDbConnectorService dbservice)
        {
            //IDbConnectorService dbservice = new DbConnectorService();

            try
            {
                var accountId = Guid.Empty;

                if (EconomyMainContext.Accounts.TryGetValue(accountAddress, out var account))
                {
                    accountId = account.Id;
                }
                else
                {
                    Console.WriteLine("Cannot create node - account not found");
                }

                if (EconomyMainContext.Nodes.TryGetValue(nodeId, out var foundnode))
                {
                    foundnode.AccountId = accountId;
                    if (isActivated)
                        foundnode.Activate();
                    else
                        foundnode.DeActivate();

                    foundnode.LoadParameters(parameters);
                    foundnode.SetNodeTriggerType(parameters.TriggerType);
                    foundnode.Name = nodeName;

                    if (EconomyMainContext.WorkWithDb)
                    {
                        if (!dbservice.SaveNode(foundnode))
                            return "Cannot save Node to the db!";
                    }
                }
                else
                {
                    var id = Guid.NewGuid();

                    if (string.IsNullOrEmpty(id.ToString()))
                    {
                        id = Guid.NewGuid();
                    }

                    if (string.IsNullOrEmpty(ownerid.ToString()))
                    {
                        ownerid = Guid.NewGuid();
                    }

                    var node = NodeFactory.GetNode(type, id, accountId, nodeName, isActivated, parameters);

                    if (node != null)
                    {
                        node.ActionRequest += Node_ActionRequest;

                        EconomyMainContext.Nodes.TryAdd(node.Id.ToString(), node);

                        if (EconomyMainContext.WorkWithDb)
                        {
                            if (!dbservice.SaveNode(node))
                                return "Cannot save Node to the db!";
                        }
                    }
                }

                return "OK";

            }
            catch (Exception ex)
            {
                log.Error("Cannot create node", ex);
                return "Cannot create node!";
            }
        }


        public override async Task<string> SeNodeActivation(string nodeId, bool isActivated, IDbConnectorService dbservice)
        {
            //IDbConnectorService dbservice = new DbConnectorService();

            try
            {
                if (EconomyMainContext.Nodes.TryGetValue(nodeId, out var node))
                {
                    if (isActivated)
                        node.Activate();
                    else
                        node.DeActivate();

                    if (EconomyMainContext.WorkWithDb)
                    {
                        if (!dbservice.SaveNode(node))
                            return "Cannot save Node to the db!";
                    }

                    return "OK";
                }
                else
                {
                    log.Error("Cannot find node");
                    return "Cannot find node!";
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot set node", ex);
                return "Cannot set node!";
            }
        }

        public override async Task<string> SetNodeTrigger(string nodeId, NodeActionTriggerTypes type, IDbConnectorService dbservice)
        {
            //IDbConnectorService dbservice = new DbConnectorService();

            try
            {
                if (EconomyMainContext.Nodes.TryGetValue(nodeId, out var node))
                {
                    node.SetNodeTriggerType(type);

                    if (EconomyMainContext.WorkWithDb)
                    {
                        if (!dbservice.SaveNode(node))
                            return "Cannot save Node to the db!";
                    }

                    return "OK";
                }
                else
                {
                    log.Error("Cannot find node");
                    return "Cannot find node!";
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot set node", ex);
                return "Cannot set node!";
            }
        }


        public override bool LoadNodesFromDb(IDbConnectorService dbservice)
        {
            //IDbConnectorService dbservice = new DbConnectorService();

            try
            {
                var nodes = dbservice.GetNodes();

                if (nodes != null)
                {
                    //refresh main wallet dictionary
                    EconomyMainContext.Nodes.Clear();
                    foreach (var n in nodes)
                    {
                        n.ActionRequest += Node_ActionRequest;
                        EconomyMainContext.Nodes.TryAdd(n.Id.ToString(), n);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot Load nodes from Db.", ex);
                return false;
            }
        }

        public override async Task<string> TriggerNodesActions(NodeActionTriggerTypes type, NewTransactionDTO txdata, object payload)
        {
            var result = "ERROR";
            if (EconomyMainContext.Accounts.TryGetValue(txdata.AccountAddress, out var account))
            {
                var pld = JsonConvert.SerializeObject(payload);
                var acc = JsonConvert.SerializeObject(account);

                foreach (var node in EconomyMainContext.Nodes)
                {
                    if (node.Value.AccountId == account.Id)
                    {
                        var res = await node.Value.InvokeNodeFunction(type, new string[] { pld, acc }); ;
                    }
                }
            }

            return result;
        }

        private void Node_ActionRequest(object sender, NodeActionRequestArgs e)
        {
            var node = sender as INode;

            if (e != null)
            {
                switch (e.Type)
                {
                    case NodeActionRequestTypes.MQTTPublishNotRetain:
                        EconomyMainContext.MQTTClient.PostObjectAsJSONString(e.Topic, e.Payload, false).GetAwaiter().GetResult();
                        break;
                    case NodeActionRequestTypes.MQTTPublishRetain:
                        EconomyMainContext.MQTTClient.PostObjectAsJSONString(e.Topic, e.Payload, true).GetAwaiter().GetResult();
                        break;
                }
            }
            else
            {
                log.Error($"Node {node.Id} Action Request Event Handler did not recveived Action Request Arguments!");
            }
        }
    }
}
