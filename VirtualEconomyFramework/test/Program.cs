using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;

namespace test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Crypto World!");

            try
            {
                var pass = "";
                var key = "";
                var address = "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";// "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";//"NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7";
                var receiver = "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";

                NeblioAccount account = new NeblioAccount();
                await account.LoadAccount(pass, key, address); // put here your password
                                                               //await account.StartRefreshingData();
                await account.StartRefreshingData();
                //var r = await account.SendNeblioPayment("NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7", 1);
                Dictionary<string, string> metadata = new Dictionary<string, string>();
                metadata.Add("Data", "https://ve-nft.com/");
                //var r = await account.SendNeblioTokenPayment(NFTHelpers.TokenId, metadata, "NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7", 5);

                var NFT = await NFTFactory.GetNFT(NFTHelpers.TokenId, "c0a92312aa4c9012b293d9827f960695c800e0d7b5e80bf4a52f8d2acddfd63b");
                //await account.SendNFT(account.Address, NFT, true, 0.33);
               //await account.SendNFTPayment(receiver, NFT);

                while (true)
                {
                    await Task.Delay(100);
                }
                /*
                var NFT = new ProfileNFT("");
                NFT.Author = "fyziktom";
                NFT.Name = "Tomas";
                NFT.Surname = "Svoboda";
                NFT.Nickname = "fyziktom";
                NFT.ImageLink = "https://gateway.ipfs.io/ipfs/QmQ5qNNtShVqrZstzWMTZeWXFSnDojks3RWZpja6gJy8MJ";
                
                var rtxid = await NFTHelpers.MintProfileNFT(account, NFT);
                */
                NFT.Description = "Neblio";
                //var rtxid = await NFTHelpers.ChangeProfileNFT(account, (ProfileNFT)NFT);
                //var rtxid = await NFTHelpers.MintImageNFT(account, NFT);
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