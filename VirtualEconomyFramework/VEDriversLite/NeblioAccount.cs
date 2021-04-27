using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.NFT;
using VEDriversLite.Security;

namespace VEDriversLite
{
    public class NeblioAccount
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public double NumberOfTransaction { get; set; } = 0;
        public double NumberOfLoadedTransaction { get; } = 0;
        public double? TotalBalance { get; set; } = 0.0;
        public double? TotalSpendableBalance { get; set; } = 0.0;
        public double? TotalUnconfirmedBalance { get; set; } = 0.0;
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        public Dictionary<string, TokenSupplyDto> TokensSupplies { get; set; } = new Dictionary<string, TokenSupplyDto>();
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        [JsonIgnore]
        public EncryptionKey AccountKey { get; set; }

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
            {
                return true;
            }
        }

        public async Task<bool> CreateNewAccount(string password, bool saveToFile = false)
        {
            try
            {
               await Task.Run(async () =>
               {
                   var network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
                   Key privateKey = new Key(); // generate a random private key
                    PubKey publicKey = privateKey.PubKey;
                   BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(network);
                   var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, network);
                   Address = address.ToString();

                    // todo load already encrypted key
                    AccountKey = new Security.EncryptionKey(privateKeyFromNetwork.ToString(), password);
                   AccountKey.PublicKey = Address;

                   if (!string.IsNullOrEmpty(password))
                       AccountKey.PasswordHash = await Security.SecurityUtil.HashPassword(password);

                   if (saveToFile)
                   {
                        // save to file
                        var kdto = new KeyDto()
                       {
                           Address = Address,
                           Key = await AccountKey.GetEncryptedKey(returnEncrypted: true)
                       };

                       FileHelpers.WriteTextToFile("key.txt", JsonConvert.SerializeObject(kdto));
                   }
               });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot create account! " + ex.Message);
            }

            return false;
        }

        public async Task<bool> LoadAccount(string password)
        {
            if (FileHelpers.IsFileExists("key.txt"))
            {
                try
                {
                    var k = FileHelpers.ReadTextFromFile("key.txt");
                    var kdto = JsonConvert.DeserializeObject<KeyDto>(k);

                    AccountKey = new EncryptionKey(kdto.Key, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;

                    Address = kdto.Address;
                }
                catch(Exception ex)
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
                }
            }
            else
            {
                CreateNewAccount(password);
            }

            return false;
        }

        public async Task<bool> LoadAccount(string password, string encryptedKey, string address)
        {
            try
            {
                await Task.Run(async () =>
                {
                    AccountKey = new EncryptionKey(encryptedKey, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;

                    Address = address;
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
            }

            return false;
        }

        public async Task LoadBookmarks(string bookmarks)
        {
            try
            {
                var bkm = JsonConvert.DeserializeObject<List<Bookmark>>(bookmarks);
                if (bkm != null)
                    Bookmarks = bkm;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot deserialize the bookmarks.");
            }
        }

        public async Task<(bool,string)> AddBookmark(string name, string address, string note)
        {
            if (!Bookmarks.Any(b => b.Address == address))
                Bookmarks.Add(new Bookmark()
                {
                    Name = name,
                    Address = address,
                    Note = note
                });
            else
                return (false,"Already Exists!");

            return (true,JsonConvert.SerializeObject(Bookmarks));
        }

        public async Task<(bool,string)> RemoveBookmark(string address)
        {
            var bk = Bookmarks.FirstOrDefault(b => b.Address == address);
            if (bk != null)
                Bookmarks.Remove(bk);
            else
                return (false,"Not Found!");

            return (true,JsonConvert.SerializeObject(Bookmarks));
        }

        public async Task<string> SerializeBookmarks()
        {
            return JsonConvert.SerializeObject(Bookmarks);
        }

        public async Task ReloadTokenSupply()
        {
            TokensSupplies = await NeblioTransactionHelpers.CheckTokensSupplies(Address);
        }
    }
}
