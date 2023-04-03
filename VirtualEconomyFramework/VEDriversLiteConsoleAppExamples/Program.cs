using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;
using VEDriversLite.Security;
using VEDriversLite.NeblioAPI;

namespace VEDriversLiteConsoleAppExamples
{
    class Program
    {
        private static NeblioAccount account = new NeblioAccount();
        static async Task Main(string[] args)
        {
            try
            {
                await Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excetpion during running tests. " + ex.Message);
            }
            Console.WriteLine("End of the program. Press enter to exit...");
            Console.ReadLine();
        }

        private static async Task Start()
        {
            Console.WriteLine("Hello Crypto World!");
            Console.WriteLine("These are examples of using VEDriversLite");

            var fromVENFTBackup = FileHelpers.IsFileExists("backup.txt");
            var fromKey = FileHelpers.IsFileExists("key.txt");

            if (fromVENFTBackup)
                Console.WriteLine("Loading from VENFT Backup file.");
            else if (fromKey)
                Console.WriteLine("Loading from Key file.");
            else
                Console.WriteLine("No file backup.txt or key.txt found. App will create new address for you.");

            Console.WriteLine("---------------");
            Console.WriteLine("Input password.");
            var pass = Console.ReadLine();

            account.FirsLoadingStatus += Account_FirsLoadingStatus;

            // generate new address with private key and save it to key.txt in root exe folder
            if (!fromVENFTBackup && !fromKey)
                await GenerateNewAccount(pass);
            else if (fromVENFTBackup)
                await LoadAccountFromVENFTBackup(pass, "backup.txt");
            else
                await InitAccount(pass);// init with just password. in this case you must place key.txt in root exe folder

            // or init account with pass, key and address
            //await InitAccount(pass, key, address);

            account.FirsLoadingStatus -= Account_FirsLoadingStatus;
            account.NFTsChanged += Account_NFTsChanged;
            account.NFTAddedToPayments += Account_NFTAddedToPayments;

            while (true)
            {
                Console.WriteLine("1 - GenerateNewAccount");
                Console.WriteLine("2 - LoadAccountDetails");
                Console.WriteLine("3 - SendTransaction");
                Console.WriteLine("4 - SendTokenTransaction");
                Console.WriteLine("5 - MintNFT");
                Console.WriteLine("6 - SendNFT");
                Console.WriteLine("7 - WritePriceToNFT");
                Console.WriteLine("8 - SendNFTPayment");
                Console.WriteLine("9 - VerificationOfNFTs");
                Console.WriteLine("10 - SignAndVerifyMessage");
                Console.WriteLine("11 - Get Tx details");
                Console.WriteLine("12 - Get NFT Details");
                Console.WriteLine("13 - Exit");
                Console.WriteLine("Please input number of example and press enter: ");

                var read = 0;
                try
                {
                    read = Convert.ToInt32(Console.ReadLine());
                    if (read > 13)
                        Console.WriteLine("Wrong input. Must be lower than 13.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Wrong Input. You probably did not input number.");
                }

                try
                {
                    switch (read)
                    {
                        case 1:
                            await GenerateNewAccount();
                            break;
                        case 2:
                            await LoadAccountDetails();
                            break;
                        case 3:
                            await SendTransaction();
                            break;
                        case 4:
                            await SendTokenTransaction();
                            break;
                        case 5:
                            await MintNFT();
                            break;
                        case 6:
                            await SendNFT();
                            break;
                        case 7:
                            await WritePriceToNFT();
                            break;
                        case 8:
                            await SendNFTPayment();
                            break;
                        case 9:
                            await VerificationOfNFTs();
                            break;
                        case 10:
                            await SignAndVerifyMessage();
                            break;
                        case 11:
                            await GetTxDetails();
                            break;
                        case 12:
                            await GetNFTDetails();
                            break;
                        case 13:
                            return;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Exception occurs: " + ex.Message);
                }
                await Task.Delay(10);
            }
        }

        private static void Account_NFTAddedToPayments(object sender, (string, int) e)
        {
            Console.WriteLine($"New NFT in payments: {e.Item1}:{e.Item2}");
        }

        private static void Account_NFTsChanged(object sender, string e)
        {
            Console.WriteLine("NFT changed: " + e);
        }

        private static void Account_FirsLoadingStatus(object sender, string e)
        {
            Console.WriteLine("Loading Status: " + e);
        }

        private static async Task GenerateNewAccount(string password = "")
        {
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Please input password");
                password = Console.ReadLine();
            }
            await account.CreateNewAccount(password, true); //new address and pass will be saved to root of exe as key.txt
        }
        private static async Task<bool> InitAccount(string password, string ekey, string addr)
        {
            return await account.LoadAccount(password, ekey, addr, withoutNFTs:false); // put here your password, encrypted key and address
        }

        private static async Task<bool> LoadAccountFromVENFTBackup(string password, string filename)
        {
            return await account.LoadAccountFromVENFTBackup(password, "", filename, withoutNFTs: false); // put here your password, encrypted key and address
        }

        private static async Task<bool> InitAccount(string password)
        {
            return await account.LoadAccount(password, withoutNFTs:false); // put here your password
        }

        private static async Task LoadAccountDetails()
        {
            Console.WriteLine("Account Details");
            Console.WriteLine($"Account Address: {account.Address}");
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

        private static async Task SendTransaction()
        {
            Console.WriteLine("Input receiver: ");
            var receiver = Console.ReadLine();
            Console.WriteLine("Input Neblio amount: ");
            var amount = Convert.ToDouble(Console.ReadLine(), CultureInfo.InvariantCulture);
            var res = await account.SendNeblioPayment(receiver, amount);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        private static async Task SendTokenTransaction()
        {
            Console.WriteLine("Input receiver: ");
            var receiver = Console.ReadLine();
            Console.WriteLine("Input Neblio amount: ");
            var amount = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Write some message to the metadata:");
            var data = Console.ReadLine();
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

        private static async Task MintNFT()
        {
            Console.WriteLine("Minting NFT");
            // create NFT object
            var nft = new ImageNFT("");

            Console.WriteLine("Fill name:");
            var data = Console.ReadLine();
            if (!string.IsNullOrEmpty(data))
                nft.Name = data;
            else
                nft.Name = "My First NFT";

            Console.WriteLine("Fill Author:");
            data = Console.ReadLine();
            if (!string.IsNullOrEmpty(data))
                nft.Author = data;
            else
                nft.Author = "fyziktom";

            Console.WriteLine("Fill Description:");
            data = Console.ReadLine();
            if (!string.IsNullOrEmpty(data))
                nft.Description = data;
            else
                nft.Description = "This was created with VEDriversLite";

            Console.WriteLine("Fill Link:");
            data = Console.ReadLine();
            if (!string.IsNullOrEmpty(data))
                nft.Link = data;
            else
                nft.Link = "https://veframework.com/";

            Console.WriteLine("Fill Image Link:");
            data = Console.ReadLine();
            if (!string.IsNullOrEmpty(data))
                nft.ImageLink = data;
            else
                nft.ImageLink = "https://gateway.ipfs.io/ipfs/QmWTkVqaWn1ABZ1UMKL91pxxspzXW6yodJ9bjUn6nPLeHX";

            // send 10 VENFT to receiver with connected metadata
            var res = await account.MintNFT(nft);

            // or multimint with 5 coppies (mint 1 + 5);
            // var res = await account.MintMultiNFT(NFTHelpers.TokenId, nft, 5); 
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        private static async Task SendNFT()
        {
            Console.WriteLine("Fill input NFT Utxo: ");
            var nftutxo = Console.ReadLine();
            // load existing NFT object and wait for whole data synchronisation
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, 0, 0, true);
            // send NFT to receiver
            if (nft == null)
                throw new Exception("NFT does not exists!");
            Console.WriteLine("Fill receiver");
            var receiver = Console.ReadLine();
            var res = await account.SendNFT(receiver, nft, false, 0);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        private static async Task WritePriceToNFT()
        {
            Console.WriteLine("Fill input NFT Utxo: ");
            var nftutxo = Console.ReadLine();
            // load existing NFT object and wait for whole data synchronisation
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, 0, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            // send NFT to receiver
            var res = await account.SendNFT(account.Address, nft, true, 0.05);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        private static async Task SendNFTPayment()
        {
            Console.WriteLine("Fill input NFT Utxo: ");
            var nftutxo = Console.ReadLine();
            // load existing NFT object and wait for whole data synchronisation. NFT must have written price!
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, 0, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            // send NFT to receiver
            Console.WriteLine("Fill receiver");
            var receiver = Console.ReadLine();
            var res = await account.SendNFTPayment(receiver, nft);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        private static async Task VerificationOfNFTs()
        {
            Console.WriteLine("Fill input NFT Utxo: ");
            var nftutxo = Console.ReadLine();
            var res = await account.GetNFTVerifyCode(nftutxo);
            Console.WriteLine("Signature for TxId:");
            Console.WriteLine(res.TxId);
            Console.WriteLine("Signature:");
            Console.WriteLine(res.Signature);
            Console.WriteLine("Verifying now...");
            // res is already OwnershipVerificationCodeDto so creating new object is just example
            var vres = await OwnershipVerifier.VerifyOwner(new OwnershipVerificationCodeDto() { TxId = nftutxo, Signature = res.Signature });
            Console.WriteLine("Result of verification is: ");
            Console.WriteLine(vres.VerifyResult);
            Console.WriteLine("Owner of NFT is: ");
            Console.WriteLine(vres.Owner);
            Console.WriteLine("Loaded NFT is: ");
            Console.WriteLine(vres.NFT.Name);
        }

        private static async Task SignAndVerifyMessage()
        {
            var msg = "Neblio is the Best :)";
            // sign it with loaded account
            Console.WriteLine("Mesaage for signature: ");
            Console.WriteLine(msg);
            var signature = await account.SignMessage(msg);
            Console.WriteLine("Signature of the message is: ");
            Console.WriteLine(signature.Item2);
            // and verify the signature
            var ver = await account.VerifyMessage(msg, signature.Item2, account.Address);
            Console.WriteLine("Signature verification result is: ");
            Console.WriteLine(ver.Item2);
        }

        private static async Task GetTxDetails()
        {
            Console.WriteLine("Input Tx Id Hash");
            var txid = Console.ReadLine();
            var txinfo = await NeblioAPIHelpers.GetTransactionInfo(txid);
            // sign it with loaded account
            Console.WriteLine("Timestamp");
            Console.WriteLine(TimeHelpers.UnixTimestampToDateTime((double)txinfo.Blocktime));
            Console.WriteLine("Number of confirmations: ");
            Console.WriteLine(txinfo.Confirmations);
            Console.WriteLine("--------------");
            Console.WriteLine("Vins:");
            txinfo.Vin.ToList().ForEach(v => {
                Console.WriteLine($"Vin of value: {v.ValueSat} Nebl sat. from txid: {v.Txid} and vout index {v.Vout}");
            });
            Console.WriteLine("-------------");
            Console.WriteLine("--------------");
            Console.WriteLine("Vouts:");
            txinfo.Vout.ToList().ForEach(v => { 
                Console.WriteLine($"Vout index {v.N} of value: {v.Value} Nebl sat. to receiver scrupt pub key {v.ScriptPubKey?.Addresses?.FirstOrDefault()}"); 
            });
            Console.WriteLine("-------------");
        }

        private static async Task GetNFTDetails()
        {
            Console.WriteLine("Input NFT Tx Id Hash");
            var txid = Console.ReadLine();
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, txid, 0, 0, true);
            // sign it with loaded account
            Console.WriteLine("Timestamp");
            Console.WriteLine(nft.Time);
            Console.WriteLine("Name: ");
            Console.WriteLine(nft.Name);
            Console.WriteLine("Author: ");
            Console.WriteLine(nft.Author);
            Console.WriteLine("Description: ");
            Console.WriteLine(nft.Description);
            Console.WriteLine("Link: ");
            Console.WriteLine(nft.Link);
            Console.WriteLine("Image Link: ");
            Console.WriteLine(nft.ImageLink);
        }


    }
}
