using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;
using VEDriversLite.Security;

namespace TestVEDriversLite
{
    public static class VEDriversLiteTests
    {
        private static string password = string.Empty;
        private static NeblioAccount account = new NeblioAccount();
        private static DogeAccount dogeAccount = new DogeAccount();

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
            Console.WriteLine("Start of auto refreshing account data is called after successfull load");
            Console.WriteLine("---------------------------------");
        }

        [TestEntry]
        public static void GenerateNewAccount(string param)
        {
            GenerateNewAccountAsync(param);
        }
        public static async Task GenerateNewAccountAsync(string param)
        {
            password = param;
            await account.CreateNewAccount(password, true);
            Console.WriteLine($"Account created.");
            Console.WriteLine($"Address: {account.Address}");
            Console.WriteLine($"Encrypted Private Key: {await account.AccountKey.GetEncryptedKey("", true)}");
            StartRefreshingData( null);
        }


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
            await account.LoadAccount(password);
            StartRefreshingData(null);
        }

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
            await account.LoadAccount(pass, ekey, addr);
            StartRefreshingData(null);
        }

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
            var res = await account.AccountKey.GetEncryptedKey();
            Console.WriteLine($"Private key for address {account.Address} is: ");
            Console.WriteLine(res);
        }

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

            // send 10 VENFT to receiver with connected metadata
            var res = await account.MintNFT(nft);

            // or multimint with 5 coppies (mint 1 + 5);
            // var res = await account.MintMultiNFT(NFTHelpers.TokenId, nft, 5); 
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }


        [TestEntry]
        public static void SendNFT(string param)
        {
            SendNFTAsync(param);
        }
        public static async Task SendNFTAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input receiveraddress,utxo");
            
            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[1];
            // load existing NFT object and wait for whole data synchronisation
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, 0, true);
            // send NFT to receiver
            if (nft == null)
                throw new Exception("NFT does not exists!");
            Console.WriteLine("Receiver");
            var receiver = split[0];
            var res = await account.SendNFT(receiver, nft, false, 0);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void WritePriceToNFT(string param)
        {
            WritePriceToNFTAsync(param);
        }
        public static async Task WritePriceToNFTAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input utxo,price");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[0];
            // load existing NFT object and wait for whole data synchronisation
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            // send NFT to receiver
            var price = Convert.ToDouble(split[1], CultureInfo.InvariantCulture);
            var res = await account.SendNFT(account.Address, nft, true, price);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void SendNFTPayment(string param)
        {
            SendNFTPaymentAsync(param);
        }
        public static async Task SendNFTPaymentAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input utxo,price");

            Console.WriteLine("Input NFT Utxo: ");
            var nftutxo = split[0];
            // load existing NFT object and wait for whole data synchronisation. NFT must have written price!
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, nftutxo, 0, true);
            if (nft == null)
                throw new Exception("NFT does not exists!");
            if (!nft.PriceActive)
                throw new Exception("NFT does not have setted price.");
            // send NFT to receiver
            Console.WriteLine("Receiver");
            var receiver = split[1];
            var res = await account.SendNFTPayment(receiver, nft);
            Console.WriteLine("New TxId hash is: ");
            Console.WriteLine(res);
        }

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
            var vres = await OwnershipVerifier.VerifyOwner(new OwnershipVerificationCodeDto() { 
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
            var txinfo = await NeblioTransactionHelpers.GetTransactionInfo(param);
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


        [TestEntry]
        public static void GetNFTDetails(string param)
        {
            GetNFTDetailsAsync(param);
        }
        public static async Task GetNFTDetailsAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                throw new Exception("TxId must be filled.");

            Console.WriteLine("Input NFT Tx Id Hash");
            var txid = param;
            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, txid, 0, true);
            // sign it with loaded account
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

            var res = await ECDSAProvider.DecryptMessage(param, await account.AccountKey.GetEncryptedKey());
            Console.WriteLine("Decrypted message is: ");
            Console.WriteLine(res);
        }

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
                throw new Exception("Please input message,address.");

            var res = await ECDSAProvider.EncryptStringWithSharedSecret(split[0], split[1], account.Secret);
            Console.WriteLine("Encrypted message is: ");
            Console.WriteLine(res);
        }

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
                throw new Exception("Please input message,address.");

            var res = await ECDSAProvider.DecryptStringWithSharedSecret(split[0], split[1], account.Secret);
            Console.WriteLine("Decrypted message is: ");
            Console.WriteLine(res);
        }

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

            var res = await SymetricProvider.EncryptString(split[1], split[0]);
            Console.WriteLine("Encrypted message is: ");
            Console.WriteLine(res);
        }

        [TestEntry]
        public static void AESDecryptMessage(string param)
        {
            AESDecryptMessageAsync(param);
        }
        public static async Task AESDecryptMessageAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                throw new Exception("Please input message,password.");

            var res = await SymetricProvider.DecryptString(split[1], split[0]);
            Console.WriteLine("Decrypted message is: ");
            Console.WriteLine(res);
        }

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

        [TestEntry]
        public static void LoadMessageNFT(string param)
        {
            LoadMessageNFTAsync(param);
        }
        public static async Task LoadMessageNFTAsync(string param)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 1)
                throw new Exception("Please input txid.");

            var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, split[0], 0, true);
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
            Console.WriteLine($"Encrypted Private Key: {await dogeAccount.AccountKey.GetEncryptedKey("", true)}");
            StartRefreshingData(null);
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
            await dogeAccount.LoadAccount(password);
            DogeStartRefreshingData(null);
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
            var res = await dogeAccount.AccountKey.GetEncryptedKey();
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

    }
}
