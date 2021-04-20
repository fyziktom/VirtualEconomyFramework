using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets;

namespace NeblioConsoleAppExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello Crypto People. This is simple app with VEFramework!");

            EconomyMainContext.WorkWithDb = false;
            EconomyMainContext.WorkWithQTRPC = false;
            NeblioTransactionHelpers.qtRPCClient = new QTWalletRPCClient();

            // declare address and key
            var sender = "NeNE6a2YQCq4yBLoVbVpcCzx44jVEBLaUE"; // add your address
            var senderPrivateKey = "...................."; // add your private key for the sender address
            var receiver = "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA"; // tx will be send to this address
            var mypass = "mypass"; // fill some your address

            var account = AccountFactory.GetAccount(Guid.Empty, AccountTypes.Neblio, Guid.Empty, Guid.Empty, "My Account", sender, 0);

            if (account != null)
            {
                // load account key
                account.AccountKey = new VEDrivers.Security.EncryptionKey(senderPrivateKey, mypass);
                account.AccountKey.Type = VEDrivers.Security.EncryptionKeyType.AccountKey;

                // add account to common list
                EconomyMainContext.Accounts.TryAdd(account.Address, account);

                // create token metadata
                var metadata = new Dictionary<string, string>();
                metadata.Add("Data", "My first metadata in token with VEFramework");

                // fill input data for sending tx
                var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
                {
                    Amount = 1,
                    Id = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8", // id of token
                    Symbol = "VENFT", // symbol of token
                    Metadata = metadata,
                    Password = mypass,
                    SenderAddress = account.Address,
                    ReceiverAddress = receiver
                };

                var txid = string.Empty;
                try
                {
                    // send tx
                    txid = await NeblioTransactionHelpers.SendNTP1TokenAPI(dto);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot send neblio transaction: " + ex.Message);
                }

                Console.WriteLine($"Tx sended with txId {txid}");
                Console.WriteLine("---------------------------");
                Console.WriteLine("Press enter to end...");
                Console.ReadLine();

            }
        }
    }
}
