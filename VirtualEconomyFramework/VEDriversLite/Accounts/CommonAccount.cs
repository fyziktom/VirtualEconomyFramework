using NBitcoin;
using Newtonsoft.Json;
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
    public abstract class CommonAccount : UtxoAccountBase, IAccount
    {
        public AccountType Type { get; set; } = AccountType.Neblio;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// If this is SubAccount it is under this account
        /// </summary>
        public string ParentAddress { get; set; } = string.Empty;
        /// <summary>
        /// The account is under another parent address
        /// </summary>
        public bool IsSubAccount { get; set; } = false;
        public bool JustForObserve { get; set; } = false;
        public bool LoadJustForSignitures { get; set; } = false;
        /// <summary>
        /// If you want to run account without NFTs set this up. 
        /// Whenever during run you can clear this flag and NFTs will start loading
        /// </summary>
        public bool WithoutNFTs { get; set; } = false;
        /// <summary>
        /// When main refreshing loop is running this is set
        /// </summary>
        [JsonIgnore]
        public bool IsRefreshingRunning { get; set; } = false;
        public BitcoinSecret Secret { get; set; }
        public double NumberOfTransaction { get; set; } = 0.0;
        public double NumberOfLoadedTransaction { get; } = 0.0;
        public double TotalBalance { get; set; } = 0.0;
        public double TotalSpendableBalance { get; set; } = 0.0;
        public double TotalUnconfirmedBalance { get; set; } = 0.0;

        public EncryptionKey AccountKey { get; set; } = null;
        /// <summary>
        /// Service which gets prices of cryptocurrencies
        /// </summary>
        [JsonIgnore]
        public PriceService ExchangePriceService { get; set; } = new PriceService();
        public List<string> SubAccounts { get; set; } = new List<string>();

        /// <summary>
        /// This event is fired whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public abstract event EventHandler Refreshed;
        /// <summary>
        /// This event is fired whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public abstract event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        public abstract event EventHandler<string> NFTsChanged;

        /// <summary>
        /// This event is fired whenever some progress during multimint happens
        /// </summary>
        public abstract event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is fired whenever price from exchanges is refreshed. It provides dictionary of the actual available rates.
        /// </summary>
        public abstract event EventHandler<IDictionary<CurrencyTypes, double>> PricesRefreshed;

        /// <summary>
        /// This event is fired whenever profile nft is updated or found
        /// </summary>
        public abstract event EventHandler<INFT> ProfileUpdated;
        /// <summary>
        /// This event is fired whenever some NFT is in received payment too and it should be blocked for any further action.
        /// It provides Utxo and UtxoIndex as touple.
        /// Event is fired also for the SubAccounts when it is registred from Main Account
        /// </summary>
        public abstract event EventHandler<(string, int)> NFTAddedToPayments;
        /// <summary>
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public abstract event EventHandler<string> FirsLoadingStatus;
        /// <summary>
        /// This event is called when first loading of the account is finished
        /// </summary>
        public abstract event EventHandler<string> AccountFirsLoadFinished;

        public abstract Task<(bool, string)> AddTxToSendBuffer(TxToSend txtosend);
        public abstract Task<(string, ICollection<Utxo>)> CheckSpendableMainToken(double amount);
        public abstract Task<(string, ICollection<Utxo>)> CheckSpendableTokens(string id, int amount);
        /// <summary>
        /// This function will check if the account is locked or unlocked.
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            if (AccountKey != null)
            {
                if (AccountKey.IsEncrypted)
                {
                    if (AccountKey.IsPassLoaded)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (AccountKey.IsLoaded)
                        return false;
                    else
                        return true;
                }
            }
            else
                return true;
        }
        public abstract Task<bool> LoadAccount(AccountLoadData loaddata);
        /// <summary>
        /// This function will create new account - Neblio address and its Private key.
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> CreateNewAccount(AccountLoadData data);
        public abstract Task ReloadAccountInfo();
        public abstract Task ReloadUtxos();
        public abstract Task<(bool,string)> StartRefreshingData(int interval = 3000);

        #region EncryptionAndVerification

        /// <summary>
        /// Verify message which was signed by some address.
        /// </summary>
        /// <param name="message">Input message</param>
        /// <param name="signature">Signature of this message created by owner of some address.</param>
        /// <param name="address">Neblio address which should sign the message and should be verified.</param>
        /// <returns></returns>
        public async Task<(bool, string)> VerifyMessage(string message, string signature, string address)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
                return (false, "You must fill all inputs.");
            var ownerpubkey = await NFTHelpers.GetPubKeyFromProfileNFTTx(address);
            if (!ownerpubkey.Item1)
                return (false, "Owner did not activate the function. He must have filled the profile.");
            else
                return await ECDSAProvider.VerifyMessage(message, signature, ownerpubkey.Item2);
        }

        /// <summary>
        /// Encrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        public async Task<(bool, string)> EncryptMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return (false, "You must fill the message.");
            if (Secret == null)
                return (false, "Account is not loaded.");
            return await ECDSAProvider.EncryptMessage(message, Secret.PubKey.ToString());
        }

        /// <summary>
        /// Decrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        public async Task<(bool, string)> DecryptMessage(string emessage)
        {
            if (string.IsNullOrEmpty(emessage))
                return (false, "You must fill the encrypted message.");
            if (Secret == null)
                return (false, "Account is not loaded.");
            return await ECDSAProvider.DecryptMessage(emessage, Secret);
        }

        /// <summary>
        /// Obtain verify code of some transaction. This will combine txid and UTC time (rounded to minutes) and sign with the private key.
        /// It will create unique code, which can be verified and it is valid just one minute.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<OwnershipVerificationCodeDto> GetNFTVerifyCode(string txid)
        {
            if (string.IsNullOrEmpty(txid))
                return new OwnershipVerificationCodeDto();
            if (Secret == null)
                return new OwnershipVerificationCodeDto();
            var res = await OwnershipVerifier.GetCodeInDto(txid, Secret);
            if (res != null)
                return res;
            else
                return new OwnershipVerificationCodeDto();
        }

        /// <summary>
        /// Verification function for the NFT ownership code generated by GetNFTVerifyCode function.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<(OwnershipVerificationCodeDto, byte[])> GetNFTVerifyQRCode(string txid)
        {
            if (string.IsNullOrEmpty(txid))
                return (new OwnershipVerificationCodeDto(), new byte[0]);
            if (Secret == null)
                return (new OwnershipVerificationCodeDto(), new byte[0]);
            var res = await OwnershipVerifier.GetQRCode(txid, Secret);
            if (res.Item1)
                return (res.Item2.Item1, res.Item2.Item2);
            else
                return (new OwnershipVerificationCodeDto(), new byte[0]);
        }

        /// <summary>
        /// Sign custom message with use of account Private Key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SignMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return (false, "You must fill the message.");
            if (IsLocked())
            {
                return (false, "Account is locked.");
            }
            var key = await AccountKey.GetEncryptedKey();
            return await ECDSAProvider.SignMessage(message, key);
        }

        #endregion
    }
}
