using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Admin.Dto;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;

namespace VEDriversLite
{
    /// <summary>
    /// Basic account Handler class
    /// It can load/add/remove/handle multiple account, use their actions, etc.
    /// It can use the verification/protection of the calls with the system of the signatures of the messages
    /// </summary>
    public static class AccountHandler
    {
        /// <summary>
        /// Function will check if the requested action exists and if the signature match
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="signature"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<IAdminAction> VerifyAdminAction(string admin, string signature, string message)
        {
            if (string.IsNullOrEmpty(admin) || string.IsNullOrEmpty(signature))
                throw new Exception("You must fill the admin address and signature for this action.");

            if (VEDLDataContext.AdminAddresses.Contains(admin))
                if (VEDLDataContext.Accounts.TryGetValue(admin, out var acc))
                    if (VEDLDataContext.AdminActionsRequests.TryRemove(message, out var areq))
                    {
                        var res = await ECDSAProvider.VerifyMessage(message, signature, acc.Secret.PubKey);
                        if (res.Item1)
                            return areq;
                    }
            return null;
        }

        /// <summary>
        /// Function will add new Neblio Account to the VEDLDataContext.Accounts
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="address"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> AddEmptyNeblioAccount(AdminActionBase baseInfo, string address, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            if (VEDLDataContext.Accounts.TryAdd(address, new NeblioAccount()))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Function will add new Neblio Account without Private Key to the VEDLDataContext.Accounts
        /// Account can be used just for the observation.
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="address"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> AddReadOnlyNeblioAccount(AdminActionBase baseInfo, string address, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            var acc = new NeblioAccount();
            var res = await acc.LoadAccountWithDummyKey("", address);
            if (!res)
                return false;
            if (VEDLDataContext.Accounts.TryAdd(address, acc))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Connect Doge account to the Neblio Account
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="dto"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> ConnectDogeAccount(AdminActionBase baseInfo, ConnectDogeAccountDto dto, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            try
            {
                if (!VEDLDataContext.Accounts.TryGetValue(dto.AccountAddress, out var account))
                    return false;
                if (!VEDLDataContext.DogeAccounts.TryGetValue(dto.dogeAccountAddress, out var doge))
                    return false;

                await account.ConnectDogeAccount(doge.Address);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot connect doge account {dto.dogeAccountAddress} to the neblio account {dto.AccountAddress}. " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Disconnect Doge account from Neblio account
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="dto"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> DisconnectDogeAccount(AdminActionBase baseInfo, ConnectDogeAccountDto dto, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            try
            {
                if (!VEDLDataContext.Accounts.TryGetValue(dto.AccountAddress, out var account))
                    return false;
                if (account.ConnectedDogeAccountAddress != dto.dogeAccountAddress)
                    return true;
                if (!VEDLDataContext.DogeAccounts.TryGetValue(dto.dogeAccountAddress, out var doge))
                    return false;

                await account.DisconnectDogeAccount();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot disconnect doge account {dto.dogeAccountAddress} in the neblio account {dto.AccountAddress}. " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Decrypt and oad file from VENFT app to account
        /// If account already exists, the data is updates.
        /// If account do not exists, it will create new one and fill with the data
        /// All accounts are stored in VEDLDataContext static class
        /// After upload of the data, the local backup file is created. It is ecnrypted with private key, so key.json file is still required for the restart
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="data"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> LoadVENFTBackup(AdminActionBase baseInfo, EncryptedBackupDto data, bool verifyActive = false)
        {
            IAdminAction vres = null;
            if (verifyActive)
            {
                vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            if (vres.Type != AdminActionTypes.ImportVENFTBackup)
                throw new Exception("Provided Wrong action type.");

            if (!VEDLDataContext.Accounts.TryGetValue(baseInfo.Admin, out var admin))
                return false;

            var dadd = await ECDSAProvider.DecryptMessage(data.eadd, admin.Secret);
            var dbackup = await ECDSAProvider.DecryptMessage(data.edata, admin.Secret);
            var dpass = await ECDSAProvider.DecryptMessage(data.epass, admin.Secret);

            if (!dbackup.Item1 || !dpass.Item1 || !dadd.Item1)
                return false;
            Console.WriteLine("Loaded correct information for the update from the backup.");
            if (VEDLDataContext.Accounts.TryGetValue(dadd.Item2, out var acc))
            {
                var r = await acc.LoadAccountFromVENFTBackup(dpass.Item2, dbackup.Item2);
                if (r)
                    FileHelpers.WriteTextToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dadd.Item2 + "-backup.json"), JsonConvert.SerializeObject(data));
                Console.WriteLine("Account updated from the backup.");
                return true;
            }
            else
            {
                var account = new NeblioAccount();
                var res = await account.LoadAccountFromVENFTBackup(dpass.Item2, dbackup.Item2);
                if (res && VEDLDataContext.Accounts.TryAdd(dadd.Item2, new NeblioAccount()))
                {
                    FileHelpers.WriteTextToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dadd.Item2 + "-backup.json"), JsonConvert.SerializeObject(data));
                    if (data.asAdmin)
                        VEDLDataContext.AdminAddresses.Add(dadd.Item2);
                    Console.WriteLine("Account updated from the backup.");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove Neblio Account from the VEDLDataContext.Accounts
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="address"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> RemoveNeblioAccount(AdminActionBase baseInfo, string address, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            if (VEDLDataContext.Accounts.TryRemove(address, out var acc))
                return true;
            else
                return false;
        }

        #region NFTs

        /// <summary>
        /// Reload all NFT Hashes. 
        /// NFT Hash is shorten version of the NFT hash, just to save space if you have just smaller amount of the NFTs.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> ReloadNFTHashes()
        {
            try
            {
                var nhs = new ConcurrentDictionary<string, NFTHash>();
                VEDLDataContext.Accounts.Values.ToList()
                    .ForEach(a => a.NFTs
                    .ForEach(n => nhs.TryAdd(NFTHash.GetShortHash(n.Utxo, n.UtxoIndex), new NFTHash()
                    {
                        TxId = n.Utxo,
                        Index = n.UtxoIndex,
                        Type = n.Type,
                        MainAddress = a.Address,
                        SubAccountAddress = string.Empty,
                        Description = n.Description,
                        Name = n.Name,
                        Image = n.ImageLink,
                        Link = n.Link,
                        Price = n.Price,
                        DogePrice = n.DogePrice,
                        AuthorDogeAddress = n.DogeAddress,
                        DogeftInfo = n.DogeftInfo
                    })));

                VEDLDataContext.Accounts.Values.ToList()
                    .ForEach(a => a.SubAccounts.Values.ToList()
                    .ForEach(s => s.NFTs
                    .ForEach(n => nhs.TryAdd(NFTHash.GetShortHash(n.Utxo, n.UtxoIndex), new NFTHash()
                    {
                        TxId = n.Utxo,
                        Index = n.UtxoIndex,
                        Type = n.Type,
                        MainAddress = a.Address,
                        SubAccountAddress = s.Address,
                        Description = n.Description,
                        Name = n.Name,
                        Image = n.ImageLink,
                        Link = n.Link,
                        Price = n.Price,
                        DogePrice = n.DogePrice,
                        AuthorDogeAddress = n.DogeAddress,
                        DogeftInfo = n.DogeftInfo
                    }))));

                if (nhs != null)
                {
                    VEDLDataContext.NFTHashs.Clear();
                    VEDLDataContext.NFTHashs = nhs;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh NFTHashes." + ex.Message);
            }
            return false;
        }

        #endregion

        #region Doge
        /// <summary>
        /// Add empty Doge Account to the VEDLDataContext.DogeAccounts
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="address"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> AddEmptyDogeAccount(AdminActionBase baseInfo, string address, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            if (VEDLDataContext.DogeAccounts.TryAdd(address, new DogeAccount()))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Remove Doge Account from the VEDLDataContext.DogeAccounts
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="address"></param>
        /// <param name="verifyActive"></param>
        /// <returns></returns>
        public static async Task<bool> RemoveDogeAccount(AdminActionBase baseInfo, string address, bool verifyActive = false)
        {
            if (verifyActive)
            {
                var vres = await VerifyAdminAction(baseInfo.Admin, baseInfo.Signature, baseInfo.Message);
                if (vres == null)
                    return false;
            }

            if (VEDLDataContext.DogeAccounts.TryRemove(address, out var acc))
                return true;
            else
                return false;
        }

        #endregion
    }
}
