using NBitcoin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;
using VEDriversLite.Builder;
using System.Linq;
using System.IO;

namespace test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Crypto World!");

            try
            {
                var pass = "12345678";
                var key = "soqu+MyDwhNCmi5fXL1XvtQsSD/1z9+3/1I/rvbiy8nmOndR0DkU/EgyKH52NktGqKxE2ZOHZNETrnSPXJlORA==";
                var address = "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";// "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";//"NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7";
                var receiver = "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";
                var password = "";


                var account = new NeblioAccount();
                await account.CreateNewAccount(password);

                await account.LoadAccount(pass, key, address);

                /*
                // Read into buffer and act (uses less memory)
                //await using (Stream stream = await file.OpenReadAsync())
                {
                    var byts = File.ReadAllBytes("test.png");
                    //var fileinfo = await file.ReadFileInfoAsync();
                    //var link = string.Empty;
                    //var signature = string.Empty;
                    var dtot = new OwnershipVerificationResult();
                    using (var memoryStream = new MemoryStream(byts))
                    {
                        try
                        {
                            //stream.CopyTo(memoryStream);
                            //var bytes = memoryStream.ToArray();
                            var img = System.Drawing.Image.FromStream(memoryStream);
                            dtot = await OwnershipVerifier.VerifyFromImage(img);
                        }
                        catch(Exception ex)
                        {
                            ;
                        }
                    }

                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, dtot.TxId);
                    
                }
                return;
                */


                // sign and verify NFT
                var txid = "ed5c38fc1513d48bac724a25c37b360f306e307d27b35d3c54bf15eabdf941c3";
                var wait = false;
                // create signature of NFT
                var sig = await OwnershipVerifier.GetCode(txid, account.AccountKey);
                Console.WriteLine("-------Simple Verification of NFT Ownership----------");
                Console.WriteLine("-----------------------------------------------------");
                Console.WriteLine("TxId:");
                Console.WriteLine(txid);
                Console.WriteLine("Created signature for tx:");
                Console.WriteLine(sig.Item2);
                Console.WriteLine("-----------------------------------------------------");
                var msg = OwnershipVerifier.CreateMessage(txid);
                var r = await NFTHelpers.GetNFTWithOwner(txid);
                Console.WriteLine("Verification Message: " + msg);
                Console.WriteLine("Sender of the NFT: " + r.Item2.Sender);
                Console.WriteLine("Owner of the NFT: " + r.Item2.Owner);
                Console.WriteLine("NFT Type: " + r.Item2.NFT.TypeText);
                Console.WriteLine("NFT Name: " + r.Item2.NFT.Name);
                Console.WriteLine("NFT Image Link: " + r.Item2.NFT.ImageLink);
                Console.WriteLine("NFT Description: " + r.Item2.NFT.Description);
                Console.WriteLine("-----------------------------------------------------");
                var dto = new OwnershipVerificationCodeDto()
                {
                    TxId = txid,
                    Signature = sig.Item2
                };
                // verify the signature
                if (wait)
                {
                    Console.WriteLine("Testing wait before verification:");
                    for (int i = 0; i < 90; i++)
                    {
                        await Task.Delay(1000);
                        Console.Write(i.ToString() + ", ");
                    }
                }
                Console.WriteLine();

                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("Starting Verification...");

                var rr = await OwnershipVerifier.VerifyOwner(dto);
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("Result: ");
                Console.WriteLine(rr.VerifyResult);
                Console.WriteLine("Verification Message: " + rr.Message);
                Console.WriteLine("Sender of the NFT: " + rr.Sender);
                Console.WriteLine("Owner of the NFT: " + rr.Owner);
                Console.WriteLine("NFT Type: " + rr.NFT.TypeText);
                Console.WriteLine("NFT Name: " + rr.NFT.Name);
                Console.WriteLine("NFT Image Link: " + rr.NFT.ImageLink);
                Console.WriteLine("NFT Description: " + rr.NFT.Description);
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                return;
                /*
                // Signature and verify message
                // create receiver address
                var message = "Hello";
                BitcoinAddress recaddr = null;
                BitcoinSecret Secret = new BitcoinSecret(key, NeblioTransactionBuilder.NeblioNetwork);
                recaddr = BitcoinAddress.Create(address, NeblioTransactionBuilder.NeblioNetwork);
                PubKey k = new PubKey(Secret.PubKey.ToBytes());
                var msgSigned = Secret.PrivateKey.SignMessage(message);

                //var s = recaddr.ScriptPubKey.GetDestinationPublicKeys();
                //var s = recaddr.ScriptPubKey.CreateReader();
                var s = recaddr.ScriptPubKey.GetSigner();

                var pkh = (recaddr as IPubkeyHashUsable);

                if (pkh.VerifyMessage(message, msgSigned))
                {
                    Console.WriteLine("Match :)");
                }
                else
                {
                    Console.WriteLine("Not Match.");
                }
                */
                Console.ReadLine();
                return;
                    //var r = await account.SendNeblioPayment("NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7", 1);
                Dictionary<string, string> metadata = new Dictionary<string, string>();
                metadata.Add("Data", "https://ve-nft.com/");
                //var r = await account.SendNeblioTokenPayment(NFTHelpers.TokenId, metadata, "NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7", 5);

                //var NFT = await NFTFactory.GetNFT(NFTHelpers.TokenId, "c0a92312aa4c9012b293d9827f960695c800e0d7b5e80bf4a52f8d2acddfd63b");
                //await account.SendNFT(account.Address, NFT, true, 0.33);
                //await account.SendNFTPayment(receiver, NFT);

                /*
                await account.StartRefreshingData();
                while (true)
                {
                    await Task.Delay(100);
                }*/

                /*
                var NFT = new ProfileNFT("");
                NFT.Author = "fyziktom";
                NFT.Name = "Tomas";
                NFT.Surname = "Svoboda";
                NFT.Nickname = "fyziktom";
                NFT.Description = "Neblio";
                NFT.ImageLink = "https://gateway.ipfs.io/ipfs/QmQ5qNNtShVqrZstzWMTZeWXFSnDojks3RWZpja6gJy8MJ";
                var rtxid = await NFTHelpers.MintProfileNFT(account, NFT);
                */


                var NFT = new ImageNFT("");
                NFT.Name = "Sun";
                NFT.Description = "My Artwork - Sun";
                NFT.Author = "fyziktom";
                NFT.ImageLink = "https://gateway.ipfs.io/ipfs/QmWTkVqaWn1ABZ1UMKL91pxxspzXW6yodJ9bjUn6nPLeHX";
                var rtxid = await account.MintNFT(NFTHelpers.TokenId, NFT);

                //var rtxid = await NFTHelpers.ChangeProfileNFT(account, (ProfileNFT)NFT);

                //var rtxid = await NFTHelpers.SendNFTPayment(account, receiver, NFT, NFT.Utxo, NFT.Price);
                //var rtxid = await NFTHelpers.SendOrderedNFT(account, (PaymentNFT)NFT);
                //Console.WriteLine(rtxid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press enter to end...");
            Console.ReadLine();
        }
    }
}


//var NFT = await NFTFactory.GetNFT(NFTHelpers.TokenId, "40361c3e69ff48ad835b7a45c26d8e08bf4a42e3d1a4edb9c38d393ddb9d038f");
//var rtxid = await NFTHelpers.SendOrderedNFT(account, (PaymentNFT)NFT);