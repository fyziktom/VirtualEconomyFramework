using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Bookmarks;
using VEDriversLite.Security;
using System.Threading.Tasks;
using System.Linq;
using VEDriversLite.Events;
using NBitcoin;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.Dto;
using System.Collections.Concurrent;

namespace VEDriversLite.Neblio
{
    /// <summary>
    /// Neblio Sub Account. It has some limited functions. It is usually loaded under NeblioAccount
    /// </summary>
    public class NeblioSubAccount : NeblioAccountBase
    {
        private static object _lock { get; set; } = new object();
        /// <summary>
        /// Encrypted Private Key with use of the NBitcoin alg. with main public key enc and private key dec
        /// </summary>
        public string EKey { get; set; } = string.Empty;
        /// <summary>
        /// Encrypted Private Key with use of SHA256 hash of main private key and AES256
        /// </summary>
        public string ESKey { get; set; } = string.Empty;
        /// <summary>
        /// Name of the Sub Account
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// When this flag is set, account reload the Utxos state - inside autorefresh
        /// </summary>
        [JsonIgnore]
        public bool IsAutoRefreshActive { get; set; } = false;

        /// <summary>
        /// Bookmark related to this Sub Account loaded from the Main account if exist
        /// </summary>
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        /// <summary>
        /// Indicate if this Sub Account is in the Bookmark
        /// </summary>
        [JsonIgnore]
        public bool IsInBookmark { get; set; } = false;

        /// <summary>
        /// This event is called whenever some progress during multimint happens
        /// </summary>
        public event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public event EventHandler<string> FirsLoadingStatus;

        /// <summary>
        /// Create Sub Account Address and Private Key
        /// The Account Private key is encrypted with use of main account private key
        /// </summary>
        /// <param name="mainSecret">Main Account Private Key</param>
        /// <param name="name">Name of the Sub Account</param>
        /// <returns></returns>
        public async Task<(bool,string)> CreateAddress(BitcoinSecret mainSecret, string name)
        {
            if (!string.IsNullOrEmpty(Address))
                return (false, "Account already contains address.");

            try
            {
                Key privateKey = new Key(); // generate a random private key
                PubKey publicKey = privateKey.PubKey;
                BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(NeblioTransactionHelpers.Network);
                var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
                Address = address.ToString();
                Secret = privateKeyFromNetwork;
                // todo load already encrypted key
                AccountKey = new Security.EncryptionKey(privateKeyFromNetwork.ToString());
                AccountKey.PublicKey = Address;

                //ESKey = SymetricProvider.EncryptString(SecurityUtils.ComputeSha256Hash(mainSecret.PrivateKey.ToString()), privateKeyFromNetwork.ToString());
                EKey = ECDSAProvider.EncryptStringWithPublicKey(privateKeyFromNetwork.ToString(), mainSecret.PubKey);// TODO: some preprocessor directive for run just in old version under .NETStandard2.1
                //EKey = mainSecret.PubKey.Encrypt(privateKeyFromNetwork.ToString());// TODO: some preprocessor directive for run just in old version under .NETStandard2.1
                Name = name;
                return (true, Address);
            }
            catch(Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        private string DecryptEncryptedKey(BitcoinSecret mainSecret)
        {
            string key = string.Empty;
            if (!string.IsNullOrEmpty(ESKey))
                key = SymetricProvider.DecryptString(SecurityUtils.ComputeSha256Hash(mainSecret.PrivateKey.ToString()), ESKey);
            else if (!string.IsNullOrEmpty(EKey))
                key = ECDSAProvider.DecryptStringWithPrivateKey(EKey, mainSecret.PrivateKey); // TODO: some preprocessor directive for run just in old version under .NETStandard2.1
            return key;
        }

        /// <summary>
        /// Load Sub Account from the string with AcountExportDto data
        /// </summary>
        /// <param name="mainSecret"></param>
        /// <param name="inputData">serialized AcountExportDto</param>
        /// <returns></returns>
        public async Task<(bool,string)> LoadFromBackupString(BitcoinSecret mainSecret, string inputData)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<AccountExportDto>(inputData);
                if (dto == null)
                    return (false, "Cannot load SubAccount from backup. Wrong input data.");

                Address = dto.Address;
                EKey = dto.EKey;
                ESKey = dto.ESKey;
                Name = dto.Name;

                var key = DecryptEncryptedKey(mainSecret);

                AccountKey = new EncryptionKey(key);
                AccountKey.PublicKey = Address;
                Secret = new BitcoinSecret(key, NeblioTransactionHelpers.Network);
                return (true, Address);
            }
            catch (Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Load Sub Account from AcountExportDto
        /// </summary>
        /// <param name="mainSecret"></param>
        /// <param name="dto">account export data</param>
        /// <returns></returns>
        public async Task<(bool, string)> LoadFromBackupDto(BitcoinSecret mainSecret, AccountExportDto dto)
        {
            try
            {
                if (dto == null)
                    return (false, "Cannot load SubAccount from backup. Wrong input data.");

                Address = dto.Address;
                EKey = dto.EKey;
                ESKey = dto.ESKey;
                Name = dto.Name;
                var key = DecryptEncryptedKey(mainSecret);
                AccountKey = new EncryptionKey(key);
                AccountKey.PublicKey = Address;
                Secret = new BitcoinSecret(key, NeblioTransactionHelpers.Network);
                return (true, Address);
            }
            catch (Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Backup Sub Account data as serialized AcountExportDto
        /// </summary>
        /// <returns>true and serialized AcountExportDto</returns>
        public async Task<(bool,string)> BackupAddressToString()
        {
            if (string.IsNullOrEmpty(Address) || (string.IsNullOrEmpty(EKey) && string.IsNullOrEmpty(ESKey)))
                return (false, "Account is not loaded.");

            try
            {
                var dto = new AccountExportDto()
                {
                    Address = Address,
                    EKey = EKey,
                    ESKey = ESKey,
                    Name = Name
                };
                var res = JsonConvert.SerializeObject(dto);
                return (true, res);
            }
            catch(Exception ex)
            {
                return (false, "Cannot backup address from SubAccount. " + ex.Message);
            }
        }

        /// <summary>
        /// Backup Sub Account to AcountExportDto
        /// </summary>
        /// <returns>true and AcountExportDto</returns>
        public (bool, AccountExportDto) BackupAddressToDto()
        {
            if (string.IsNullOrEmpty(Address) || (string.IsNullOrEmpty(EKey) && string.IsNullOrEmpty(ESKey)))
                return (false, null);

            try
            {
                var dto = new AccountExportDto()
                {
                    Address = Address,
                    EKey = EKey,
                    ESKey = ESKey,
                    Name = Name
                };
                return (true, dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Canont backup address data dto. " + ex.Message);
                return (false, null);
            }
        }

        /// <summary>
        /// Load bookmark
        /// </summary>
        /// <param name="bkm"></param>
        public void LoadBookmark(Bookmark bkm)
        {
            if (!string.IsNullOrEmpty(bkm.Address) && !string.IsNullOrEmpty(bkm.Name))
            {
                IsInBookmark = true;
                BookmarkFromAccount = bkm;
            }
            else
                ClearBookmark();
        }

        /// <summary>
        /// Clear bookmark object
        /// </summary>
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }

        /// <summary>
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        public async Task<string> StartRefreshingData(int interval = 3000)
        {
            if (string.IsNullOrEmpty(Address) || !AccountKey.IsLoaded)
            {
                await InvokeErrorEvent("Please fill subaccount Address and Key first.", "Not loaded address and key.");
                return await Task.FromResult("Please fill subaccount Address and Key first.");
            }

            try
            {
                FirsLoadingStatus?.Invoke(this, "Loading of Sub Account data started.");
                AddressInfo = new GetAddressResponse();
                AddressInfo.Transactions = new List<string>();
                await ReloadUtxos();

                try
                {
                    await TxCashPreload();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot finish the preload." + ex.Message);
                }

                var tasks = new Task[4];
                tasks[0] = ReloadTokenSupply();
                tasks[1] = ReloadMintingSupply();
                FirsLoadingStatus?.Invoke(Address, $"Loading of Sub Account {Name} NFTs started.");
                tasks[2] = ReLoadNFTs(withoutMessages:true);
                tasks[3] = Task.Delay(100);
                await Task.WhenAll(tasks);

                tasks[0] = ReloadCoruzantNFTs();
                tasks[1] = ReloadHardwarioNFTs();
                tasks[2] = RefreshAddressReceivedPayments();
                tasks[3] = RefreshAddressReceivedReceipts();
                await Task.WhenAll(tasks);
                FirsLoadingStatus?.Invoke(Address, $"Sub Account {Name} first load done.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Problem during first load of subaccount. " + ex.Message);
            }

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                var firstLoad = true;
                var minorRefresh = 0;
                var tasks = new Task[4];
                
                while (true)
                {
                    try
                    {
                        if (IsAutoRefreshActive && !firstLoad)
                        {
                            await ReloadUtxos();
                            tasks[0] = ReloadTokenSupply();
                            tasks[1] = ReloadMintingSupply();
                            tasks[2] = ReLoadNFTs(withoutMessages:true);
                            tasks[3] = Task.Delay(1);
                            await Task.WhenAll(tasks);

                            tasks[0] = ReloadCoruzantNFTs();
                            tasks[1] = ReloadHardwarioNFTs();
                            tasks[2] = RefreshAddressReceivedPayments();
                            tasks[3] = RefreshAddressReceivedReceipts();

                            await Task.WhenAll(tasks);

                            if (minorRefresh >= 0)
                                minorRefresh--;
                            else
                            {
                                await CheckPayments();
                                minorRefresh = 5;
                            }
                        }
                        if (firstLoad)
                            firstLoad = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot refresh data. " + ex.Message);
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }

                    await Task.Delay(interval);
                }

            });

            return await Task.FromResult("RUNNING");
        }

    }
}
