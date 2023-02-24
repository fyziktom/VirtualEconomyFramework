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
            var start = new DateTime(2022, 1, 1);
            // SET Number of days which you want to simulate
            var daysOfSimulation = 31;
            // create simulator objects
            var eGrid = new BaseEntitiesHandler();
            var PVESim = new PVPanelsGroupsHandler();
            var StorageSim = new BatteryBlockHandler(null,
                                                     "battery",
                                                     BatteryBlockHandler.DefaultChargingFunction,
                                                     BatteryBlockHandler.DefaultDischargingFunction);

            var owner = "hotel";

            var powerOfAC = 2;
            var powerOfFridge = 0.12;
            var numberOfRooms = 30;
            var averageOccupancy = 0.6;
            var numberOfOffices = 2;

            //////////////////////////////////////////////////
            #region MainEntity
            var name = "network";
            var networkId = string.Empty;
            var network = new GroupNetwork() { Type = EntityType.Consumer, Name = name, ParentId = owner };
            var res = eGrid.AddEntity(network, name, owner);
            if (res.Item1)
                networkId = res.Item2.Item2;

            #endregion

            /////////////////////////////////////////////////
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
                0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0.3, 0.4, 0.5, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.4, 0.6, 0.7, 0.7, 0.5, 0.5, 0.4, 0.3
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
            //////////////////////////////////////////////

            //////////////////////////////////////////////
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

            eGrid.AddSubEntityToEntity(networkId, sharedGroupId);
            eGrid.AddSubEntityToEntity(sharedGroupId, poolsId);
            eGrid.AddSubEntityToEntity(sharedGroupId, kitchenId);

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

            var kitchenSim = new DeviceSimulator(cooker, 2);
            _ = kitchenEnt.AddSimulator(kitchenSim);
            var otherKitchenSim = new DeviceSimulator(othersKitchen, 2);
            _ = kitchenEnt.AddSimulator(otherKitchenSim);

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
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.7, 0.3, 0.4, 0.6, 0.6, 0.6, 0.5, 0.5, 0.5, 0.4, 0.3, 0.3, 0.2, 0.0, 0.0, 0.0
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

            var panelAzimuthE = MathHelpers.DegreeToRadians(-10);
            var panelAzimuthS = MathHelpers.DegreeToRadians(0);
            var panelAzimuthW = MathHelpers.DegreeToRadians(10);

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
                    BaseAngle = MathHelpers.DegreeToRadians(23),
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
            /////////////////////////////////////////////////////////

            // calculate bilance in specific range
            var bilance = eGrid.GetConsumptionOfEntity(networkId,
                                                       BlockTimeframe.Hour,
                                                       start,
                                                       start.AddDays(1),
                                                       true,
                                                       true,
                                                       new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                       new List<BlockType>() { BlockType.Simulated });

            await DrawBlocks("Bilance", bilance, start, start.AddDays(1));

            var bilanceProfile = DataProfileHelpers.ConvertBlocksToDataProfile(bilance);

            // calculate consumption in specific range
            var consumption = eGrid.GetConsumptionOfEntity(networkId,
                                                           BlockTimeframe.Hour,
                                                           start,
                                                           start.AddDays(1),
                                                           true,
                                                           true,
                                                           new List<BlockDirection>() { BlockDirection.Consumed },
                                                           new List<BlockType>() { BlockType.Simulated });
            
            await DrawBlocks("Consumption", consumption, start, start.AddDays(1));

            // calculate production in specific range
            var production = eGrid.GetConsumptionOfEntity(networkId,
                                                          BlockTimeframe.Hour,
                                                          start,
                                                          start.AddDays(1),
                                                          true,
                                                          false,
                                                          new List<BlockDirection>() { BlockDirection.Created },
                                                          new List<BlockType>() { BlockType.Simulated });
            
            await DrawBlocks("Production", production, start, start.AddDays(1));

            var export = eGrid.ExportToConfig();
            if (export.Item1)
                FileHelpers.WriteTextToFile("eGrid-BocaSimon-02.json", export.Item2);
            var exportpve = PVESim.ExportConfig();
            if (exportpve.Item1)
                FileHelpers.WriteTextToFile("PVESim-BocaSimon-02.json", exportpve.Item2);
        }

        private static async Task DrawBlocks(string heading, List<IBlock> blocks, DateTime start, DateTime end)
        {
            await Console.Out.WriteLineAsync($"-------------------{heading}--------------------:");
            await Console.Out.WriteLineAsync("Results:");
            await Console.Out.WriteLineAsync($"StartTime: {start}");
            await Console.Out.WriteLineAsync($"EndTime: {end}");
            await Console.Out.WriteLineAsync($"Calculated Data:");
            foreach (var block in blocks)
            {
                await Console.Out.WriteLineAsync($"\t{block.StartTime} - {block.EndTime}, Amount: {block.Amount} kWh.");
            }
            await Console.Out.WriteLineAsync("--------------------END------------------:");

        }

    }
}