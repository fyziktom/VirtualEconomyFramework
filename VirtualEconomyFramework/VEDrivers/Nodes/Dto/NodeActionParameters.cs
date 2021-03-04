using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Nodes.Dto
{
    public enum NodeActionTriggerTypes
    {
        None,
        NewTxArrived,
        TokenTxArrived,
        TxSent,
        TokenTxSent,
        UnconfirmedTokenTxArrived,
        UnconfirmedTxArrived,
        UnconfirmedTokenTxSent,
        UnconfirmedTxSent
    }

    public class NodeActionParameters
    {
        /// <summary>
        /// Main command name
        /// </summary>
        public string Command { get; set; } = string.Empty;
        /// <summary>
        /// Custom JavaScript code
        /// Can carry one JS function
        /// main function must name nodeJSfunction and return bool, 
        /// example: 
        /// function nodeJSfunction(payload, params){ return (customFunction(payload, params) => { return payload.ActualBalance > parseInt(params[0])}}
        /// </summary>
        public string Script { get; set; } = "function nodeJSfunction(payload, params) { return JSON.stringify({ 'done' : true, payload : JSON.stringify({ 'payload' : payload, 'params': params }) }); }";
        /// <summary>
        /// If this is set, Custom JS Script will be executed
        /// </summary>
        public bool IsScriptActive { get; set; } = false;
        /// <summary>
        /// Custom JavaScript parameters list
        /// </summary>
        public List<string> ScriptParametersList { get; set; } = new List<string>();
        /// <summary>
        /// Other parameters related to the node
        /// </summary>
        public string Parameters { get; set; } = string.Empty;
        /// <summary>
        /// Time delay of action in ms
        /// </summary>
        public int TimeDelay { get; set; } = 0;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public NodeActionTriggerTypes TriggerType { get; set; }
    }
}
