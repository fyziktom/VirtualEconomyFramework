using Dasync.Collections;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.DogeAPI;
using VEDriversLite.Security;

namespace VEDriversLite.Dogecoin
{
    public class ImageNFT
    {
        public string Utxo { get; set; } = string.Empty;
        public int UtxoIndex { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public double Price { get; set; } = 0.0;
        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
    public static class NFTFactory
    {
        public static async Task<ImageNFT> GetImageNFT(Utxo utxo)
        {
            if (Convert.ToDouble(utxo.Value,CultureInfo.InvariantCulture) != DogeNFTAccount.NFTValue) return null;
            try
            {
                var nft = new ImageNFT();
                nft.Utxo = utxo.TxId;
                nft.UtxoIndex = utxo.N;
                nft.Time = TimeHelpers.UnixTimestampToDateTime(utxo.Time * 1000);

                var txinfo = await DogeTransactionHelpers.TransactionInfoAsync(utxo.TxId);
                if (txinfo != null)
                {
                    var msg = await DogeTransactionHelpers.ParseDogeMessage(txinfo);
                    if (msg.Item1)
                    {
                        var bytes = await VEDriversLite.NFT.NFTHelpers.IPFSDownloadFromInfuraAsync(msg.Item2);
                        if (bytes.Length > 0)
                        {
                            var s = Encoding.UTF8.GetString(bytes);
                            var nftp = JsonConvert.DeserializeObject<ImageNFT>(s);
                            return nftp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot parse the NFT. " + ex.Message);
            }
            return null;
        }
    }

    public class DogeNFTAccount
    {
        public static double NFTValue { get; } = 0.01;
        public string Address { get; set; } = string.Empty;
        public BitcoinSecret Secret { get; set; }
        public List<Utxo> Utxos { get; set; } = new List<Utxo>();
        public ConcurrentDictionary<string, ImageNFT> NFTsDict { get; set; } = new ConcurrentDictionary<string, ImageNFT>();
        public List<ImageNFT> NFTs
        {
            get => NFTsDict.Values.OrderBy(n => n.Time).Reverse().ToList();
        }

        public event EventHandler Refreshed;

        public async Task<bool> LoadAccount(string privatekey)
        {
            try
            {
                var key = await DogeTransactionHelpers.IsPrivateKeyValid(privatekey);
                if (key.Item1)
                {
                    Secret = key.Item2;
                    var add = await DogeTransactionHelpers.GetAddressFromPrivateKey(Secret.ToString());
                    if (add.Item1)
                    {
                        Address = add.Item2;
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot Load the Account. Probably wrong private key. ");
            }
            return false;
        }

        public async Task StartRefreshingData(int interval = 5000)
        {
            try
            {
                await ReloadUtxos();
                await ReloadNFT();
                Refreshed?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load utxos and nfts.");
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await ReloadUtxos();
                        await ReloadNFT();
                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        //
                    }
                    await Task.Delay(interval);
                }
            });
        }

        public async Task ReloadUtxos()
        {
            var count = Utxos.Count;
            try
            {
                var ux = await DogeTransactionHelpers.AddressUtxosAsync(Address);
                var ouxox = ux?.Data.Utxos.OrderBy(u => u.Confirmations)?.ToList();
                if (ouxox?.Count > 0)
                {
                    Utxos.Clear();
                    foreach (var u in ouxox)
                        Utxos.Add(u);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot get dogecoin address utxos. Please check the internet connection. " + ex.Message);
                return;
            }
        }

        public async Task ReloadNFT()
        {
            var nftutxos = Utxos.Where(u => (Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) == NFTValue)).ToArray();
            if (nftutxos == null || nftutxos.Count() == 0)
                return;
            var lastnftsCount = NFTsDict.Count();
            var nftutxosCount = nftutxos.Count();

            if (NFTsDict.Count > 0)
                foreach (var n in NFTsDict.Values)
                    if (nftutxos.FirstOrDefault(nu => nu.TxId == n.Utxo && nu.N == n.UtxoIndex) == null)
                        NFTsDict.TryRemove($"{n.Utxo}:{n.UtxoIndex}", out var nft);

            await nftutxos.ParallelForEachAsync(async n =>
            {
                if (!NFTsDict.TryGetValue($"{n.TxId}:{n.N}", out var nft))
                {
                    nft = await NFTFactory.GetImageNFT(n);
                    if (nft != null)
                        NFTsDict.TryAdd($"{n.TxId}:{n.N}", nft);
                }
            }, maxDegreeOfParallelism: 10);
        }

        public async Task<(string, ICollection<Utxo>)> CheckSpendableDoge(double amount)
        {
            try
            {
                var utxos = await DogeTransactionHelpers.GetAddressSpendableUtxo(Address, 0.2, amount);
                if (utxos == null || utxos.Count == 0)
                    return ($"You dont have enough Doge on the address. You need 5 Doge more than you want to send (for max fee). Probably waiting for more than {DogeTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                    return ("OK", utxos);
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Doge. " + ex.Message, null);
            }
        }

        public async Task<string> MintNFT(ImageNFT nft, string receiver)
        {
            if (string.IsNullOrEmpty(receiver)) return "You must fill the receiver";
            if (receiver == Address) return "You must fill receiver different than your address";
            var fee = 1;
            var receivers = new Dictionary<string, double>();
            receivers.Add(receiver, NFTValue);
            var utxos = await CheckSpendableDoge(2);
            if (utxos.Item2 == null) return "Not spendable Dogecoin";
            var total = 0.0;
            foreach (var u in utxos.Item2)
                total += Convert.ToDouble(u.Value, CultureInfo.InvariantCulture);
            receivers.Add(Address, total - fee - NFTValue);
            var ack = new EncryptionKey(Secret.ToString());
            var data = JsonConvert.SerializeObject(nft);
            byte[] byteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);
            var nftipfshash = await VEDriversLite.NFT.NFTHelpers.UploadInfura(stream, "nft.txt");
            if (string.IsNullOrEmpty(nftipfshash)) return "Cannot upload NFT data to the IPFS";
            nftipfshash = nftipfshash.Replace("https://ipfs.infura.io/ipfs/", string.Empty);
            var txid = await DogeTransactionHelpers.SendDogeTransactionWithMessageMultipleOutputAsync(receivers, ack, utxos.Item2, message: nftipfshash);
            return txid;
        }

        public async Task<string> SendNFT(ImageNFT nft, string receiver = "")
        {
            var fee = 1;
            var receivers = new Dictionary<string, double>();
            if (string.IsNullOrEmpty(receiver)) receiver = Address;
            receivers.Add(receiver, NFTValue);
            var utxos = await CheckSpendableDoge(2);
            if (utxos.Item2 == null) return "Not spendable Dogecoin";
            var total = 0.0;
            foreach (var u in utxos.Item2)
                total += Convert.ToDouble(u.Value, CultureInfo.InvariantCulture);
            receivers.Add(Address, total - fee - NFTValue);
            var ack = new EncryptionKey(Secret.ToString());
            var data = JsonConvert.SerializeObject(nft);
            var utxs = new List<Utxo>();
            var ux = Utxos.FirstOrDefault(u => u.TxId == nft.Utxo && u.N == nft.UtxoIndex);
            utxs.Add(ux);
            foreach (var u in utxos.Item2)
                utxs.Add(u);
            var txid = await DogeTransactionHelpers.SendDogeTransactionWithMessageMultipleOutputAsync(receivers, ack, utxs, message: data);
            return txid;
        }

        public async Task<string> SendTx(Dictionary<string, double> receivers, string message = "")
        {
            var fee = 1;
            var utxos = await CheckSpendableDoge(2);
            if (utxos.Item2 == null) return "Not spendable Dogecoin";
            var total = 0.0;
            foreach (var u in utxos.Item2)
                total += Convert.ToDouble(u.Value, CultureInfo.InvariantCulture);
            var ack = new EncryptionKey(Secret.ToString());
            var txid = await DogeTransactionHelpers.SendDogeTransactionWithMessageMultipleOutputAsync(receivers, ack, utxos.Item2, message: message);
            return txid;
        }
    }
}
