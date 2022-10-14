using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.PVECalculations;

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
        /// Add PVPanel to the group
        /// </summary>
        /// <param name="block"></param>
        /// <returns>Id of new added panel</returns>
        public string AddBatteryBlock(BatteryBlock block)
        {
            if (block == null || string.IsNullOrEmpty(block.Id))
                return string.Empty;
            if (!BatteryBlocks.ContainsKey(block.Id))
                BatteryBlocks.TryAdd(block.Id, block);

            return block.Id;
        }
        /// <summary>
        /// Remove panel from the group based on Id
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
        public IEnumerable<(DateTime, double)> GetChargingProfileData(DataProfile inputProfile)
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
                    var dtMaxChargePower = AverageMaxChargePower * (step.Key - laststeptime).TotalHours;

                    if (step.Value >= dtMaxChargePower)
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
                        loaded += dtMaxChargePower;
                    }
                    else if (step.Value > 0 && step.Value < dtMaxChargePower)
                    {
                        loaded += step.Value;
                    }

                    if (loaded <= 0)
                        loaded = 0.000001;

                    foreach (var bat in BatteryBlocks.Values)
                        bat.ActualFilledCapacity = loaded / BatteryBlocks.Count;

                    laststeptime = step.Key;
                    yield return (step.Key, loaded);
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
        public IEnumerable<(DateTime, double)> GetGetDischargingProfileData(DataProfile inputProfile)
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
                    var dtMaxDischargePower = AverageMaxDischargePower * (step.Key - laststeptime).TotalHours;

                    if (step.Value >= dtMaxDischargePower)
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
                        loaded -= dtMaxDischargePower;
                    }
                    else if (step.Value > 0 && step.Value < dtMaxDischargePower)
                    {
                        loaded -= step.Value;
                    }
                    if (loaded <= 0)
                        loaded = 0.000001;

                    foreach (var bat in BatteryBlocks.Values)
                        bat.ActualFilledCapacity = loaded / BatteryBlocks.Count;

                    laststeptime = step.Key;
                    yield return (step.Key, loaded);
                }
            }
        }
    }
}
