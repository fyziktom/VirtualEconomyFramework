# Seznámení pro frontend vývojáře

Dozvíte se, jak si vytvořit vlastní customizovanou aplikaci nad funkčním backendem.

Komponenta [`VEBlazor.Demo.Publishing.Server`](../VirtualEconomyFramework/VEBlazor.Demo.Publishing.Server) vystavuje jednoduché [API](../VEconomy-swagger.json)

## Tutoriál

1. Vyzkoušejte si VENFT aplikaci a [vytvořte si účet](https://apptest.ve-nft.com/) nebo si vytvořte [Neblio peněženku](https://nebl.io/wallets/)
2. Stáhněte si backup vaší Neblio peněženky z VENFT aplikace
   1. Uložte ho do souboru `backup.txt`
3. Seznamte se s VENFT aplikací, tentokrát jejím [kódem](../VirtualEconomyFramework/VEBlazor.Demo.VENFTApp.Client)
    - Konfigurace se nachází v [App.razor](../VirtualEconomyFramework/VEBlazor.Demo.VENFTApp.Client/App.razor), ale do budoucna bude v JSON souboru
4. Podívejte se, jak rozšířit, přepsat a přetížit každou stránku a upravit komponenty v [publishing demu](../VirtualEconomyFramework/VEBlazor.Demo.Publishing.Client)
   1. Využijte komponentu `AddNew` a její `SpecificParamsStepHeader` a `SpecificParamsStepContent`
5. Vytvořte vlastní aplikaci přepsáním [dema publishing display](../VirtualEconomyFramework/VEFramework.Demo.PublishingDisplay)
   - Načte a zobrazí NFT bez možnosti editace
   - Šablona je použita na webu [Coruzant](https://nft.coruzant.com/)
   1. Forkněte tento repozitář
   2. Upravte aplikaci podle svých představ
   3. Nastavte server v [`Services/AppData.cs`](../VirtualEconomyFramework/VEFramework.Demo.PublishingDisplay/Services/AppData.cs), pokud chcete změnit adresu, ke které se budete připojovat
