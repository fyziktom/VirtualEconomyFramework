using Blazorise;
using System;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.Sources;
using VEDriversLite.EntitiesBlocks.StorageCalculations;
using VEDriversLite.Common.Calendar;
using Microsoft.AspNetCore.Components;
using VEDriversLite.EntitiesBlocks.Entities;
using VEFramework.VEBlazor.EntitiesBlocks.Model;

namespace VEFramework.VEBlazor.EntitiesBlocks.Services;

public partial class CalculationService
{
    private async Task<Dictionary<string, List<CalculationResult>>> DoCalculation(string filteredEntitiesConfig, string filteredPveConfig, string filteredStorageConfig,
        decimal budget, decimal interestRate, DateTime endDate, IDictionary<string, bool> deviceLeadingMap)
    {
        Console.WriteLine("Running calculation");
        var result = new Dictionary<string, List<CalculationResult>>();
        
        // SET start of Day tariff
        var startOfDT = new TimeSpan(6, 0, 0);
        // SET start of Night tariff
        var startOfNT = new TimeSpan(21, 0, 0);

        var lines = new List<string>();
        var linesBilance = new List<string>();

        var start = endDate.AddDays(-1);

        var lastStorageLoadedEndState = 0.0;

        // create simulator objects
        var eGrid = new BaseEntitiesHandler();
        var PVESim = new PVPanelsGroupsHandler();
        var StorageSim = new BatteryBlockHandler(null,
                                                 "battery",
                                                 BatteryBlockHandler.DefaultChargingFunction,
                                                 BatteryBlockHandler.DefaultDischargingFunction);

        // find first alocation scheme
        var alocationScheme = eGrid.AlocationSchemes.Values.FirstOrDefault();
        if (alocationScheme == null)
            alocationScheme = new AlocationScheme();

        // load simulators
        if (!eGrid.LoadFromConfig(filteredEntitiesConfig).Item1) return null;
        if (!PVESim.ImportConfig(filteredPveConfig).Item1) return null;
        if (!StorageSim.ImportConfig(filteredStorageConfig).Item1) return null;

        // load financial info
        PVESim.FinancialInfo.Discont.DiscontInPercentagePerYear = (double)interestRate;
        PVESim.FinancialInfo.BuyDate = new DateTime(2022,1,1);
        StorageSim.FinancialInfo.Discont.DiscontInPercentagePerYear = (double)interestRate;
        StorageSim.FinancialInfo.BuyDate = new DateTime(2022, 1, 1);

        var network = eGrid.GetEntity("7b27c442-ad40-4679-b6d5-8873d9763996", EntityType.Consumer);
        eGrid.RemoveAllEntityBlocks(network.Id);
        var pvesource = eGrid.GetEntity("617132c1-2f70-4d98-bdb1-18f9f01c29ef", EntityType.Source);
        eGrid.RemoveAllEntityBlocks(pvesource.Id);

        //var fmsId = deviceLeadingMap.Where(d => d.Value).FirstOrDefault();
        var firstmeasurespot = eGrid.GetEntity(pvesource.ParentId, EntityType.Consumer);
        eGrid.RemoveAllEntityBlocks(firstmeasurespot.Id);

        //var firstmeasurespotDevice = eGrid.FindEntityByName("firstspotdevice");
        var mainbattery = eGrid.GetEntity("d29a0515-a112-4ca1-9e57-373cace32330", EntityType.Source);
        if (mainbattery != null)
            eGrid.RemoveAllEntityBlocks(mainbattery.Id);

        var pveInvestment = PVESim.TotalInvestmentBasedOnPeakPower;
        var storageInvestment = StorageSim.TotalInvestmentBasedOnPeakPower;

        var coord = new Coordinates(PVESim.MedianLatitude, PVESim.MedianLongitude);

        if (network != null && pvesource != null && firstmeasurespot != null && mainbattery != null)
        {
            // add PVE simulator to entity
            eGrid.AddSimulatorToEntity(pvesource.Id, PVESim);

            // add TDD Consumption simulators to entities
            eGrid.AddSimulatorToEntity(firstmeasurespot.Id,
                                       new ConsumerTDDSimulator(new List<DataProfile>() { appData.TDDs[0] }));
            
            var dtmp = start;
            var dend = endDate;

            var totalDays = (endDate - start).TotalDays;
            var day = 1;
            // iterate through days until end
            while (dtmp < dend)
            {
                Console.WriteLine($"Analysing {dtmp} Day {day} of {totalDays} total Days to analyze...");
                
                // get two list of blocks after first phase of calculation,
                // when first measured spot (connection place for PVE) consume imediatelly what it can
                // the first list contains rest of the production after the consumption by entity.
                // the second list contains rest of the consumption which cannot be covered by the PVE.
                var firstmeasuredspotPhase1 = GetEntityBalanceBlocksAfterAlocationOfPVEBlocks(firstmeasurespot, eGrid, dtmp);

                // keep Id connection between source and rest blocks
                foreach (var b in firstmeasuredspotPhase1.Item1)
                {
                    var bs = pvesource.Blocks.Values.Where(bl => bl.StartTime == b.StartTime).FirstOrDefault();
                    if (bs != null)
                        b.Id = bs.Id;
                }

                // add rest of consumption
                eGrid.AddBlocksToEntity(firstmeasurespot.Id, firstmeasuredspotPhase1.Item2);
                // add receipts blocks
                eGrid.AddBlocksToEntity(firstmeasurespot.Id, BlockHelpers.CloneBlocks(firstmeasuredspotPhase1.Item1, true, true, BlockType.Forwarded).ToList());
                eGrid.AddBlocksToEntity(firstmeasurespot.Id, BlockHelpers.CloneBlocks(firstmeasuredspotPhase1.Item2, true, true, BlockType.NotCovered).ToList());

                // add rest of production
                eGrid.AddBlocksToEntity(network.Id, firstmeasuredspotPhase1.Item1, alocationScheme.Id);

                var consumptions = new Dictionary<string, (List<IBlock>, DataProfile)>();
                var devicephase = new Dictionary<string, (List<IBlock>, List<IBlock>)>();
                foreach (var om in deviceLeadingMap)
                {
                    if (!om.Value)
                    {
                        var ent = eGrid.GetEntity(om.Key, EntityType.Consumer);
                        if (ent != null)
                        {
                            Console.WriteLine($"Adding TDD simulator to entity {ent.Name}...");
                            eGrid.AddSimulatorToEntity(ent.Id,
                                       new ConsumerTDDSimulator(new List<DataProfile>() { appData.TDDs[0] }));

                            var devicePhase = GetEntityBalanceBlocksAfterAlocationOfPVEBlocks(ent, eGrid, dtmp);
                            foreach (var b in devicePhase.Item1)
                            {
                                var bs = ent.Blocks.Values.Where(bl => bl.StartTime == b.StartTime).FirstOrDefault();
                                if (bs != null)
                                    b.Id = bs.Id;
                            }

                            devicephase.TryAdd(ent.Id, devicePhase);

                            // add rest of consumption to entity
                            eGrid.AddBlocksToEntity(ent.Id, devicePhase.Item2);
                            // add receipts blocks
                            eGrid.AddBlocksToEntity(ent.Id, BlockHelpers.CloneBlocks(devicePhase.Item1, true, true, BlockType.Forwarded).ToList());
                            eGrid.AddBlocksToEntity(ent.Id, BlockHelpers.CloneBlocks(devicePhase.Item2, true, true, BlockType.NotCovered).ToList());

                            // add rest of overproduced to network
                            eGrid.AddBlocksToEntity(network.Id, devicePhase.Item1);
                        }
                    }
                }

                
                // calculate actual PVE rest in network
                var productionPhase3 = eGrid.GetConsumptionOfEntity(network.Id,
                                                                    BlockTimeframe.Hour,
                                                                    dtmp,
                                                                    dtmp.AddDays(1),
                                                                    true,
                                                                    true,
                                                                    new List<BlockDirection>() { BlockDirection.Created },
                                                                    new List<BlockType>() { BlockType.Simulated },
                                                                    false);

                var productionPhase3Profile = DataProfileHelpers.ConvertBlocksToDataProfile(productionPhase3);

                // calculate actual PVE rest in network
                var consumptionPhase3 = eGrid.GetConsumptionOfEntity(network.Id,
                                                                     BlockTimeframe.Hour,
                                                                     dtmp,
                                                                     dtmp.AddDays(1),
                                                                     true,
                                                                     true,
                                                                     new List<BlockDirection>() { BlockDirection.Consumed },
                                                                     new List<BlockType>() { BlockType.Simulated },
                                                                     false);

                var consumptionPhase3Profile = DataProfileHelpers.ConvertBlocksToDataProfile(consumptionPhase3);

                // create charge and discharge and balance data profiles for storage
                var profiles = StorageSim.GetChargingAndDischargingProfiles(productionPhase3Profile,
                                                                            consumptionPhase3Profile,
                                                                            true,
                                                                            lastStorageLoadedEndState);

                if (profiles != null &&
                    profiles.TryGetValue("charge", out var ch) &&
                    profiles.TryGetValue("dcharge", out var dch) &&
                    profiles.TryGetValue("discharge", out var disch) &&
                    profiles.TryGetValue("ddischarge", out var ddisch) &&
                    profiles.TryGetValue("balance", out var bil))
                {
                    if (ch.ProfileData.TryGetValue(ch.LastDate, out var lastcharge) &&
                        disch.ProfileData.TryGetValue(disch.LastDate, out var lastdischarge))
                    {
                        // get blocks which represents stored energy in battery per hour for one day
                        // divide blocks values by 1000 to convert to kWh
                        var dchargeblocks = DataProfileHelpers.ConvertDataProfileToBlocks(dch,
                                                                                          BlockDirection.Stored,
                                                                                          BlockType.Simulated,
                                                                                          mainbattery.Id,
                                                                                          1000,
                                                                                          true).ToList();
                        // get blocks which represents energy pulled from the storage (atc as source = BlockDirection.Created in model)
                        // divide blocks values by 1000 to convert to kWh
                        var ddischargeblocks = DataProfileHelpers.ConvertDataProfileToBlocks(ddisch,
                                                                                             BlockDirection.Created,
                                                                                             BlockType.Simulated,
                                                                                             mainbattery.Id,
                                                                                             1000,
                                                                                             true).ToList();

                        // add blocks to battery entity
                        eGrid.AddBlocksToEntity(mainbattery.Id, dchargeblocks);
                        eGrid.AddBlocksToEntity(mainbattery.Id, ddischargeblocks);

                        // get consumption of the whole network in the day
                        var netw = eGrid.GetConsumptionOfEntity(network.Id,
                                                                BlockTimeframe.Hour,
                                                                dtmp,
                                                                dtmp.AddDays(1),
                                                                true,
                                                                true,
                                                                new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                                new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real },
                                                                false);

                        // get consumption of the whole network in the day in window of Day Tariff
                        var netwDT = eGrid.GetConsumptionOfEntityWithWindow(network.Id,
                                                                            BlockTimeframe.Hour,
                                                                            dtmp,
                                                                            dtmp.AddDays(1),
                                                                            dtmp + startOfDT,
                                                                            dtmp + startOfNT,
                                                                            false,
                                                                            true,
                                                                            true,
                                                                            new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                                            new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real },
                                                                            false);

                        // get consumption of the whole network in the day in window of Night Tariff (set invertWindow parameter as true)
                        var netwNT = eGrid.GetConsumptionOfEntityWithWindow(network.Id,
                                                                            BlockTimeframe.Hour,
                                                                            dtmp,
                                                                            dtmp.AddDays(1),
                                                                            dtmp + startOfDT,
                                                                            dtmp + startOfNT,
                                                                            true,
                                                                            true,
                                                                            true,
                                                                            new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                                            new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real },
                                                                            false);



                        ////////////// whole day stats
                        // total needed from Day tariff
                        var totalDT = Math.Round(netwDT.Where(b => b.Amount < 0).Select(b => b.Amount).Sum(), 4);
                        // total over production during the day if some exists
                        var totalDTOverProduction = Math.Round(netwDT.Where(b => b.Amount > 0).Select(b => b.Amount).Sum(), 4);
                        // total needed from Night tariff
                        var totalNT = Math.Round(netwNT.Where(b => b.Amount < 0).Select(b => b.Amount).Sum(), 4);
                        // total over production during the night if some exists
                        var totalNTOverProduction = Math.Round(netwNT.Where(b => b.Amount > 0).Select(b => b.Amount).Sum(), 4);

                        // total produced from own PVE
                        var pveproduced = pvesource.GetSimulatorsBlocks(BlockTimeframe.Hour, dtmp, dtmp.AddDays(1))
                                                               .Where(b => b.Amount > 0)
                                                               .Select(b => b.Amount);
                        var totalproduced = Math.Round(pveproduced.Sum(), 4);// total produced from own PVE over the consumption during the production phase of PVE
                        var totalOverProducedAfterConsumedImmediately = Math.Round(productionPhase3Profile.ProfileData.Values.Sum(), 4);
                        // total stored in storage from PVE production
                        var totalStored = Math.Round(dch.ProfileData.Values.Sum(), 4);
                        // total used from storage for consumption 
                        var totalUsedFromStorage = Math.Round(ddisch.ProfileData.Values.Sum() / 1000, 4);

                        // total consumption from first measure spot (base on tdd here)
                        var consumedFirstMS = firstmeasurespot.GetSimulatorsBlocks(BlockTimeframe.Hour, dtmp, dtmp.AddDays(1))
                                                              .Where(b => b.Amount > 0)
                                                              .Select(b => b.Amount);
                        var totalConsumptionFirstMeasureSpot = Math.Round(consumedFirstMS.Sum(), 4);
                        // device not consumed and forwarded up
                        var totalfirstmeasurespotForwardedSourceBlocks = Math.Round(firstmeasuredspotPhase1.Item1.Where(b => b.Amount != 0).Select(b => b.Amount).Sum(), 4);
                        // device not covered
                        var totalfirstmeasureSpotNotCoveredConsuptionBlocks = Math.Round(firstmeasuredspotPhase1.Item2.Where(b => b.Amount != 0).Select(b => b.Amount).Sum(), 4);
                        // total consumption on device 

                        foreach(var om in deviceLeadingMap)
                        {
                            if (!om.Value)
                            {
                                var ent = eGrid.GetEntity(om.Key, EntityType.Consumer);
                                if (ent != null)
                                {
                                    Console.WriteLine($"Device Id: {om.Key}.");

                                    if (devicephase.TryGetValue(om.Key, out var phasedata))
                                    {
                                        var consumedByDevice = Math.Round(ent.GetSimulatorsBlocks(BlockTimeframe.Hour, dtmp, dtmp.AddDays(1))
                                                                                .Where(b => b.Amount != 0)
                                                                                .Select(b => b.Amount).Sum(), 4);

                                        var forwardedByDevice = Math.Round(phasedata.Item1.Where(b => b.Amount != 0).Select(b => b.Amount).Sum(), 4);
                                        var notCoveredByPVEInDevice = Math.Round(phasedata.Item2.Where(b => b.Amount != 0).Select(b => b.Amount).Sum(), 4);

                                        var calculationResult = new CalculationResult()
                                        {
                                            Date = dtmp,
                                            ConsumedFromFVE = consumedByDevice,
                                            OverProductionFromFVE = forwardedByDevice,
                                            Deficiency = notCoveredByPVEInDevice
                                        };
                                        Console.WriteLine($"{Newtonsoft.Json.JsonConvert.SerializeObject(calculationResult, Newtonsoft.Json.Formatting.Indented)}");

                                        if (result.ContainsKey(om.Key))
                                        {
                                            result[om.Key].Add(calculationResult);
                                        }
                                        else
                                        {
                                            result.Add(om.Key, new List<CalculationResult>() { calculationResult });
                                        }
                                    }
                                }
                            }
                        }

                        // get rest in batery to load it next day morning as overstored from last day
                        // it is inserted in charge and discharge profiles calculations
                        if (bil.ProfileData.TryGetValue(bil.LastDate, out var val))
                            lastStorageLoadedEndState = val;

                        // add line with day stats
                        linesBilance.Add($"{dtmp}\t" +
                                         $"{totalDT}\t" +
                                         $"{totalDTOverProduction}\t" +
                                         $"{totalNT}\t" +
                                         $"{totalNTOverProduction}\t" +
                                         $"{totalproduced}\t" +
                                         $"{totalOverProducedAfterConsumedImmediately}\t" +
                                         $"{totalStored}\t" +
                                         $"{totalUsedFromStorage}\t" +
                                         $"{totalConsumptionFirstMeasureSpot}\t" +
                                         $"{totalfirstmeasurespotForwardedSourceBlocks}\t" +
                                         $"{totalfirstmeasureSpotNotCoveredConsuptionBlocks}\t");

                        // create line for export
                        var line = $"{dtmp}";
                        foreach (var v in netw)
                            line += $"\t{Math.Round(v.Amount, 4)}";

                        lines.Add(line);
                    }
                }
                dtmp = dtmp.AddDays(1);
                day++;
            }
        }


        return result;
    }

    public static (List<IBlock>, List<IBlock>) GetEntityBalanceBlocksAfterAlocationOfPVEBlocks(IEntity entity, IEntitiesHandler eGrid, DateTime dtmp)
    {

        var res = GetEntityBalanceAfterAlocationOfPVEBlocks(entity, eGrid, dtmp);
        var result = (new List<IBlock>(), new List<IBlock>());
        if (res.Item1 != null && res.Item2 != null)
        {
            var consumptionblocks = DataProfileHelpers.ConvertDataProfileToBlocks(res.Item2,
                                                                                  BlockDirection.Consumed,
                                                                                  BlockType.Simulated,
                                                                                  entity.Id).ToList();

            var productionblocks = DataProfileHelpers.ConvertDataProfileToBlocks(res.Item1,
                                                                                 BlockDirection.Created,
                                                                                 BlockType.Simulated,
                                                                                 entity.Id).ToList();

            result = (productionblocks, consumptionblocks);
        }
        return result;
    }

    public static (DataProfile, DataProfile) GetEntityBalanceAfterAlocationOfPVEBlocks(IEntity entity, IEntitiesHandler eGrid, DateTime dtmp)
    {
        // get bilance of the consumption and production
        var cons = eGrid.GetConsumptionOfEntity(entity.Id,
                                                BlockTimeframe.Hour,
                                                dtmp,
                                                dtmp.AddDays(1),
                                                true,
                                                true,
                                                new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                new List<BlockType>() { BlockType.Simulated },
                                                true);

        var consprof = DataProfileHelpers.ConvertBlocksToDataProfile(cons);
        // get production after some part was consumed with sun-day consumption
        var productionafterconsumed = new DataProfile();
        foreach (var k in consprof.ProfileData.Keys)
        {
            if (consprof.ProfileData.TryGetValue(k, out var v))
            {
                if (v < 0)
                    productionafterconsumed.ProfileData.TryAdd(k, 0);
                else
                    productionafterconsumed.ProfileData.TryAdd(k, v);
            }
        }
        // get rest of the consumption which needs to be covered from storage or network
        var consumptionafterpve = new DataProfile();
        foreach (var k in consprof.ProfileData.Keys)
        {
            if (consprof.ProfileData.TryGetValue(k, out var v))
            {
                if (v < 0)
                    consumptionafterpve.ProfileData.TryAdd(k, Math.Abs(v));
                else
                    consumptionafterpve.ProfileData.TryAdd(k, 0);
            }
        }

        return (productionafterconsumed, consumptionafterpve);
    }
}