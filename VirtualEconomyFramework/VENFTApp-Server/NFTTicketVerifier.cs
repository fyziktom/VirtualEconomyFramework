using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;
using VEDriversLite.Security;

namespace VENFTApp_Server
{
    public class VerifyNFTTicketDto
    {
        public TicketNFT NFT { get; set; } = new TicketNFT("");
        public bool IsSignatureValid { get; set; } = false;
        public bool IsUsedOnSameAddress { get; set; } = false;
        public bool IsMintedByAllowedAddress { get; set; } = false;
        public string MintAddress { get; set; } = string.Empty;
        public string TxId { get; set; } = string.Empty;
        public string OwnerAddress { get; set; } = string.Empty;
        public PubKey OwnerPubKey { get; set; }

    }
    public static class NFTTicketVerifier
    {
        public static async Task<VerifyNFTTicketDto> LoadNFTTicketToVerify(OwnershipVerificationCodeDto dto, string eventId, List<string> allowedMintingAddresses)
        {
            if (string.IsNullOrEmpty(dto.TxId))
                throw new Exception("Utxo id must be provided.");
            if (string.IsNullOrEmpty(dto.Signature))
                throw new Exception("Signature id must be provided.");
            if (string.IsNullOrEmpty(eventId))
                throw new Exception("Event Id must be provided.");
            if (allowedMintingAddresses == null || allowedMintingAddresses.Count == 0)
                throw new Exception("You must provide list of allowed minting addresses.");

            var msg = CreateMessage(dto.TxId); // create verification message ASAP because of time relation

            var txi = await NeblioTransactionHelpers.GetTransactionInfo(dto.TxId);

            var Time = TimeHelpers.UnixTimestampToDateTime((double)txi.Blocktime);
            bool isYesterdayOrOlder = DateTime.Today - Time.Date >= TimeSpan.FromDays(1);
            if (isYesterdayOrOlder)
                throw new Exception("This ticket was used earlier than today. Or it is not used at all.");

            var metadata = await NeblioTransactionHelpers.GetTransactionMetadata(NFTHelpers.TokenId, dto.TxId);
            if (metadata.TryGetValue("NFT", out var isnft))
                if (isnft != "true")
                    throw new Exception("This is not NFT");
            if (metadata.TryGetValue("Type", out var type))
                if (type != "NFT Ticket")
                    throw new Exception("This is not NFT Ticket");
            if (metadata.TryGetValue("EventId", out var nfteventId))
                if (nfteventId != eventId)
                    throw new Exception("Event Id on the ticket does not match the requested.");
            if (metadata.TryGetValue("Used", out var used))
            {
                if (used != "true")
                    throw new Exception("This NFT Ticket is not used.");
            }
            else
            {
                throw new Exception("This NFT Ticket is not used.");
            }

            var tx = Transaction.Parse(txi.Hex, NeblioTransactionHelpers.Network);

            var outDto = new VerifyNFTTicketDto();

            if (tx != null && tx.Outputs.Count > 0 && tx.Inputs.Count > 0)
            {
                var outp = tx.Outputs[0];
                var inpt = tx.Inputs[0];
                if (outp != null && inpt != null)
                {
                    var scr = outp.ScriptPubKey;
                    var add = scr.GetDestinationAddress(NeblioTransactionHelpers.Network);
                    var addi = inpt.ScriptSig.GetSignerAddress(NeblioTransactionHelpers.Network);
                    if (add != addi)
                        throw new Exception("This ticket was not used on this address.");
                    var pubkey = inpt.ScriptSig.GetAllPubKeys().FirstOrDefault();
                    if (pubkey == null)
                        throw new Exception("Cannot Load the owner Public Key.");

                    // verify of the signature of the NFT
                    var verres = await ECDSAProvider.VerifyMessage(msg, dto.Signature, pubkey);
                    //var vmsg = await ECDSAProvider.VerifyMessage(msg, dto.Signature, pubkey);
                    if (!verres.Item1)
                        throw new Exception("Signature of the NFT is not valid.");

                    // check if the NFT is still as utxo on the address
                    var utxos = await NeblioTransactionHelpers.GetAddressUtxosObjects(add.ToString());
                    if (utxos.FirstOrDefault(u => (u.Txid == dto.TxId && u.Value == 10000 && u.Tokens.Count > 0 && u.Tokens.FirstOrDefault()?.Amount == 1)) == null)
                        throw new Exception("This ticket is not available on the address as spendable.");

                    // check if in previous transaction the ticket was unused
                    var prevmeta = await NeblioTransactionHelpers.GetTransactionMetadata(NFTHelpers.TokenId, inpt.PrevOut.Hash.ToString());
                    if (prevmeta.TryGetValue("NFT", out var isprevnft))
                        if (isprevnft != "true")
                            throw new Exception("This is not NFT");
                    if (prevmeta.TryGetValue("Type", out var prevtype))
                        if (prevtype != "NFT Ticket")
                            throw new Exception("This is not NFT Ticket");
                    if (prevmeta.TryGetValue("EventId", out var prevnfteventId))
                        if (prevnfteventId != eventId)
                            throw new Exception("Event Id on the ticket does not match the requested.");
                    if (prevmeta.TryGetValue("Used", out var prevused))
                        if (prevused == "true")
                            throw new Exception("This NFT Ticket was already used in previous transaction.");

                    // todo track origin for check minting address
                    var res = await VerifyNFTTicketOrigin(inpt.PrevOut.Hash.ToString(), eventId, allowedMintingAddresses);
                    if (!res.Item1)
                        throw new Exception($"Ticket was not minted on allowed address. The origin is from: {res.Item2.Item2}");
                    outDto.IsMintedByAllowedAddress = true;
                    outDto.MintAddress = res.Item2.Item2;

                    var nft = new TicketNFT("");
                    await nft.LoadLastData(metadata); // fill with already loaded the newest NFT metadata
                    nft.Time = Time;
                    outDto.IsSignatureValid = true;
                    outDto.NFT = nft;
                    outDto.OwnerAddress = add.ToString();
                    outDto.IsUsedOnSameAddress = true;
                    outDto.OwnerPubKey = pubkey;
                    outDto.TxId = dto.TxId;
                }
            }

            return outDto;
        }

        public static async Task<(bool, (string, string))> VerifyNFTTicketOrigin(string txid, string eventId, List<string> allowedMintingAddresses)
        {
            var found = false;
            var intxid = txid;
            var mintingAddress = string.Empty;
            while (!found)
            {
                var info = await NeblioTransactionHelpers.GetTransactionInfo(intxid);

                if (info != null && info.Vin != null && info.Vin.Count > 0)
                {
                    var vin = info.Vin.ToArray()?[0];
                    if (vin.Tokens != null && vin.Tokens.Count > 0)
                    {
                        var vintok = vin.Tokens.ToArray()?[0];
                        if (vintok != null)
                        {
                            var meta = await NeblioTransactionHelpers.GetTransactionMetadata(NFTHelpers.TokenId, intxid);
                            if (meta.TryGetValue("NFT", out var isnft))
                                if (isnft != "true")
                                    throw new Exception("This is not NFT");
                            if (meta.TryGetValue("Type", out var type))
                                if (type != "NFT Ticket")
                                    throw new Exception("This is not NFT Ticket");
                            if (meta.TryGetValue("EventId", out var nfteventId))
                                if (nfteventId != eventId)
                                    throw new Exception($"Event Id on the ticket does not match the requested. Found during search for origin. Explored TxId {intxid}.");
                            if (meta.TryGetValue("Used", out var used))
                                if (used == "true")
                                    throw new Exception($"This NFT Ticket is already used. Found during search for origin. Used TxId {intxid}.");

                            if (vintok.Amount > 1)
                            {
                                mintingAddress = vin.PreviousOutput.Addresses.ToArray()?[0];
                                if (!string.IsNullOrEmpty(mintingAddress))
                                    if (allowedMintingAddresses.Contains(mintingAddress))
                                        return (true, (intxid, mintingAddress));
                                found = true;
                            }
                            else if (vintok.Amount == 1)
                                intxid = vin.Txid;
                        }
                    }
                }
            }

            return (false, (intxid, mintingAddress));
        }

        public static string CreateMessage(string txid)
        {
            var time = DateTime.UtcNow.ToString("dd MM yyyy hh:mm");
            var combo = txid + time;//SecurityUtils.ComputeSha256Hash(txid + time);
           // var msg = String.Format("{0:X}", combo.GetHashCode());
            return combo;
        }
    }
}
