using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEconomy.Common;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets.Handlers;
using VEDrivers.Messages.DTO;
using VEDrivers.Security;

namespace VEDrivers.Messages
{
    public static class MessagesHelpers
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static BasicAccountHandler accountHandler = new BasicAccountHandler();
        public static string TokenId = EconomyMainContext.MessagingToken.Id;
        public static string TokenSymbol = EconomyMainContext.MessagingToken.Symbol;
        public static async Task<string> SendMessage(SendMessageDto dto)
        {
            if (EconomyMainContext.Wallets.TryGetValue(dto.WalletId, out var wallet))
            {
                if (wallet.Accounts.TryGetValue(dto.SenderAddress, out var account))
                {
                    var txdto = new SendTokenTxData();

                    txdto.Amount = 1;
                    txdto.Id = TokenId; // token ID - todo MSGT
                    txdto.Symbol = TokenSymbol; // token symbol - todo MSGT

                    txdto.UseRPCPrimarily = false;
                    txdto.Password = dto.AccountPassword;
                    txdto.SenderAddress = dto.SenderAddress;
                    txdto.ReceiverAddress = dto.ReceiverAddress;


                    ///////////////////////////////
                    // prepare the message to send

                    if (dto.InitMessage)
                    {
                        // for init message lets create new uid
                        txdto.Metadata.Add("MessageStreamUID", Guid.NewGuid().ToString());
                    }
                    else
                    {
                        // for not init message message stream uid must be filled
                        if (!string.IsNullOrEmpty(dto.MessageStreamUID))
                            txdto.Metadata.Add("MessageStreamUID", dto.MessageStreamUID.ToString());
                        else
                            throw new Exception("Cannot send message - this is not set as init message but no MessageStreamUID was provided!");
                    }

                    var msg = string.Empty;
                    var prevmsgdata = string.Empty;
                    var prevmsg = string.Empty;
                    var prevtxid = string.Empty;

                    if (!dto.InitMessage)
                    {
                        var pretoken = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, TokenId, dto.TokenTxId);

                        if (string.IsNullOrEmpty(pretoken.TxId))
                            throw new Exception("Cannot send message - Cannot find previous message token metadata!");

                        if (pretoken.MetadataAvailable)
                        {
                            if (pretoken.Metadata.TryGetValue("PrevTxId", out var prevtxidfrommeta))
                            {
                                prevtxid = prevtxidfrommeta;
                            }

                            IToken prepretoken = null;
                            if (pretoken.Metadata.TryGetValue("InitMessage", out var prevmsginit))
                            {
                                if (prevmsginit != "true")
                                {
                                    prepretoken = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, TokenId, prevtxid);

                                    if (string.IsNullOrEmpty(prepretoken.TxId))
                                        throw new Exception("Cannot send message - Cannot find pre-previous message token metadata and previous message is not init message!");
                                }
                            }

                            if (pretoken.Metadata.TryGetValue("MessageData", out var prevmsgdatafromMeta))
                            {
                                prevmsgdata = prevmsgdatafromMeta;

                                if (prepretoken != null)
                                {
                                    if (prepretoken.MetadataAvailable)
                                    {
                                        if (prepretoken.Metadata.TryGetValue("SenderPubKey", out var preprevtokenReceiverKey))
                                        {
                                            var accEncKey = account.AccountKeys.FirstOrDefault(k => k.PublicKey == preprevtokenReceiverKey);
                                            if (accEncKey != null)
                                            {
                                                try
                                                {
                                                    // dencrypt my prevoious message just when I know that it will be encrypt before send
                                                    // if this will not happen, the prevmsgdata will contain original data from token metadata which can be encrypted
                                                    if (pretoken.Metadata.ContainsKey("SenderPubKey"))
                                                    {
                                                        var decprevmsg = AsymmetricProvider.DecryptString(prevmsgdata, accEncKey.GetEncryptedKey(dto.Password));

                                                        prevmsgdata = decprevmsg;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception("Cannot send message - Cannot decrypt prevouis message, requeired keuy is not correct format!");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // if sender sent in previous message pubkey he wants to encrypt the message before sending
                            if (pretoken.Metadata.TryGetValue("SenderPubKey", out var receiverpubkey))
                            {
                                if (!string.IsNullOrEmpty(receiverpubkey))
                                {
                                    try
                                    {
                                        // encrypt new message
                                        msg = AsymmetricProvider.EncryptString(dto.Message, receiverpubkey);
                                        // encrypt prevoius message
                                        prevmsg = AsymmetricProvider.EncryptString(prevmsgdata, receiverpubkey);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Cannot send message - Cannot encrypt the message, probably wrong Key");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Cannot send message - Required key is not correct format!");
                                }

                            }
                            else
                            {
                                // if he dont want to encrypt message just copy
                                msg = dto.Message;
                                // if he dont want encrypted message copy previous message too
                                // but if that message was encrypted before witout his new encryption request it will not be provided in decrypted form and he cannot read it
                                // this is because i dont want to publish my encrypted previous message if this transfer will not be encrypted
                                prevmsg = prevmsgdata;
                            }
                        }
                    }

                    var senderPUB = string.Empty;
                    if (dto.Encrypt)
                    {
                        if (!string.IsNullOrEmpty(dto.KeyId))
                        {
                            var key = account.AccountKeys.FirstOrDefault(k => k.Id.ToString() == dto.KeyId);
                            if(key != null)
                            {
                                senderPUB = key.PublicKey;
                                // if encryption is required token metadata will contains sender pub key
                                txdto.Metadata.Add("SenderPubKey", senderPUB);
                            }
                        }
                    }

                    if (msg == null)
                        msg = string.Empty;

                    if (dto.InitMessage)
                    {
                        txdto.Metadata.Add("InitMessage", "true");
                        msg = dto.Message;
                    }
                    else
                    {
                        txdto.Metadata.Add("InitMessage", "false");
                    }

                    txdto.Metadata.Add("PrevTxId", dto.TokenTxId);
                    txdto.Metadata.Add("PreviousMessage", prevmsg);
                    txdto.Metadata.Add("MessageData", msg);

                    var res = await NeblioTransactionHelpers.SendNTP1TokenAPI(txdto, 50000);

                    return res;
                }
                else
                {
                    throw new Exception("Cannot send message - Cannot find the Account!");
                }
            }
            else
            {
                throw new Exception("Cannot send message - Cannot find the wallet!");
            }

        }

        public static async Task<DecryptedMessageResponseDto> DecryptMessage(GetDecryptedMessageDto data)
        {
            try
            {
                if (EconomyMainContext.Wallets.TryGetValue(data.walletId, out var wallet))
                {
                    if (string.IsNullOrEmpty(data.accountAddress))
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot decrypt message, account address cannot be empty!");

                    if (wallet.Accounts.TryGetValue(data.accountAddress, out var account))
                    {
                        var msgtok = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, EconomyMainContext.MessagingToken.Id, data.TxId);

                        if (string.IsNullOrEmpty(msgtok.TxId))
                            throw new Exception("Cannot decrypt message - Cannot find message token data by provided TxId!");

                        EncryptionKey key = null;

                        IToken prepretoken = null;
                        if (msgtok.Metadata.TryGetValue("InitMessage", out var prevmsginit))
                        {
                            if (prevmsginit != "true")
                            {
                                if (msgtok.Metadata.TryGetValue("PrevTxId", out var prevtxid))
                                {
                                    prepretoken = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, TokenId, prevtxid);

                                    if (string.IsNullOrEmpty(prepretoken.TxId))
                                        throw new Exception("Cannot send message - Cannot find pre-previous message token metadata and previous message is not init message!");

                                }
                                else
                                {
                                    throw new Exception("Cannot send message - Cannot find pre-previous message token metadata and previous message is not init message!");
                                }
                            }
                        }

                        if (msgtok.Metadata.TryGetValue("MessageData", out var prevmsgdatafromMeta))
                        {
                            if (prepretoken != null)
                            {
                                if (prepretoken.MetadataAvailable)
                                {
                                    if (prepretoken.Metadata.TryGetValue("SenderPubKey", out var preprevtokenReceiverKey))
                                    {
                                        var accEncKey = account.AccountKeys.FirstOrDefault(k => k.PublicKey == preprevtokenReceiverKey);
                                        if (accEncKey != null)
                                        {
                                            key = accEncKey;
                                        }
                                    }
                                }
                            }
                        }

                        var resp = new DecryptedMessageResponseDto();

                        if (key != null)
                        {
                            if (msgtok.MetadataAvailable)
                            {
                                if (msgtok.Metadata.TryGetValue("PreviousMessage", out var prevmsg))
                                {
                                    try
                                    {
                                        resp.PrevMsg = AsymmetricProvider.DecryptString(prevmsg, key.GetEncryptedKey(data.Password));
                                    }
                                    catch(Exception ex)
                                    {
                                        ;//todo
                                    }
                                }
                                if (msgtok.Metadata.TryGetValue("MessageData", out var newvmsg))
                                {
                                    try
                                    {
                                        resp.NewMsg = AsymmetricProvider.DecryptString(newvmsg, key.GetEncryptedKey(data.Password));
                                    }
                                    catch(Exception ex)
                                    {
                                        ;//todo
                                    }
                                }
                            }

                            return resp;
                        }
                        else
                        {
                            throw new Exception("Cannot decrypt message - Key does not exists!");
                        }
                    }
                    else
                    {
                        throw new HttpResponseException((HttpStatusCode)501, $"Cannot decrypt message, Account Not Found!");
                    }
                }
                else
                {
                    throw new HttpResponseException((HttpStatusCode)501, $"Cannot decrypt message, Wallet Not Found!");
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot decrypt message!", ex);
                throw new HttpResponseException((HttpStatusCode)501, $"Cannot decrypt messagen!");
            }
        }
    }
}
