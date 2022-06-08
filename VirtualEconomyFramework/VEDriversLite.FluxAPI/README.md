# VEDriversLite Flux API wrapper

This is .NET 6.0 API wrapper for [Flux Blockchain](https://runonflux.io/) API

## Nuget Package

You can download the [Nuget package here](https://www.nuget.org/packages/VEFramework.VEDriversLite.FluxAPI)

# Example Usage

1. Create empty console Application


```code
mkdir CreateFluxAPIExample
cd CreateFluxAPIExample
dotnet new console
```

2. Add project reference or nuget package 


```code 
dotnet add package VEFramework.VEDriversLite.FluxAPI
```


```csharp

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var addr = "t1edbnsYnGXxyYNt4kJJPhZktDWNaKwkznY";

Console.WriteLine("Address Utxos:");
Console.WriteLine("---------------------------------------------------------------------------------------------");

var utxos = await VEDriversLite.FluxAPI.FluxAPIHelpers.GetListOfAddressUtxos(addr);
if (utxos != null)
{
    Console.WriteLine("Address Utxos:");
    foreach (var u in utxos)
        Console.WriteLine($"Utxo TxId: {u.TxId}:{u.Index}, Satoshi: {u.Value}");
}

var balance = await VEDriversLite.FluxAPI.FluxAPIHelpers.GetListOfAddressBalance(addr);
Console.WriteLine($"Address Balance: {balance} Flux satoshi");
Console.WriteLine("---------------------------------------------------------------------------------------------");

var list = await VEDriversLite.FluxAPI.FluxAPIHelpers.GetListOfAppsLocations("Veframe");

if (list != null)
{
    Console.WriteLine("Flux VENFT App Server locations:");
    foreach (var item in list)
    {
        Console.WriteLine("---------------------------------------------------------------------------------------------");
        Console.WriteLine($"Location: {item.IP} - Broadcasted At: {item.BroadcastedAt}, Expiration At: {item.ExpireAt}");
        Console.WriteLine($"Open VENFT Api Swagger utility for this location: http://{item.IP}:34909/swagger/index.html ");
        Console.WriteLine("---------------------------------------------------------------------------------------------");
        Console.WriteLine("");
    }

    var firstLocation = list.FirstOrDefault();
    if (firstLocation != null)
    {
        Console.WriteLine("Detailed info for the first location:");
        Console.WriteLine("---------------------------------------------------------------------------------------------");
        var ftx = await VEDriversLite.FluxAPI.FluxAPIHelpers.GetListOfNodeFluxTransactions(firstLocation.IP);
        if (ftx != null)
        {
            Console.WriteLine("Flux Transactions:");
            foreach (var f in ftx)
                Console.WriteLine($"Flux Transaction - Type: {f.Type}, UpdateType: {f.UpdateType}, TxId: {f.TxId}:{f.Index}, BenchTier: {f.BenchTier}");

        }
    }
}
else
{
    Console.WriteLine("No locations found.");
}
Console.ReadLine();
Console.WriteLine("Press any key to exit...");
```

### Result

![](https://ipfs.infura.io/ipfs/QmbqZYZAea4jxmGopnCWJqz8thLuzvqPe7XbPVpxLryq8S)
