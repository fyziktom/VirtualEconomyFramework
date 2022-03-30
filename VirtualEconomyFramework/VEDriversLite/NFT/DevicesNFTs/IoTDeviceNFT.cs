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
    /// <summary>
    /// IoT Device NFT - can load the data from the API and resend them as NFT IoTMessages
    /// </summary>
    public class IoTDeviceNFT : CommonNFT
    {
        /// <summary>
        /// Constructor of the empty NFT IoTDevice
        /// </summary>
        /// <param name="utxo"></param>
        public IoTDeviceNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.IoTDevice;
            TypeText = "NFT IoTDevice";
        }

        private static object _lock = new object();
        /// <summary>
        /// Related NFT Device hash
        /// </summary>
        public string DeviceNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// Address of the owner of the NFT
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Address where should NFT IoTDevice send the NFT IoTMessages when some new is available
        /// </summary>
        public string ReceivingMessageAddress { get; set; } = string.Empty;
        /// <summary>
        /// Type of the driver as text
        /// </summary>
        public string IoTDataDriverTypeText { get; set; } = string.Empty;
        /// <summary>
        /// Type of the driver
        /// </summary>
        [JsonIgnore]
        public IoTDataDriverType IoTDDType { get; set; } = IoTDataDriverType.HARDWARIO;
        /// <summary>
        /// Loaded IoT Data Driver Settings with parameters for the connection to the API
        /// </summary>
        public IoTDataDriverSettings IDDSettings { get; set; } = new IoTDataDriverSettings();
        /// <summary>
        /// Activate automatically the NFT when it is loaded on some address
        /// </summary>
        public bool AutoActivation { get; set; } = false;
        /// <summary>
        /// Encrypted settings. It will encrypt the settings with use of the BitcoinSecret
        /// </summary>
        public bool EncryptSetting { get; set; } = false;
        /// <summary>
        /// Set if you want to encrypt all sent messages
        /// </summary>
        public bool EncryptMessages { get; set; } = false;

        /// <summary>
        /// Location name
        /// </summary>
        public string Location { get; set; } = string.Empty;
        /// <summary>
        /// Location coordinates string
        /// </summary>
        public string LocationCoordinates { get; set; } = string.Empty;
        /// <summary>
        /// Location coordinate Latitude
        /// </summary>
        public double LocationCoordinatesLat { get; set; } = 0.0;
        /// <summary>
        /// Location coordinate Longitude
        /// </summary>
        public double LocationCoordinatesLen { get; set; } = 0.0;

        /// <summary>
        /// Run just NFT which was minted on this address or resent to itself on same address
        /// </summary>
        [JsonIgnore]
        public bool RunJustOwn { get; set; } = false;
        /// <summary>
        /// Allow output to the console when new message will arrive
        /// </summary>
        [JsonIgnore]
        public bool AllowConsoleOutputWhenNewMessage { get; set; } = false;
        /// <summary>
        /// NFT IoTDevice is active and communicate with API
        /// </summary>
        [JsonIgnore]
        public bool Active { get; set; } = false;
        /// <summary>
        /// The settings is already decrypted if this is true
        /// </summary>
        [JsonIgnore]
        public bool DecryptedSetting { get; set; } = false;
        /// <summary>
        /// List of all processed messages or messages which should be processed
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, INFT> MessageNFTs { get; set; } = new Dictionary<string, INFT>();
        /// <summary>
        /// Related Device NFT
        /// </summary>
        [JsonIgnore]
        public INFT SourceDeviceNFT { get; set; } = new DeviceNFT("");
        /// <summary>
        /// Encrypted form of settings as serialized string
        /// </summary>
        [JsonIgnore]
        public string EncryptedSettingString { get; set; } = string.Empty;
        /// <summary>
        /// Encrypted form of the location
        /// </summary>
        public string EncryptedLocationString { get; set; } = string.Empty;
        /// <summary>
        /// Encrypted form of location coordinates
        /// </summary>
        public string EncryptedLocationCoordinatesString { get; set; } = string.Empty;

        /// <summary>
        /// New message arrived event
        /// </summary>
        public event EventHandler<(string,INFT)> NewMessage;

        /// <summary>
        /// IoT Data Driver
        /// </summary>
        [JsonIgnore]
        public IIoTDataDriver IoTDataDriver { get; set; }

        /// <summary>
        /// Fill the data of the NFT
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
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
            ReceivingMessageAddress = nft.ReceivingMessageAddress;

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

        /// <summary>
        /// Parse specific data for the NFT
        /// </summary>
        /// <param name="metadata"></param>
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
            if (metadata.TryGetValue("RcvMsgAdd", out var rmsg))
                ReceivingMessageAddress = rmsg;
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
                        catch
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

            //just for the test
            Console.WriteLine("Metadata:");
            foreach(var meta in metadata)
                Console.WriteLine($"\t{meta.Key}:{meta.Value}");

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
        /// <summary>
        /// Parse origin data of the NFT
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get last data for this NFT
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Get NFT metadata.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="key"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();

            if (!string.IsNullOrEmpty(DeviceNFTHash))
                metadata.Add("DeviceNFT", DeviceNFTHash);
            if (!string.IsNullOrEmpty(Address))
                metadata.Add("Address", Address);

            if (string.IsNullOrEmpty(receiver) && !string.IsNullOrEmpty(address))
                receiver = address;

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
                    try
                    {
                        var res = await ECDSAProvider.EncryptStringWithSharedSecret(JsonConvert.SerializeObject(IDDSettings), receiver, key);
                        if (res.Item1)
                        {
                            metadata.Add("IDDSettings", res.Item2);
                            EncryptedSettingString = res.Item2;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(EncryptedSettingString))
                                metadata.Add("IDDSettings", EncryptedSettingString);
                        }
                    }
                    catch
                    {
                        if (!string.IsNullOrEmpty(EncryptedSettingString))
                            metadata.Add("IDDSettings", EncryptedSettingString);
                    }

                    if (!string.IsNullOrEmpty(Location))
                    {
                        try
                        {
                            var resl = await ECDSAProvider.EncryptStringWithSharedSecret(Location, receiver, key);
                            if (resl.Item1)
                            {
                                metadata.Add("Location", resl.Item2);
                                EncryptedLocationString = resl.Item2;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(EncryptedLocationString))
                                    metadata.Add("Location", EncryptedLocationString);
                            }
                        }
                        catch
                        {
                            if (!string.IsNullOrEmpty(EncryptedLocationString))
                                metadata.Add("Location", EncryptedLocationString);
                        }
                    }
                    if (!string.IsNullOrEmpty(LocationCoordinates))
                    {
                        try
                        {
                            var resl = await ECDSAProvider.EncryptStringWithSharedSecret(LocationCoordinates, receiver, key);
                            if (resl.Item1)
                            {
                                metadata.Add("LocationC", resl.Item2);
                                EncryptedLocationCoordinatesString = resl.Item2;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(EncryptedLocationCoordinatesString))
                                    metadata.Add("LocationC", EncryptedLocationCoordinatesString);
                            }
                        }
                        catch
                        {
                            if (!string.IsNullOrEmpty(EncryptedLocationCoordinatesString))
                                metadata.Add("LocationC", EncryptedLocationCoordinatesString);
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(ReceivingMessageAddress))
                metadata.Add("RcvMsgAdd", ReceivingMessageAddress);
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
        /// <summary>
        /// Decrypt the IoT connection settings. You must provide the BitcoinSecret
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public async Task DecryptSetting(BitcoinSecret secret = null)
        {
            if (EncryptSetting && !DecryptedSetting)
            {
                if (secret == null)
                {
                    Console.WriteLine($"Please fill the BitcoinSecret for decryption of the connection setting in the IoTDeviceNFT {Utxo}.");
                    return;
                }
                try
                {
                    if (string.IsNullOrEmpty(Address))
                        Address = secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

                    var d = await ECDSAProvider.DecryptStringWithSharedSecret(EncryptedSettingString, Address, secret);
                    if (d.Item1)
                    {
                        IDDSettings = JsonConvert.DeserializeObject<IoTDataDriverSettings>(d.Item2);
                        DecryptedSetting = true;
                    }
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
                                catch
                                {
                                    Console.WriteLine($"Cannot parse location coordinates in IoTDeviceNFT {Utxo}.");
                                }
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot decrypt the setting in IoTDeviceNFT {Utxo}, exception {ex.Message}");
                    return;
                }

            }
        }
        /// <summary>
        /// Start the communication. If the settings is encrypted you must provide the bitcoin secret
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
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

                    await DecryptSetting(secret);

                    await IoTDataDriver.SetConnParams(IDDSettings.ConnectionParams);
                    await IoTDataDriver.Init(null);
                    Active = true;
                }
            }
            catch (Exception ex)
            {
                Active = false;
                Console.WriteLine("Cannot deserialize the IoT Driver Setting, when initializing the new IoTDataDrvier. " + ex.Message);
            }
        }

        private void IoTDataDriver_NewDataReceived(object sender, string e)
        {
            if(AllowConsoleOutputWhenNewMessage)
                Console.WriteLine($"IoT-{DateTime.UtcNow.ToString("MM-DD-yyyy-hh-mm-ss")}: New IoT message is: " + e);
            var msg = new IoTMessageNFT("");
            msg.Author = $"{Utxo}:{UtxoIndex}";
            msg.Name = "IoTDeviceMessage";
            msg.Description = e;
            msg.Encrypt = EncryptMessages;
            var mn = "ToProcess-" + DateTime.UtcNow.ToString("dd-MM-yyyy-hh-mm-ss-ff");
            MessageNFTs.Add(mn, msg);
            NewMessage?.Invoke(this, (mn,msg));
        }

        /// <summary>
        /// Mark the message as processed. It will add it to the dictionary with new flag Processed instead of ToProcess
        /// </summary>
        /// <param name="messageName"></param>
        /// <returns></returns>
        public bool MarkMessageAsProcessed(string messageName)
        {
            if (MessageNFTs.TryGetValue(messageName, out var nft))
            {
                lock (_lock)
                {
                    MessageNFTs.Remove(messageName);
                    MessageNFTs.Add(messageName.Replace("ToProcess", "Processed"), nft);
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Deinitalization of the communication with the IoT API
        /// </summary>
        /// <returns></returns>
        public async Task DeInitCommunication()
        {
            if (IoTDataDriver != null)
            {
                IoTDataDriver.NewDataReceived -= IoTDataDriver_NewDataReceived;
                await IoTDataDriver.DeInit();
                Active = false;
            }
        }
    }
}
