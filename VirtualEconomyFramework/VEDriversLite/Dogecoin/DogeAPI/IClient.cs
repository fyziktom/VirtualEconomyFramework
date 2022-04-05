using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.DogeAPI
{
    /// <summary>
    /// Doge API common client interface
    /// </summary>
    public partial interface IClient
    {
        /// <summary>Returns the doge balance</summary>
        /// <param name="address">Doge address</param>
        /// <returns>Object containing doge balance, if address symbol does not exist on network, empty object is returned.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressBalanceResponse> GetAddressBalanceAsync(string address);
        /// <summary>Returns the doge balance</summary>
        /// <param name="address">Doge address</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object containing doge balance, if address symbol does not exist on network, empty object is returned.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressBalanceResponse> GetAddressBalanceAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> BroadcastTxAsync(BroadcastTxRequest body);
        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="cancellationToken"></param>
        /// <param name="body"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> BroadcastTxAsync(BroadcastTxRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="body"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> ChainSoBroadcastTxAsync(ChainSoBroadcastTxRequest body);
        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> ChainSoBroadcastTxAsync(ChainSoBroadcastTxRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="body"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> VENFTBroadcastTxAsync(VENFTBroadcastTxRequest body);
        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> VENFTBroadcastTxAsync(VENFTBroadcastTxRequest body, System.Threading.CancellationToken cancellationToken);


        /// <summary>Information On an Doge Transaction</summary>
        /// <param name="txid">Doge txid to get information on.</param>
        /// <returns>An object represending this transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTransactionInfoResponse> GetTransactionInfoAsync(string txid);
        /// <summary>Information On an Doge Transaction</summary>
        /// <param name="txid">Doge txid to get information on.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An object represending this transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTransactionInfoResponse> GetTransactionInfoAsync(string txid, System.Threading.CancellationToken cancellationToken);


        /// <summary>Doge Address Unspended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressUtxosResponse> GetAddressUtxosAsync(string address);
        /// <summary>Doge Address Unspended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressUtxosResponse> GetAddressUtxosAsync(string address, System.Threading.CancellationToken cancellationToken);


        /// <summary>Doge Address spended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressSpentTxsResponse> GetAddressSentTxAsync(string address);
        /// <summary>Doge Address spended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressSpentTxsResponse> GetAddressSentTxAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Doge Address spended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressReceivedTxsResponse> GetAddressReceivedTxAsync(string address);
        /// <summary>Doge Address spended transactions</summary>
        /// <param name="address">Doge Address</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object containing collection of Utxos</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressReceivedTxsResponse> GetAddressReceivedTxAsync(string address, System.Threading.CancellationToken cancellationToken);

    }

}
