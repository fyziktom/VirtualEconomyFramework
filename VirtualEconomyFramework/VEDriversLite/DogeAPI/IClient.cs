using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.DogeAPI
{
    public partial interface IClient
    {
        /// <summary>Returns the doge balance</summary>
        /// <param name="address">Doge address</param>
        /// <returns>Object containing doge balance, if address symbol does not exist on network, empty object is returned.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressBalanceResponse> GetAddressBalanceAsync(string address);
        System.Threading.Tasks.Task<GetAddressBalanceResponse> GetAddressBalanceAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> BroadcastTxAsync(BroadcastTxRequest body);
        System.Threading.Tasks.Task<BroadcastTxResponse> BroadcastTxAsync(BroadcastTxRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Information On an Doge Transaction</summary>
        /// <param name="txid">Doge txid to get information on.</param>
        /// <returns>An object represending this transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTransactionInfoResponse> GetTransactionInfoAsync(string txid);
        System.Threading.Tasks.Task<GetTransactionInfoResponse> GetTransactionInfoAsync(string txid, System.Threading.CancellationToken cancellationToken);


        /// <summary>Doge Address Unspended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressUtxosResponse> GetAddressUtxosAsync(string address);
        System.Threading.Tasks.Task<GetAddressUtxosResponse> GetAddressUtxosAsync(string address, System.Threading.CancellationToken cancellationToken);

    }

}
