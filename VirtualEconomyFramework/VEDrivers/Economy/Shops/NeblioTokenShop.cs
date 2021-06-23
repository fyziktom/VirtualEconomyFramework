using log4net;
using VEDriversLite.NeblioAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Shops
{
    public class NeblioTokenShop : CommonShop
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NeblioTokenShop(string address, string tokenId)
        {
            ShopItems = new List<IShopItem>();

            Type = ShopTypes.NeblioTokenShop;
            Address = address;
            TokenId = tokenId;
            var found = findSettingToken().GetAwaiter().GetResult();
            if (!found)
                throw new Exception("Cannot find Setting Token on the address!");

            IsSettingFound = true;
        }
        public string TokenId { get; set; } = string.Empty;
        private ICryptocurrency neblio = new NeblioCryptocurrency(false);
        private int shopRefresh = 5000;

        private async Task<bool> findSettingToken()
        {
            var utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address);

            var txId = string.Empty;

            if (utxos != null)
            {
                foreach(var u in utxos)
                {
                    if(u.Tokens?.ToList()?[0]?.TokenId == TokenId)
                    {
                        var info = await NeblioTransactionHelpers.TransactionInfoAsync(null, TransactionTypes.Neblio, u.Txid, Address);

                        if (info != null)
                        {
                            // owner txs
                            if (info.To[0] == info.From[0]) // to prevent another person will send setting token with wrong setting
                            {
                                if (info.VoutTokens[0].Metadata != null)
                                {
                                    if (info.VoutTokens[0].Metadata.TryGetValue("ShopSettingToken", out string value)) 
                                    { 
                                        if (value == "true")
                                        {
                                            SettingToken = info.VoutTokens[0];
                                            loadSettingsFromToken();
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void loadSettingsFromToken()
        {
            if (SettingToken != null)
            {
                if (SettingToken.Metadata.TryGetValue("Name", out string name))
                    Name = name;
                if (SettingToken.Metadata.TryGetValue("Description", out string desc))
                    Description = desc;

                try
                {
                    if (SettingToken.Metadata.TryGetValue("ShopItems", out string shopItems))
                    {
                        var tsi = JsonConvert.DeserializeObject<List<TokenShopItem>>(shopItems);
                        foreach (var i in tsi)
                        {
                            i.Token = TokenFactory.GetTokenById(TokenTypes.NTP1, i.Name, TokenId, 0.0);
                            ShopItems.Add(i);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Cannot load list of ShopItems", ex);
                }
            }
        }

        public override async Task<string> GetShopComponent()
        {
            throw new NotImplementedException();
        }

        public override async Task<string> StartShop()
        {
            _ = Task.Run(async () =>
             {
                 // todo cancel
                 while(true)
                 {
                     IsRunning = true;
                     try
                     {
                         if (IsActive)
                         {
                             var mainItem = ShopItems.FirstOrDefault();
                             if (mainItem != null) {
                                 var neblUtxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0001, 1000000);

                                 if (neblUtxos?.Count > 0)
                                 {
                                     foreach (var u in neblUtxos)
                                     {
                                         var value = (u.Value / neblio.FromSatToMainRatio);

                                         if (value >= (mainItem.Price - 0.0002) && value <= (mainItem.Price + 0.0002))
                                         {
                                             // todo load from account which creates the shop
                                             var txinfo = await NeblioTransactionHelpers.TransactionInfoAsync(null, TransactionTypes.Neblio, u.Txid);
                                             if ((txinfo.From[0] != Address && txinfo.To[0] != Address) && !string.IsNullOrEmpty(txinfo.From[0])) // this is received order from some address
                                             {
                                                 var resp = await processOrder(u, txinfo, mainItem);
                                             }
                                         }
                                     }
                                 }
                             }
                         }

                         await Task.Delay(shopRefresh);
                     }
                     catch (Exception ex)
                     {
                         log.Error("Error during refreshing shop", ex);
                     }
                 }
             });

            return await Task.FromResult("Running!");
        }

        private async Task<string> processOrder(Utxos utxo, ITransaction txinfo, IShopItem mainItem)
        {
            var amountOfTokens = (int)((mainItem.Price / (utxo.Value / neblio.FromSatToMainRatio)) * mainItem.Lot);

            if (amountOfTokens != 0)
            {
                try
                {
                    var meta = new Dictionary<string, string>();
                    meta.Add("Token Order", "true");
                    meta.Add("PaymentTxId", utxo.Txid);

                    var sutxs = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, TokenId, amountOfTokens-1);
                    if (sutxs.Count != 0)
                    {
                        var sendutxos = new List<string>();
                        foreach (var s in sutxs)
                            sendutxos.Add(s.Txid + ":" + s.Index);

                        var dto = new SendTokenTxData()
                        {
                            Amount = amountOfTokens,
                            Id = TokenId,
                            Metadata = meta,
                            Password = string.Empty, // shop must be unlocked
                            SenderAddress = Address,
                            ReceiverAddress = txinfo.From[0],
                            sendUtxo = sendutxos,
                            NeblUtxo = utxo.Txid,
                            UseRPCPrimarily = false
                        };

                        var resp = await NeblioTransactionHelpers.SendNTP1TokenAPI(dto, isItMintNFT: true);

                        return resp;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Cannot send ordered tokens!", ex);
                }
            }

            return string.Empty;
        }
    }
}
