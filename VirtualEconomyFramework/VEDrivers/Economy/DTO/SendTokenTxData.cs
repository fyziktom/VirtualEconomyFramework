using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.DTO
{
    public class SendTokenTxData
    {
        /// <summary>
        /// Address where token will be send
        /// </summary>
        public string ReceiverAddress { get; set; }
        /// <summary>
        /// Symbol of token
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Id of token
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Amount of the tokens
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Metadata dictionary, key-value pairs
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }
    }
}
