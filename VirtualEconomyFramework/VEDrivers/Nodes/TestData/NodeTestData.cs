using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Nodes.TestData
{
    public static class NodeTestData
    {
        public static string[] GetTestData(string AccountAddress = "TestAddress1234567890",
                                string senderAddress = "SenderAddress0987654321", 
                                string txId = "1234567890987654321", 
                                string TokenName = "PLFToken", 
                                string TokenSymbol = "PLF",
                                double amount = 1.0)
        {
            return new string[] {
            //Tx Data
            "{\"AccountAddress\":\"" + AccountAddress + "\",\"WalletName\":\"TestWallet\",\"Type\":2,\"Direction\":0,\"token\":" +
                "{\"Id\":\"La8PnkibogTQoKobFgV2szZguQS5Qhp6vJ74mh\",\"Name\":\"PLFToken\",\"Symbol\":\"PLF\",\"BaseURL\":\"\",\"ImageUrl\":\"\",\"IssuerName\":\"fyziktom\"," +
                "\"MetadataAvailable\":false,\"MaxSupply\":1000000000000000.0,\"CirculatingSuply\":0.0,\"TransferFee\":0.0,\"ActualBTCPrice\":0.0,\"ActualBalance\":1.0}," +
                "\"data\":{\"WalletName\":\"TestWallet\",\"Type\":2," +
                "\"TransactionDetails\":{\"Type\":2,\"Direction\":0,\"TxId\":\"" + txId + "\"," +
                "\"From\":[\"" + senderAddress + "\"]," +
                "\"To\":[\"" + AccountAddress + "\"]," +
                "\"Ammount\":0.0,\"Confirmations\":0,\"TimeStamp\":\"2021-03-06T15:30:46.9699032Z\"," +
                "\"VinTokens\":[{\"Id\":\"La8PnkibogTQoKobFgV2szZguQS5Qhp6vJ74mh\"," +
                    "\"Name\":\"PLFToken\",\"Symbol\":\"PLF\",\"BaseURL\":\"\",\"ImageUrl\":\"\",\"IssuerName\":\"fyziktom\"," +
                    "\"MetadataAvailable\":false,\"MaxSupply\":1000000000000000.0,\"CirculatingSuply\":0.0,\"TransferFee\":0.0,\"ActualBTCPrice\":0.0,\"ActualBalance\":85.0}]," +
                "\"VoutTokens\":[{\"Id\":\"La8PnkibogTQoKobFgV2szZguQS5Qhp6vJ74mh\"," +
                    "\"Name\":\"" + TokenName + "\",\"Symbol\":\"" + TokenSymbol + "\",\"BaseURL\":\"\",\"ImageUrl\":\"\",\"IssuerName\":\"fyziktom\"," +
                    "\"MetadataAvailable\":false,\"MaxSupply\":1000000000000000.0,\"CirculatingSuply\":0.0,\"TransferFee\":0.0,\"ActualBTCPrice\":0.0,\"ActualBalance\":" + amount.ToString() + "}]," +
                "\"Metadata\":null},\"OwnerId\":\"00000000-0000-0000-0000-000000000000\"," +
                "\"AccountAddress\":\"" + AccountAddress + "\",\"TxId\":\"" + txId + "\"}}",
            //Account Data
            "{\"Id\":\"fc5df2f1-6def-4f08-973b-d9584e85b4d2\"," +
                "\"Name\":\"TestAccount\"," +
                "\"Address\":\"" + AccountAddress + "\",\"WalletId\":\"41ea1423-199f-432c-af3d-9b6181f77f3b\",\"Type\":1," +
                "\"OwnerId\":\"00000000-0000-0000-0000-000000000000\",\"NumberOfTransaction\":76.0,\"TotalBalance\":0.0023,\"TotalSpendableBalance\":0.0,\"TotalUnconfirmedBalance\":0.0001," +
                "\"CreatedBy\":\"fyziktom\",\"ModifiedBy\":\"fyziktom\",\"Version\":\"0.1\",\"Deleted\":false," +
                "\"ModifiedOn\":\"2021-02-21T20:47:50.632868\",\"CreatedOn\":\"2021-02-21T20:47:50.632868\"," +
                "\"Tokens\":" +
                    "{\"La8PnkibogTQoKobFgV2szZguQS5Qhp6vJ74mh\":" +
                        "{\"Id\":\"La8PnkibogTQoKobFgV2szZguQS5Qhp6vJ74mh\"," +
                        "\"Name\":\"PLFToken\",\"Symbol\":\"PLF\",\"BaseURL\":\"\"," +
                        "\"ImageUrl\":\"https://ntp1-icons.ams3.digitaloceanspaces.com/a8eb156fe4078fa15ba9ddb37ac1c82de78e74f9.PNG\"," +
                        "\"IssuerName\":\"fyziktom\",\"MetadataAvailable\":false,\"MaxSupply\":1000000000000000.0,\"CirculatingSuply\":0.0,\"TransferFee\":0.0," +
                        "\"ActualBTCPrice\":0.0,\"ActualBalance\":33.0}," +
                    "\"La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei\":" +
                        "{\"Id\":\"La8N1QroEDxxjkKYaPdPzatRj12nvRnL9JbUei\"," +
                        "\"Name\":\"CARToken\",\"Symbol\":\"CART\",\"BaseURL\":\"\"," +
                        "\"ImageUrl\":\"https://ntp1-icons.ams3.digitaloceanspaces.com/6b2053f6f0f50100f7931bc1273cd4e043401796.png\"," +
                        "\"IssuerName\":\"fyziktom\",\"MetadataAvailable\":false,\"MaxSupply\":1000000000000000.0,\"CirculatingSuply\":0.0,\"TransferFee\":0.0," +
                        "\"ActualBTCPrice\":0.0,\"ActualBalance\":1.0}," +
                    "\"La3R1Y4px7Za5wb1itqi6AzfareDfmpBapbw8S\":" +
                        "{\"Id\":\"La3R1Y4px7Za5wb1itqi6AzfareDfmpBapbw8S\"," +
                        "\"Name\":\"" + TokenName + "\",\"Symbol\":\"" + TokenSymbol + "\",\"BaseURL\":\"\"," +
                        "\"ImageUrl\":\"https://ntp1-icons.ams3.digitaloceanspaces.com/a8eb156fe4078fa15ba9ddb37ac1c82de78e74f9.PNG\"," +
                        "\"IssuerName\":\"Z\",\"MetadataAvailable\":false,\"MaxSupply\":1000000.0,\"CirculatingSuply\":0.0,\"TransferFee\":0.0," +
                        "\"ActualBTCPrice\":0.0,\"ActualBalance\":10.0}}," +
                "\"Transactions\":{}}"
            };

        }

    }
}
