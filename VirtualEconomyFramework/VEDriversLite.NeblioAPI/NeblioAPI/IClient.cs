using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NeblioAPI
{
    /// <summary>
    /// Neblio API client interface
    /// </summary>
    public partial interface IClient
    {
        /// <summary>Returns the tokenId representing a token</summary>
        /// <param name="tokensymbol">Token symbol</param>
        /// <returns>Object containing the token symbol and ID, if token symbol does not exist on network, empty object is returned.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenIdResponse> GetTokenIdAsync(string tokensymbol);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns the tokenId representing a token</summary>
        /// <param name="tokensymbol">Token symbol</param>
        /// <returns>Object containing the token symbol and ID, if token symbol does not exist on network, empty object is returned.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenIdResponse> GetTokenIdAsync(string tokensymbol, System.Threading.CancellationToken cancellationToken);

        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="body"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> BroadcastTxAsync(BroadcastTxRequest body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Broadcasts a signed raw transaction to the network</summary>
        /// <param name="body"></param>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> BroadcastTxAsync(BroadcastTxRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Information On a Neblio Address</summary>
        /// <param name="address">Neblio Address to get information on.</param>
        /// <returns>An object with an array of UTXOs for this address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressInfoResponse> GetAddressInfoAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Information On a Neblio Address</summary>
        /// <param name="address">Neblio Address to get information on.</param>
        /// <returns>An object with an array of UTXOs for this address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressInfoResponse> GetAddressInfoAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Information On an NTP1 Transaction</summary>
        /// <param name="txid">Neblio txid to get information on.</param>
        /// <returns>An object represending this transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTransactionInfoResponse> GetTransactionInfoAsync(string txid);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Information On an NTP1 Transaction</summary>
        /// <param name="txid">Neblio txid to get information on.</param>
        /// <returns>An object represending this transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTransactionInfoResponse> GetTransactionInfoAsync(string txid, System.Threading.CancellationToken cancellationToken);

        /// <summary>Get Metadata of Token</summary>
        /// <param name="tokenid">TokenId to request metadata for</param>
        /// <param name="verbosity">0 (Default) is fastest, 1 contains token stats, 2 contains token holding addresses</param>
        /// <returns>An object containing the metadata of a token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenMetadataResponse> GetTokenMetadataAsync(string tokenid, double? verbosity);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get Metadata of Token</summary>
        /// <param name="tokenid">TokenId to request metadata for</param>
        /// <param name="verbosity">0 (Default) is fastest, 1 contains token stats, 2 contains token holding addresses</param>
        /// <returns>An object containing the metadata of a token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenMetadataResponse> GetTokenMetadataAsync(string tokenid, double? verbosity, System.Threading.CancellationToken cancellationToken);

        /// <summary>Get UTXO Metadata of Token</summary>
        /// <param name="tokenid">TokenId to request metadata for</param>
        /// <param name="utxo">Specific UTXO to request metadata for</param>
        /// <param name="verbosity">0 (Default) is fastest, 1 contains token stats, 2 contains token holding addresses</param>
        /// <returns>An object containing the metadata of a token for a UTXO</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenMetadataResponse> GetTokenMetadataOfUtxoAsync(string tokenid, string utxo, double? verbosity);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get UTXO Metadata of Token</summary>
        /// <param name="tokenid">TokenId to request metadata for</param>
        /// <param name="utxo">Specific UTXO to request metadata for</param>
        /// <param name="verbosity">0 (Default) is fastest, 1 contains token stats, 2 contains token holding addresses</param>
        /// <returns>An object containing the metadata of a token for a UTXO</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenMetadataResponse> GetTokenMetadataOfUtxoAsync(string tokenid, string utxo, double? verbosity, System.Threading.CancellationToken cancellationToken);

        /// <summary>Get Addresses Holding a Token</summary>
        /// <param name="tokenid">TokenId to request metadata for</param>
        /// <returns>An object containing all of the addresses holding a token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenHoldersResponse> GetTokenHoldersAsync(string tokenid);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get Addresses Holding a Token</summary>
        /// <param name="tokenid">TokenId to request metadata for</param>
        /// <returns>An object containing all of the addresses holding a token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTokenHoldersResponse> GetTokenHoldersAsync(string tokenid, System.Threading.CancellationToken cancellationToken);

        /// <summary>Builds a transaction that issues a new NTP1 Token</summary>
        /// <returns>An object representing the token created</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<IssueTokenResponse> IssueTokenAsync(IssueTokenRequest body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Builds a transaction that issues a new NTP1 Token</summary>
        /// <param name="body"></param>
        /// <returns>An object representing the token created</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<IssueTokenResponse> IssueTokenAsync(IssueTokenRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Builds a transaction that sends an NTP1 Token</summary>
        /// <returns>An object representing the tx to send the token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<SendTokenResponse> SendTokenAsync(SendTokenRequest body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Builds a transaction that sends an NTP1 Token</summary>
        /// <returns>An object representing the tx to send the token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<SendTokenResponse> SendTokenAsync(SendTokenRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Builds a transaction that burns an NTP1 Token</summary>
        /// <returns>An object representing the tx to burn the token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BurnTokenResponse> BurnTokenAsync(BurnTokenRequest body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Builds a transaction that burns an NTP1 Token</summary>
        /// <returns>An object representing the tx to burn the token</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BurnTokenResponse> BurnTokenAsync(BurnTokenRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Broadcasts a signed raw transaction to the network (not NTP1 specific)</summary>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> SendTxAsync(BroadcastTxRequest body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Broadcasts a signed raw transaction to the network (not NTP1 specific)</summary>
        /// <returns>An object containing the TXID if the broadcast was successful</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<BroadcastTxResponse> SendTxAsync(BroadcastTxRequest body, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns information regarding a Neblio block</summary>
        /// <param name="blockhash">Block Hash</param>
        /// <returns>Object containing all information on a blockchain block</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetBlockResponse> GetBlockAsync(string blockhash);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns information regarding a Neblio block</summary>
        /// <param name="blockhash">Block Hash</param>
        /// <returns>Object containing all information on a blockchain block</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetBlockResponse> GetBlockAsync(string blockhash, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns block hash of block</summary>
        /// <param name="blockindex">Block Index</param>
        /// <returns>Object containing block hash</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetBlockIndexResponse> GetBlockIndexAsync(double blockindex);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns block hash of block</summary>
        /// <param name="blockindex">Block Index</param>
        /// <returns>Object containing block hash</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetBlockIndexResponse> GetBlockIndexAsync(double blockindex, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns transaction object</summary>
        /// <param name="txid">Transaction ID</param>
        /// <returns>Object containing transaction info</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTxResponse> GetTxAsync(string txid);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns transaction object</summary>
        /// <param name="txid">Transaction ID</param>
        /// <returns>Object containing transaction info</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTxResponse> GetTxAsync(string txid, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns raw transaction hex</summary>
        /// <param name="txid">Transaction ID</param>
        /// <returns>Object containing raw hex of transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetRawTxResponse> GetRawTxAsync(string txid);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns raw transaction hex</summary>
        /// <param name="txid">Transaction ID</param>
        /// <returns>Object containing raw hex of transaction</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetRawTxResponse> GetRawTxAsync(string txid, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns address object</summary>
        /// <param name="address">Address</param>
        /// <returns>Object containing address info</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressResponse> GetAddressAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns address object</summary>
        /// <param name="address">Address</param>
        /// <returns>Object containing address info</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetAddressResponse> GetAddressAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns address balance in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Address balance</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressBalanceAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns address balance in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Address balance</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressBalanceAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns address unconfirmed balance in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Address unconfirmed balance</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressUnconfirmedBalanceAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns address unconfirmed balance in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Address unconfirmed balance</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressUnconfirmedBalanceAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns total received by address in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Total received by address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressTotalReceivedAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns total received by address in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Total received by address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressTotalReceivedAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns all UTXOs at a given address</summary>
        /// <param name="address">Address</param>
        /// <returns>UTXOs at an address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<Anonymous>> GetAddressUtxosAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns all UTXOs at a given address</summary>
        /// <param name="address">Address</param>
        /// <returns>UTXOs at an address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<Anonymous>> GetAddressUtxosAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Returns total sent by address in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Total sent by address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressTotalSentAsync(string address);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Returns total sent by address in sats</summary>
        /// <param name="address">Address</param>
        /// <returns>Total sent by address</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<double> GetAddressTotalSentAsync(string address, System.Threading.CancellationToken cancellationToken);

        /// <summary>Get transactions by block or address</summary>
        /// <param name="address">Address</param>
        /// <param name="block">Block Hash</param>
        /// <param name="pageNum">Page number to display</param>
        /// <returns>List of transactions</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTxsResponse> GetTxsAsync(string address, string block, double? pageNum);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get transactions by block or address</summary>
        /// <param name="address">Address</param>
        /// <param name="block">Block Hash</param>
        /// <param name="pageNum">Page number to display</param>
        /// <returns>List of transactions</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetTxsResponse> GetTxsAsync(string address, string block, double? pageNum, System.Threading.CancellationToken cancellationToken);

        /// <summary>Get node sync status</summary>
        /// <returns>Sync Info</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetSyncResponse> GetSyncAsync();

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Get node sync status</summary>
        /// <returns>Sync Info</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetSyncResponse> GetSyncAsync(System.Threading.CancellationToken cancellationToken);

        /// <summary>Utility API for calling several blockchain node functions</summary>
        /// <param name="q">Function to call, getInfo, getDifficulty, getBestBlockHash, or getLastBlockHash</param>
        /// <returns>Function Response</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetStatusResponse> GetStatusAsync(string q);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Utility API for calling several blockchain node functions</summary>
        /// <param name="q">Function to call, getInfo, getDifficulty, getBestBlockHash, or getLastBlockHash</param>
        /// <returns>Function Response</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetStatusResponse> GetStatusAsync(string q, System.Threading.CancellationToken cancellationToken);
    }
    /// <summary>
    /// Testnet API client interface
    /// </summary>
    public partial interface ITestnetClient : IClient
    {

        /// <summary>Withdraws testnet NEBL to the specified address</summary>
        /// <param name="address">Your Neblio Testnet Address</param>
        /// <param name="amount">Amount of NEBL to withdrawal in satoshis</param>
        /// <returns>Object containing the transaction ID of the withdrawal.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetFaucetResponse> GetFaucetAsync(string address, double? amount);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>Withdraws testnet NEBL to the specified address</summary>
        /// <param name="address">Your Neblio Testnet Address</param>
        /// <param name="amount">Amount of NEBL to withdrawal in satoshis</param>
        /// <returns>Object containing the transaction ID of the withdrawal.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<GetFaucetResponse> GetFaucetAsync(string address, double? amount, System.Threading.CancellationToken cancellationToken);

    }

}
