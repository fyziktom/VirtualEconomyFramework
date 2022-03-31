using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Common;
using VEDriversLite.Devices;
using VEDriversLite.Dto;
using VEDriversLite.Neblio;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.NFT.DevicesNFTs;
using VEDriversLite.Security;
using VEDriversLite.UnstoppableDomains;
using VEDriversLite.WooCommerce;

namespace TestVEDriversLite
{
    public static class VEDriversLiteTests
    {
        private static string password = string.Empty;
        /// <summary>
        /// Global Main Neblio Account used in most of the functions
        /// </summary>
        private static NeblioAccount account = new NeblioAccount();
        /// <summary>
        /// Global Main Dogecoin Account used in most of the functions
        /// </summary>
        private static DogeAccount dogeAccount = new DogeAccount();
        private static object _lock = new object();

        [TestEntry]
        public static void Help(string param)
        {
            Console.WriteLine("Help for The Tests");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("For detail info about functions please look inside of the functions.");
            Console.WriteLine("Important for running:");
            Console.WriteLine("- run GenerateNewAccount if you dont have account. It will create file key.txt where is your new address and key.");
            Console.WriteLine("- run LoadAccount if you have account and stored key.txt file");
            Console.WriteLine("- run LoadAccountWithCreds if you have account but you want to fill manually pass,ekey,address");
            Console.WriteLine("---------------------------------");
        }

        /// <summary>
        /// This function will create new Neblio Account.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void GenerateNewAccount(string param)
        {
            GenerateNewAccountAsync(param);
        }
        public static async Task GenerateNewAccountAsync(string param)
        {
            password = param;
            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.FirsLoadingStatus += Account_FirsLoadingStatus;
            await account.CreateNewAccount(password, true);
            Console.WriteLine($"Account created.");
            Console.WriteLine($"Address: {account.Address}");
            Console.WriteLine($"Encrypted Private Key: {account.AccountKey.GetEncryptedKey("", true)}");
        }
        /// <summary>
        /// This handler provides the data from the initial loading of the account.
        /// It can be used for loading progressbar label, etc.
        /// </summary>
        /// <param name="sender">Address which is loading</param>
        /// <param name="e">This contains message what is going on during the initial load.</param>
        private static void Account_FirsLoadingStatus(object sender, string e)
        {
            Console.WriteLine($"Loading Account {(sender as NeblioAccount).Address}: {e}.");
        }

        /// <summary>
        /// Load Neblio account from the file key.txt
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void LoadAccount(string param)
        {
            LoadAccountAsync(param);
        }
        public static async Task LoadAccountAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Password cannot be empty.");

            password = param;
            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.FirsLoadingStatus += Account_FirsLoadingStatus;
            await account.LoadAccount(password);
        }

        /// <summary>
        /// Load Neblio Account from specific file
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void LoadAccountFromFile(string param)
        {
            LoadAccountFromFileAsync(param);
        }
        public static async Task LoadAccountFromFileAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input pass,filename");
            var pass = split[0];
            var file = split[1];
            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.FirsLoadingStatus += Account_FirsLoadingStatus;
            await account.LoadAccount(pass, file, false);
        }

        /// <summary>
        /// Load Neblio account and destroy all NFTs on it
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void LoadAccountFromFileAndDestroyAllNFTs(string param)
        {
            LoadAccountFromFileAndDestroyAllNFTsAsync(param);
        }
        public static async Task LoadAccountFromFileAndDestroyAllNFTsAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input pass,filename");
            var file = string.Empty;
            var pass = string.Empty;

            if (split.Length == 1)
                file = split[0];
            else if (split.Length == 2)
            {
                pass = split[0];
                file = split[1];
            }
            else
            {
                Console.WriteLine("wrong input.");
                return;
            }

            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.FirsLoadingStatus += Account_FirsLoadingStatus;
            await account.LoadAccount(pass, file, false);

            Console.WriteLine("Loading account NFTs...");
            var attempts = 100;
            while(account.NFTs.Count == 0)
            {
                await Task.Delay(1000);
                if (attempts < 0)
                {
                    Console.WriteLine("Cannot find any NFTs on this address.");
                    return;
                }
                else
                {
                    attempts--;
                }
            }

            Console.WriteLine($"NFTs found. There are {account.NFTs.Count} to destroy. Starting now...");
            while (account.NFTs.Count > 1)
            {
                var nftsToDestroy = new List<INFT>();

                if (account.NFTs.Count >= 10)
                    for (var i = 0; i < 10; i++)
                        nftsToDestroy.Add(account.NFTs[i]);
                else
                    foreach (var nft in account.NFTs)
                        nftsToDestroy.Add(nft);

                if (nftsToDestroy.Count == 0)
                    break;

                Console.WriteLine($"Starting to destroy lot of {nftsToDestroy.Count} NFts from rest of {account.NFTs.Count}. ");
                var done = false;
                while (!done)
                {
                    try
                    {
                        var res = await account.DestroyNFTs(nftsToDestroy);
                        done = res.Item1;
                        if (!done)
                        {
                            await Task.Delay(5000);
                            Console.WriteLine("Probably waiting for enough confirmations." + res.Item2);
                        }
                        else
                        {
                            Console.WriteLine("Another NFTs destroyed. TxId:" + res.Item2);
                            await Task.Delay(7000); // wait to until main account update.
                        }
                        if (nftsToDestroy.Count == 0 || account.NFTs.Count == 0)
                            break;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(5000);
                        Console.WriteLine("Probably .Waiting for enough confirmations." + ex.Message);
                    }
                }
            }

            Console.WriteLine("All NFTs destroyed.");

        }

        /// <summary>
        /// Load VENFT App Backup.
        /// It loads Neblio Account, Dogecoin Account, All Neblio SubAccount, Tabs, MessageTabs and bookmarks
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void LoadAccountFromVENFTBackup(string param)
        {
            LoadAccountFromVENFTBackupAsync(param);
        }
        public static async Task LoadAccountFromVENFTBackupAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input pass,filename");
            var pass = split[0];
            var file = split[1];
            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.FirsLoadingStatus += Account_FirsLoadingStatus;
            await account.LoadAccountFromVENFTBackup(pass,filename: file);

            //StartRefreshingData(null);
        }

        /// <summary>
        /// This is not necessary. Most of the load account functions already call it.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void StartRefreshingData(string param)
        {
            StartRefreshingDataAsync(param);
        }
        public static async Task StartRefreshingDataAsync(string param)
        {
            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");

            await account.StartRefreshingData();
            Console.WriteLine("Refreshing started.");
        }

        /// <summary>
        /// Load Neblio Account with the Credentials like password, private key and address
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void LoadAccountWithCreds(string param)
        {
            LoadAccountWithCredsAsync(param);
        }
        public static async Task LoadAccountWithCredsAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input pass,encryptedprivatekey,address");
            var pass = split[0];
            var ekey = split[1];
            var addr = split[2];
            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.FirsLoadingStatus += Account_FirsLoadingStatus;
            await account.LoadAccount(pass, ekey, addr);
        }

        /// <summary>
        /// Return decrypted private key form the Neblio Account
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void GetDecryptedPrivateKey(string param)
        {
            GetDecryptedPrivateKeyAsync(param);
        }
        public static async Task GetDecryptedPrivateKeyAsync(string param)
        {
            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");
            if (account.IsLocked())
                throw new Exception("Account is locked.");
            var res = account.AccountKey.GetEncryptedKey();
            Console.WriteLine($"Private key for address {account.Address} is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Display some Neblio Account details such as Balance, Number of NFTs, etc.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void DisplayAccountDetails(string param)
        {
            Console.WriteLine("Account Details");
            Console.WriteLine($"Account Total Balance: {account.TotalBalance} NEBL");
            Console.WriteLine($"Account Total Spendable Balance: {account.TotalSpendableBalance} NEBL");
            Console.WriteLine($"Account Total Unconfirmed Balance: {account.TotalUnconfirmedBalance} NEBL");
            Console.WriteLine($"Account Total Tx Count: {account.AddressInfo.Transactions?.Count}");
            Console.WriteLine($"Account Total Utxos Count: {account.Utxos.Count}");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Account Utxos:");
            account.Utxos.ForEach(u => { Console.WriteLine($"Utxo: {u.Txid}:{u.Index}"); });
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine($"Account Total NFTs Count: {account.AddressNFTCount}");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Account NFTs Names:");
            account.NFTs.ForEach(n => { Console.WriteLine($"NFT Name is: {n.Name}"); });
            Console.WriteLine("-----------------------------------------------------");
        }

        /// <summary>
        /// Send classic Neblio transaction
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendTransaction(string param)
        {
            SendTransactionAsync(param);
        }
        public static async Task SendTransactionAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input receiveraddress,amountofneblio");
            var receiver = split[0];
            var am = split[1];
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);
            var res = await account.SendNeblioPayment(receiver, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Function will send all NFTs on the address back to the creator (based on minting tx info).
        /// You can pass Neblio Sub Account Address as the parameter.
        /// </summary>
        /// <param name="param">Leave empty for Main Account or fill Sub Account Address</param>
        /// <returns></returns>
        [TestEntry]
        public static void SendAllNFTsBackToOwners(string param)
        {
            SendAllNFTsBackToOwnersAsync(param);
        }
        public static async Task SendAllNFTsBackToOwnersAsync(string param)
        {

            Console.WriteLine("-------------------------");
            Console.WriteLine("Send All NFTs Back to Owner started");
            Console.WriteLine($"Parameter: {param}");
            Console.WriteLine("-------------------------");

            List<INFT> nfts = new List<INFT>();
            bool isSubAccount = false;
            if (!string.IsNullOrEmpty(param))
            {
                var va = NeblioTransactionHelpers.ValidateNeblioAddress(param);
                if (!string.IsNullOrEmpty(va))
                {
                    if (account.SubAccounts.TryGetValue(param, out var sa))
                    {
                        isSubAccount = true;
                        lock (_lock)
                        {
                            nfts = sa.NFTs;
                        }
                    }
                    else
                    {
                        isSubAccount = false;
                        Console.WriteLine("The provided parameter is the Sub Account address. Exiting the function. ");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("The provided parameter is not the Neblio address. Exiting the function. ");
                    return;
                }
            }
            else
            {
                lock (_lock)
                {
                    nfts = account.NFTs;
                }
            }

            Console.WriteLine($"Total count of NFTs to process is {nfts.Count}.");

            var filename = $"{DateTime.UtcNow.ToString("dd_MM_yyyy_hh_mm_ss")}-sendNFTsBackToOwners.txt";
            Console.WriteLine($"Saving file. {filename}");
            FileHelpers.AppendLineToTextFile("NFTName\tOwner\tTxid", filename);

            foreach (var nft in nfts)
            {
                Console.WriteLine($"Getting creator for NFT {nft.Utxo}:{nft.UtxoIndex}.");
                var sender = await NeblioTransactionHelpers.GetTransactionSender(nft.NFTOriginTxId);
                if (string.IsNullOrEmpty(sender))
                    sender = await NeblioTransactionHelpers.GetTransactionSender(nft.Utxo, nft.TxDetails);
                if (!string.IsNullOrEmpty(sender))
                {
                    Console.WriteLine($"Creator of NFT {nft.Utxo}:{nft.UtxoIndex} is {sender}.");
                    var skip = false;
                    if (!isSubAccount && sender == account.Address) skip = true;
                    if (isSubAccount && account.SubAccounts.TryGetValue(sender, out var sac)) skip = true;
                    if (!skip)
                    {
                        try
                        {
                            (bool, string) res = (false, string.Empty);
                            var done = false;
                            int attempts = 100;
                            Console.WriteLine($"Sending NFT {nft.Utxo}:{nft.UtxoIndex} back to {sender}.");
                            while (!done && attempts > 0)
                            {
                                try
                                {
                                    Console.WriteLine($"Try to send {101 - attempts} of 100 max attempts.");
                                    if (!isSubAccount)
                                        res = await account.SendNFT(sender, nft);
                                    else
                                        res = await account.SendNFTFromSubAccount(param, sender, nft);
                                    if (res.Item1)
                                    {
                                        Console.WriteLine($" NFT {nft.Name} was send to the owner {sender} in txid: {res.Item2}");
                                        Console.WriteLine(res);
                                        FileHelpers.AppendLineToTextFile($"{nft.Name}\t{sender}\t{res.Item2}", filename);
                                        done = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Cannot send the NFT {nft.Utxo}:{nft.UtxoIndex} back to the Owner {sender}. " + res.Item2);
                                        await TryAgainWait(5);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Cannot send the NFT {nft.Utxo}:{nft.UtxoIndex} back to the Owner {sender}. " + ex.Message);
                                    await TryAgainWait(5);
                                }
                            }
                            await Task.Delay(5);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot send the NFT {nft.Name} with txid: {nft.Utxo} with index: {nft.UtxoIndex}. " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"NFT {nft.Utxo}:{nft.UtxoIndex} was created on same address {sender} as trying to send from. It is skiped.");
                    }
                }
                else
                {
                    Console.WriteLine($"Cannot find sender for the NFT {nft.Utxo}:{nft.UtxoIndex}. It is skiped.");
                }
            }
            Console.WriteLine("-------------------------");
            Console.WriteLine("Script ends.");
            Console.WriteLine("-------------------------");
        }

        private static async Task TryAgainWait(int seconds)
        {
            Console.Write("Try again in: ");
            while (seconds > 0)
            {
                await Task.Delay(1000);
                seconds--;
                if (seconds <= 0) break;
            }
        }

        /// <summary>
        /// Send classic Neblio transaction which includes message
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendTransactionWithMessage(string param)
        {
            SendTransactionWithMessageAsync(param);
        }
        public static async Task SendTransactionWithMessageAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input receiveraddress,amountofneblio,message");
            var receiver = split[0];
            var am = split[1];
            var msg = split[2];
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);
            var res = await account.SendNeblioPayment(receiver, amount, msg);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send special Neblio split transaction. It can split the neblio coin to multiple lots
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendSplitTransaction(string param)
        {
            SendSplitTransactionAsync(param);
        }
        public static async Task SendSplitTransactionAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input receiveraddress,amountofneblio,count");
            var receiver = split[0];
            var am = split[1];
            var cnt = split[2];
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);
            var count = Convert.ToInt32(cnt, CultureInfo.InvariantCulture);
            var res = await account.SplitNeblioCoin(new List<string>() { receiver }, count, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Destroy specific NFT or list of the NFts
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void DestroyNFTs(string param)
        {
            DestroyNFTsAsync(param);
        }
        public static async Task DestroyNFTsAsync(string param)
        {
            var nfts = new List<INFT>();

            var nftlist = new List<(string, int)>()
            {
                ("5d5ccf74b2d142c063e01bec584b98edd73e5ca529cf2810db36114eb6bfd208",0),
                ("5d5ccf74b2d142c063e01bec584b98edd73e5ca529cf2810db36114eb6bfd208",1),
                ("5d5ccf74b2d142c063e01bec584b98edd73e5ca529cf2810db36114eb6bfd208",2),
                ("5d5ccf74b2d142c063e01bec584b98edd73e5ca529cf2810db36114eb6bfd208",3),
                ("5d5ccf74b2d142c063e01bec584b98edd73e5ca529cf2810db36114eb6bfd208",4)
            };

            foreach (var nft in nftlist)
            {
                var n = await NFTFactory.GetNFT(NFTHelpers.TokenId, nft.Item1, nft.Item2, 0, true);
                n.UtxoIndex = nft.Item2;
                nfts.Add(n);
            }

            var res = await account.DestroyNFTs(nfts);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send Airdrop transaction to some Neblio Address
        /// This can send tokens and neblio in the same transaction
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendAirdrop(string param)
        {
            SendAirdropAsync(param);
        }
        public static async Task SendAirdropAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input receiveraddress,tokenId,tokenamount,amountofneblio");
            var receiver = split[0];
            var tokid = split[1];
            var tam = split[2];
            var am = split[3];
            var tamount = Convert.ToDouble(tam, CultureInfo.InvariantCulture);
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);
            var res = await account.SendAirdrop(receiver, tokid, tamount, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send 100 VENFT Tokens airdrop including 0.05NEBL
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendVENFTAirdrop(string param)
        {
            SendVENFTAirdropAsync(param);
        }
        public static async Task SendVENFTAirdropAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input receiveraddress");
            var receiver = split[0];
            var tokid = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
            var tamount = 100;
            var amount = 0.05;
            var res = await account.SendAirdrop(receiver, tokid, tamount, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send automatic VENFT Airdrop for multiple addresses based on the list with these addresses.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendVENFTAirdropFromFile(string param)
        {
            SendVENFTAirdropFromFileAsync(param);
        }
        public static async Task SendVENFTAirdropFromFileAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input filename");
            var filename = split[0];
            var receivers = FileHelpers.ReadTextFromFile(filename);
            var receiversList = new List<string>();
            var tokid = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
            var tamount = 100;
            var amount = 0.05;

            (bool, string) res = (false, string.Empty);

            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Automatic VENFT Airdrop started");
            Console.WriteLine("----------------------------------------");
            using (var reader = new StringReader(receivers))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var done = false;
                    while (!done)
                    {
                        try
                        {
                            Console.WriteLine($"Airdrop For address: {line} started.");
                            res = await account.SendAirdrop(line, tokid, tamount, amount);
                            done = res.Item1;
                        }
                        catch (Exception ex)
                        {
                            // probably just waiting for enought confirmation
                            Console.WriteLine("Waiting for confirmation. " + ex.Message);
                            await Task.Delay(5000);
                        }
                    }

                    Console.WriteLine($"New Airdrop for {line} has TxId hash ");
                    Console.WriteLine(res.Item2);
                    Console.WriteLine("----------------------------------------");
                }
            }
        }

        /// <summary>
        /// Send classci Neblio transaction with the tokens.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendTokenTransaction(string param)
        {
            SendTokenTransactionAsync(param);
        }
        public static async Task SendTokenTransactionAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input receiveraddress,amountoftokens,data");
            var receiver = split[0];
            var am = split[1];
            var data = split[2];

            var amount = Convert.ToInt32(am);
            // create metadata
            var metadata = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(data))
                metadata.Add("Data", data);
            else
                metadata.Add("Data", "My first Neblio token metadata");
            // send 10 VENFT to receiver with connected metadata
            var res = await account.SendNeblioTokenPayment(NFTHelpers.TokenId, metadata, receiver, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint new Neblio NFT.
        /// In this example the ImageNFT is created
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFT(string param)
        {
            MintNFTAsync(param);
        }
        public static async Task MintNFTAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 5)
                throw new Exception("Please input name,author,description,link,imagelink");

            Console.WriteLine("Minting NFT");
            // create NFT object
            var nft = new ImageNFT("");

            Console.WriteLine("Fill name:");
            var data = split[0];
            if (!string.IsNullOrEmpty(data))
                nft.Name = data;
            else
                nft.Name = "My First NFT";

            Console.WriteLine("Fill Author:");
            data = split[1];
            if (!string.IsNullOrEmpty(data))
                nft.Author = data;
            else
                nft.Author = "fyziktom";

            Console.WriteLine("Fill Description:");
            data = split[2];
            if (!string.IsNullOrEmpty(data))
                nft.Description = data;
            else
                nft.Description = "This was created with VEDriversLite";

            Console.WriteLine("Fill Link:");
            data = split[3];
            if (!string.IsNullOrEmpty(data))
                nft.Link = data;
            else
                nft.Link = "https://veframework.com/";

            Console.WriteLine("Fill Image Link:");
            data = split[4];
            if (!string.IsNullOrEmpty(data))
                nft.ImageLink = data;
            else
                nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmWTkVqaWn1ABZ1UMKL91pxxspzXW6yodJ9bjUn6nPLeHX";

            // MintNFT
            var res = await account.MintNFT(nft);

            // or multimint with 5 coppies (mint 1 + 5);
            // var res = await account.MintMultiNFT(NFTHelpers.TokenId, nft, 5); 
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio NFT Ticket
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTTicket(string param)
        {
            MintNFTTicketAsync(param);
        }
        public static async Task MintNFTTicketAsync(string param)
        {
            Console.WriteLine("Minting NFT");
            // create NFT object
            var nft = new TicketNFT("");

            /*
            nft.Author = "fyziktom";
            nft.Name = "Birth of the fyziktom :D";
            nft.Description = "The moment when the fyziktom start to see the world with own eyes.";
            nft.Tags = "fyziktom birth revolution robot timetravel";
            nft.Link = "https://veframework.com/";
            nft.AuthorLink = "https://fyziktom.com/";
            nft.EventId = "531221bc8a59b5b36af8dcaf6dac317b204b89dfc3ed301d497032c3bcf5799c";
            nft.EventDate = DateTime.Parse("1991-03-11T10:20:00");
            nft.Location = "Brno,Czech Republic";
            nft.LocationCoordinates = "49.175621,16.569651";
            nft.Seat = "Row A, Seat 55";
            nft.Price = 0.05;
            nft.PriceInDoge = 10;
            nft.TicketClass = ClassOfNFTTicket.VIP;
            nft.VideoLink = "https://youtu.be/dwemJ4Sx1CA";
            nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmQ5qNNtShVqrZstzWMTZeWXFSnDojks3RWZpja6gJy8MJ";
            */

            nft.Author = "Elton John";
            nft.Name = "FAREWELL - The Final Tour";
            nft.Description = "The Final Tour of genius of the music.";
            nft.Tags = "eltonjohn tour genius";
            nft.Link = "https://www.eltonjohn.com/";
            nft.AuthorLink = "https://www.eltonjohn.com/";
            nft.EventId = "531221bc8a59b5b36af8dcaf6dac317b204b89dfc3ed301d497032c3bcf5799c";
            nft.EventAddress = "NWHozNL3B85PcTXhipmFoBMbfonyrS9WiR";
            nft.EventDate = DateTime.Parse("2022-07-15T04:20:00");
            nft.Location = "Philadelphia,The USA";
            nft.LocationCoordinates = "39.947041,-75.165295";
            nft.Seat = "Section A";
            nft.Price = 10;
            nft.DogePrice = 10;
            nft.TicketClass = ClassOfNFTTicket.VIP;
            nft.VideoLink = "https://youtu.be/ZHwVBirqD2s";
            nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmW91e2zi7ndzgonneee7LNWRASHGPnqAS6FvxgCPThaPv";
            nft.MusicInLink = true;

            // count of the tickets
            int cps = 3;

            Console.WriteLine("Start of minting tickets.");
            int lots = 0;
            int rest = 0;
            rest += cps % NeblioTransactionHelpers.MaximumTokensOutpus;
            lots += (int)((cps - rest) / NeblioTransactionHelpers.MaximumTokensOutpus);
            (bool, string) res = (false, string.Empty);

            if (lots > 1 || (lots == 1 && rest > 0))
            {
                var done = false;
                for (int i = 0; i < lots; i++)
                {
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine($"Minting lot {i} from {lots}:");
                    done = false;
                    while (!done)
                    {
                        res = await account.MintMultiNFT(nft, NeblioTransactionHelpers.MaximumTokensOutpus);
                        done = res.Item1;
                        if (!done)
                        {
                            Console.WriteLine("Waiting for spendable utxo...");
                            await Task.Delay(5000);
                        }
                    }
                    Console.WriteLine("New TxId hash is: ");
                    Console.WriteLine(res.Item2);
                }
                if (rest > 0)
                {
                    Console.WriteLine($"Minting rest {rest} tickets:");
                    done = false;
                    while (!done)
                    {
                        res = await account.MintMultiNFT(nft, rest);
                        done = res.Item1;
                        if (!done)
                        {
                            Console.WriteLine("Waiting for spendable utxo...");
                            await Task.Delay(5000);
                        }
                    }
                    Console.WriteLine("New TxId hash is: ");
                    Console.WriteLine(res.Item2);
                }
            }
            else
            {
                res = await account.MintMultiNFT(nft, cps);
            }

            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio Ticket based on provided hash of existing EventNFT
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTTicketFromEvent(string param)
        {
            MintNFTTicketFromEventAsync(param);
        }
        public static async Task MintNFTTicketFromEventAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input eventId,eventAddress,tickettype,receiver,amount");
            var eventId = split[0];
            var eventAddress = split[1];
            var ticketType = split[2];
            var receiver = split[3];
            var amount = Convert.ToInt32(split[4]);

            Console.WriteLine("Minting NFT");

            var enft = await NFTFactory.GetNFT("", eventId, 0, 0, true, true, NFTTypes.Event);
            if (enft == null)
            {
                Console.WriteLine("Cannot find event NFT. Quit...");
                return;
            }
            // create NFT object
            var nft = new TicketNFT("");
            await nft.FillFromEventNFT(enft);
            nft.TicketClass = (ClassOfNFTTicket)Enum.Parse(typeof(ClassOfNFTTicket), ticketType);

            // count of the tickets
            int cps = amount;
            Console.WriteLine("Start of minting tickets.");
            int lots = 0;
            int rest = 0;
            rest += cps % NeblioTransactionHelpers.MaximumTokensOutpus;
            lots += (int)((cps - rest) / NeblioTransactionHelpers.MaximumTokensOutpus);
            (bool, string) res = (false, string.Empty);

            if (lots > 1 || (lots == 1 && rest > 0))
            {
                var done = false;
                for (int i = 0; i < lots; i++)
                {
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine($"Minting lot {i} from {lots}:");
                    done = false;
                    while (!done)
                    {
                        res = await account.MintMultiNFT(nft, NeblioTransactionHelpers.MaximumTokensOutpus, receiver);
                        done = res.Item1;
                        if (!done)
                        {
                            Console.WriteLine("Waiting for spendable utxo...");
                            await Task.Delay(5000);
                        }
                    }
                    Console.WriteLine("New TxId hash is: ");
                    Console.WriteLine(res.Item2);
                }
                if (rest > 0)
                {
                    Console.WriteLine($"Minting rest {rest} tickets:");
                    done = false;
                    while (!done)
                    {
                        res = await account.MintMultiNFT(nft, rest, receiver);
                        done = res.Item1;
                        if (!done)
                        {
                            Console.WriteLine("Waiting for spendable utxo...");
                            await Task.Delay(5000);
                        }
                    }
                    Console.WriteLine("New TxId hash is: ");
                    Console.WriteLine(res.Item2);
                }
            }
            else
            {
                res = await account.MintMultiNFT(nft, cps, receiver);
            }

            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio NFT Event
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTEvent(string param)
        {
            MintNFTEventAsync(param);
        }
        public static async Task MintNFTEventAsync(string param)
        {
            Console.WriteLine("Minting NFT Event");
            // create NFT object
            var nft = new EventNFT("");

            nft.Author = "Elton John";
            nft.Name = "FAREWELL - The Final Tour";
            nft.Description = "The Final Tour of genius of the music.";
            nft.Tags = "eltonjohn tour genius";
            nft.Link = "https://www.eltonjohn.com/";
            nft.AuthorLink = "https://www.eltonjohn.com/";
            nft.EventId = "";
            nft.EventDate = DateTime.Parse("2022-07-15T04:20:00");
            nft.Location = "Philadelphia,The USA";
            nft.LocationCoordinates = "39.947041,-75.165295";
            nft.Price = 10;
            nft.PriceInDoge = 10;
            nft.EventClass = ClassOfNFTEvent.Concert;
            nft.VideoLink = "https://youtu.be/ZHwVBirqD2s";
            nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmW91e2zi7ndzgonneee7LNWRASHGPnqAS6FvxgCPThaPv";
            nft.MusicInLink = true;

            Console.WriteLine("Start of minting tickets.");

            var res = await account.MintNFT(nft);
            
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Change informations inside of Neblio EventNFT
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void ChangeNFTEvent(string param)
        {
            ChangeNFTEventAsync(param);
        }
        public static async Task ChangeNFTEventAsync(string param)
        {
            Console.WriteLine("Loading NFT Event");

            var NFT = await NFTFactory.GetNFT("", param, 0, 0, true);

            Console.WriteLine("Changing NFT Event");
            // create NFT object
            var nft = NFT as EventNFT;

            nft.Name = "FAREWELL - The Final Tour";
            nft.EventId = "";
            nft.EventDate = DateTime.UtcNow;//DateTime.Parse("2022-07-15T04:20:00");
            nft.LocationCoordinates = "39.947041,-75.165295";

            Console.WriteLine("Start of minting tickets.");

            var res = await account.MintNFT(nft);

            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send the Neblio NFT
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendNFT(string param)
        {
            SendNFTAsync(param);
        }
        public static async Task SendNFTAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input receiveraddress,utxo,index");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[1];
            Console.WriteLine("Input NFT Utxo Index: ");
            var nftutxoindex = Convert.ToInt32(split[2]);
            // load existing NFT object and wait for whole data synchronisation
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, nftutxoindex, 0, true);
            // send NFT to receiver
            if (nft == null)
                throw new Exception("NFT does not exists!");
            Console.WriteLine("Receiver");
            var receiver = split[0];
            var res = await account.SendNFT(receiver, nft, false, 0);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio NFT Producer Profile
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTProducerProfile(string param)
        {
            MintNFTProducerProfileAsync(param);
        }
        public static async Task MintNFTProducerProfileAsync(string param)
        {
            Console.WriteLine("Minting NFT Product");
            // create NFT object
            var nft = new ProfileNFT("");

            nft.Author = "HARDWARIO";
            nft.Name = "HARDWARIO";
            nft.Description = "We design and manufacture unique IoT devices.";
            nft.Link = "https://www.hardwario.com/";
            nft.ID = "04998511";
            nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmX1FJhwjohbWnTCLUqj85Mg5jbUCrhaRDiTTME2bHbe8f";

            Console.WriteLine("Start of minting producer profile.");

            var res = await account.MintNFT(nft);

            
            if (res.Item1)
                if (!string.IsNullOrEmpty(res.Item2))
                    await MintNFTProductAsync(res.Item2 + ":0");
            
            Console.WriteLine("New TxId hash is: ");

            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio NFT Product
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTProduct(string param)
        {
            MintNFTProductAsync(param);
        }
        public static async Task MintNFTProductAsync(string param)
        {
            Console.WriteLine("Minting NFT Product");
            // create NFT object
            var nft = new ProductNFT("");
            
            nft.Author = "HARDWARIO";
            nft.Name = "CHESTER";
            nft.ProducerProfileNFT = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf:0";
            nft.Description = "Extensible IoT gateway for Industry 4.0, smart city, e-metering, and agricultural applications. CHESTER connects sensors, actuators, PLC controllers, and other devices to the internet. Flexible power supply and LPWAN communication technologies enable reliable connectivity from distant and deep indoor places.";
            nft.Tags = "IoT Gateway CHESTER HARDWARIO HW FW Industry40";
            nft.Link = "https://www.hardwario.com/chester/";
            nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmRiZy9ERxCqXQknTD8TPysQjN7mzZLhLnrq3appp1kFwi";
            nft.Datasheet = "https://gateway.ipfs.io/ipfs/QmPs7A7nnTCXXdXwQo6brHAEXLsMwv2tCEUK44HjaiyS9c";
            nft.UnitPrice = 120; // in USD
            
            Console.WriteLine("Start of minting product.");

            var res = await account.MintNFT(nft);

            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio NFT Invoice
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTInvoice(string param)
        {
            MintNFTInvoiceAsync(param);
        }
        public static async Task MintNFTInvoiceAsync(string param)
        {
            Console.WriteLine("Minting NFT Invoice");
            // create NFT object
            var nft = new InvoiceNFT("");

            // load the product
            var pnft = await NFTFactory.GetNFT("", "a09cb9f28d2b7c1e0820cdb819832e86fb2b0b98f0abe979e40bb7262807f15f", 0, 0, true, true, NFTTypes.Product);
            // load the profile
            INFT profile = null;
            if (!string.IsNullOrEmpty(account.Profile.Utxo))
                profile = await NFTFactory.GetNFT("", "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf", 0, 0, true);
            else
                profile = account.Profile;

            var buyerProfile = await NFTFactory.GetNFT("", "1b8a4b68257fffe943fcf821552e5f108a15dcbed4b7cbfe95ad613abb1c22cf", 0, 0, true);

            nft.Author = "HARDWARIO";
            nft.Name = "CHESTER IoT Gateway Invoice";
            nft.Tags = "IoT Gateway CHESTER HARDWARIO HW FW Industry40";
            nft.Link = pnft.Link;
            nft.ImageLink = pnft.ImageLink;
            nft.FileLink = "";

            nft.AddInvoiceItem(pnft.Utxo, pnft.UtxoIndex, (pnft as ProductNFT).UnitPrice, 3);
            nft.AddInvoiceItem(pnft.Utxo, pnft.UtxoIndex, (pnft as ProductNFT).UnitPrice, 5);

            nft.SellerProfileNFT = profile.Utxo + ":" + profile.UtxoIndex;
            nft.BuyerProfileNFT = buyerProfile.Utxo + ":" + buyerProfile.UtxoIndex;
            nft.OrderTxId = "8ad2e3c6d3bd6fcf096fbf682bfbb7a181ba143785b27231a0955620f814ff84";

            Console.WriteLine("Start of minting invoice.");

            var res = await account.MintNFT(nft);

            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Mint Neblio NFT Order
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void MintNFTOrder(string param)
        {
            MintNFTOrderAsync(param);
        }
        public static async Task MintNFTOrderAsync(string param)
        {
            Console.WriteLine("Minting NFT Order");
            // create NFT object
            var nft = new OrderNFT("");

            // load the product
            var pnft = await NFTFactory.GetNFT("", "a09cb9f28d2b7c1e0820cdb819832e86fb2b0b98f0abe979e40bb7262807f15f", 0, 0, true, true, NFTTypes.Product);
            // load the profile
            INFT profile = null;
            if (!string.IsNullOrEmpty(account.Profile.Utxo))
                profile = await NFTFactory.GetNFT("", "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf", 0, 0, true);
            else
                profile = account.Profile;

            var buyerProfile = await NFTFactory.GetNFT("", "1b8a4b68257fffe943fcf821552e5f108a15dcbed4b7cbfe95ad613abb1c22cf", 0, 0, true);

            nft.Author = "HARDWARIO";
            nft.Name = "CHESTER IoT Gateway Order";
            nft.Tags = "IoT Gateway CHESTER HARDWARIO HW FW Industry40";
            nft.Link = pnft.Link;
            nft.ImageLink = pnft.ImageLink;
            nft.FileLink = "";

            nft.AddInvoiceItem(pnft.Utxo, pnft.UtxoIndex, (pnft as ProductNFT).UnitPrice, 3);
            nft.AddInvoiceItem(pnft.Utxo, pnft.UtxoIndex, (pnft as ProductNFT).UnitPrice, 5);

            nft.SellerProfileNFT = profile.Utxo + ":" + profile.UtxoIndex;
            nft.BuyerProfileNFT = buyerProfile.Utxo + ":" + buyerProfile.UtxoIndex;

            Console.WriteLine("Start of minting order.");

            var res = await account.MintNFT(nft);

            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Split Neblio tokens transaction. This transaction can split the token lot to smaller lots
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SplitNeblioTokens(string param)
        {
            SplitNeblioTokensAsync(param);
        }
        public static async Task SplitNeblioTokensAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Please input filename");

            var file = FileHelpers.ReadTextFromFile(param);
            if (string.IsNullOrEmpty(file))
                throw new Exception("File is empty.");

            var dto = JsonConvert.DeserializeObject<SplitNeblioTokensDto>(file);
            if (dto == null)
                throw new Exception("Cannot deserialize file content.");

            var meta = new Dictionary<string, string>();
            meta.Add("Data", "Thank you.");
            var res = await account.SplitTokens(dto.tokenId, meta, dto.receivers, dto.lots, dto.amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Split Neblio transaction. This transaction can split the Neblio coin to lot to smaller lots
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SplitNeblio(string param)
        {
            SplitNeblioAsync(param);
        }
        public static async Task SplitNeblioAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Please input filename");

            var file = FileHelpers.ReadTextFromFile(param);
            if (string.IsNullOrEmpty(file))
                throw new Exception("File is empty.");

            var dto = JsonConvert.DeserializeObject<SplitNeblioDto>(file);
            if (dto == null)
                throw new Exception("Cannot deserialize file content.");

            var res = await account.SplitNeblioCoin(dto.receivers, dto.lots, dto.amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Write the Neblio price into the NFT
        /// This will enable the NFT for the sale
        /// If you want to remove the price, just resend the NFT to yourself
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void WritePriceToNFT(string param)
        {
            WritePriceToNFTAsync(param);
        }
        public static async Task WritePriceToNFTAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input utxo,index,price");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[0];
            Console.WriteLine("Input NFT Utxo Index: ");
            var nftutxoindex = Convert.ToInt32(split[1]);
            // load existing NFT object and wait for whole data synchronisation
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, nftutxoindex, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            // send NFT to receiver
            var price = Convert.ToDouble(split[2], CultureInfo.InvariantCulture);
            var res = await account.SendNFT(account.Address, nft, true, price);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send NFT payments for some specific NFT which is available to buy.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendNFTPayment(string param)
        {
            SendNFTPaymentAsync(param);
        }
        public static async Task SendNFTPaymentAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input utxo, utxoindex, receiver");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[0];
            Console.WriteLine("Input NFT Utxo Index: ");
            var nftutxoindex = Convert.ToInt32(split[1]);
            // load existing NFT object and wait for whole data synchronisation. NFT must have written price!
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, nftutxoindex, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            if (!nft.PriceActive)
                throw new Exception("NFT does not have setted price.");
            // send NFT to receiver
            Console.WriteLine("Receiver");
            var receiver = split[2];
            var res = await account.SendNFTPayment(receiver, nft);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Return unwanted or bad NFT Payment
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void ReturnNFTPayment(string param)
        {
            ReturnNFTPaymentAsync(param);
        }
        public static async Task ReturnNFTPaymentAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input utxo, utxoindex");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[0];
            Console.WriteLine("Input NFT Utxo Index: ");
            var nftutxoindex = Convert.ToInt32(split[1]);
            // load existing NFT object and wait for whole data synchronisation. NFT must have written price!
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, nftutxoindex, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            // send NFT to receiver
            Console.WriteLine("Receiver: " + (nft as PaymentNFT).Sender);
            var res = await account.ReturnNFTPayment((nft as PaymentNFT).Sender, nft as PaymentNFT);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Obtain and test the Verification code of the NFT
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void VerificationOfNFTs(string param)
        {
            VerificationOfNFTsAsync(param);
        }
        public static async Task VerificationOfNFTsAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Input Utxo must be filled.");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = param;
            var res = await account.GetNFTVerifyCode(nftutxo);
            Console.WriteLine("Signature for TxId:");
            Console.WriteLine(res.TxId);
            Console.WriteLine("Signature:");
            Console.WriteLine(res.Signature);
            Console.WriteLine("Verifying now...");
            // res is already OwnershipVerificationCodeDto so creating new object is just example
            var vres = await OwnershipVerifier.VerifyOwner(new OwnershipVerificationCodeDto()
            {
                TxId = nftutxo,
                Signature = res.Signature
            });

            Console.WriteLine("Result of verification is: ");
            Console.WriteLine(vres.VerifyResult);
            Console.WriteLine("Owner of NFT is: ");
            Console.WriteLine(vres.Owner);
            Console.WriteLine("Loaded NFT is: ");
            Console.WriteLine(vres.NFT.Name);
        }

        /// <summary>
        /// Sign the message with use of the Neblio Private Key
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SignMessage(string param)
        {
            SignMessageAsync(param);
        }
        public static async Task SignMessageAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Message must be filled.");

            Console.WriteLine("Mesaage for signature: ");
            Console.WriteLine(param);
            var signature = await account.SignMessage(param);
            Console.WriteLine("Signature of the message is: ");
            Console.WriteLine(signature.Item2);
        }

        /// <summary>
        /// Verify the message with use of the Neblio address
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void VerifyMessage(string param)
        {
            VerifyMessageAsync(param);
        }
        public static async Task VerifyMessageAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input message,signature,address. Dont use any other separator in this case.");

            Console.WriteLine("Mesaage for signature: ");
            Console.WriteLine(split[0]);
            Console.WriteLine("Signature: ");
            Console.WriteLine(split[1]);
            Console.WriteLine("Address: ");
            Console.WriteLine(split[2]);
            var ver = await ECDSAProvider.VerifyMessage(split[0], split[1], split[2]);
            Console.WriteLine("Signature verification result is: ");
            Console.WriteLine(ver.Item2);
        }

        /// <summary>
        /// Obtain the transaction details from the Neblio API
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void GetTxDetails(string param)
        {
            GetTxDetailsAsync(param);
        }
        public static async Task GetTxDetailsAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("TxId must be filled.");

            Console.WriteLine("Input Tx Id Hash");
            var txinfo = await NeblioTransactionHelpers.GetTransactionInfo(param); //get old tx info from neblio api

            // sign it with loaded account
            Console.WriteLine("Timestamp");
            Console.WriteLine(TimeHelpers.UnixTimestampToDateTime((double)txinfo.Blocktime));
            Console.WriteLine("Number of confirmations: ");
            Console.WriteLine(txinfo.Confirmations);
            Console.WriteLine("--------------");
            Console.WriteLine("Vins:");
            txinfo.Vin.ToList().ForEach(v =>
            {
                Console.WriteLine($"Vin of value: {v.ValueSat} Nebl sat. from txid: {v.Txid} and vout index {v.Vout}");
            });
            Console.WriteLine("-------------");
            Console.WriteLine("--------------");
            Console.WriteLine("Vouts:");
            txinfo.Vout.ToList().ForEach(v =>
            {
                Console.WriteLine($"Vout index {v.N} of value: {v.Value} Nebl sat. to receiver scrupt pub key {v.ScriptPubKey?.Addresses?.FirstOrDefault()}");
            });
            Console.WriteLine("-------------");
        }

        /// <summary>
        /// Load the specific NFT details
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void GetNFTDetails(string param)
        {
            GetNFTDetailsAsync(param);
        }
        public static async Task GetNFTDetailsAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input utxo,index");

            Console.WriteLine("Input NFT Tx Id Hash");
            var txid = split[0];
            Console.WriteLine("Input NFT Utxo Index: ");
            var nftutxoindex = Convert.ToInt32(split[1]);
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, txid, nftutxoindex, 0, true);
            if (nft.Type == NFTTypes.Message && !account.IsLocked())
            {
                await (nft as MessageNFT).Decrypt(account.Secret);
            }
            // sign it with loaded account
            Console.WriteLine("Name: ");
            Console.WriteLine(nft.Name);
            if(System.Text.RegularExpressions.Regex.Match(nft.Name, RegexMatchPaterns.EmojiPattern).Success)
                Console.WriteLine("there are emojis in the name");
            Console.WriteLine("Author: ");
            Console.WriteLine(nft.Author);
            Console.WriteLine("Description: ");
            Console.WriteLine(nft.Description);
            Console.WriteLine("Link: ");
            Console.WriteLine(nft.Link);
            Console.WriteLine("Image Link: ");
            Console.WriteLine(nft.ImageLink);
        }

        /// <summary>
        /// Encrypt the message with use of the Neblio Address
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void EncryptMessage(string param)
        {
            EncryptMessageAsync(param);
        }
        public static async Task EncryptMessageAsync(string param)
        {
            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");
            if (account.IsLocked())
                throw new Exception("Account is locked.");
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input message,address.");

            var pk = await NFTHelpers.GetPubKeyFromLastFoundTx(split[1]);
            if (!pk.Item1)
                throw new Exception("Cannot load public key of this address. Probably does not have any spended transaction.");
            var res = await ECDSAProvider.EncryptMessage(split[0], pk.Item2.ToHex());
            Console.WriteLine("Encrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Encrypt the file content with use of Neblio Address
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void EncryptTextFileContent(string param)
        {
            EncryptTextFileContentAsync(param);
        }
        public static async Task EncryptTextFileContentAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input address,filename.");

            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");
            if (account.IsLocked())
                throw new Exception("Account is locked.");
            if (string.IsNullOrEmpty(split[0]))
                throw new Exception("Please input address.");
            if (!FileHelpers.IsFileExists(split[1]))
                throw new Exception("File does not exists.");

            var filecontent = FileHelpers.ReadTextFromFile(split[1]);
            if (string.IsNullOrEmpty(filecontent))
                throw new Exception("File is empty.");

            var pk = await NFTHelpers.GetPubKeyFromLastFoundTx(split[0]);
            if (!pk.Item1)
                throw new Exception("Cannot load public key of this address. Probably does not have any spended transaction.");
            var res = await ECDSAProvider.EncryptMessage(filecontent, pk.Item2.ToHex());
            if (res.Item1)
                FileHelpers.WriteTextToFile("encrypted-" + split[1], res.Item2);
            Console.WriteLine("Encrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Decrypt the message with use of the Neblio Private Key
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void DecryptMessage(string param)
        {
            DecryptMessageAsync(param);
        }
        public static async Task DecryptMessageAsync(string param)
        {
            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");
            if (account.IsLocked())
                throw new Exception("Account is locked.");

            var res = await ECDSAProvider.DecryptMessage(param, account.AccountKey.GetEncryptedKey());
            Console.WriteLine("Decrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Encrypt the message with use of the shared secret. You must provide address of the receiver
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void EncryptMessageWithSharedSecret(string param)
        {
            EncryptMessageWithSharedSecretAsync(param);
        }
        public static async Task EncryptMessageWithSharedSecretAsync(string param)
        {
            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");
            if (account.IsLocked())
                throw new Exception("Account is locked.");
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input message,receiverAddress.");

            var res = await ECDSAProvider.EncryptStringWithSharedSecret(split[0], split[1], account.Secret);
            Console.WriteLine("Encrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Decrypt the message with use of the shared secret.
        /// you must provide the receiver Neblio address.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void DecryptMessageWithSharedSecret(string param)
        {
            DecryptMessageWithSharedSecretAsync(param);
        }
        public static async Task DecryptMessageWithSharedSecretAsync(string param)
        {
            if (string.IsNullOrEmpty(account.Address))
                throw new Exception("Account is not initialized.");
            if (account.IsLocked())
                throw new Exception("Account is locked.");
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input message,receiverAddress.");

            var res = await ECDSAProvider.DecryptStringWithSharedSecret(split[0], split[1], account.Secret);
            Console.WriteLine("Decrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Classic symmetrical encryption based on the AES
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void AESEncryptMessage(string param)
        {
            AESEncryptMessageAsync(param);
        }
        public static async Task AESEncryptMessageAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input message,password.");

            var res = SymetricProvider.EncryptString(split[1], split[0]);
            Console.WriteLine("Encrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Classic symmetrical decryption based on the AES
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void AESDecryptMessage(string param)
        {
            AESDecryptMessageAsync(param);
        }
        public static void AESDecryptMessageAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input message,password.");

            var res = SymetricProvider.DecryptString(split[1], split[0]);
            Console.WriteLine("Decrypted message is: ");
            Console.WriteLine(res);
        }

        /// <summary>
        /// Send the Message NFT. It can contains the encrypted metadata with use of the shared secret.
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void SendMessageNFT(string param)
        {
            SendMessageNFTAsync(param);
        }
        public static async Task SendMessageNFTAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input name,message,receiver.");

            if (string.IsNullOrEmpty(split[1]))
                throw new Exception("Message cannot be empty!");

            if (string.IsNullOrEmpty(split[2]))
                throw new Exception("Receiver cannot be empty!");

            var res = await account.SendMessageNFT(split[0], split[1], split[2]);
            if (res.Item1)
                Console.WriteLine("NFT TxId is: ");
            Console.WriteLine(res.Item2);
        }

        /// <summary>
        /// Load the MessageNFT and decrypt the included data with use of the shared secret
        /// </summary>
        /// <param name="param"></param>
        [TestEntry]
        public static void LoadMessageNFT(string param)
        {
            LoadMessageNFTAsync(param);
        }
        public static async Task LoadMessageNFTAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input txid, txidindex");

            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, split[0], Convert.ToInt32(split[1]), 0, true);
            await (nft as MessageNFT).Decrypt(account.Secret);
            Console.WriteLine($"NFT TxId is: {nft.Utxo}");
            Console.WriteLine($"NFT Name is: {nft.Name}");
            Console.WriteLine($"NFT Description is: {nft.Description}");
            Console.WriteLine();
        }


        #region DogeTests

        ////////////////////////////////////////////////////////////
        ///// Doge Tests

        [TestEntry]
        public static void DogeGenerateNewAccount(string param)
        {
            GenerateNewDogeAccountAsync(param);
        }
        public static async Task GenerateNewDogeAccountAsync(string param)
        {
            password = param;
            await dogeAccount.CreateNewAccount(password, true);
            Console.WriteLine($"Account created.");
            Console.WriteLine($"Address: {dogeAccount.Address}");
            Console.WriteLine($"Encrypted Private Key: {dogeAccount.AccountKey.GetEncryptedKey("", true)}");
            Console.WriteLine($"Decrypted Private Key: {dogeAccount.AccountKey.GetEncryptedKey("", false)}");
            //StartRefreshingData(null);
        }

        [TestEntry]
        public static void DogeLoadAccount(string param)
        {
            LoadDogeAccountAsync(param);
        }
        public static async Task LoadDogeAccountAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Password cannot be empty.");

            password = param;
            await dogeAccount.LoadAccount(password); // read from default file dogekey.txt
            //DogeStartRefreshingData(null);
        }

        [TestEntry]
        public static void DogeLoadAccountFromFile(string param)
        {
            LoadDogeAccountFromFileAsync(param);
        }
        public static async Task LoadDogeAccountFromFileAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input pass,filename");
            var pass = split[0];
            var file = split[1];
            await dogeAccount.LoadAccount(pass,file);
        }

        [TestEntry]
        public static void DogeLoadAccountForObserve(string param)
        {
            DogeLoadAccountForObserveAsync(param);
        }
        public static async Task DogeLoadAccountForObserveAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Address cannot be empty.");

            await dogeAccount.LoadAccountWithDummyKey(param);
            DogeDisplayAccountDetails(null);
        }

        [TestEntry]
        public static void DogeStartRefreshingData(string param)
        {
            StartRefreshingDogeDataAsync(param);
        }
        public static async Task StartRefreshingDogeDataAsync(string param)
        {
            if (string.IsNullOrEmpty(dogeAccount.Address))
                throw new Exception("Account is not initialized.");

            await dogeAccount.StartRefreshingData();
            Console.WriteLine("Refreshing started.");
        }

        [TestEntry]
        public static void DogeLoadAccountWithCreds(string param)
        {
            LoadDogeAccountWithCredsAsync(param);
        }
        public static async Task LoadDogeAccountWithCredsAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input pass,encryptedprivatekey,address");
            var pass = split[0];
            var ekey = split[1];
            var addr = split[2];
            await dogeAccount.LoadAccount(pass, ekey, addr);
            DogeStartRefreshingData(null);
        }

        [TestEntry]
        public static void DogeGetDecryptedPrivateKey(string param)
        {
            GetDogeDecryptedPrivateKeyAsync(param);
        }
        public static async Task GetDogeDecryptedPrivateKeyAsync(string param)
        {
            if (string.IsNullOrEmpty(dogeAccount.Address))
                throw new Exception("Account is not initialized.");
            if (dogeAccount.IsLocked())
                throw new Exception("Account is locked.");
            var res = dogeAccount.AccountKey.GetEncryptedKey();
            Console.WriteLine($"Private key for address {dogeAccount.Address} is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void DogeDisplayAccountDetails(string param)
        {
            Console.WriteLine("Account Details");
            Console.WriteLine($"Account Total Balance: {dogeAccount.TotalBalance} DOGE");
            Console.WriteLine($"Account Total Spendable Balance: {dogeAccount.TotalSpendableBalance} DOGE");
            Console.WriteLine($"Account Total Unconfirmed Balance: {dogeAccount.TotalUnconfirmedBalance} DOGE");
            Console.WriteLine($"Account Total Utxos Count: {dogeAccount.Utxos.Count}");
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine("Account Utxos:");
            dogeAccount.Utxos.ForEach(u => { Console.WriteLine($"Utxo: {u.TxId}:{u.N}"); });
            Console.WriteLine("-----------------------------------------------------");
        }

        [TestEntry]
        public static void DogeGetTxDetails(string param)
        {
            DogeGetTxDetailsAsync(param);
        }
        public static async Task DogeGetTxDetailsAsync(string param)
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("Request Tx Info");
            var txid = param;
            var txinfo = await DogeTransactionHelpers.TransactionInfoAsync(txid);
            var msg = DogeTransactionHelpers.ParseDogeMessage(txinfo);
            Console.WriteLine("TxInfo:");
            Console.WriteLine(JsonConvert.SerializeObject(txinfo, Formatting.Indented));
            Console.WriteLine("-------------------------------------------------------");
            if (msg.Item1)
                Console.WriteLine("This Transaction contains message: " + msg.Item2);
        }

        [TestEntry]
        public static void DogeSendTransaction(string param)
        {
            SendDogeTransactionAsync(param);
        }
        public static async Task SendDogeTransactionAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input receiveraddress,amountofdoge");
            var receiver = split[0];
            var am = split[1];
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);
            var res = await dogeAccount.SendPayment(receiver, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void DogeSendTransactionWithMessage(string param)
        {
            DogeSendTransactionWithMessageAsync(param);
        }
        public static async Task DogeSendTransactionWithMessageAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 4)
                throw new Exception("Please input receiveraddress,amountofdoge,fee,message");
            var receiver = split[0];
            var am = split[1];            
            var message = split[3];
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);            
            var res = await dogeAccount.SendPayment(receiver, amount, message);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void DogeSendTransactionWithIPFSUpload(string param)
        {
            DogeSendTransactionWithIPFSUploadAsync(param);
        }
        public static async Task DogeSendTransactionWithIPFSUploadAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input receiveraddress,amountofdoge,filename");
            var receiver = split[0];
            var am = split[1];
            var amount = Convert.ToDouble(am, CultureInfo.InvariantCulture);
            var fileName = split[2];
            var filebytes = File.ReadAllBytes(fileName);
            var link = string.Empty;
            try
            {
                using (Stream stream = new MemoryStream(filebytes))
                {
                    var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, fileName);
                    link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }

            var res = await dogeAccount.SendPayment(receiver, amount, link);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void DogeBuyNFT(string param)
        {
            DogeBuyNFTAsync(param);
        }
        public static async Task DogeBuyNFTAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input receiveraddress,neblioaddress,nfttxid");
            var receiver = split[0];
            var neblioaddress = split[1];
            var nftid = split[2];
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftid);

            var res = await dogeAccount.BuyNFT(neblioaddress, receiver, nft);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void DogeSignMessage(string param)
        {
            DogeSignMessageAsync(param);
        }
        public static async Task DogeSignMessageAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("Message must be filled.");

            Console.WriteLine("Mesaage for signature: ");
            Console.WriteLine(param);
            var signature = await dogeAccount.SignMessage(param);
            Console.WriteLine("Signature of the message is: ");
            Console.WriteLine(signature.Item2);
        }

        [TestEntry]
        public static void DogeVerifyMessage(string param)
        {
            DogeVerifyMessageAsync(param);
        }
        public static async Task DogeVerifyMessageAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input message,signature,address. Dont use any other separator in this case.");

            Console.WriteLine("Mesaage for signature: ");
            Console.WriteLine(split[0]);
            Console.WriteLine("Signature: ");
            Console.WriteLine(split[1]);
            Console.WriteLine("Address: ");
            Console.WriteLine(split[2]);
            var ver = await ECDSAProvider.VerifyDogeMessage(split[0], split[1], split[2]);
            Console.WriteLine("Signature verification result is: ");
            Console.WriteLine(ver.Item2);
        }

        #endregion
        ///////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////

        #region Coruzant

        [TestEntry]
        public static void CoruzantCreateEmptyProfileNFTFile(string param)
        {
            CoruzantCreateEmptyProfileNFTFileAsync(param);
        }
        public static async Task CoruzantCreateEmptyProfileNFTFileAsync(string param)
        {

            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for CoruzantProfile NFT.");

            // create NFT object
            var nft = new CoruzantProfileNFT("");
            // Example data
            nft.Name = "Tomas";
            nft.Surname = "Svoboda";
            nft.Nickname = "fyziktom";
            nft.Age = 30;
            nft.Description = "Tomas Svoboda is the Founder and CTO at TechnicInsider and is a technology enthusiast. He has been working with technology since he was a kid. He studied medical electronic devices in high school, and at his university.";
            nft.Author = "Brian E. Thomas";
            nft.ImageLink = "https://coruzant.com/wp-content/uploads/2021/03/svoboda-tomas.jpg";
            nft.IconLink = "https://ntp1-icons.ams3.digitaloceanspaces.com/6e05f020d88f8490190a9d9a625f37b649b7dae0.png";
            nft.Link = "https://fyziktom.com/";
            nft.PodcastLink = "https://gateway.ipfs.io/ipfs/QmTWPM5cCE1wbR5Cn9yr1ZwkzYdaZ9xNSovaA6dirDYp9S";
            nft.PersonalPageLink = "https://coruzant.com/profiles/tomas-svoboda/";
            nft.Linkedin = "fyziktom";
            nft.Twitter = "fyziktom";
            nft.CompanyLink = "https://technicinsider.com/";
            nft.CompanyName = "Technicinsider";
            nft.WorkingPosition = "CEO";
            nft.Tags = "fyziktom technologies industry40 blockchain neblio";
            nft.TokenId = CoruzantNFTHelpers.CoruzantTokenId;

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));

            Console.WriteLine("File created.");
        }

        [TestEntry]
        public static void CoruzantMintProfileFormFileNFT(string param)
        {
            CoruzantMintProfileFormFileNFTAsync(param);
        }
        public static async Task CoruzantMintProfileFormFileNFTAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.CoruzantProfile)
                {
                    var nft = JsonConvert.DeserializeObject<CoruzantProfileNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                    }
                }
                else
                {
                    Console.WriteLine("Input file is not template for Coruzant Profile NFT.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }

        #endregion
        //////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////
        #region Devices

        private static string IoTProtocolNFTUtxo = string.Empty;
        private static string IoTHWSrcNFTUtxo = string.Empty;
        private static string IoTFWSrcNFTUtxo = string.Empty;
        private static string IoTSWSrcNFTUtxo = string.Empty;
        private static string IoTMechSrcNFTUtxo = string.Empty;
        private static string IoTDeviceNFTUtxo = string.Empty;
        private static string IoTIoTDeviceNFTUtxo = string.Empty;

        [TestEntry]
        public static void IoTCreateEmptyNFTProtocolFile(string param)
        {
            IoTCreateEmptyNFTProtocolFileAsync(param);
        }
        public static async Task IoTCreateEmptyNFTProtocolFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for Protocol NFT.");
            // create NFT object
            var nft = new ProtocolNFT("");
            // Example data
            nft.Name = "HARDWARIO-CHESTER";
            nft.Version = "CHESTER-1-0-0";
            nft.Description = "";
            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf";
            nft.Tags = "hardwario technologies industry40 iot gateway protocol";

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));
            Console.WriteLine("File created.");
        }
        
        [TestEntry]
        public static void IoTCreateNFTProtocolFormFile(string param)
        {
            IoTCreateNFTProtocolFormFileAsync(param);
        }
        public static async Task IoTCreateNFTProtocolFormFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.Protocol)
                {
                    var nft = JsonConvert.DeserializeObject<ProtocolNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                        if (res.Item1)
                            IoTProtocolNFTUtxo = res.Item2;
                    }
                }
                else
                    Console.WriteLine("Input file is not template for Protocol NFT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }

        [TestEntry]
        public static void IoTCreateEmptyHWSrcNFTFile(string param)
        {
            IoTCreateEmptyHWSrcNFTFileAsync(param);
        }
        public static async Task IoTCreateEmptyHWSrcNFTFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for HWSrc NFT.");
            // create NFT object
            var nft = new HWSrcNFT("");
            // Example data
            nft.Name = "HARDWARIO-CHESTER";
            nft.Version = "CHESTER-1-0-0";
            nft.Description = "";
            nft.Tool = "Eagle";//just example
            nft.RepositoryType = "GitHub";
            nft.RepositoryLink = "https://github.com/hardwario/bc-hardware";//just example
            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf";
            nft.Tags = "hardwario technologies industry40 iot gateway hardware";

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));
            Console.WriteLine("File created.");
        }

        [TestEntry]
        public static void IoTCreateHWSrcNFTFormFile(string param)
        {
            IoTCreateHWSrcNFTFormFileAsync(param);
        }
        public static async Task IoTCreateHWSrcNFTFormFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.HWSrc)
                {
                    var nft = JsonConvert.DeserializeObject<HWSrcNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                        if (res.Item1)
                            IoTHWSrcNFTUtxo = res.Item2;
                    }
                }
                else
                    Console.WriteLine("Input file is not template for HWSrc NFT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }

        [TestEntry]
        public static void IoTCreateEmptyFWSrcNFTFile(string param)
        {
            IoTCreateEmptyFWSrcNFTFileAsync(param);
        }
        public static async Task IoTCreateEmptyFWSrcNFTFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for FWSrc NFT.");
            // create NFT object
            var nft = new FWSrcNFT("");
            // Example data
            nft.Name = "HARDWARIO-CHESTER";
            nft.Version = "CHESTER-1-0-0";
            nft.Description = "";
            nft.Tool = "gcc";
            nft.RepositoryType = "GitHub";
            nft.RepositoryLink = "https://github.com/hardwario/twr-sdk";//just example
            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf";
            nft.Tags = "hardwario technologies industry40 iot gateway firmware";

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));
            Console.WriteLine("File created.");
        }

        [TestEntry]
        public static void IoTCreateFWSrcNFTFormFile(string param)
        {
            IoTCreateFWSrcNFTFormFileAsync(param);
        }
        public static async Task IoTCreateFWSrcNFTFormFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.FWSrc)
                {
                    var nft = JsonConvert.DeserializeObject<FWSrcNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                        if (res.Item1)
                            IoTFWSrcNFTUtxo = res.Item2;
                    }
                }
                else
                    Console.WriteLine("Input file is not template for FWSrc NFT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }

        [TestEntry]
        public static void IoTCreateEmptySWSrcNFTFile(string param)
        {
            IoTCreateEmptySWSrcNFTFileAsync(param);
        }
        public static async Task IoTCreateEmptySWSrcNFTFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for SWSrc NFT.");
            // create NFT object
            var nft = new HWSrcNFT("");
            // Example data
            nft.Name = "HARDWARIO-CHESTER";
            nft.Version = "CHESTER-1-0-0";
            nft.Description = "";
            nft.Tool = "npm"; //just example
            nft.RepositoryType = "GitHub";
            nft.RepositoryLink = "https://github.com/hardwario/bch-playground";//just example
            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf";
            nft.Tags = "hardwario technologies industry40 iot gateway software";

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));
            Console.WriteLine("File created.");
        }

        [TestEntry]
        public static void IoTCreateSWSrcNFTFormFile(string param)
        {
            IoTCreateSWSrcNFTFormFileAsync(param);
        }
        public static async Task IoTCreateSWSrcNFTFormFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.SWSrc)
                {
                    var nft = JsonConvert.DeserializeObject<SWSrcNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                        if (res.Item1)
                            IoTSWSrcNFTUtxo = res.Item2;
                    }
                }
                else
                    Console.WriteLine("Input file is not template for SWSrc NFT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }

        [TestEntry]
        public static void IoTCreateEmptyMechSrcNFTFile(string param)
        {
            IoTCreateEmptyMechSrcNFTFileAsync(param);
        }
        public static async Task IoTCreateEmptyMechSrcNFTFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for MechSrc NFT.");
            // create NFT object
            var nft = new MechSrcNFT("");
            // Example data
            nft.Name = "HARDWARIO-CHESTER";
            nft.Version = "CHESTER-1-0-0";
            nft.Description = "";
            nft.Tool = "SolidWorks";//just example
            nft.RepositoryType = "GitHub";
            nft.RepositoryLink = "https://github.com/hardwario/bc-hardware";//just example
            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf";
            nft.Tags = "hardwario technologies industry40 iot gateway mechanical";

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));
            Console.WriteLine("File created.");
        }

        [TestEntry]
        public static void IoTCreateMechSrcNFTFormFile(string param)
        {
            IoTCreateMechSrcNFTFormFileAsync(param);
        }
        public static async Task IoTCreateMechSrcNFTFormFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.MechSrc)
                {
                    var nft = JsonConvert.DeserializeObject<MechSrcNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                        if (res.Item1)
                            IoTMechSrcNFTUtxo = res.Item2;
                    }
                }
                else
                    Console.WriteLine("Input file is not template for MechSrc NFT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }

        [TestEntry]
        public static void IoTCreateEmptyDeviceNFTFile(string param)
        {
            IoTCreateEmptyDeviceNFTFileAsync(param);
        }
        public static async Task IoTCreateEmptyDeviceNFTFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. It will be saved as json file.");
                return;
            }
            Console.WriteLine("Creating File with template for Device NFT.");
            // create NFT object
            var nft = new DeviceNFT("");
            // Example data
            nft.Name = "HARDWARIO-CHESTER";
            nft.Version = "CHESTER-1-0-0";
            nft.Description = "";
            nft.ProtocolNFTHash = IoTProtocolNFTUtxo;
            nft.HWSrcNFTHash = IoTHWSrcNFTUtxo;
            nft.FWSrcNFTHash = IoTFWSrcNFTUtxo;
            nft.SWSrcNFTHash = IoTSWSrcNFTUtxo;
            nft.MechSrcNFTHash = IoTMechSrcNFTUtxo;
            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf";
            nft.Link = "https://gateway.ipfs.io/ipfs/QmPs7A7nnTCXXdXwQo6brHAEXLsMwv2tCEUK44HjaiyS9c"; // link to the datasheet
            nft.Tags = "hardwario technologies industry40 iot gateway mechanical";

            FileHelpers.WriteTextToFile(param + ".json", JsonConvert.SerializeObject(nft, Formatting.Indented));
            Console.WriteLine("File created.");
        }

        [TestEntry]
        public static void IoTCreateDeviceNFTFormFile(string param)
        {
            IoTCreateDeviceNFTFormFileAsync(param);
        }
        public static async Task IoTCreateDeviceNFTFormFileAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                Console.WriteLine("Please fill file name without type. Template must be json format with extension .json!");
                return;
            }

            var filecontent = FileHelpers.ReadTextFromFile(param + ".json");
            if (string.IsNullOrEmpty(filecontent))
            {
                Console.WriteLine("File is empty!");
                return;
            }

            Console.WriteLine("Minting NFT");
            // create NFT object
            try
            {
                var inft = JsonConvert.DeserializeObject<PostNFT>(filecontent);
                if (inft.Type == NFTTypes.Device)
                {
                    var nft = JsonConvert.DeserializeObject<DeviceNFT>(filecontent);
                    if (nft != null)
                    {
                        var res = await account.MintNFT(nft);

                        Console.WriteLine("New TxId hash is: ");
                        Console.WriteLine(res.Item2);
                        if (res.Item1)
                            IoTDeviceNFTUtxo = res.Item2;
                    }
                }
                else
                    Console.WriteLine("Input file is not template for Device NFT.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize template. Please chcek if it is correct." + ex.Message);
            }
        }


        [TestEntry]
        public static void IoTCreateIoTDeviceNFT(string param)
        {
            IoTCreateIoTDeviceNFTAsync(param);
        }
        public static async Task IoTCreateIoTDeviceNFTAsync(string param)
        {
            Console.WriteLine("Creating IoT Device NFT...");
            if (param == null) param = string.Empty;

            if (param.Contains("dummyaccount"))
            {
                //create dummy account - just because of the testing the encryption of the messages
                account = new NeblioAccount();
                await account.CreateNewAccount("");
            }
            else
            {
                if (account == null || string.IsNullOrEmpty(account.Address))
                {
                    Console.WriteLine("Please load the account or use as parameter of this function \" dummyaccount \".");
                    return;
                }
            }

            var nft = new IoTDeviceNFT("");

            nft.Author = "f6f58d275e58970b8879edd164ac605f1413ae8c62e0e2beaca9cb6a6b860aaf"; //hardwario profile utxo

            nft.Address = account.Address;
            nft.AutoActivation = true;
            nft.IDDSettings.IoTComType = VEDriversLite.Devices.IoTCommunicationType.API;
            nft.IDDSettings.ComSchemeType = VEDriversLite.Devices.CommunicationSchemeType.Requests;
            nft.IoTDDType = VEDriversLite.Devices.IoTDataDriverType.HARDWARIO;

            if (param.Contains("chesterdevice") || (!param.Contains("chesterdevice") && !param.Contains("minteddevice")))
            {
                nft.DeviceNFTHash = "f78605d6adb3979019213f3d002bff4aa45ae77802fc8e67eb900af7962c8184";
            }
            else if (param.Contains("minteddevice"))
            {
                nft.DeviceNFTHash = IoTDeviceNFTUtxo;
            }
            if (!string.IsNullOrEmpty(nft.DeviceNFTHash))
            {
                try
                {
                    await nft.LoadDeviceNFT();
                    await (nft.SourceDeviceNFT as DeviceNFT).LoadSourceNFTs();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot load the DeviceNFT or Source of the DeviceNFT. " + ex.Message);
                }
            }
            else
            {
                if (account == null || string.IsNullOrEmpty(account.Address))
                {
                    Console.WriteLine("Please load the create DeviceNFT first and use \" minteddevice \" as parameter or use predefined \" chesterdevice \" as parameter.");
                    return;
                }
            }

            nft.Name = nft.SourceDeviceNFT.Name;
            nft.Description = nft.SourceDeviceNFT.Description;
            nft.ImageLink = nft.SourceDeviceNFT.ImageLink;
            nft.Link = nft.SourceDeviceNFT.Link;
            //nft.Utxo = "f6f58d275e5aa00b8879edc164ac605ff413fe8462e042beaca9cb6a6b860aaf"; //now just random for testing
            nft.EncryptMessages = false; //encrypt all messages
            nft.EncryptSetting = true;//encrypt connection settings in metadata when minting NFT
            nft.AllowConsoleOutputWhenNewMessage = true; //allow console output when the new message will arrive from IoTDataDriver to IoTDeviceNFT
            //nft.RunJustOwn = true; this will check if the NFT was minted or resend on same address. otherwise it will not allow to init it!
            nft.IDDSettings.ConnectionParams.Url = "https://api.hardwario.cloud/v1";
            nft.IDDSettings.ConnectionParams.GroupId = "61c895b629d3ef00117bdaec";
            nft.IDDSettings.ConnectionParams.DeviceId = "5f7722790305b500185b2847";
            nft.IDDSettings.ConnectionParams.Secured = true;
            nft.IDDSettings.ConnectionParams.SType = VEDriversLite.Devices.Dto.CommunitacionSecurityType.Bearer;
            nft.IDDSettings.ConnectionParams.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ0eXBlIjoidXNlciIsImlkIjoiNjFjODk2MzQyOWQzZWYwMDExN2JkYmMxIiwiaWF0IjoxNjQwNTM1NjA0fQ.7pnqPVsMcQZQvXlvtDIUgfdOqBq30qZLtKrkN5cVFeA";
            nft.IDDSettings.ConnectionParams.CommonRefreshInterval = 10000;
            
            nft.LocationCoordinates = "37.7880168,-122.3959002"; // obtain actual from hardwario chester messages? setting flag for this option?

            //var resp = await account.MintNFT(nft);

            var mtd = await nft.GetMetadata(account.Address, account.Secret.ToString(), account.Address); // just for the test of the encryption of the settings

            nft.NewMessage += Nft_NewMessage;

            if (nft.EncryptSetting)
                await nft.InitCommunication(account.Secret);
            else
                await nft.InitCommunication();

            
            int attemtps = 10000;
            while (attemtps > 0)
            {
                Console.WriteLine("Waiting 60s for reading values...");
                await Task.Delay(60000);
                attemtps--;
                Console.WriteLine($"Only {attemtps} from 10000 cycles left.");
            }
            
            /*
            while (true)
            {
                Console.WriteLine("Waiting 10 minutes for new values:");
                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(60000);
                    Console.Write($"# {i} m,");
                }
            }
            //await Task.Delay(-1); //wait forewer
            */

            
            Console.WriteLine("Reading complete!");
            Console.WriteLine("Readed messages from IoT Device NFT:");
            await nft.DeInitCommunication();
            //var msgs = new Dictionary<string, INFT>(nft.MessageNFTs);
            var metaforTest = new Dictionary<string, string>();
            var testname = string.Empty;
            var testdescription = string.Empty;
            Console.WriteLine($"Total Number of loaded messages: {nft.MessageNFTs.Count}");
            if (param.Contains("writedownallmessages"))
            {
                foreach (var msg in nft.MessageNFTs)
                {
                    Console.WriteLine($"Message: {msg.Key} is: {msg.Value.Description}");
                    try
                    {
                        var meta = await msg.Value.GetMetadata(account.Address, account.Secret.ToString(), account.Address);
                        if (metaforTest.Count == 0)
                        {
                            metaforTest = new Dictionary<string, string>(meta);
                            testname = msg.Value.Name;
                            testdescription = msg.Value.Description;
                        }
                        Console.WriteLine($"\tMetadata of the NFT Message are:");
                        foreach (var m in meta)
                            Console.WriteLine($"\t\"{m.Key}\": {m.Value}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cannot get the NFT Message Metadata: {ex.Message}");
                    }
                }

                try
                {
                    var loadedNFTMessage = new MessageNFT("");
                    await loadedNFTMessage.LoadLastData(metaforTest);
                    loadedNFTMessage.Partner = account.Address;
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("Decrypted loaded Message NFT:");
                    Console.WriteLine("Author: " + loadedNFTMessage.Author);
                    Console.WriteLine("Name: " + loadedNFTMessage.Name);
                    Console.WriteLine("Description: " + loadedNFTMessage.Description);
                    await loadedNFTMessage.Decrypt(account.Secret, true);
                    Console.WriteLine("Decrypted Name: " + loadedNFTMessage.Name);
                    Console.WriteLine("Decrypted Description: " + loadedNFTMessage.Description);
                    if (loadedNFTMessage.Name == testname && loadedNFTMessage.Description == testdescription)
                        Console.WriteLine("The loading of IoT Message NFT and decryption works.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot decrypt the NFT Message: " + ex.Message);
                }
            }

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine("---------------Metadata of IoTDeviceNFT---------------------");
            try
            {
                var meta = await nft.GetMetadata(account.Address, account.Secret.ToString(), account.Address);
                Console.WriteLine($"\tMetadata of the NFT are:");
                foreach (var m in meta)
                    Console.WriteLine($"\t\"{m.Key}\": {m.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot get the IoTDeviceNFT Metadata: {ex.Message}");
            }
        }

        private static void Nft_NewMessage(object sender, (string,INFT) e)
        {
            var n = e.Item2;
            Console.WriteLine("New Message received from the IoTDevice to main.");
            MintNFTMessageForIoTDeviceEvent((sender as IoTDeviceNFT).Utxo, n.Name, n.Description, n);
        }

        private static async Task MintNFTMessageForIoTDeviceEvent(string senderUtxo, string name, string message, INFT nft)
        {
            if (account.TotalSpendableBalance > 0.01)
            {
                //var res = await account.MintNFT(nft);
                var res = await account.SendMessageNFT(name, message, account.Address, encrypt:(nft as MessageNFT).Encrypt, rewriteAuthor: "9cd2ea2fdd98bc5d39c56f652ba8a123f1e739c45520b97e59c12be1619a5b4d");
                if (res.Item1)
                    Console.WriteLine($"NFT Message from IoTDeviceNFT {senderUtxo} was minted OK. New Tx Hash is: {res.Item2}.");
                else
                    Console.WriteLine($"Cannot mint the NFT: {res.Item2}");
            }
        }


        [TestEntry]
        public static void GetMessagesFromHardwarioCloud(string param)
        {
            GetMessagesFromHardwarioCloudAsync(param);
        }
        public static async Task GetMessagesFromHardwarioCloudAsync(string param)
        {
            Console.WriteLine("---------------------------------------------");
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
            {
                Console.WriteLine("Error: Please input groupid,deviceid,apitoken");
                return;
            }
            var groupid = split[0];
            var deviceid = split[1];
            var apitoken = split[2];
            Console.WriteLine("Requesting the HARDWARIO Cloud...");
            Console.WriteLine($"\tDevice:{deviceid}");
            Console.WriteLine($"\tGroup:{groupid}");
            Console.WriteLine($"\tApiToken:{apitoken}");

            HARDWARIOIoTDataDriver hwiotdd = new HARDWARIOIoTDataDriver();
            await hwiotdd.SetConnParams(new VEDriversLite.Devices.Dto.CommonConnectionParams()
            {
                Url = "https://api.hardwario.cloud/v1",
                Secured = true,
                SType = VEDriversLite.Devices.Dto.CommunitacionSecurityType.Bearer,
                Token = apitoken
            });
            try
            {
                //var resp = await hwiotdd.HWApiClient.GetMessages(deviceid, groupid);
                var resp = await hwiotdd.HWApiClient.GetUnreadedMessage(deviceid, groupid);
                if (resp.Item1)
                {
                    Console.WriteLine("Response from HARSWARIO Cloud:");
                    Console.WriteLine(JsonConvert.SerializeObject(resp.Item2, Formatting.Indented));

                    Console.WriteLine("Post to  HARSWARIO Cloud to confirm read of the message:");
                    var resp1 = await hwiotdd.HWApiClient.ConfirmReadOfMessage(deviceid, groupid, resp.Item2.id);

                    Console.WriteLine("Response from HARSWARIO Cloud:");
                    if (resp1.Item1)
                        Console.WriteLine(JsonConvert.SerializeObject(resp1.Item2, Formatting.Indented));
                    else
                        Console.WriteLine("Cannot confirm read info to HARDWARIO Cloud.");
                }
                else
                    Console.WriteLine("Cannot obtain info from HARDWARIO Cloud.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot obtain info from HARDWARIO Cloud.");
                Console.WriteLine($"Exception Message: {ex.Message}.");
            }
            Console.WriteLine("---------------------------------------------");
        }


        #endregion
        //////////////////////////////////////////////////////////////////


        //////////////////////////////////////////////////////////////////
        #region Tools

        [TestEntry]
        public static void IPFSFileUpload(string param)
        {
            IPFSFileUploadAsync(param);
        }
        public static async Task IPFSFileUploadAsync(string param)
        {
            //var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //if (split.Length < 3)
            //    throw new Exception("Please input filename");
            //var fileName = split[0];
            var filebytes = File.ReadAllBytes(param);
            var link = string.Empty;
            try
            {
                using (Stream stream = new MemoryStream(filebytes))
                {
                    var imageLink = await NFTHelpers.UploadInfura(stream, param);
                    Console.WriteLine("Image Link: " + imageLink);
                    //var imageLink = await NFTHelpers.ipfs.FileSystem.AddAsync(stream, fileName);
                    //link = "https://gateway.ipfs.io/ipfs/" + imageLink.ToLink().Id.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }

        }

        public class IPFSLinksNFTsDto
        {
            public string Address { get; set; } = string.Empty;
            public string Utxo { get; set; } = string.Empty;
            public int Index { get; set; } = 0;
            public string Link { get; set; } = string.Empty;
            public string ImageLink { get; set; } = string.Empty;
            public string PodcastLink { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public NFTTypes Type { get; set; } = NFTTypes.Image;
        }
        [TestEntry]
        public static void GetAllIpfsLinks(string param)
        {
            GetAllIpfsLinksAsync(param);
        }
        public static async Task GetAllIpfsLinksAsync(string param)
        {

            var owners = await NeblioTransactionHelpers.GetTokenOwners(NFTHelpers.TokenId);

            var ipfsLinkNFTs = new Dictionary<string, IPFSLinksNFTsDto>();
            var ipfsCIDs = new List<string>();
            Console.WriteLine("Starting searching the owners:");
            Console.WriteLine("--------------------------------");
            foreach (var own in owners)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Owner {own.Address}. Loading NFTS...");
                var addnfts = await NFTHelpers.LoadAddressNFTs(own.Address);
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"----------{addnfts.Count} NFT Loaded------------");
                foreach (var nft in addnfts)
                {
                    if (!string.IsNullOrEmpty(nft.Link) || !string.IsNullOrEmpty(nft.ImageLink))
                    {
                        var save = false;
                        if (!string.IsNullOrEmpty(nft.Link))
                            if (nft.Link.Contains("https://gateway.ipfs.io/ipfs/"))
                                save = true;
                        if (!string.IsNullOrEmpty(nft.ImageLink))
                            if (nft.ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
                                save = true;

                        if (save)
                        {
                            var dto = new IPFSLinksNFTsDto()
                            {
                                Address = own.Address,
                                Utxo = nft.Utxo,
                                Index = nft.UtxoIndex,
                                Link = nft.Link,
                                ImageLink = nft.ImageLink,
                                Name = nft.Name,
                                Type = nft.Type
                            };
                            if (dto.Link == null)
                                dto.Link = string.Empty;
                            if (dto.ImageLink == null)
                                dto.ImageLink = string.Empty;

                            if (ipfsLinkNFTs.Values.FirstOrDefault(n => n.Link == dto.Link) != null)
                                dto.Link = string.Empty;
                            if (ipfsLinkNFTs.Values.FirstOrDefault(n => n.ImageLink == dto.ImageLink) != null)
                                dto.ImageLink = string.Empty;
                            if (ipfsLinkNFTs.Values.FirstOrDefault(n => n.PodcastLink == dto.PodcastLink) != null)
                                dto.PodcastLink = string.Empty;

                            if (nft.Type == NFTTypes.CoruzantProfile || nft.Type == NFTTypes.CoruzantArticle || nft.Type == NFTTypes.CoruzantPodcast)
                                dto.PodcastLink = (nft as CommonCoruzantNFT).PodcastLink;

                            ipfsLinkNFTs.Add($"{nft.Utxo}:{nft.UtxoIndex}", dto);
                        }
                    }

                }
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine($"-------Processing of address {own.Address} done---------");
            }

            var filename = $"{TimeHelpers.DateTimeToUnixTimestamp(DateTime.UtcNow)}-ipfsLinkNFTs.txt";
            Console.WriteLine($"Completed search. Saving file. {filename}");

            foreach (var link in ipfsLinkNFTs)
            {
                
                if (link.Value.ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
                {
                    var a = link.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty);
                    if (!ipfsCIDs.Contains(a))
                    //{
                        //ipfsCIDs.Add(a);
                        FileHelpers.AppendLineToTextFile(a, filename);
                    //}
                }
                if (link.Value.Link.Contains("https://gateway.ipfs.io/ipfs/"))
                {
                    var a = link.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty);
                    //if (!ipfsCIDs.Contains(a))
                    //{
                        //ipfsCIDs.Add(a);
                        FileHelpers.AppendLineToTextFile(a, filename);
                    //}
                }
                
            }
            /*
            var ipfs = new Ipfs.Http.IpfsClient("http://127.0.0.1:5001");
            foreach (var nft in ipfsLinkNFTs)
            {
                try
                {

                    if (!string.IsNullOrEmpty(nft.Value.ImageLink) && nft.Value.ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
                    {
                        Console.WriteLine("Pinning...");
                        await ipfs.Pin.AddAsync(nft.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                        Console.WriteLine("Pinned. " + nft.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Cannot pin " + nft.Value.ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                }
                try
                {
                    if (!string.IsNullOrEmpty(nft.Value.Link) && nft.Value.Link.Contains("https://gateway.ipfs.io/ipfs/"))
                    {
                        Console.WriteLine("Pinning...");
                        await ipfs.Pin.AddAsync(nft.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                        Console.WriteLine("Pinned. " + nft.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Cannot pin " + nft.Value.Link.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                }
                try
                {
                    if (!string.IsNullOrEmpty(nft.Value.PodcastLink) && nft.Value.PodcastLink.Contains("https://gateway.ipfs.io/ipfs/"))
                    {
                        Console.WriteLine("Pinning...");
                        await ipfs.Pin.AddAsync(nft.Value.PodcastLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                        Console.WriteLine("Pinned. " + nft.Value.PodcastLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Cannot pin " + nft.Value.PodcastLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty));
                }
            }
            */

            //var filename = $"{TimeHelpers.DateTimeToUnixTimestamp(DateTime.UtcNow)}-ipfsLinkNFTs.json";
            //Console.WriteLine($"Completed search. Saving file. {filename}");
            //var output = JsonConvert.SerializeObject(ipfsCIDs, Formatting.Indented);
            //FileHelpers.WriteTextToFile(filename, output);
        }

        [TestEntry]
        public static void PinIPFSFile(string param)
        {
            PinIPFSFileAsync(param);
        }
        public static async Task PinIPFSFileAsync(string param)
        {
            var ipfs = new Ipfs.Http.IpfsClient("http://127.0.0.1:5001");

            try
            {
                Console.WriteLine("Start Pinning...");
                var res = await ipfs.Pin.AddAsync(param);
                Console.WriteLine("Pinned.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Cannot pin " + param + ". " + ex.Message);
            }
            #endregion
        ///////////////////////////////////////////////////////////////////
        }

        [TestEntry]
        public static void GetNeblioAddressFromUDomains(string param)
        {
            GetNeblioAddressFromUDomainsAsync(param);
        }
        public static async Task GetNeblioAddressFromUDomainsAsync(string param)
        {
            Console.WriteLine("Requesting the Unstoppable domains...");

            var add = await UnstoppableDomainsHelpers.GetNeblioAddress(param);

            Console.WriteLine("Neblio Address is:");
            Console.WriteLine(add);
            Console.WriteLine("---------------------"); 
        }

        [TestEntry]
        public static void ValidateNeblioAddress(string param)
        {
            ValidateNeblioAddressAsync(param);
        }
        public static async Task ValidateNeblioAddressAsync(string param)
        {
            Console.WriteLine("Validating the Neblio Address...");

            var add = NeblioTransactionHelpers.ValidateNeblioAddress(param);

            Console.WriteLine($"Neblio Address {param} is:");
            if (!string.IsNullOrEmpty(add))
                Console.WriteLine("Valid.");
            else
                Console.WriteLine("Not Valid.");

            Console.WriteLine("---------------------");
        }

        //////////////////////////////////////////////
        #region WooCommerce

        [TestEntry]
        public static void WoCInitWooCommerceShop(string param)
        {
            WoCInitWooCommerceShopAsync(param);
        }
        public static async Task WoCInitWooCommerceShopAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 4)
                throw new Exception("Please input apiurl,apikey,apisecret,jwt");
            var apiurl = split[0];
            var apikey = split[1];
            var secret = split[2];
            var jwt = split[3];

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("---------WooCommerce Shop Init----------");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("API Url: " + apiurl);
            Console.WriteLine("-----------------------------------------");

            var res = await WooCommerceHelpers.InitStoreApiConnection(apiurl, apikey, secret, jwt, true);

            //await WoCGetShopStatsAsync(string.Empty);
        }

        [TestEntry]
        public static void WoCGetWPJWTToken(string param)
        {
            WoCGetWPJWTTokenAsync(param);
        }
        public static async Task WoCGetWPJWTTokenAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                throw new Exception("Please input apiurl,wplogin,wppass");
            var apiurl = split[0];
            var wplogin = split[1];
            var wppass = split[2];

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("-------WordPress JWT Token Request-------");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("API Url: " + apiurl);
            Console.WriteLine("-----------------------------------------");

            var res = await WooCommerceHelpers.GetJWTToken(apiurl, wplogin, wppass);
            if (res.Item1) 
                Console.WriteLine("Token received from the server.");
            Console.WriteLine("");
            Console.WriteLine(res.Item2);
        }

        [TestEntry]
        public static void WoCGetShopStats(string param)
        {
            WoCGetShopStatsAsync(param);
        }
        public static async Task WoCGetShopStatsAsync(string param)
        {
            if (!WooCommerceHelpers.IsInitialized)
            {
                Console.WriteLine("Shop is not initialized.");
                return;
            }
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("---------WooCommerce Shop Init----------");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("API Url: " + WooCommerceHelpers.Shop.WooCommerceStoreUrl);
            Console.WriteLine("-----------------------------------------");

            Console.WriteLine($"---------Products {WooCommerceHelpers.Shop.Products.Count}----------");

            foreach (var p in WooCommerceHelpers.Shop.Products.Values)
                Console.WriteLine($"Product {p.id} - {p.name}, image url: {p.images.FirstOrDefault()?.src}, price {p.regular_price}.");

            Console.WriteLine($"---------Orders {WooCommerceHelpers.Shop.Orders.Count}---------");

            foreach (var o in WooCommerceHelpers.Shop.Orders.Values)
            {
                Console.WriteLine($"========Order {o.order_key}========");
                Console.WriteLine($"ID: {o.id}");
                Console.WriteLine($"Status: {o.status}");
                Console.WriteLine($"Price: {o.total} {o.currency}.");
                Console.WriteLine($"Items - {o.line_items.Count}:");
                var txid = !string.IsNullOrEmpty(o.transaction_id) ? o.transaction_id : "none";
                Console.WriteLine($"Transaction: {txid}.");

                foreach (var pr in o.line_items)
                    Console.WriteLine($"  - Item {pr.name}, product id {pr.product_id} - price {pr.price}, qty {pr.quantity}.");

                Console.WriteLine("========================");
                if (!string.IsNullOrEmpty(o.billing.first_name))
                {
                    Console.WriteLine("---------");
                    Console.WriteLine($"Billing:");
                    Console.WriteLine(JsonConvert.SerializeObject(o.billing, Formatting.Indented));
                }
                if (!string.IsNullOrEmpty(o.shipping.first_name))
                {
                    Console.WriteLine("---------");
                    Console.WriteLine($"Shipping:");
                    Console.WriteLine(JsonConvert.SerializeObject(o.shipping, Formatting.Indented));
                }
                Console.WriteLine("=========================");
            }

            Console.WriteLine("------------------------------");
            Console.WriteLine("-----------End-----------");
            /*
            var wait = 20;
            while(true)
            {
                await Task.Delay(10000);
               
                if (wait < 0)
                {
                    Console.WriteLine("Continue?");
                    var c = Console.ReadLine();
                    if (c != "" && c != "y" && c != "Y")
                        break;
                }
            }*/
            //Console.WriteLine(res);
        }

        [TestEntry]
        public static void WoCUpdateOrderStatus(string param)
        {
            WoCUpdateOrderStatusAsync(param);
        }
        public static async Task WoCUpdateOrderStatusAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input orderid,status");
            var orderkey = split[0];
            
            Console.WriteLine($"Order Key: {orderkey}");
            
            var res = await WooCommerceHelpers.Shop.UpdateOrderStatus(orderkey, split[1]);
            Console.WriteLine($"Actual status: {res.statusclass}");
            Console.WriteLine($"New Status status: {split[1]}");
            Console.WriteLine("");
            Console.WriteLine("");
            await WoCGetOrderAsync(res.id.ToString());
            //await WoCGetShopStatsAsync("");
        }

        [TestEntry]
        public static void WoCUpdateOrderTransactionId(string param)
        {
            WoCUpdateOrderTransactionIdAsync(param);
        }
        public static async Task WoCUpdateOrderTransactionIdAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input orderid,txid");
            var orderkey = split[0];

            Console.WriteLine($"Order Key: {orderkey}");
            var res = await WooCommerceHelpers.Shop.UpdateOrderTxId(orderkey, split[1]);
            Console.WriteLine($"Transaction Id: {res.transaction_id}");
            Console.WriteLine("");  
            Console.WriteLine("");
            await WoCGetOrderAsync(res.id.ToString());
            //await WoCGetShopStatsAsync("");
        }

        [TestEntry]
        public static void WoCGetProduct(string param)
        {
            WoCGetProductAsync(param);
        }
        public static async Task WoCGetProductAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input productid");
            var productid = Convert.ToInt32(split[0]);

            Console.WriteLine($"Product Id: {productid}");
            var res = await WooCommerceHelpers.Shop.GetProduct(productid);
            Console.WriteLine($"Name: {res.name}");
            Console.WriteLine($"Transaction Id: {res.regular_price}");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        [TestEntry]
        public static void WoCGetOrder(string param)
        {
            WoCGetOrderAsync(param);
        }
        public static async Task WoCGetOrderAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input orderid");
            var orderid = Convert.ToInt32(split[0]);

            Console.WriteLine($"Order Id: {orderid}");
            var res = await WooCommerceHelpers.Shop.GetOrder(orderid);
            Console.WriteLine($"Order Key: {res.order_key}");
            Console.WriteLine($"Status: {res.status}");
            Console.WriteLine($"Total: {res.total} {res.currency}");
            Console.WriteLine($"TxId: {res.transaction_id}");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        [TestEntry]
        public static void UploadImageToWPFromIPFS(string param)
        {
            UploadImageToWPFromIPFSAsync(param);
        }
        public static async Task UploadImageToWPFromIPFSAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input link,filename");
            var link = split[0];
            var filename = split[1];
            //var res = await WooCommerceHelpers.UploadIFPSImageToWPByAPI(link, filename);
            var res = await WooCommerceHelpers.UploadIFPSImageToWP(link, filename);
            Console.WriteLine("New Url is: ");
            Console.WriteLine(res.Item2);
        }
        #endregion
    }
}
