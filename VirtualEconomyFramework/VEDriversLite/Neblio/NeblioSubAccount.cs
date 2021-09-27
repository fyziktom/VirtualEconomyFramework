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
    public class NeblioSubAccount : NeblioAccountBase
    {
        private static object _lock { get; set; } = new object();

        public string EKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// When this flag is set, account reload the Utxos state - inside autorefresh
        /// </summary>
        [JsonIgnore]
        public bool IsAutoRefreshActive { get; set; } = false;

        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
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
                EKey = mainSecret.PubKey.Encrypt(privateKeyFromNetwork.ToString());
                Name = name;
                return (true, Address);
            }
            catch(Exception ex)
            {
                //todo
                return (false, ex.Message);
            }
        }

        public async Task<(bool,string)> LoadFromBackupString(BitcoinSecret mainSecret, string inputData)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<AccountExportDto>(inputData);
                if (dto == null)
                    return (false, "Cannot load SubAccount from backup. Wrong input data.");

                Address = dto.Address;
                EKey = dto.EKey;
                Name = dto.Name;
                var key = mainSecret.PrivateKey.Decrypt(dto.EKey);
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

        public async Task<(bool, string)> LoadFromBackupDto(BitcoinSecret mainSecret, AccountExportDto dto)
        {
            try
            {
                if (dto == null)
                    return (false, "Cannot load SubAccount from backup. Wrong input data.");

                Address = dto.Address;
                EKey = dto.EKey;
                Name = dto.Name;
                var key = mainSecret.PrivateKey.Decrypt(dto.EKey);
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

        public async Task<(bool,string)> BackupAddressToString()
        {
            if (string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(EKey))
                return (false, "Account is not loaded.");

            try
            {
                var dto = new AccountExportDto()
                {
                    Address = Address,
                    EKey = EKey,
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

        public async Task<(bool, AccountExportDto)> BackupAddressToDto()
        {
            if (string.IsNullOrEmpty(Address) || string.IsNullOrEmpty(EKey))
                return (false, null);

            try
            {
                var dto = new AccountExportDto()
                {
                    Address = Address,
                    EKey = EKey,
                    Name = Name
                };
                return (true, dto);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }

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

                var tasks = new Task[3];
                tasks[0] = ReloadTokenSupply();
                tasks[1] = ReloadMintingSupply();
                FirsLoadingStatus?.Invoke(Address, $"Loading of Sub Account {Name} NFTs started.");
                tasks[2] = ReLoadNFTs();
                await Task.WhenAll(tasks);

                tasks[0] = ReloadCoruzantNFTs();
                tasks[1] = RefreshAddressReceivedPayments();
                tasks[2] = RefreshAddressReceivedReceipts();
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
                var tasks = new Task[3];
                
                while (true)
                {
                    try
                    {
                        if (IsAutoRefreshActive && !firstLoad)
                        {
                            await ReloadUtxos();
                            tasks[0] = ReloadTokenSupply();
                            tasks[1] = ReloadMintingSupply();
                            tasks[2] = ReLoadNFTs();

                            await Task.WhenAll(tasks);

                            tasks[0] = ReloadCoruzantNFTs();
                            tasks[1] = RefreshAddressReceivedPayments();
                            tasks[2] = RefreshAddressReceivedReceipts();

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
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }

                    await Task.Delay(interval);
                }

            });

            return await Task.FromResult("RUNNING");
        }

    }
}
