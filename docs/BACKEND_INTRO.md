# Seznámení pro backend vývojáře

Dozvíte se, jak využít [`VEDriversLite`](https://www.nuget.org/packages/VEFramework.VEDriversLite/) k vytvoření konzolové aplikace nebo tvorbě vlastní služby.

_Nepleťte si [`VEDriversLite`](../VirtualEconomyFramework/VEDriversLite) s [`VEDrivers`](../VirtualEconomyFramework/VEDrivers)_
   - `VEDriversLite` = aktuální drivery, používejte je
   - `VEDrivers` = zastaralé, vyžadují údržbu, jsou mnohem komplexnější, podívejte se na ně, až zvládnete používat `VEDriversLite`

## Tutoriál

1. Vyzkoušejte si VENFT aplikaci a [vytvořte si účet](https://apptest.ve-nft.com/) nebo si vytvořte [Neblio peněženku](https://nebl.io/wallets/)
2. Stáhněte si backup vaší Neblio peněženky z VENFT aplikace
   1. Uložte ho do souboru `backup.txt`
3. Vycházejte z [příkladu konzolové aplikace](../VirtualEconomyFramework/VEDriversLiteConsoleAppExamples/Program.cs)
   1. Umistěte soubor `backup.txt` do rootu adresáře
4. Podívejte se na příklad [TestVEDriversLite](../VirtualEconomyFramework/TestVEDriversLite/Program.cs), který je pokročilejší
    - Obsahuje nastavení serveru a přetížení IPFS pomocí `Storage.AddDriver`
