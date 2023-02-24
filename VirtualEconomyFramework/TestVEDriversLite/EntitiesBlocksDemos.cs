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


        #region DataProfileCreators

        private static DataProfile GetACandFridgesDataProfile(double powerAC, double powerFridge, int numOfRooms, DateTime start, int days)
        {
            var profile = new DataProfile();

            if (start == DateTime.MinValue)
                start = new DateTime(2023, 1, 1);

            var tmp = start;
            for (var i = 0; i < days; i++)
            {
                var dayhours = tmp;

                var dayConsumption = GetDayRoomConsumption(powerAC, powerFridge, numOfRooms);
                for (var j = 0; j < 24; j++)
                {
                    profile.ProfileData.Add(dayhours, dayConsumption[j]);
                    dayhours = dayhours.AddHours(1);
                }

                tmp = tmp.AddDays(1);
            }

            return profile;
        }

        private static double[] GetDayRoomConsumption(double powerAC, double powerFridge, int numberOfRooms = 1)
        {
            double[] dayconsumption = new double[24];

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

            for (var i = 0; i < 24; i++)
                dayconsumption[i] = (powerAC * acRun[i] + powerFridge * fridgeRun[i]) * numberOfRooms;

            return dayconsumption;
        }

        private static DataProfile GetOfficesDataProfile(double powerAC, int numOfOffices, DateTime start, int days)
        {
            var profile = new DataProfile();

            if (start == DateTime.MinValue)
                start = new DateTime(2023, 1, 1);

            var tmp = start;
            for (var i = 0; i < days; i++)
            {
                var dayhours = tmp;

                var dayConsumption = GetOfficeConsumption(powerAC, numOfOffices);
                for (var j = 0; j < 24; j++)
                {
                    profile.ProfileData.Add(dayhours, dayConsumption[j]);
                    dayhours = dayhours.AddHours(1);
                }

                tmp = tmp.AddDays(1);
            }

            return profile;
        }
        private static double[] GetOfficeConsumption(double powerAC, int numberOfOffices = 1)
        {
            double[] dayconsumption = new double[24];

            double[] acRun = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.7, 0.3, 0.4, 0.6, 0.6, 0.6, 0.5, 0.5, 0.5, 0.4, 0.3, 0.3, 0.2, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            double[] itTech = new double[24]
            {
                0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.1, 0.1, 0.1
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            for (var i = 0; i < 24; i++)
                dayconsumption[i] = (powerAC * acRun[i] + itTech[i]) * numberOfOffices;

            return dayconsumption;
        }

        private static DataProfile GetPoolDataProfile(double pumpPower, DateTime start, int days)
        {
            var profile = new DataProfile();

            if (start == DateTime.MinValue)
                start = new DateTime(2023, 1, 1);

            var tmp = start;
            for (var i = 0; i < days; i++)
            {
                var dayhours = tmp;

                var dayConsumption = GetPoolConsumption(pumpPower);
                for (var j = 0; j < 24; j++)
                {
                    profile.ProfileData.Add(dayhours, dayConsumption[j]);
                    dayhours = dayhours.AddHours(1);
                }

                tmp = tmp.AddDays(1);
            }

            return profile;
        }

        private static double[] GetPoolConsumption(double pumpPower)
        {
            double[] dayconsumption = new double[24];

            double[] pump = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, 0.0, 0.8, 0.0, 0.8, 0.0, 0.0, 0.0, 0.3, 0.3, 0.3, 0.8, 0.0, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            for (var i = 0; i < 24; i++)
                dayconsumption[i] = (pumpPower * pump[i]);

            return dayconsumption;
        }

        private static DataProfile GetKitchenDataProfile(int numOfKitchens, DateTime start, int days)
        {
            var profile = new DataProfile();

            if (start == DateTime.MinValue)
                start = new DateTime(2023, 1, 1);

            var tmp = start;
            for (var i = 0; i < days; i++)
            {
                var dayhours = tmp;

                var dayConsumption = GetKitchenConsumption(numOfKitchens);
                for (var j = 0; j < 24; j++)
                {
                    profile.ProfileData.Add(dayhours, dayConsumption[j]);
                    dayhours = dayhours.AddHours(1);
                }

                tmp = tmp.AddDays(1);
            }

            return profile;
        }
        private static double[] GetKitchenConsumption(int numberOfKitchens = 1)
        {
            double[] dayconsumption = new double[24];

            double[] cooker = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.7, 0.7, 0.0, 0.4, 0.8, 0.3, 0.0, 0.0, 0.0, 0.4, 0.7, 0.5, 0.0, 0.0, 0.0, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            double[] others = new double[24]
            {
                0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.1, 0.2, 0.3, 0.1, 0.2, 0.5, 0.4, 0.2, 0.1, 0.2, 0.3, 0.5, 0.5, 0.3, 0.1, 0.1, 0.0
             //  00,  01,  02,  03,  04,  05, 06,  07,  08,  09,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23 
            };

            for (var i = 0; i < 24; i++)
                dayconsumption[i] = (cooker[i] + others[i]) * numberOfKitchens;

            return dayconsumption;
        }

        #endregion 

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
            var name = "network";
            var networkId = string.Empty;
            var network = new GroupNetwork() { Type = EntityType.Consumer, Name = name, ParentId = owner };
            var res = eGrid.AddEntity(network, name, owner);
            if (res.Item1)
                networkId = res.Item2.Item2;

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

            var dataprofile = GetACandFridgesDataProfile(2, 0.12, 30, start, daysOfSimulation);
            var blocks = DataProfileHelpers.ConvertDataProfileToBlocks(dataprofile,
                                                                       BlockDirection.Consumed,
                                                                       BlockType.Simulated,
                                                                       allroomsId,
                                                                       0,
                                                                       false,
                                                                       "ACandFridge",
                                                                       "Consmption of AC and refrigerator",
                                                                       "",
                                                                       "");

            if (allRoomsEnt.AddBlocks(blocks.ToList()))
                await Console.Out.WriteLineAsync("Simulated blocks of AC and Refrigerators added to the AllRoomsEntity.");

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

            var dppool = GetPoolDataProfile(2, start, daysOfSimulation);
            var poolBlocks = DataProfileHelpers.ConvertDataProfileToBlocks(dppool,
                                                                           BlockDirection.Consumed,
                                                                           BlockType.Simulated,
                                                                           allroomsId,
                                                                           0,
                                                                           false,
                                                                           "Pool",
                                                                           "Consmption of Pool pump",
                                                                           "",
                                                                           "");

            if (poolsEnt.AddBlocks(poolBlocks.ToList()))
                await Console.Out.WriteLineAsync("Simulated blocks of Pool added to the Pool Entity.");

            var dpkitchen = GetKitchenDataProfile(1, start, daysOfSimulation);
            var kitchenBlocks = DataProfileHelpers.ConvertDataProfileToBlocks(dpkitchen,
                                                                              BlockDirection.Consumed,
                                                                              BlockType.Simulated,
                                                                              allroomsId,
                                                                              0,
                                                                              false,
                                                                              "Pool",
                                                                              "Consmption of Pool pump",
                                                                              "",
                                                                              "");

            if (kitchenEnt.AddBlocks(kitchenBlocks.ToList()))
                await Console.Out.WriteLineAsync("Simulated blocks of Kitchen added to the Kitchen Entity.");

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

            var dpoffices = GetOfficesDataProfile(2, 2, start, daysOfSimulation);
            var officesBlocks = DataProfileHelpers.ConvertDataProfileToBlocks(dpoffices,
                                                                              BlockDirection.Consumed,
                                                                              BlockType.Simulated,
                                                                              allroomsId,
                                                                              0,
                                                                              false,
                                                                              "Offices",
                                                                              "Consmption of AC and IT in offices.",
                                                                              "",
                                                                              "");

            if (officesEnt.AddBlocks(officesBlocks.ToList()))
                await Console.Out.WriteLineAsync("Simulated blocks of Offices added to the Offices Entity.");


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

            // calculate consumption in specific range
            var bilance = eGrid.GetConsumptionOfEntity(networkId,
                                                       BlockTimeframe.Hour,
                                                       start,
                                                       start.AddDays(1),
                                                       true,
                                                       true,
                                                       new List<BlockDirection>() { BlockDirection.Created, BlockDirection.Consumed },
                                                       new List<BlockType>() { BlockType.Simulated });

            var bilanceProfile = DataProfileHelpers.ConvertBlocksToDataProfile(bilance);

            var export = eGrid.ExportToConfig();
            if (export.Item1)
                FileHelpers.WriteTextToFile("eGrid-BocaSimon-02.json", export.Item2);
            var exportpve = PVESim.ExportConfig();
            if (exportpve.Item1)
                FileHelpers.WriteTextToFile("PVESim-BocaSimon-02.json", exportpve.Item2);
        }

    }
}