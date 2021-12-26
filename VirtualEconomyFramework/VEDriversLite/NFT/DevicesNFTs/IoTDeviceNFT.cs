using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Devices;
using VEDriversLite.Devices.Dto;
using VEDriversLite.Security;

namespace VEDriversLite.NFT.DevicesNFTs
{
    public class IoTDeviceNFT : CommonNFT
    {
        public IoTDeviceNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.IoTDevice;
            TypeText = "NFT IoTDevice";
        }

        public string DeviceNFTHash { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string IoTDataDriverTypeText { get; set; } = string.Empty;
        [JsonIgnore]
        public IoTDataDriverType IoTDDType { get; set; } = IoTDataDriverType.HARDWARIO;
        public IoTDataDriverSettings IDDSettings { get; set; } = new IoTDataDriverSettings();
        public bool AutoActivation { get; set; } = false;
        public bool EncryptSetting { get; set; } = false;
        public bool EncryptMessages { get; set; } = false;

        public string Location { get; set; } = string.Empty;
        public string LocationCoordinates { get; set; } = string.Empty;
        public double LocationCoordinatesLat { get; set; } = 0.0;
        public double LocationCoordinatesLen { get; set; } = 0.0;

        [JsonIgnore]
        public bool RunJustOwn { get; set; } = false;
        [JsonIgnore]
        public bool AllowConsoleOutputWhenNewMessage { get; set; } = false;
        [JsonIgnore]
        public bool Active { get; set; } = false;
        [JsonIgnore]
        public bool DecryptedSetting { get; set; } = false;
        [JsonIgnore]
        public Dictionary<string, INFT> MessageNFTs { get; set; } = new Dictionary<string, INFT>();
        [JsonIgnore]
        public INFT SourceDeviceNFT { get; set; } = new DeviceNFT("");
        [JsonIgnore]
        public string EncryptedSettingString { get; set; } = string.Empty;
        public string EncryptedLocationString { get; set; } = string.Empty;
        public string EncryptedLocationCoordinatesString { get; set; } = string.Empty;

        public event EventHandler<INFT> NewMessage;

        [JsonIgnore]
        public IIoTDataDriver IoTDataDriver { get; set; }

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as IoTDeviceNFT;
            DeviceNFTHash = nft.DeviceNFTHash;
            Address = nft.Address;
            IoTDataDriverTypeText = nft.IoTDataDriverTypeText;
            IoTDDType = nft.IoTDDType;
            IDDSettings = nft.IDDSettings;
            AutoActivation = nft.AutoActivation;
            Active = nft.Active;
            MessageNFTs = nft.MessageNFTs;
            SourceDeviceNFT = nft.SourceDeviceNFT;
            IoTDataDriver = nft.IoTDataDriver;
            
            Location = nft.Location;
            LocationCoordinates = nft.LocationCoordinates;
            LocationCoordinatesLat = nft.LocationCoordinatesLat;
            LocationCoordinatesLen = nft.LocationCoordinatesLen;

            EncryptMessages = nft.EncryptMessages;
            EncryptSetting = nft.EncryptSetting;
            DecryptedSetting = nft.DecryptedSetting;
            EncryptedSettingString = nft.EncryptedSettingString;
            EncryptedLocationString = nft.EncryptedLocationString;
            EncryptedLocationCoordinatesString = nft.EncryptedLocationCoordinatesString;

        }
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("EncryptSetting", out var encs))
            {
                if (bool.TryParse(encs, out bool bencs))
                    EncryptSetting = bencs;
                else
                    EncryptSetting = false;
            }
            else
            {
                EncryptSetting = false;
            }
            if (metadata.TryGetValue("DeviceNFT", out var dh))
                DeviceNFTHash = dh;
            if (metadata.TryGetValue("Address", out var add))
                Address = add;
            if (metadata.TryGetValue("Location", out var location))
            {
                if (!EncryptSetting)
                {
                    Location = location;
                }
                else
                {
                    EncryptedLocationString = location;
                }
            }
            if (metadata.TryGetValue("LocationC", out var loc))
            {
                if (!EncryptSetting)
                {
                    LocationCoordinates = loc;
                    var split = loc.Split(',');
                    if (split.Length > 1)
                    {
                        try
                        {
                            LocationCoordinatesLat = Convert.ToDouble(split[0], CultureInfo.InvariantCulture);
                            LocationCoordinatesLen = Convert.ToDouble(split[1], CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot parse location coordinates in IoTDeviceNFT {Utxo}.");
                        }
                    }
                }
                else
                {
                    EncryptedLocationCoordinatesString = loc;
                }
            }
            if (metadata.TryGetValue("IoTDataDriverType", out var iotddt))
            {
                IoTDataDriverTypeText = iotddt;
                try
                {
                    IoTDDType = (IoTDataDriverType)Enum.Parse(typeof(IoTDataDriverType), iotddt);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Cannot parse enum type of the IoTDataDriverType in NFT: {Utxo}, exception: {ex.Message} ");
                }
            }

            if (metadata.TryGetValue("IDDSettings", out var idd))
            {
                if(!string.IsNullOrEmpty(idd))
                {
                    if (!EncryptSetting)
                    {
                        try
                        {
                            IDDSettings = JsonConvert.DeserializeObject<IoTDataDriverSettings>(idd);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot deserialize the IDDSetting in NFT: {Utxo}, exception: {ex.Message}");
                            IDDSettings = new IoTDataDriverSettings();
                        }
                    }
                    else
                    {
                        EncryptedSettingString = idd;
                    }
                }
                else
                {
                    IDDSettings = new IoTDataDriverSettings();
                }
            }
            if (metadata.TryGetValue("AutoActivation", out var aa))
            {
                if (bool.TryParse(aa, out bool baa))
                    AutoActivation = baa;
                else
                    AutoActivation = false;
            }
            else
            {
                AutoActivation = false;
            }
            if (metadata.TryGetValue("EncryptMessages", out var enc))
            {
                if (bool.TryParse(enc, out bool benc))
                    EncryptMessages = benc;
                else
                    EncryptMessages = false;
            }
            else
            {
                EncryptMessages = false;
            }
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);
                ParsePrice(lastmetadata);
                await ParseDogeftInfo(lastmetadata);
                ParseSoldInfo(lastmetadata);
                ParseSpecific(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
                IsLoaded = true;
            }
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);
                ParsePrice(nftData.NFTMetadata);
                ParseSpecific(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            IsLoaded = true;
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();

            if (!string.IsNullOrEmpty(DeviceNFTHash))
                metadata.Add("DeviceNFT", DeviceNFTHash);
            if (!string.IsNullOrEmpty(Address))
                metadata.Add("Address", Address);


            metadata.Add("IoTDataDriverType", Enum.GetName(typeof(IoTDataDriverType), IoTDDType));

            if (IDDSettings != null)
            {
                if (!EncryptSetting)
                {
                    metadata.Add("IDDSettings", JsonConvert.SerializeObject(IDDSettings));

                    if (!string.IsNullOrEmpty(Location))
                        metadata.Add("Location", Location);
                    if (!string.IsNullOrEmpty(LocationCoordinates))
                        metadata.Add("LocationC", LocationCoordinates);
                }
                else
                {
                    var res = await ECDSAProvider.EncryptStringWithSharedSecret(JsonConvert.SerializeObject(IDDSettings), receiver, key);
                    if (res.Item1)
                    {
                        metadata.Add("IDDSettings", res.Item2);
                        EncryptedSettingString = res.Item2;
                    }

                    if (!string.IsNullOrEmpty(Location))
                    {
                        var resl = await ECDSAProvider.EncryptStringWithSharedSecret(Location, receiver, key);
                        if (resl.Item1)
                        {
                            metadata.Add("Location", resl.Item2);
                            EncryptedLocationString = resl.Item2;
                        }
                    }
                    if (!string.IsNullOrEmpty(LocationCoordinates))
                    {
                        var resl = await ECDSAProvider.EncryptStringWithSharedSecret(LocationCoordinates, receiver, key);
                        if (resl.Item1)
                        {
                            metadata.Add("LocationC", resl.Item2);
                            EncryptedLocationCoordinatesString = resl.Item2;
                        }
                    }
                }
            }
            if (AutoActivation)
                metadata.Add("AutoActivation", "true");
            if (EncryptMessages)
                metadata.Add("EncryptMessages", "true");
            if (EncryptSetting)
                metadata.Add("EncryptSetting", "true");

            return metadata;
        }

        public async Task LoadDeviceNFT()
        {
            if (!string.IsNullOrEmpty(DeviceNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", DeviceNFTHash, 0, 0, true, true, NFTTypes.Device);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        SourceDeviceNFT = nft;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Cannot load the Device NFT: {DeviceNFTHash} in IoTDeviceNFT: {Utxo}, exception: {ex.Message}");
                }
            }
        }

        public async Task InitCommunication(BitcoinSecret secret = null)
        {
            try
            {
                if (RunJustOwn)
                {
                    var sender = await NeblioTransactionHelpers.GetTransactionSender(Utxo, TxDetails);
                    var receiver = await NeblioTransactionHelpers.GetTransactionReceiver(Utxo, TxDetails);
                    if (sender != receiver)
                        throw new Exception("This NFT is not allowed to run on this address. The Sender and Receiver of the tx must be the same!");
                }
                IoTDataDriver = await IoTDataDriverFactory.GetIoTDataDriver(IoTDDType);
                if (IoTDataDriver != null)
                {
                    IoTDataDriver.NewDataReceived += IoTDataDriver_NewDataReceived;
                    if (EncryptSetting)
                    {
                        if (secret == null)
                        {
                            Console.WriteLine($"Please fill the BitcoinSecret for decryption of the connection setting in the IoTDeviceNFT {Utxo}.");
                            return;
                        }
                        try
                        {
                            var d = await ECDSAProvider.DecryptStringWithSharedSecret(EncryptedSettingString, Address, secret);
                            if (d.Item1)
                                IDDSettings = JsonConvert.DeserializeObject<IoTDataDriverSettings>(d.Item2);
                            if (!string.IsNullOrEmpty(EncryptedLocationString))
                            {
                                var l = await ECDSAProvider.DecryptStringWithSharedSecret(EncryptedLocationString, Address, secret);
                                if (l.Item1)
                                    Location = l.Item2;
                            }
                            if (!string.IsNullOrEmpty(EncryptedLocationCoordinatesString))
                            {
                                var lc = await ECDSAProvider.DecryptStringWithSharedSecret(EncryptedLocationCoordinatesString, Address, secret);
                                if (lc.Item1)
                                {
                                    LocationCoordinates = lc.Item2;
                                    var split = lc.Item2.Split(',');
                                    if (split.Length > 1)
                                    {
                                        try
                                        {
                                            LocationCoordinatesLat = Convert.ToDouble(split[0], CultureInfo.InvariantCulture);
                                            LocationCoordinatesLen = Convert.ToDouble(split[1], CultureInfo.InvariantCulture);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Cannot parse location coordinates in IoTDeviceNFT {Utxo}.");
                                        }
                                    }
                                }
                            }
                            DecryptedSetting = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot decrypt the setting in IoTDeviceNFT {Utxo}, exception {ex.Message}");
                            return;
                        }

                    }
                    await IoTDataDriver.SetConnParams(IDDSettings.ConnectionParams);
                    await IoTDataDriver.Init(null);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize the IoT Driver Setting, when initializing the new IoTDataDrvier.");
            }
        }

        private void IoTDataDriver_NewDataReceived(object sender, string e)
        {
            if(AllowConsoleOutputWhenNewMessage)
                Console.WriteLine($"IoT-{DateTime.UtcNow.ToString("MM-DD-yyyy-hh-mm-ss")}: New IoT message is: " + e);
            var msg = new MessageNFT("");
            msg.Author = $"{Utxo}:{UtxoIndex}";
            msg.Name = "IoTDeviceMessage";
            msg.Description = e;
            msg.Encrypt = EncryptMessages;
            MessageNFTs.Add("ToProcess-" + DateTime.UtcNow.ToString("dd-MM-yyyy-hh-mm-ss-ff"), msg);
            NewMessage?.Invoke(this, msg);
        }

        public async Task DeInitCommunication()
        {
            if (IoTDataDriver != null)
            {
                IoTDataDriver.NewDataReceived -= IoTDataDriver_NewDataReceived;
                await IoTDataDriver.DeInit();
            }
        }
    }
}
