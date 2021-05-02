using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.Security;

namespace VEDriversLite
{
    public class NeblioAccount
    {
        public NeblioAccount()
        {
            Profile = new ProfileNFT("");
            NFTs = new List<INFT>();
            Bookmarks = new List<Bookmark>();
        }
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public double NumberOfTransaction { get; set; } = 0;
        public double NumberOfLoadedTransaction { get; } = 0;
        public bool EnoughBalanceToBuySourceTokens { get; set; } = false;
        public double TotalBalance { get; set; } = 0.0;
        public double TotalSpendableBalance { get; set; } = 0.0;
        public double TotalUnconfirmedBalance { get; set; } = 0.0;
        public double SourceTokensBalance { get; set; } = 0.0;
        public double AddressNFTCount { get; set; } = 0.0;
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        public Dictionary<string, TokenSupplyDto> TokensSupplies { get; set; } = new Dictionary<string, TokenSupplyDto>();
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public GetAddressResponse AddressInfo { get; set; } = new GetAddressResponse();

        public event EventHandler Refreshed;

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

        public async Task<string> StartRefreshingData(int interval = 3000)
        {
            try
            {
                await ReloadAccountInfo();
                await ReloadMintingSupply();
                await ReloadCountOfNFTs();
                await ReloadTokenSupply();
                await ReLoadNFTs();
            }
            catch (Exception ex)
            {
                // todo
            }

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                var lastNFTcount = AddressNFTCount;
                while (true)
                {
                    try
                    {
                        await ReloadAccountInfo();
                        await ReloadMintingSupply();
                        await ReloadCountOfNFTs();
                        await ReloadTokenSupply();

                        if (lastNFTcount != AddressNFTCount)
                            await ReLoadNFTs();

                        lastNFTcount = AddressNFTCount;

                        var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                        if (pnfts.Count > 0)
                        {
                            foreach (var p in pnfts)
                            {
                                var pn = NFTs.Where(n => n.Utxo == ((PaymentNFT)p).NFTUtxoTxId).FirstOrDefault();
                                if (pn != null)
                                {
                                    var rtxid = await NFTHelpers.SendOrderedNFT(this, (PaymentNFT)p);
                                    Console.WriteLine(rtxid);
                                    await Task.Delay(200);
                                    await ReLoadNFTs();
                                }
                            }
                        }

                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        // todo
                    }

                    await Task.Delay(interval);
                }

            });

            return await Task.FromResult("RUNNING");
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

                await StartRefreshingData();

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

                    await StartRefreshingData();
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

                await StartRefreshingData();

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

        public async Task ReloadCountOfNFTs()
        {
            var nftsu = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address);
            if (nftsu != null)
                AddressNFTCount = nftsu.Count;
        }

        public async Task ReloadMintingSupply()
        {
            var mintingSupply = await NeblioTransactionHelpers.GetActualMintingSupply(Address);
            SourceTokensBalance = mintingSupply.Item1;

        }

        public async Task ReloadAccountInfo()
        {
            AddressInfo = await NeblioTransactionHelpers.AddressInfoAsync(Address);
            if (AddressInfo != null)
            {
                TotalBalance = (double)AddressInfo.Balance;
                TotalUnconfirmedBalance = (double)AddressInfo.UnconfirmedBalance;
                AddressInfo.Transactions = AddressInfo.Transactions.Reverse().ToList();
            }
            else
            {
                AddressInfo = new GetAddressResponse();
            }

            if (TotalBalance > 1)
                EnoughBalanceToBuySourceTokens = true;
        }

        public async Task ReLoadNFTs()
        {
            if (!string.IsNullOrEmpty(Address))
            {
                NFTs = await NFTHelpers.LoadAddressNFTs(Address);
            }
        }

        public async Task<(bool,double)> HasSomeSpendableNeblio(double amount = 0.0002)
        {
            var nutxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0001, amount);
            if (nutxos.Count == 0)
            {
                return (false, 0.0);
            }
            else
            {
                var a = 0.0;
                foreach (var u in nutxos)
                    a += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);

                if (a > amount)
                    return (true, a);
                else
                    return (false, a);
            }
        }

        public async Task<(bool, int)> HasSomeSourceForMinting()
        {
            var tutxos = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, NFTHelpers.TokenId, 1);


            if (tutxos.Count == 0)
            {
                return (false, 0);
            }
            else
            {
                var a = 0;
                foreach (var u in tutxos)
                {
                    var t = u.Tokens.ToArray()[0];
                    a += (int)t.Amount;
                }
                return (true, a);
            }
        }

        public async Task<(bool, string)> ValidateNFTUtxo(string utxo)
        {
            var u = await NeblioTransactionHelpers.ValidateOneTokenNFTUtxo(Address, utxo);
            if (!u.Item1)
            {
                var msg = "Provided source tx transaction is not spendable. Probably waiting for more than 1 confirmation.";
                return (false, msg);
            }
            else
            {
                return (true, "OK");
            }
        }
    }
}
