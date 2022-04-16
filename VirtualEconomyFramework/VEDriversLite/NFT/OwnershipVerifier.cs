using Newtonsoft.Json;
//using QRCoder;
using System;
using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Security;
//using ZXing;
//using ZXing.Common;
//using ZXing.QrCode;

namespace VEDriversLite.NFT
{
    public class OwnershipVerificationCodeDto
    {
        public string TxId { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }
    public class OwnershipVerificationResult
    {
        public string TxId { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public bool IsValid { get; set; } = false;
        public INFT NFT { get; set; } = new ImageNFT("");
        public string Message { get; set; } = string.Empty;
        public string VerifyResult { get; set; } = string.Empty;
    }
    public static class OwnershipVerifier
    {
        //private static QRCodeGenerator qrGenerator = new QRCodeGenerator();

        public static string CreateMessage(string txid)
        {
            var time = DateTime.UtcNow.ToString("dd MM yyyy hh:mm");
            var combo = txid + time;//SecurityUtils.ComputeSha256Hash(txid + time);
            var msg = String.Format("{0:X}", combo.GetHashCode());
            return combo;
        }
        public static async Task<(bool, string)> GetCode(string txid, EncryptionKey key)
        {
            var msg = CreateMessage(txid);
            var signed = await ECDSAProvider.SignMessage(msg, key.GetEncryptedKey());
            return signed;
        }
        public static async Task<(bool, string)> GetCode(string txid, NBitcoin.BitcoinSecret key)
        {
            var msg = CreateMessage(txid);
            var signed = await ECDSAProvider.SignMessage(msg, key);
            return signed;
        }
        public static async Task<OwnershipVerificationCodeDto> GetCodeInDto(string txid, NBitcoin.BitcoinSecret key)
        {
            var dto = new OwnershipVerificationCodeDto();
            dto.TxId = txid;
            dto.Signature = (await GetCode(txid, key)).Item2;
            return dto;
        }

        public static async Task<OwnershipVerificationCodeDto> GetCodeInDto(string txid, EncryptionKey key)
        {
            var dto = new OwnershipVerificationCodeDto();
            dto.TxId = txid;
            dto.Signature = (await GetCode(txid, key)).Item2;
            return dto;
        }
        public static async Task<(bool, (OwnershipVerificationCodeDto, byte[]))> GetQRCode(string txid, NBitcoin.BitcoinSecret key)
        {
            var signature = await GetCode(txid, key);
            if (signature.Item1)
            {
                var dto = new OwnershipVerificationCodeDto();
                dto.TxId = txid;
                dto.Signature = signature.Item2;
                var dtos = JsonConvert.SerializeObject(dto);
                if (dtos != null)
                {
                    //QRCodeData qrCodeData = qrGenerator.CreateQrCode(dtos, QRCodeGenerator.ECCLevel.Q);
                    //var qrCode = new BitmapByteQRCode(qrCodeData);
                    //var g = qrCode.GetGraphic(20);

                    return (true,(dto, new byte[0]));
                }
            }
            return (false, (null, null));
        }

        /*
        public static async Task<(bool,(OwnershipVerificationCodeDto,byte[]))> GetQRCode(string txid, EncryptionKey key)
        {
            var signature = await GetCode(txid, key);
            if (signature.Item1)
            {
                var dto = new OwnershipVerificationCodeDto();
                dto.TxId = txid;
                dto.Signature = signature.Item2;
                var dtos = JsonConvert.SerializeObject(dto);
                if (dtos != null)
                {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(dtos, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new BitmapByteQRCode(qrCodeData);
                    var g = qrCode.GetGraphic(20);
                    return (true, (dto, g));
                }
            }
            return (false, (null, null));
        }
        */

        public static async Task<OwnershipVerificationResult> VerifyOwner(OwnershipVerificationCodeDto dto)
        {
            var msg = CreateMessage(dto.TxId);
            var res = await NFTHelpers.GetNFTWithOwner(dto.TxId);
            if (res.Item1)
            {
                //var ownerpubkey = await NFTHelpers.GetPubKeyFromProfileNFTTx(res.Item2.Owner);
                /*var ownerpubkey = await NFTHelpers.GetPubKeyFromLastFoundTx(res.Item2.Owner);
                if (!ownerpubkey.Item1)
                    return new OwnershipVerificationResult()
                    {
                        Owner = res.Item2.Owner,
                        Sender = res.Item2.Sender,
                        NFT = res.Item2.NFT,
                        Message = msg,
                        VerifyResult = "Cannot get owner pubkey. He probably did not allow this function."
                    };*/

                var r = await ECDSAProvider.VerifyMessage(msg, dto.Signature, res.Item2.Owner);
                if (r.Item1)
                {
                    return new OwnershipVerificationResult()
                    {
                        Owner = res.Item2.Owner,
                        Sender = res.Item2.Sender,
                        NFT = res.Item2.NFT,
                        Message = msg,
                        VerifyResult = "Verified. This is owner. Signature is valid."
                    };
                }
                else
                {
                    if (r.Item2 != null)
                        return new OwnershipVerificationResult()
                        {
                            Owner = res.Item2.Owner,
                            Sender = res.Item2.Sender,
                            NFT = res.Item2.NFT,
                            Message = msg,
                            VerifyResult = "Not Owner of NFT or provided signature is no longer valid."
                        };
                }
            }
            return new OwnershipVerificationResult() { VerifyResult = "Not Valid input." };
        }
        private static Object thisLock = new Object();
        /*
        public static async Task<OwnershipVerificationResult> VerifyFromImage(Image image)
        {
            //stream.Seek(0, SeekOrigin.Begin);
            //var bytes = stream.ToArray();
            //var img = new Bitmap(stream);
            
            //var img = ByteArrayToImage(image);
            //https://stackoverflow.com/questions/56738058/c-sharp-using-zxing-net-to-decode-png-qr-code
            // important step copy BitmapLuminanceSource to project. it is not part of ZXing.NET
            var source = new BitmapLuminanceSource(new Bitmap(image));
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));
            Result result = new MultiFormatReader().decode(binaryBitmap);

            var r = JsonConvert.DeserializeObject<OwnershipVerificationCodeDto>(result.Text);
            var msg = CreateMessage(r.TxId);
            var res = await NFTHelpers.GetNFTWithOwner(r.TxId);
            //var ownerpubkey = await NFTHelpers.GetPubKeyFromProfileNFTTx(res.Item2.Owner);
            var ownerpubkey = await NFTHelpers.GetPubKeyFromLastFoundTx(res.Item2.Owner);
            if (!ownerpubkey.Item1)
                return new OwnershipVerificationResult()
                    {
                        Owner = res.Item2.Owner,
                        Sender = res.Item2.Sender,
                        NFT = res.Item2.NFT,
                        Message = msg,
                        VerifyResult = "Cannot get owner pubkey. He probably didn activate the function."
                    };

            
            var vmsg = await ECDSAProvider.VerifyMessage(msg, r.Signature, ownerpubkey.Item2);

            if (vmsg.Item1)
            {
                return new OwnershipVerificationResult()
                {
                    Owner = res.Item2.Owner,
                    Sender = res.Item2.Sender,
                    NFT = res.Item2.NFT,
                    Message = msg,
                    VerifyResult = "Verified. This is owner. Signature is valid."
                };
            }
            else
            {
                if (vmsg.Item2 != null)
                    return new OwnershipVerificationResult()
                    {
                        Owner = res.Item2.Owner,
                        Sender = res.Item2.Sender,
                        NFT = res.Item2.NFT,
                        Message = msg,
                        VerifyResult = "Not Owner of NFT or provided signature is no longer valid."
                    };
            }
            return null;
        }
        */
        /*
        private static byte[] ToByteArray(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using (var ms = new MemoryStream(byteArrayIn))
                return Image.FromStream(ms);
        }

        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
        */
    }
}
