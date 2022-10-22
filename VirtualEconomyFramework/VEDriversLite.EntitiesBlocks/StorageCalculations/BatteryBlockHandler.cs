using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Financial;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.PVECalculations.Dto;
using VEDriversLite.EntitiesBlocks.StorageCalculations.Dto;

namespace VEDriversLite.EntitiesBlocks.StorageCalculations
{
    public enum BatteryBlockHandlerDischargeMode
    {
        DischargeAfterFullCharge,
        DischargeJustInNight,
        DischargeAnytime
    }

    /// <summary>
    /// Delegate for Charging function
    /// </summary>
    /// <param name="totalcapacity"></param>
    /// <param name="actualstoredpower"></param>
    /// <param name="resistance"></param>
    /// <param name="time"></param>
    /// <param name="step"></param>
    /// <returns></returns>

    public delegate double ChargingFunction(double totalcapacity, double actualstoredpower, double resistance, double time, double step);
    
    /// <summary>
    /// Delegate for Discharging functoin
    /// </summary>
    /// <param name="totalcapacity"></param>
    /// <param name="actualstoredpower"></param>
    /// <param name="resistance"></param>
    /// <param name="time"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public delegate double DischargingFunction(double totalcapacity, double actualstoredpower, double resistance, double time, double step);

    /// <summary>
    /// Battery Block Handler represents whole energy storage.
    /// It contains dictionary of all battery blocks. It can provide aggregated information about whole capacity, etc.
    /// This class provides the charging and discharging profiles based on input of source/consumption.
    /// </summary>
    public class BatteryBlockHandler
    {
        /// <summary>
        /// Input unique Id and name and functions for calculation of charging/discharging functions
        /// </summary>
        /// <param name="id">Unique Id, if null it will create new Guid</param>
        /// <param name="name">any readable name</param>
        /// <param name="chargingFunctionDelegate">You can use static charging function in this handler class</param>
        /// <param name="dischargingFunctionDelegate">You can use static charging function in this handler class</param>
        public BatteryBlockHandler(string? id, string name, ChargingFunction chargingFunctionDelegate, DischargingFunction dischargingFunctionDelegate)
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();
            Id = id;
            Name = name;
            ChargingFunctionDelegate = chargingFunctionDelegate;
            DischargingFunctionDelegate = dischargingFunctionDelegate;
        }

        /// <summary>
        /// Id of the Battery block group
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Name of the battery block group
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Charging function delegate. It is used in the calculation of charging profile
        /// </summary>
        public ChargingFunction ChargingFunctionDelegate { get; set; }
        /// <summary>
        /// Discharging function delegate. It is used in the calculation of discharging profile
        /// </summary>
        public DischargingFunction DischargingFunctionDelegate { get; set; }

        /// <summary>
        /// Total capacity of all battery blocks
        /// </summary>
        public double TotalCapacity
        {
            get => BatteryBlocks.Values.Where(b => b.Capacity >= 0).Select(b => b.Capacity).Sum();
        }
        /// <summary>
        /// Maximum Average Charge power
        /// </summary>
        public double AverageMaxChargePower
        {
            get => BatteryBlocks.Values.Where(b => b.MaximumChargePower >= 0).Select(b => b.MaximumChargePower).Average();
        }
        /// <summary>
        /// Maximum Average Discharge power
        /// </summary>
        public double AverageMaxDischargePower
        {
            get => BatteryBlocks.Values.Where(b => b.MaximumDischargePower >= 0).Select(b => b.MaximumDischargePower).Average();
        }
        /// <summary>
        /// Total actual filled capacity of all battery blocks
        /// </summary>
        public double TotalActualFilledCapacity
        {
            get => BatteryBlocks.Values.Where(b => b.ActualFilledCapacity >= 0).Select(b => b.ActualFilledCapacity).Sum();
        }
        /// <summary>
        /// Total Internal resistance of all battery blocks. Consider blocks as parallel settings
        /// </summary>
        public double TotalInternalResistance
        {
            get
            {
                var res = 0.0;
                foreach (var blockresistance in BatteryBlocks.Values.Where(b => b.InternalResistance > 0).Select(b => b.InternalResistance))
                {
                    res += 1 / blockresistance;
                }
                if (res != 0)
                    return 1 / res;
                else
                    return double.MaxValue;
            }
        }
        /// <summary>
        /// Dictionary of all battery blocks
        /// </summary>
        public ConcurrentDictionary<string, BatteryBlock> BatteryBlocks { get; set; } = new ConcurrentDictionary<string, BatteryBlock>();

        /// <summary>
        /// Template battery for this Battery Group
        /// </summary>
        public BatteryBlock CommonBattery { get; set; } = new BatteryBlock();
        /// <summary>
        /// Financial info about the whole Storage
        /// </summary>
        public FinancialInfo FinancialInfo { get; set; } = new FinancialInfo();
        /// <summary>
        /// Get total investment based on total power capacity of storage in time based on actual time and specified discont
        /// </summary>
        public double TotalInvestmentBasedOnPeakPower { get => FinancialInfo.DiscontedPrice; }

        /// <summary>
        /// Set parameters for the Common battery template
        /// </summary>
        /// <param name="batteryblock"></param>
        public void SetCommonBattery(BatteryBlock batteryblock)
        {
            CommonBattery = batteryblock.Clone();
            CommonBattery.GroupId = Id;
        }

        /// <summary>
        /// Set discont of financial info
        /// </summary>
        /// <param name="discont"></param>
        public void SetDiscont(double discont)
        {
            FinancialInfo.Discont.DiscontInPercentagePerYear = discont;
        }

        /// <summary>
        /// Function will refresh some internal stats which relates on number of battery blocks, etc.
        /// </summary>
        public void Refresh()
        {
            FinancialInfo.InitialUnitPrice = FinancialInfoHelpers.AvgPricePerWCapacityForStorage * TotalCapacity;
        }
        /// <summary>
        /// Get disconted value of the investment in time
        /// </summary>
        /// <param name="end">End time when the disconted price is requested</param>
        /// <returns></returns>
        public double GetInvestmentDiscontedInTime(DateTime end)
        {
            return FinancialInfo.GetDiscontedValue(end);
        }

        /// <summary>
        /// Export Config file to the Json
        /// </summary>
        /// <returns></returns>
        public string ExportSettingsToJSON()
        {
            try
            {
                var exp = JsonConvert.SerializeObject(CreateConfigFile());
                if (exp != null)
                    return exp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot serialize Battery Storage settings. " + ex.Message);
            }
            return string.Empty;
        }

        /// <summary>
        /// Fill the Config file
        /// </summary>
        /// <returns></returns>
        public BatteryStorageDto CreateConfigFile()
        {
            var config = new BatteryStorageDto();
            config.Id = Id;
            config.Name = Name;
            config.CommonBattery = CommonBattery;

            foreach (var battery in BatteryBlocks.Values)
                config.BatteryBlocks.TryAdd(battery.Id, battery);

            return config;
        }

        /// <summary>
        /// Import settings of the Battery Storage from JSON
        /// </summary>
        /// <param name="jsonConfig"></param>
        /// <returns></returns>
        public bool ImportConfigFromJson(string jsonConfig)
        {
            if (string.IsNullOrEmpty(jsonConfig))
                return false;

            var config = JsonConvert.DeserializeObject<BatteryStorageDto>(jsonConfig);
            if (config != null)
                return ImportConfig(config);
            else
                return false;
        }

        /// <summary>
        /// Import config from config file
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool ImportConfig(BatteryStorageDto config)
        {
            if (config == null)
                return false;

            Id = config.Id;
            Name = config.Name;
            CommonBattery = config.CommonBattery;

            BatteryBlocks.Clear();

            foreach (var block in config.BatteryBlocks.Values)
                BatteryBlocks.TryAdd(block.Id, block);
                
            return true;
        }

        /// <summary>
        /// Add Battery Block to the group
        /// </summary>
        /// <param name="block"></param>
        /// <param name="addcount">Add multiple times. In that case each panel will have new created Id</param>
        /// <returns>Id of new added panel</returns>
        public IEnumerable<string> AddBatteryBlock(BatteryBlock block, int addcount = 1)
        {
            if (block != null)
            { 
                while (addcount > 0)
                {
                    var b = block.Clone();
                    b.Id = Guid.NewGuid().ToString();
                    b.GroupId = Id;
                    if (BatteryBlocks.TryAdd(b.Id, b))
                    { 
                        addcount--;
                        yield return b.Id;
                    }
                }
            }
        }

        /// <summary>
        /// Remove battery block from the group based on Id
        /// </summary>
        /// <param name="blockId">Panel Id</param>
        /// <returns>true if success</returns>
        public bool RemoveBatteryBlock(string blockId)
        {
            if (string.IsNullOrEmpty(blockId))
                return false;
            if (BatteryBlocks.TryRemove(blockId, out var panel))
                return true;

            return false;
        }

        /// <summary>
        /// Default charging function with exponential function for calculate actula state of loaded capacity based on time.
        /// It is not used in calculation now, it needs better tests before usage.
        /// </summary>
        /// <param name="totalcapacity"></param>
        /// <param name="actualstoredpower"></param>
        /// <param name="resistance"></param>
        /// <param name="time"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static double DefaultChargingFunction(double totalcapacity, double actualstoredpower, double resistance, double time, double step)
        {
            return totalcapacity * (1 - Math.Exp(-time / (totalcapacity * resistance)));
        }

        /// <summary>
        /// Default discharging function with exponential function for calculate actula state of loaded capacity based on time.
        /// It is not used in calculation now, it needs better tests before usage.
        /// </summary>
        /// <param name="totalcapacity"></param>
        /// <param name="actualstoredpower"></param>
        /// <param name="resistance"></param>
        /// <param name="time"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static double DefaultDischargingFunction(double totalcapacity, double actualstoredpower, double resistance, double time, double step)
        {
            return actualstoredpower * (Math.Exp(-time / (totalcapacity * resistance)));
        }

        /// <summary>
        /// Get IEnumerable of double values which represents actual charging capacity of the battery group
        /// Input profile are values which represents inputs Source
        /// The source charging the battery blocks
        /// </summary>
        /// <param name="inputProfile"></param>
        /// <returns></returns>
        public IEnumerable<(DateTime, double)> GetChargingProfileData(DataProfile inputProfile, bool addRelativeValues = false, bool inputInkWh = false)
        {
            if (ChargingFunctionDelegate != null)
            {
                var starttime = inputProfile.FirstDate;
                var endtime = inputProfile.LastDate;
                var totalcapacity = TotalCapacity;

                var loaded = TotalActualFilledCapacity;
                var laststeptime = starttime;
                if (inputProfile.ProfileData.Count > 1)
                    laststeptime = starttime - (inputProfile.ProfileData.Keys.Skip(1).Take(1).First() - starttime);
                
                foreach (var step in inputProfile.ProfileData)
                {
                    var value = step.Value;
                    if (inputInkWh)
                        value *= 1000;

                    var dtMaxChargePower = AverageMaxChargePower * (step.Key - laststeptime).TotalHours;

                    var addvalue = value;
                    if (value >= dtMaxChargePower)
                    {
                        /*
                            var resistance = 1 / AverageMaxChargePower;

                            if (resistance <= 0)
                                resistance = 0.00001;

                            var load = ChargingFunctionDelegate(totalcapacity,
                                                              TotalActualFilledCapacity,
                                                              resistance,
                                                              (step.Key - starttime).TotalHours,
                                                              (step.Key - laststeptime).TotalHours);

                            loaded += (load / totalcapacity) * dtMaxChargePower;
                            */
                        addvalue = dtMaxChargePower;
                    }

                    if ((loaded + addvalue) > totalcapacity && loaded < totalcapacity)
                        addvalue = totalcapacity - loaded - totalcapacity * 0.001;

                    if ((loaded + addvalue) < totalcapacity)
                    {
                        loaded += addvalue;
                        
                        if (loaded <= 0)
                            loaded = 0.000001;

                        foreach (var bat in BatteryBlocks.Values)
                            bat.ActualFilledCapacity = loaded / BatteryBlocks.Count;
                    }

                    laststeptime = step.Key;
                    if (!addRelativeValues)
                        yield return (step.Key, loaded);
                    else
                        yield return (step.Key, loaded / totalcapacity);
                }
            }
        }

        /// <summary>
        /// Get IEnumerable of double values which represents actual charging capacity of the battery group
        /// Input profile are values which represents inputs Source
        /// The source charging the battery blocks
        /// </summary>
        /// <param name="inputProfile"></param>
        /// <returns></returns>
        public IEnumerable<(DateTime, double)> GetGetDischargingProfileData(DataProfile inputProfile, bool addRelativeValues = false, bool inputInkWh = false)
        {
            if (ChargingFunctionDelegate != null)
            {
                var starttime = inputProfile.FirstDate;
                var endtime = inputProfile.LastDate;
                var totalcapacity = TotalCapacity;

                var loaded = TotalActualFilledCapacity;
                var laststeptime = starttime;
                if (inputProfile.ProfileData.Count > 1)
                {
                    laststeptime = starttime - (inputProfile.ProfileData.Keys.Skip(1).Take(1).First() - starttime);
                }
                foreach (var step in inputProfile.ProfileData)
                {
                    var value = step.Value;
                    if (inputInkWh)
                        value *= 1000;

                    var dtMaxDischargePower = AverageMaxDischargePower * (step.Key - laststeptime).TotalHours;

                    var addvalue = value;
                    if (value >= dtMaxDischargePower)
                    {
                        /*
                            var resistance = 1 / dtMaxDischargePower;

                            if (resistance <= 0)
                                resistance = 0.00001;

                            var load = DischargingFunctionDelegate(totalcapacity,
                                                                 TotalActualFilledCapacity,
                                                                 resistance,
                                                                 (step.Key - starttime).TotalHours,
                                                                 (step.Key - laststeptime).TotalHours);

                            loaded -= (load/totalcapacity) * dtMaxDischargePower;
                            */
                        addvalue = dtMaxDischargePower;
                    }

                    if ((loaded - addvalue) < 0 && loaded > 0)
                        addvalue = loaded - 0.000001;

                    if ((loaded - addvalue) > 0)
                    {
                        loaded -= addvalue;
                        
                        if (loaded <= 0)
                            loaded = 0.000001;

                        foreach (var bat in BatteryBlocks.Values)
                            bat.ActualFilledCapacity = loaded / BatteryBlocks.Count;
                    }

                    laststeptime = step.Key;
                    if (!addRelativeValues)
                        yield return (step.Key, loaded);
                    else
                        yield return (step.Key, loaded / totalcapacity);
                }
            }
        }

        /// <summary>
        /// Calculate Charging, Discharging and Balance profile for storage based on input production and consumption data
        /// It does not count with actual loaded capacity in battery storage. It calculate indepenedent characteristics.
        /// Function logic will start discharge after the battery charging was considered as stable enough (not loading anymore, or full capacity).
        /// </summary>
        /// <param name="sourceData">dataprofile with source/charging data</param>
        /// <param name="consumptionData">dataprofile with consumption/discharging data</param>
        /// <param name="inputInkWh">if the input data are in kWh set this and it will be convrted to Wh</param>
        /// <param name="loaded">start state of loaded capacity of battery</param>
        /// <param name="startDischarge">charging change ratio against max charge power when discharge starts (battery is not charging anymore)</param>
        /// <param name="minimumLoadedToDischarge">minimum amount in storage to start discharge. it is ratio against totalcapacity (for example 0.1 for 10% of totalcapacity</param>
        /// <returns></returns>
        public Dictionary<string, DataProfile> GetChargingAndDischargingProfiles(DataProfile sourceData,
                                                                                 DataProfile consumptionData,
                                                                                 bool inputInkWh,
                                                                                 double loaded = 0.0,
                                                                                 double startDischarge = 0.07,
                                                                                 double minimumLoadedToDischarge = 0.04)
        {
            var result = new Dictionary<string, DataProfile>();
            var chargingProfile = new DataProfile(); // keeps total charged in time
            var dchargingProfile = new DataProfile(); // keeps charge step in time (delta t1 - t0)
            var dischargingProfile = new DataProfile(); // keeps total discharged in time
            var ddischargingProfile = new DataProfile(); // keeps discharge step in time (delta t1 - t0)
            var balanceProfile = new DataProfile(); // keeps balance in time

            var starttime = sourceData.FirstDate;
            var endtime = sourceData.LastDate;
            var totalcapacity = TotalCapacity;

            var laststeptime = starttime;

            var lastConsumedData = new List<DateTime>();

            if (consumptionData.ProfileData.Count > 1)
                laststeptime = starttime - (consumptionData.ProfileData.Keys.Skip(1).Take(1).ToList().First() - starttime);

            if (loaded > minimumLoadedToDischarge * totalcapacity)
            {
                foreach (var step in consumptionData.ProfileData)
                {
                    var value = step.Value;
                    if (inputInkWh)
                        value *= 1000;

                    var dtMaxDischargePower = AverageMaxDischargePower * (step.Key - laststeptime).TotalHours;

                    var subvalue = value;
                    if (value >= dtMaxDischargePower)
                        subvalue = dtMaxDischargePower;

                    if ((loaded - subvalue) < 0 && loaded > 0)
                        subvalue = loaded - 0.000001;

                    if ((loaded - subvalue) > 0)
                    {
                        loaded -= subvalue;
                        ddischargingProfile.ProfileData.TryAdd(step.Key, subvalue);
                        if (loaded <= 0)
                            loaded = 0.000001;
                    }
                    else
                    {
                        ddischargingProfile.ProfileData.TryAdd(step.Key, subvalue);
                    }

                    dischargingProfile.ProfileData.TryAdd(step.Key, loaded);
                    balanceProfile.ProfileData.TryAdd(step.Key, loaded);

                    lastConsumedData.Add(step.Key);

                    if (loaded < 0.02 * totalcapacity)
                        break;
                }
            }

            if (sourceData.ProfileData.Count > 1)
                laststeptime = starttime - (sourceData.ProfileData.Keys.Skip(1).Take(1).ToList().First() - starttime);

            foreach (var step in sourceData.ProfileData)
            {
                var value = step.Value;
                if (inputInkWh)
                    value *= 1000;

                var dtMaxChargePower = AverageMaxChargePower * (step.Key - laststeptime).TotalHours;

                var addvalue = value;
                if (value >= dtMaxChargePower)
                    addvalue = dtMaxChargePower;

                if ((loaded + addvalue) > totalcapacity && loaded < totalcapacity)
                    addvalue = totalcapacity - loaded - totalcapacity * 0.001;

                if ((loaded + addvalue) < totalcapacity)
                {
                    loaded += addvalue;
                    dchargingProfile.ProfileData.TryAdd(step.Key, addvalue);
                    if (loaded <= 0)
                        loaded = 0.000001;
                }
                else
                {
                    dchargingProfile.ProfileData.TryAdd(step.Key, 0);
                }

                laststeptime = step.Key;
                chargingProfile.ProfileData.TryAdd(step.Key, loaded);

            }

            var dischargestarted = false;
            var avgmaxcharge = AverageMaxChargePower;
            var lastval = 0.0;
            var unloaded = 0.0;
            if (consumptionData.ProfileData.Count > 1)
                laststeptime = starttime - (consumptionData.ProfileData.Keys.Skip(1).Take(1).ToList().First() - starttime);

            foreach (var step in consumptionData.ProfileData)
            {
                if (!lastConsumedData.Contains(step.Key))
                {
                    if (chargingProfile.ProfileData.TryGetValue(step.Key, out var chargeLoadValue))
                    {
                        if (!dischargestarted &&
                            chargeLoadValue > totalcapacity * minimumLoadedToDischarge &&
                            Math.Abs((chargeLoadValue - lastval)) <= avgmaxcharge * startDischarge)
                        {
                            dischargestarted = true;
                            unloaded = chargeLoadValue;
                        }
                        lastval = chargeLoadValue;

                        if (dischargestarted)
                        {
                            var value = step.Value;
                            if (inputInkWh)
                                value *= 1000;

                            var dtMaxDischargePower = AverageMaxDischargePower * (step.Key - laststeptime).TotalHours;

                            var subvalue = value;
                            if (value >= dtMaxDischargePower)
                                subvalue = dtMaxDischargePower;

                            if ((unloaded - subvalue) < 0 && unloaded > 0)
                                subvalue = unloaded - 0.000001;

                            if ((unloaded - subvalue) > 0)
                            {
                                unloaded -= subvalue;
                                ddischargingProfile.ProfileData.TryAdd(step.Key, subvalue);
                                if (unloaded <= 0)
                                    unloaded = 0.000001;
                            }
                            else
                            {
                                ddischargingProfile.ProfileData.TryAdd(step.Key, subvalue);
                            }

                            dischargingProfile.ProfileData.TryAdd(step.Key, unloaded);
                            balanceProfile.ProfileData.TryAdd(step.Key, chargeLoadValue - (chargeLoadValue - unloaded));
                        }
                        else
                        {
                            dischargingProfile.ProfileData.TryAdd(step.Key, 0);
                            balanceProfile.ProfileData.TryAdd(step.Key, chargeLoadValue);
                        }

                        laststeptime = step.Key;
                    }
                }
            }

            result.Add("charge", chargingProfile);
            result.Add("dcharge", dchargingProfile);
            result.Add("discharge", dischargingProfile);
            result.Add("ddischarge", ddischargingProfile);
            result.Add("balance", balanceProfile);

            return result;
        }

        public bool DischargeAllBatteryBlocks()
        {
            foreach(var battery in BatteryBlocks.Values)
                battery.ActualFilledCapacity = 0.0001;

            return true;
        }
    }
}
