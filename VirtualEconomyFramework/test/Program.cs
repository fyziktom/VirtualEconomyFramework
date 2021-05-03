using System;
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
                var address = "NTYZBARSWkB1rBmzdC3W8NnVyxv9dchtqG";
                var receiver = "NWzAr4qG9VnVv9GajWYWJmBVdNU4SACEvn";

                NeblioAccount account = new NeblioAccount();
                await account.LoadAccount(pass, key, address); // put here your password
                                                               //await account.StartRefreshingData();

                var NFT = await NFTFactory.GetNFT(NFTHelpers.TokenId, "2254f7ebae44a723b072ad95130f99c8732a99a4d5da62e9e2e164cdcc27a735");

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
                var rtxid = await NFTHelpers.SendNFTPayment(account, receiver, NFT, NFT.Utxo, NFT.Price);
                Console.WriteLine(rtxid);
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