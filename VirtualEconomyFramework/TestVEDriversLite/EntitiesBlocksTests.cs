using Org.BouncyCastle.Asn1.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.StorageCalculations;
using WordPressPCL.Models;

namespace TestVEDriversLite
{
    public static class EntitiesBlocksTests
    {
        [TestEntry]
        public static void EB_LoadTDD(string param)
        {
            EB_LoadTDDAsync(param);
        }
        public static async Task EB_LoadTDDAsync(string param)
        {
            var content = FileHelpers.ReadTextFromFile(param);
            if (!string.IsNullOrEmpty(content))
            {
                var tdds = ConsumersHelpers.LoadTDDs(content);
                var start = new DateTime(2022, 1, 1);

                var first = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[0], 
                                                                      start, start.AddMonths(1), 
                                                                      BlockTimeframe.Day);
                var second = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[0],
                                                                       start, start.AddYears(1),
                                                                       BlockTimeframe.Month);

                var firstblocks = DataProfileHelpers.ConvertDataProfileToBlocks(first, 
                                                                                BlockDirection.Consumed, 
                                                                                BlockType.Simulated, 
                                                                                "1234").ToList();

                var secondblocks = DataProfileHelpers.ConvertDataProfileToBlocks(second,
                                                                                 BlockDirection.Consumed,
                                                                                 BlockType.Simulated,
                                                                                 "1234").ToList();


            }
        }

        [TestEntry]
        public static void EB_LoadTDDAndCalcBalance(string param)
        {
            EB_LoadTDDAndCalcBalanceAsync(param);
        }
        public static async Task EB_LoadTDDAndCalcBalanceAsync(string param)
        {
            // SET Coordinates of PVE
            Coordinates coord = new Coordinates(49.194103, 16.608998);
            // SET Start of the simulation
            var start = new DateTime(2022, 1, 1);
            // SET Number of days which you want to simulate
            var daysOfSimulation = 365;
            // SET start of Day tariff
            var startOfDT = new TimeSpan(6, 0, 0);
            // SET start of Night tariff
            var startOfNT = new TimeSpan(21, 0, 0);

            // end of the simulation
            var end = start.AddDays(daysOfSimulation);

            var split = param.Split(',');
            if (split.Length == 4)
            {
                // name of file with TDDs
                var tdd = split[0];
                // name of config file for grid (for example in Demo.Energy/wwwroot: "sampledata.json")
                var grid = split[1];
                // name of config file for PVE simulator (for example in Demo.Energy/wwwroot: "samplepve.json")
                var pve = split[2];
                // name of config file for Battery Storage simulator (for example in Demo.Energy/wwwroot: "samplestorage.json")
                var storage = split[3];

                // create simulator objects
                var eGrid = new BaseEntitiesHandler();
                var PVESim = new PVPanelsGroupsHandler();
                var StorageSim = new BatteryBlockHandler(null, 
                                                         "battery", 
                                                         BatteryBlockHandler.DefaultChargingFunction, 
                                                         BatteryBlockHandler.DefaultDischargingFunction);

                // load configs of simulators
                var tddfromfile = FileHelpers.ReadTextFromFile(tdd);
                var cpve = FileHelpers.ReadTextFromFile(pve);
                var cgrid = FileHelpers.ReadTextFromFile(grid);
                var cstorage = FileHelpers.ReadTextFromFile(storage);

                // prepare output files names
                var lines = new List<string>();
                var filename = $"{DateTime.UtcNow.ToString("yyyy_MM_ddThh_mm_ss")}-blocks.txt";
                var linesBilance = new List<string>();
                var filenameBilance = $"{DateTime.UtcNow.ToString("yyyy_MM_ddThh_mm_ss")}-grid-bilance.txt";

                if (!string.IsNullOrEmpty(tddfromfile) && !string.IsNullOrEmpty(cgrid) && !string.IsNullOrEmpty(cpve))
                {
                    var tdds = ConsumersHelpers.LoadTDDs(tddfromfile);

                    // load simulators
                    if (!eGrid.LoadFromConfig(cgrid).Item1) return;
                    if (!PVESim.ImportConfigFromJson(cpve)) return;
                    if (!StorageSim.ImportConfigFromJson(cstorage)) return;

                    var lastStorageLoadedEndState = 0.0;

                    // load entities because of their Ids (blocks will reffer to their Ids)
                    var network = eGrid.FindEntityByName("network");
                    var device = eGrid.FindEntityByName("device1");
                    var device3 = eGrid.FindEntityByName("device3");
                    var pvesource = eGrid.FindEntityByName("mainPVE");
                    var firstmeasurespot = eGrid.FindEntityByName("FirstMeasureSpot");
                    var firstmeasurespotDevice = eGrid.FindEntityByName("firstspotdevice");
                    var mainbattery = eGrid.FindEntityByName("mainbattery");

                    // create alocation scheme
                    var alocationScheme = new AlocationScheme()
                    {
                        Id = System.Guid.NewGuid().ToString(),
                        IsActive = true,
                        Name = "Main Scheme"
                    };
                    alocationScheme.DepositPeers.TryAdd(device.Id, new DepositPeer()
                    {
                        PeerId = device.Id,
                        Name = device.Name,
                        Percentage = 20
                    });
                    alocationScheme.DepositPeers.TryAdd(device.Id, new DepositPeer()
                    {
                        PeerId = device3.Id,
                        Name = device3.Name,
                        Percentage = 20
                    });
                    eGrid.AlocationSchemes.TryAdd(alocationScheme.Id, alocationScheme);

                    if (device != null && pvesource != null && mainbattery != null)
                    {
                        var dtmp = start;
                        var dend = end;

                        var totalDays = (end - start).TotalDays;
                        var day = 1;
                        // iterate through days until end
                        while (dtmp < dend)
                        {
                            Console.WriteLine($"Analysing {dtmp} Day {day} of {totalDays} total Days to analyze...");
                            // simulate production for each hour of day
                            var production = new DataProfile();
                            var tmp = dtmp;
                            var tmpend = dtmp.AddDays(1);

                            while (tmp < tmpend)
                            {
                                production.ProfileData.TryAdd(tmp, PVESim.GetTotalPowerInDateTime(tmp, coord, 1));
                                tmp = tmp.AddHours(1);
                            }

                            // convert simulated PVE production dataprofile to blocks
                            var productionblocks = DataProfileHelpers.ConvertDataProfileToBlocks(production,
                                                                                                 BlockDirection.Created,
                                                                                                 BlockType.Simulated,
                                                                                                 pvesource.Id).ToList();

                            // add day production blocks to the mainPVE entity
                            eGrid.AddBlocksToEntity(pvesource.Id, productionblocks);

                            // get consumption based on TDD for MeasureSpot1 (place where the PVE is connected through)
                            // it will cover whole consumption of this entity. 
                            var consumption = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[0],
                                                                                        dtmp,
                                                                                        dtmp.AddDays(1),
                                                                                        BlockTimeframe.Hour);

                            // convert consumption by TDD dataprofile to blocks
                            var consumptionblocks = DataProfileHelpers.ConvertDataProfileToBlocks(consumption,
                                                                                                  BlockDirection.Consumed,
                                                                                                  BlockType.Simulated,
                                                                                                  firstmeasurespotDevice.Id,
                                                                                                  0,
                                                                                                  false,
                                                                                                  "",
                                                                                                  "",
                                                                                                  consumption.Id).ToList();

                            // add day consumption blocks to the device in Frist Measure Spot entity
                            eGrid.AddBlocksToEntity(firstmeasurespotDevice.Id, consumptionblocks);

                            // get two list of blocks after first phase of calculation,
                            // when first measured spot (connection place for PVE) consume imediatelly what it can
                            // the first list contains rest of the production after the consumption by entity.
                            // the second list contains rest of the consumption which cannot be covered by the PVE.
                            var firstmeasuredspotPhase1 = GetEntityBalanceBlocksAfterAlocationOfPVEBlocks(firstmeasurespot, eGrid, dtmp);

                            // change temporary blocks from pvesource and firstmeasurespot device to "Records" direction - not used in calcs
                            eGrid.ChangeAllBlocksType(firstmeasurespotDevice.Id, BlockType.Record, BlockType.Simulated);
                            eGrid.ChangeAllBlocksType(pvesource.Id, BlockType.Record, BlockType.Simulated);

                            // add day consumption blocks to the device in Frist Measure Spot entity after the consumption is covered by PVE in first entity
                            eGrid.AddBlocksToEntity(firstmeasurespotDevice.Id, firstmeasuredspotPhase1.Item2);
                            
                            // add day production blocks to the mainPVE entity based on allocation scheme
                            // if there is something over the alocation scheme peers values it is stored in network entity
                            eGrid.AddBlocksToEntity(network.Id, firstmeasuredspotPhase1.Item1, alocationScheme.Id);


                            /////////////////////////
                            // add another consumptions to the entities

                            var consumption1 = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[1],
                                                                                         dtmp,
                                                                                         dtmp.AddDays(1),
                                                                                         BlockTimeframe.Hour);
                            // convert consumption by TDD dataprofile to blocks
                            var consumptionblocks1 = DataProfileHelpers.ConvertDataProfileToBlocks(consumption,
                                                                                                   BlockDirection.Consumed,
                                                                                                   BlockType.Simulated,
                                                                                                   device.Id,
                                                                                                   0,
                                                                                                   false,
                                                                                                   "",
                                                                                                   "",
                                                                                                   consumption1.Id).ToList();
                            // add day consumption blocks to the device1 in devicegroup
                            eGrid.AddBlocksToEntity(device.Id, consumptionblocks1);

                            var consumption2 = ConsumersHelpers.GetConsumptionBasedOnTDD(tdds[2],
                                                                                         dtmp,
                                                                                         dtmp.AddDays(1),
                                                                                         BlockTimeframe.Hour);

                            // convert consumption by TDD dataprofile to blocks
                            var consumptionblocks2 = DataProfileHelpers.ConvertDataProfileToBlocks(consumption,
                                                                                                   BlockDirection.Consumed,
                                                                                                   BlockType.Simulated,
                                                                                                   device.Id,
                                                                                                   0,
                                                                                                   false,
                                                                                                   "",
                                                                                                   "",
                                                                                                   consumption2.Id).ToList();
                            // add day consumption blocks to the device3 in devicegroup1
                            eGrid.AddBlocksToEntity(device3.Id, consumptionblocks2);

                            //////////////////////////

                            ///////////////////////////
                            // calculate the rests for the entities and cover their consumption based on the alocation

                            var devicePhase2 = GetEntityBalanceBlocksAfterAlocationOfPVEBlocks(device, eGrid, dtmp);
                            // change temporary blocks in device to "Records" direction - not used in calcs
                            eGrid.ChangeAllBlocksType(device.Id, BlockType.Record, BlockType.Simulated);
                            eGrid.ChangeAllBlocksType(device.Id, BlockType.Record, BlockType.Simulated);
                            eGrid.AddBlocksToEntity(device.Id, devicePhase2.Item2);
                            // add rest of PVE production after consumption to network
                            eGrid.AddBlocksToEntity(network.Id, devicePhase2.Item1);

                            var device3Phase2 = GetEntityBalanceBlocksAfterAlocationOfPVEBlocks(device3, eGrid, dtmp);
                            // change temporary blocks in device to "Records" direction - not used in calcs
                            eGrid.ChangeAllBlocksType(device3.Id, BlockType.Record, BlockType.Simulated);
                            eGrid.ChangeAllBlocksType(device3.Id, BlockType.Record, BlockType.Simulated);
                            eGrid.AddBlocksToEntity(device3.Id, device3Phase2.Item2);
                            // add rest of PVE production after consumption to network
                            eGrid.AddBlocksToEntity(network.Id, device3Phase2.Item1);

                            /////////////////////////////
                            // calculate rest for charge and discharge storage

                            // calculate actual PVE rest in network
                            var productionPhase3 = eGrid.GetConsumptionOfEntity(network.Id,
                                                                                BlockTimeframe.Hour,
                                                                                dtmp,
                                                                                dtmp.AddDays(1),
                                                                                true,
                                                                                true,
                                                                                new List<BlockDirection>() { BlockDirection.Created },
                                                                                new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real });

                            var productionPhase3Profile = DataProfileHelpers.ConvertBlocksToDataProfile(productionPhase3);

                            // calculate actual PVE rest in network
                            var consumptionPhase3 = eGrid.GetConsumptionOfEntity(network.Id,
                                                                                 BlockTimeframe.Hour,
                                                                                 dtmp,
                                                                                 dtmp.AddDays(1),
                                                                                 true,
                                                                                 true,
                                                                                 new List<BlockDirection>() { BlockDirection.Consumed },
                                                                                 new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real });

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
                                                                            new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real });

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
                                                                                        new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed });

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
                                                                                        new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real });

                                    ////////////// whole day stats
                                    // total needed from Day tariff
                                    var totalDT = Math.Round(netwDT.Where(b => b.Amount < 0).Select(b => b.Amount).Sum(),4);
                                    // total over production during the day if some exists
                                    var totalDTOverProduction = Math.Round(netwDT.Where(b => b.Amount > 0).Select(b => b.Amount).Sum(), 4);
                                    // total needed from Night tariff
                                    var totalNT = Math.Round(netwNT.Where(b => b.Amount < 0).Select(b => b.Amount).Sum(), 4);
                                    // total over production during the night if some exists
                                    var totalNTOverProduction = Math.Round(netwNT.Where(b => b.Amount > 0).Select(b => b.Amount).Sum(), 4);

                                    // total produced from own PVE
                                    var totalproduced = Math.Round(production.ProfileData.Values.Sum(), 4);
                                    // total produced from own PVE over the consumption during the production phase of PVE
                                    var totalOverProducedAfterConsumedImmediately = Math.Round(productionPhase3Profile.ProfileData.Values.Sum(), 4);
                                    // total stored in storage from PVE production
                                    var totalStored = Math.Round(dch.ProfileData.Values.Sum(), 4);
                                    // total used from storage for consumption 
                                    var totalUsedFromStorage = Math.Round(ddisch.ProfileData.Values.Sum() / 1000, 4);
                                    
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
                                                     $"{totalUsedFromStorage}");

                                    // create line for export
                                    var line = $"{dtmp}";
                                    foreach (var v in netw)
                                        line += $"\t{Math.Round(v.Amount,4)}";

                                    lines.Add(line);
                                }
                            }
                            dtmp = dtmp.AddDays(1);
                            day++;
                        }
                    }
                }

                foreach(var line in lines)
                    FileHelpers.AppendLineToTextFile(line, filename);

                var header = "Date\t" +
                             "totalDT\t" +
                             "totalDTOverProduction\t" +
                             "totalNT\t" +
                             "totalNTOverProduction\t" +
                             "totalproduced\t" +
                             "totalOverProducedAfterConsumedImmediately\t" +
                             "totalStored\t" +
                             "totalUsedFromStorage";

                FileHelpers.AppendLineToTextFile(header, filenameBilance);
                foreach (var line in linesBilance)
                    FileHelpers.AppendLineToTextFile(line, filenameBilance);

                // export created blocks 
                var config = eGrid.ExportToConfig();
                if (config.Item1)
                    FileHelpers.WriteTextToFile($"{DateTime.UtcNow.ToString("yyyy_MM_ddThh_mm_ss")}-eGrid.json", config.Item2);
            }
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

                var productionblocks = DataProfileHelpers.ConvertDataProfileToBlocks(res.Item2,
                                                                                     BlockDirection.Created,
                                                                                     BlockType.Simulated,
                                                                                     entity.Id).ToList();

                result = (productionblocks, consumptionblocks);
            }
            return result;
        }

        public static (DataProfile,DataProfile) GetEntityBalanceAfterAlocationOfPVEBlocks(IEntity entity, IEntitiesHandler eGrid, DateTime dtmp)
        {
            // get bilance of the consumption and production
            var cons = eGrid.GetConsumptionOfEntity(entity.Id,
                                                    BlockTimeframe.Hour,
                                                    dtmp,
                                                    dtmp.AddDays(1),
                                                    true,
                                                    true,
                                                    new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                    new List<BlockType>() { BlockType.Simulated, BlockType.Calculated, BlockType.Real });

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
}
