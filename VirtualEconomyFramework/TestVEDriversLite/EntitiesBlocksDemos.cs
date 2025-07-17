using Org.BouncyCastle.Asn1.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VEDriversLite.Common;
using VEDriversLite.Common.Calendar;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Entities;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.Sources;
using VEDriversLite.EntitiesBlocks.StorageCalculations;
using VEDriversLite.EntitiesBlocks.Tree;
using WordPressPCL.Models;

namespace TestVEDriversLite
{
    public static class EntitiesBlocksDemos
    {

        [TestEntry]
        public static void EB_SmallHotelDemo(string param)
        {
            EB_SmallHotelDemoAsync(param);
        }
        public static async Task EB_SmallHotelDemoAsync(string param)
        {

            await CalculateHotelNetwork();
        }

        private static async Task CalculateHotelNetwork()
        {
            // SET Coordinates of PVE
            Coordinates coord = new Coordinates(12.097178, -68.914773);
            // SET Start of the simulation
            var start = new DateTime(2022, 1, 3); // 3rd of January 2022 is Monday
            // SET Number of days which you want to simulate
            var daysOfSimulation = 7;
            // SET output timeframe which will be calculated as step
            var outputTimeframe = BlockTimeframe.Hour;
            // create simulator objects
            var eGrid = new BaseEntitiesHandler();
            var PVESim = new PVPanelsGroupsHandler();

            var owner = "hotel";

            var filename = $"{DateTime.UtcNow.ToString("yyyy_MM_ddThh_mm_ss")}-{owner}-blocks.csv";

            var powerOfAC = 1.5;
            var powerOfFridge = 0.12;
            var numberOfRooms = 30;
            var averageOccupancy = 0.6;
            var numberOfOffices = 2;

            ////////////////////////////////////////////////
            #region MainEntity
            var name = "network";
            var networkId = string.Empty;
            var network = new GroupNetwork() { Type = EntityType.Consumer, Name = name, ParentId = owner };
            var res = eGrid.AddEntity(network, name, owner);
            if (res.Item1)
                networkId = res.Item2.Item2;

            #endregion
            ////////////////////////////////////////////////
            #region Rooms

            name = "rooms";
            var roomsGroupId = string.Empty;
            var roomsGroup = new GroupNetwork() { Type = EntityType.Consumer, Name = name, ParentId = networkId };
            res = eGrid.AddEntity(roomsGroup, name, owner);
            if (res.Item1)
                roomsGroupId = res.Item2.Item2;

            name = "allrooms";
            var allroomsId = string.Empty;
            var allRoomsEnt = new Device() { Type = EntityType.Consumer, Name = name, ParentId = roomsGroupId };
            res = eGrid.AddEntity(allRoomsEnt, name, owner);
            if (res.Item1)
                allroomsId = res.Item2.Item2;

            eGrid.AddSubEntityToEntity(networkId, roomsGroupId);
            eGrid.AddSubEntityToEntity(roomsGroupId, allroomsId);

            double[] acRun = new double[24]
            {
                0.2, 0.2, 0.2, 0.2, 0.2, 0.3, 0.3, 0.4, 0.4, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.3, 0.4, 0.6, 0.6, 0.4, 0.4, 0.4, 0.3
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            double[] fridgeRun = new double[24]
            {
                0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var acSim = new DeviceSimulator(acRun, powerOfAC * numberOfRooms * averageOccupancy);
            _ = allRoomsEnt.AddSimulator(acSim);
            var fridgeSim = new DeviceSimulator(fridgeRun, powerOfFridge * numberOfRooms * averageOccupancy);
            _ = allRoomsEnt.AddSimulator(fridgeSim);

            #endregion
            ////////////////////////////////////////////////

            ////////////////////////////////////////////////
            #region SharedSpaces

            name = "shared";
            var sharedGroupId = string.Empty;
            var sharedGroup = new GroupNetwork() { Type = EntityType.Consumer, Name = name, ParentId = networkId };
            res = eGrid.AddEntity(sharedGroup, name, owner);
            if (res.Item1)
                sharedGroupId = res.Item2.Item2;

            name = "pools";
            var poolsId = string.Empty;
            var poolsEnt = new Device() { Type = EntityType.Consumer, Name = name, ParentId = sharedGroupId };
            res = eGrid.AddEntity(poolsEnt, name, owner);
            if (res.Item1)
                poolsId = res.Item2.Item2;

            name = "kitchen";
            var kitchenId = string.Empty;
            var kitchenEnt = new Device() { Type = EntityType.Consumer, Name = name, ParentId = sharedGroupId };
            res = eGrid.AddEntity(kitchenEnt, name, owner);
            if (res.Item1)
                kitchenId = res.Item2.Item2;

            name = "laundry";
            var laundryId = string.Empty;
            var laundryEnt = new Device() { Type = EntityType.Consumer, Name = name, ParentId = sharedGroupId };
            res = eGrid.AddEntity(laundryEnt, name, owner);
            if (res.Item1)
                laundryId = res.Item2.Item2;

            name = "mosquitoTraps";
            var mosquitoTrapsId = string.Empty;
            var mosquitoTrapsEnt = new Device() { Type = EntityType.Consumer, Name = name, ParentId = sharedGroupId };
            res = eGrid.AddEntity(mosquitoTrapsEnt, name, owner);
            if (res.Item1)
                mosquitoTrapsId = res.Item2.Item2;


            eGrid.AddSubEntityToEntity(networkId, sharedGroupId);
            eGrid.AddSubEntityToEntity(sharedGroupId, poolsId);
            eGrid.AddSubEntityToEntity(sharedGroupId, kitchenId);
            eGrid.AddSubEntityToEntity(sharedGroupId, laundryId);
            eGrid.AddSubEntityToEntity(sharedGroupId, mosquitoTrapsId);

            double[] pump = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, 0.0, 0.8, 0.0, 0.8, 0.0, 0.0, 0.0, 0.3, 0.3, 0.3, 0.8, 0.0, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var pumpSim = new DeviceSimulator(pump, 2);
            _ = poolsEnt.AddSimulator(pumpSim);

            double[] cooker = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.7, 0.7, 0.0, 0.4, 0.8, 0.3, 0.0, 0.0, 0.0, 0.4, 0.7, 0.5, 0.0, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            double[] othersKitchen = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.1, 0.2, 0.3, 0.1, 0.2, 0.5, 0.4, 0.2, 0.1, 0.2, 0.3, 0.5, 0.5, 0.3, 0.1, 0.1, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var kitchenSim = new DeviceSimulator(cooker, 2 * averageOccupancy);
            _ = kitchenEnt.AddSimulator(kitchenSim);
            var otherKitchenSim = new DeviceSimulator(othersKitchen, 2 * averageOccupancy );
            _ = kitchenEnt.AddSimulator(otherKitchenSim);


            double[] washingMachine = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            double[] dryer = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.6, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var washingMachineSimulator = new DeviceSimulator(washingMachine, 1 * averageOccupancy, Week.WorkDays);
            _ = laundryEnt.AddSimulator(washingMachineSimulator);
            var dryerSimulator = new DeviceSimulator(dryer, 1 * averageOccupancy, Week.WorkDays);
            _ = laundryEnt.AddSimulator(dryerSimulator);

            double[] mosquitoTrap = new double[24]
            {
                1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0
             //  00,  01,  02,  03,  04,  05,  06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var mosquitoTrapsSim = new DeviceSimulator(mosquitoTrap, 0.03 * 4);
            _ = mosquitoTrapsEnt.AddSimulator(mosquitoTrapsSim);

            #endregion
            ////////////////////////////////////////////////

            ////////////////////////////////////////////////
            #region Offices
            name = "officesGroup";
            var officesGroupId = string.Empty;
            var officesGroup = new GroupNetwork() { Type = EntityType.Consumer, Name = name, ParentId = networkId };
            res = eGrid.AddEntity(officesGroup, name, owner);
            if (res.Item1)
                officesGroupId = res.Item2.Item2;

            name = "offices";
            var officesId = string.Empty;
            var officesEnt = new Device() { Type = EntityType.Consumer, Name = name, ParentId = officesGroupId };
            res = eGrid.AddEntity(officesEnt, name, owner);
            if (res.Item1)
                officesId = res.Item2.Item2;

            eGrid.AddSubEntityToEntity(networkId, officesGroupId);
            eGrid.AddSubEntityToEntity(officesGroupId, officesId);

            double[] acOfficeRun = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, 0.3, 0.4, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.4, 0.3, 0.3, 0.2, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            double[] itTech = new double[24]
            {
                0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.1, 0.1, 0.1
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            var acOfficeSim = new DeviceSimulator(acOfficeRun, powerOfAC * numberOfOffices);
            _ = officesEnt.AddSimulator(acOfficeSim);
            var itSim = new DeviceSimulator(itTech, numberOfOffices);
            _ = officesEnt.AddSimulator(itSim);

            #endregion
            ////////////////////////////////////////////////

            ////////////////////////////////////////////////
            #region PVESource

            name = "pvesource";
            var pvesourceId = string.Empty;
            var pvesource = new PVESource() { Type = EntityType.Source, Name = name, ParentId = networkId };
            res = eGrid.AddEntity(pvesource, name, owner);
            if (res.Item1)
                pvesourceId = res.Item2.Item2;

            eGrid.AddSubEntityToEntity(networkId, pvesourceId);

            var panelAzimuthE = MathHelpers.DegreeToRadians(-20);
            var panelAzimuthS = MathHelpers.DegreeToRadians(0);
            var panelAzimuthW = MathHelpers.DegreeToRadians(20);

            var eastPanelsId = PVESim.AddGroup("East");
            var southPanelsId = PVESim.AddGroup("South");
            var westPanelsId = PVESim.AddGroup("West");

            // setup common panel parameters
            if (PVESim != null)
            {
                var panel = new PVPanel()
                {
                    Name = "test",
                    Azimuth = 0,
                    BaseAngle = MathHelpers.DegreeToRadians(15),
                    DirtRatio = 0.05 / 365,
                    Efficiency = 1,
                    Height = 2000,
                    Width = 1000,
                    Latitude = coord.Latitude,
                    Longitude = coord.Longitude,
                    PeakPower = 0.3,
                    PanelPeakAngle = MathHelpers.DegreeToRadians(90)
                };
                PVESim.SetCommonPanel(panel);
            }

            var numOfPanelsInString = 20;

            // add panels in this PVE Simulator
            PVESim.CommonPanel.Azimuth = panelAzimuthE;
            _ = PVESim.AddPanelToGroup(eastPanelsId, PVESim.CommonPanel, numOfPanelsInString).ToList();
            PVESim.CommonPanel.Azimuth = panelAzimuthS;
            _ = PVESim.AddPanelToGroup(southPanelsId, PVESim.CommonPanel, numOfPanelsInString).ToList();
            PVESim.CommonPanel.Azimuth = panelAzimuthW;
            _ = PVESim.AddPanelToGroup(westPanelsId, PVESim.CommonPanel, numOfPanelsInString).ToList();
            
            // add PVE simulator to entity
            eGrid.AddSimulatorToEntity(pvesourceId, PVESim);

            #endregion
            ////////////////////////////////////////////////

            // print Entities tree

            await Console.Out.WriteLineAsync("-------------------Entities Tree------------------------");
            var tree = eGrid.GetTree(networkId);
            TreeViewHelpers.PrintTree(tree, "\n", false);

            await Console.Out.WriteLineAsync("--------------------------------------------------------");
            // print PVE info

            await Console.Out.WriteLineAsync("---------------------PVE Info------------------------");
            await Console.Out.WriteLineAsync($"Total Peak Power of PVE: {Math.Round(PVESim.TotalPeakPower,2)} kWp");
            await Console.Out.WriteLineAsync($"Total Number Of panels: {PVESim.PanelCount}");
            await Console.Out.WriteLineAsync($"Dimension of one common panel: Width: {Math.Round((PVESim.CommonPanel.Width / 1000),3)} m, Height: {Math.Round((PVESim.CommonPanel.Height / 1000),3)} m");
            await Console.Out.WriteLineAsync($"Area of one common panel: {Math.Round((PVESim.CommonPanel.Width / 1000) * (PVESim.CommonPanel.Height / 1000),3)} m2");
            await Console.Out.WriteLineAsync($"Total Panels Area: {Math.Round(PVESim.PanelCount * (PVESim.CommonPanel.Width / 1000) * (PVESim.CommonPanel.Height / 1000), 3)} m2");
            await Console.Out.WriteLineAsync("--------------------------------------------------------");

            // calculate bilance in specific range
            var bilance = eGrid.GetConsumptionOfEntity(networkId,
                                                       outputTimeframe,
                                                       start,
                                                       start.AddDays(daysOfSimulation),
                                                       true,
                                                       true,
                                                       new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                       new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Bilance", bilance, start, start.AddDays(daysOfSimulation));

            var bilanceProfile = DataProfileHelpers.ConvertBlocksToDataProfile(bilance);

            // calculate consumption in specific range
            var consumption = eGrid.GetConsumptionOfEntity(networkId,
                                                           outputTimeframe,
                                                           start,
                                                           start.AddDays(daysOfSimulation),
                                                           true,
                                                           true,
                                                           new List<BlockDirection>() { BlockDirection.Consumed },
                                                           new List<BlockType>() { BlockType.Simulated });
            
            await DrawBlocks("Consumption", consumption, start, start.AddDays(daysOfSimulation));

            // calculate production in specific range
            var production = eGrid.GetConsumptionOfEntity(networkId,
                                                          outputTimeframe,
                                                          start,
                                                          start.AddDays(daysOfSimulation),
                                                          true,
                                                          false,
                                                          new List<BlockDirection>() { BlockDirection.Created },
                                                          new List<BlockType>() { BlockType.Simulated });
            
            await DrawBlocks("Production", production, start, start.AddDays(daysOfSimulation));

            // calculate consumption of rooms in specific range
            var consumptionOfRooms = eGrid.GetConsumptionOfEntity(allroomsId,
                                                                  outputTimeframe,
                                                                  start,
                                                                  start.AddDays(daysOfSimulation),
                                                                  true,
                                                                  true,
                                                                  new List<BlockDirection>() { BlockDirection.Consumed },
                                                                  new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Consumption Of Rooms", consumptionOfRooms, start, start.AddDays(daysOfSimulation));

            // calculate consumption of offices in specific range
            var consumptionOfOffices = eGrid.GetConsumptionOfEntity(officesId,
                                                                    outputTimeframe,
                                                                    start,
                                                                    start.AddDays(daysOfSimulation),
                                                                    true,
                                                                    true,
                                                                    new List<BlockDirection>() { BlockDirection.Consumed },
                                                                    new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Consumption Of Offices", consumptionOfOffices, start, start.AddDays(daysOfSimulation));

            // calculate consumption of shared spaces in specific range
            var consumptionOfShared = eGrid.GetConsumptionOfEntity(sharedGroupId,
                                                                   outputTimeframe,
                                                                   start,
                                                                   start.AddDays(daysOfSimulation),
                                                                   true,
                                                                   true,
                                                                   new List<BlockDirection>() { BlockDirection.Consumed },
                                                                   new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Consumption Of Shared spaces", consumptionOfShared, start, start.AddDays(daysOfSimulation));

            // calculate consumption of laundry in specific range
            var consumptionOfLaundry = eGrid.GetConsumptionOfEntity(laundryId,
                                                                   outputTimeframe,
                                                                   start,
                                                                   start.AddDays(daysOfSimulation),
                                                                   true,
                                                                   true,
                                                                   new List<BlockDirection>() { BlockDirection.Consumed },
                                                                   new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Consumption Of Laundry", consumptionOfLaundry, start, start.AddDays(daysOfSimulation));

            // calculate consumption of electric mosquito traps in specific range
            var consumptionOfMosquitoTraps = eGrid.GetConsumptionOfEntity(mosquitoTrapsId,
                                                                   outputTimeframe,
                                                                   start,
                                                                   start.AddDays(daysOfSimulation),
                                                                   true,
                                                                   true,
                                                                   new List<BlockDirection>() { BlockDirection.Consumed },
                                                                   new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Consumption Of Mosquito Traps", consumptionOfMosquitoTraps, start, start.AddDays(daysOfSimulation));

            /////////////////////////////////////////
            // Save values to the file
            #region SaveValues

            var header = "Date\t" +
                         "Start\t" +
                         "End\t" +
                         "Bilance\t" +
                         "Consumption\t" +
                         "Production\t" +
                         "Rooms\t" +
                         "Offices\t" +
                         "Shared Spaces\t" +
                         "Laundry\t" +
                         "Mosquito Traps";

            FileHelpers.AppendLineToTextFile(header, filename);
            for (var i = 0; i < bilance.Count; i++)
            {
                var line = $"{bilance.ElementAt(i).StartTime.ToString("yyyy:MM:dd")}\t" +
                           $"{bilance.ElementAt(i).StartTime.ToString("hh:mm:ss")}\t" +
                           $"{bilance.ElementAt(i).EndTime.ToString("hh:mm:ss")}\t" +
                           $"{Math.Round(bilance.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(consumption.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(production.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(consumptionOfRooms.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(consumptionOfOffices.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(consumptionOfShared.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(consumptionOfLaundry.ElementAt(i).Amount,2)}\t" +
                           $"{Math.Round(consumptionOfMosquitoTraps.ElementAt(i).Amount,2)}";

                FileHelpers.AppendLineToTextFile(line, filename);
            }

            var export = eGrid.ExportToConfig();
            if (export.Item1)
                FileHelpers.WriteTextToFile("eGrid-BocaSimon-02.json", export.Item2);
            var exportpve = PVESim.ExportConfig();
            if (exportpve.Item1)
                FileHelpers.WriteTextToFile("PVESim-BocaSimon-02.json", exportpve.Item2);

            #endregion
            /////////////////////////////////////////////////////
        }

        private static async Task DrawBlocks(string heading, IReadOnlyCollection<IBlock> blocks, DateTime start, DateTime end)
        {
            await Console.Out.WriteLineAsync($"-------------------{heading}--------------------:");
            await Console.Out.WriteLineAsync("Results:");
            await Console.Out.WriteLineAsync($"StartTime: {start}");
            await Console.Out.WriteLineAsync($"EndTime: {end}");
            await Console.Out.WriteLineAsync($"Calculated Data:");
            var total = 0.0;
            foreach (var block in blocks)
            {
                await Console.Out.WriteLineAsync($"\t{block.StartTime.ToString("yyyy_MM_dd-hh:mm")} - {block.EndTime.ToString("yyyy_MM_dd-hh:mm")}, Amount: {Math.Round(block.Amount, 2)} kWh.");
                total += block.Amount;
            }
            await Console.Out.WriteLineAsync($"Total Bilance: {total} kWh");
            await Console.Out.WriteLineAsync("--------------------END------------------:");

        }

    }
}