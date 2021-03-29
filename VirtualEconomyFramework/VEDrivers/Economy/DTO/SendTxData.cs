using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.DTO
{
    public class SendTxData
    {
        public SendTxData()
        {
        }
        /// <summary>
        /// Address from where token will be send
        /// </summary>
        public string SenderAddress { get; set; }
        public string Password { get; set; }
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
        /// Amount of the currency
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Custom message if it is supported by currency
        /// </summary>
        public string CustomMessage { get; set; }
    }
}
