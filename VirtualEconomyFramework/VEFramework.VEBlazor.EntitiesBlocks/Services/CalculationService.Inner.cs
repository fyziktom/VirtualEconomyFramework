using System;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Handlers;
using VEDriversLite.EntitiesBlocks.PVECalculations;
using VEDriversLite.EntitiesBlocks.Sources;
using VEDriversLite.EntitiesBlocks.StorageCalculations;

namespace VEFramework.VEBlazor.EntitiesBlocks.Services;

public partial class CalculationService
{
    private Task DoCalculation(string filteredEntitiesConfig, string filteredPveConfig, string filteredStorageConfig,
        decimal budget, decimal interestRate, DateTime endDate, IDictionary<string, bool> deviceLeadingMap)
    {

        // create simulator objects
        var eGrid = new BaseEntitiesHandler();
        var PVESim = new PVPanelsGroupsHandler();
        var StorageSim = new BatteryBlockHandler(null,
                                                 "battery",
                                                 BatteryBlockHandler.DefaultChargingFunction,
                                                 BatteryBlockHandler.DefaultDischargingFunction);

        // load simulators
        if (!eGrid.LoadFromConfig(filteredEntitiesConfig).Item1) return Task.CompletedTask;
        if (!PVESim.ImportConfigFromJson(filteredPveConfig)) return Task.CompletedTask;
        if (!StorageSim.ImportConfigFromJson(filteredStorageConfig)) return Task.CompletedTask;

        var network = eGrid.FindEntityByName("network");
        var pvesource = eGrid.FindEntityByName("mainPVE");
        var firstmeasurespot = eGrid.FindEntityByName("FirstMeasureSpot");
        var firstmeasurespotDevice = eGrid.FindEntityByName("firstspotdevice");
        var mainbattery = eGrid.FindEntityByName("mainbattery");


        Console.WriteLine("Running calculation");
        return Task.CompletedTask;
    }
}