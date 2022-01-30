using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;
using VEDriversLite.Cryptocurrencies;
using VEDriversLite.Events;
using VEDriversLite.NFT;
using VEDriversLite.Security;

namespace VEDriversLite.Accounts
{
    public enum AccountType
    {
        Neblio,
        Dogecoin
    }
    public interface IAccount
    {
        /// <summary>
        /// Type of the account
        /// </summary>
        AccountType Type { get; set; }
        /// <summary>
        /// Name of the account
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Neblio Address hash
        /// </summary>
        string Address { get; set; }
        /// <summary>
        /// Indicate if the account was loaded just for observation and it does not have correct private key
        /// </summary>
        bool JustForObserve { get; set; }
        /// <summary>
        /// Indicate if the account was loaded just to sign the transactions. It does not have running the refreshing or loaded any more complicated objects.
        /// </summary>
        bool LoadJustForSignitures { get; set; }
        /// <summary>
        /// If you want to run account without NFTs set this up. 
        /// Whenever during run you can clear this flag and NFTs will start loading
        /// </summary>
        bool WithoutNFTs { get; set; }
        /// <summary>
        /// When main refreshing loop is running this is set
        /// </summary>
        bool IsRefreshingRunning { get; set; }
        /// <summary>
        /// Loaded Secret, NBitcoin Class which carry Public Key and Private Key
        /// </summary>
        BitcoinSecret Secret { get; set; }

        /// <summary>
        /// Number of the transactions on the address. not used now
        /// </summary>
        double NumberOfTransaction { get; set; }
        /// <summary>
        /// Number of already loaded transaction on the address. not used now
        /// </summary>
        double NumberOfLoadedTransaction { get; }
        /// <summary>
        /// Total actual balance based on Utxos. This means sum of spendable and unconfirmed balances.
        /// </summary>
        double TotalBalance { get; set; }
        /// <summary>
        /// Total spendable balance based on Utxos.
        /// </summary>
        double TotalSpendableBalance { get; set; }
        /// <summary>
        /// Total balance which is now unconfirmed based on Utxos.
        /// </summary>
        double TotalUnconfirmedBalance { get; set; }
        /// <summary>
        /// Carrier for encrypted private key from storage and its password.
        /// </summary>
        /// <summary>
        /// Actual list of all Utxos on this address.
        /// </summary>
        List<Utxo> Utxos { get; set; }

        EncryptionKey AccountKey { get; set; }
        /// <summary>
        /// Service which gets prices of cryptocurrencies
        /// </summary>
        PriceService ExchangePriceService { get; set; }


        #region Events
        /// <summary>
        /// This event is fired whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        event EventHandler Refreshed;
        /// <summary>
        /// This event is fired whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        event EventHandler<string> NFTsChanged;

        /// <summary>
        /// This event is fired whenever some progress during multimint happens
        /// </summary>
        event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is fired whenever price from exchanges is refreshed. It provides dictionary of the actual available rates.
        /// </summary>
        event EventHandler<IDictionary<CurrencyTypes, double>> PricesRefreshed;

        /// <summary>
        /// This event is fired whenever profile nft is updated or found
        /// </summary>
        event EventHandler<INFT> ProfileUpdated;
        /// <summary>
        /// This event is fired whenever some NFT is in received payment too and it should be blocked for any further action.
        /// It provides Utxo and UtxoIndex as touple.
        /// Event is fired also for the SubAccounts when it is registred from Main Account
        /// </summary>
        event EventHandler<(string, int)> NFTAddedToPayments;
        /// <summary>
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        event EventHandler<string> FirsLoadingStatus;
        /// <summary>
        /// This event is called when first loading of the account is finished
        /// </summary>
        event EventHandler<string> AccountFirsLoadFinished;

        #endregion

        /// <summary>
        /// This function will check if the account is locked or unlocked.
        /// </summary>
        /// <returns></returns>
        bool IsLocked();
        /// <summary>
        /// Load account. If there is password it is used to decrypt the private key
        /// This function will load Secret property with Key
        /// </summary>
        /// <param name="loaddata">Object with parameters to load the account</param>
        /// <returns></returns>
        Task<bool> LoadAccount(AccountLoadData loaddata);
        /// <summary>
        /// This function will create new account - Neblio address and its Private key.
        /// </summary>
        /// <param name="password">Input password, which will encrypt the Private key</param>
        /// <param name="saveToFile">if you want to save it to the file (dont work in the WASM) set this. It will save to root exe path as key.txt</param>
        /// <param name="filename">default filename is key.txt you can change it, but remember to load same name when loading the account.</param>
        /// <returns></returns>
        Task<bool> CreateNewAccount(string password, bool saveToFile = false, string filename = "key.txt");
        /// <summary>
        /// Reload address Utxos list. It will sort descending the utxos based on the utxos time stamps.
        /// </summary>
        /// <returns></returns>
        Task ReloadUtxos();
        /// <summary>
        /// This function will load actual address info an adress utxos. It is used mainly for loading list of all transactions.
        /// </summary>
        /// <returns></returns>
        Task ReloadAccountInfo();
        /// <summary>
        /// This function will check if there is some spendable neblio of specific amount and returns list of the utxos for the transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<(string, ICollection<Dto.Utxo>)> CheckSpendableMainToken(double amount);
        /// <summary>
        /// This function will check if there is some spendable tokens of specific Id and amount and returns list of the utxos for the transaction.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<(string, ICollection<Dto.Utxo>)> CheckSpendableTokens(string id, int amount);
        /// <summary>
        /// Commmon command to accept transaction into the transaction buffer
        /// </summary>
        /// <param name="txtosend"></param>
        /// <returns></returns>
        Task<(bool, string)> AddTxToSendBuffer(TxToSend txtosend);
        /// <summary>
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        Task<(bool,string)> StartRefreshingData(int interval = 3000);

        /// <summary>
        /// Sign custom message with use of account Private Key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<(bool, string)> SignMessage(string message);
        /// <summary>
        /// Verify message which was signed by some address.
        /// </summary>
        /// <param name="message">Input message</param>
        /// <param name="signature">Signature of this message created by owner of some address.</param>
        /// <param name="address">Neblio address which should sign the message and should be verified.</param>
        /// <returns></returns>
        Task<(bool, string)> VerifyMessage(string message, string signature, string address);
        /// <summary>
        /// Encrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        Task<(bool, string)> EncryptMessage(string message);
        /// <summary>
        /// Decrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        Task<(bool, string)> DecryptMessage(string emessage);
        /// <summary>
        /// Obtain verify code of some transaction. This will combine txid and UTC time (rounded to minutes) and sign with the private key.
        /// It will create unique code, which can be verified and it is valid just one minute.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        Task<OwnershipVerificationCodeDto> GetNFTVerifyCode(string txid);
        /// <summary>
        /// Verification function for the NFT ownership code generated by GetNFTVerifyCode function.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        Task<(OwnershipVerificationCodeDto, byte[])> GetNFTVerifyQRCode(string txid);

    }
}
