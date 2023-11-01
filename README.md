[![VEDriversLite](https://img.shields.io/nuget/v/VEFramework.VEDriversLite?label=VEDriversLite)](https://www.nuget.org/packages/VEFramework.VEDriversLite/)
[![VEBlazor](https://img.shields.io/nuget/v/VEFramework.VEBlazor?label=VEFramework.VEBlazor)](https://www.nuget.org/packages/VEFramework.VEBlazor/)
[![VEBlazor](https://img.shields.io/nuget/v/VEFramework.VEDriversLite.EntitiesBlocks?label=VEFramework.VEDriversLite.EntitiesBlocks)](https://www.nuget.org/packages/VEFramework.VEDriversLite.EntitiesBlocks)
[![VEBlazor](https://img.shields.io/nuget/v/VEFramework.VEBlazor.EntitiesBlocks?label=VEFramework.VEBlazor.EntitiesBlocks)](https://www.nuget.org/packages/VEFramework.VEBlazor.EntitiesBlocks)
[![VEDriversLite.AI.OpenAI](https://img.shields.io/nuget/v/VEDriversLite.AI.OpenAI?label=VEDriversLite.AI.OpenAI)](https://www.nuget.org/packages/VEDriversLite.AI.OpenAI)
[![VEBlazor.Editable](https://img.shields.io/nuget/v/VEFramework.VEBlazor.Editable?label=VEBlazor.Editable)](https://www.nuget.org/packages/VEFramework.VEBlazor.Editable/)
[![VEDriversLite.NeblioAPI](https://img.shields.io/nuget/v/VEFramework.VEDriversLite.NeblioAPI?label=VEDriversLite.NeblioAPI)](https://www.nuget.org/packages/VEFramework.VEDriversLite.NeblioAPI/)


# VEFramework

Virtual Economy framework zjednodušuje tvorbu Web3, blockchain, IoT a NFT aplikací.

VEFramework je L2 vrstva nad prakticky kterýmkoliv blockchainem. Aktuálně na ukázku podporuje Neblio a Dogecoin.

VEFramework obsahuje také knihovnu [EntitiesBlocks](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEDriversLite.EntitiesBlocks), kterou lze jednoduše použít pro výpočty energetických modelů. Knihovna má k dispozici i knihovnu UI komponent pro Blazor aplikace tu najdete [zde](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks).

![Development with VEFramework](https://ve-framework.com/ipfs/QmVoMw53Vo2ZsbMYLwXjEuh2wkmVvWKkM1mrac7qdKosWs)

![Account](https://ve-framework.com/ipfs/QmdKkks4xmkiy1jdMoWqroYDKC8NRa6u5VeurKHXcpxMrY)

![NFTs](https://ve-framework.com/ipfs/QmdVy8ZozfEicFd4v8NKiUAeX1vn8QmLYCFYAuB9u7PUCJ)

## Použití

**Všechny balíčky jsou dostupné pro .NET verze 6 a 7 ** a jsou k dispozici v [NuGet Gallery](https://www.nuget.org/profiles/fyziktom).

- [`VEDriversLite`](https://fyziktom.github.io/VirtualEconomyFramework/api/VEDriversLite.html) - Aktuální a doporučená verze (Neblio a NFT drivers)

```shell
dotnet add package VEFramework.VEDriversLite
```


- [`VEDriversLite.NeblioAPI`](./VirtualEconomyFramework/VEDriversLite.NeblioAPI) pro zjednodušení práce s Neblio Blockchain API.
- VEDriversLite obsahují tuto knihovnu. Pokud potřebujete jen přístup k API a nepotřebujete podepisovat transakce lze načíst jen knihovnu pro NeblioAPI a tím snížit počet závislostí v projektu.

```shell
dotnet add package VEFramework.VEDriversLite.NeblioAPI
```

- [`VEBlazor`](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor) - Blazor knihovna komponent a dApp vzor

```shell
dotnet add package VEFramework.VEBlazor
```

- Dodatečná knihovna [`VEBlazor.Editable`](./VirtualEconomyFramework/VEFramework.VEBlazor.Editable) pro zobrazení a editaci parametrů tříd v UI

```shell
dotnet add package VEFramework.VEBlazor.Editable
```

Zkuste si náš [hello world příklad](#hello-world).

Více informací najdete na

- [Wiki](https://github.com/fyziktom/VirtualEconomyFramework/wiki)
- [Dokumentaci tříd a funkcí](https://fyziktom.github.io/VirtualEconomyFramework/)
- [Readme VBlazor](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor)
- [Readme VEconomy](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEconomy#veconomy-application)

### Docker

- [Dockerizace VENFT serveru](https://github.com/fyziktom/VirtualEconomyFramework/wiki/Dockerizing-VENFT-APP-Server)
- [Dockerizace Nebliod (Neblio Blockchain)](https://github.com/fyziktom/VirtualEconomyFramework/wiki/Dockerizing-Nebliod)
- [Dockerizace IPFS](https://github.com/fyziktom/VirtualEconomyFramework/wiki/Dockerizing-IPFS)
- [Použití `docker-compose` ke spuštění VENFT serveru, Nebliod a IPFS](https://github.com/fyziktom/VirtualEconomyFramework/wiki/Docker-Compose---VENFTAPP-Server,-Nebliod,-IPFS-Node)

## VENFT online demo

<p align="center">
  <img width="200" src="https://ve-framework.com/ipfs/QmT8wYFsDR55wCKKUwRwXYfxu67uGWpbVCKfg4kxiCM7pJ">
<p>

Webová peněženka s integrovaným NFT marketplacem a mint mechanismem

Vyzkoušejte testovací verzi VENFT postavenou na VEBlazor, vytvořte si nový účet a [prozkoumejte funkce](https://apptest.ve-nft.com/).

## Hello World

1. [Jsem backend vývojář](./docs/BACKEND_INTRO.md)
2. [Jsem UX / UI / frontend vývojář](./docs/FRONTEND_INTRO.md)

### VEDriversLite

1. [Install the .NET Core 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) nebo [.NET 7 SDK.](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
2. Vytvořte nový `dotnet` projekt

```shell
mkdir CreateAccountExample
cd CreateAccountExample

dotnet new console
dotnet add package VEFramework.VEDriversLite
```

3. [Vytvořte si novou Neblio peněženku](https://ve-nft.com/), pokud už nějakou nemáte
4. [Aidropněte si tokeny a Neblio pro testování](https://about.ve-nft.com/#airdrop) do vaší peněženky
5. Napište jednoduchý kód, který vymintuje NFT

```csharp
using System;
using VEDriversLite;

namespace CreateAccountExample
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Create NeblioAccount object
            var account = new NeblioAccount();
            // Create new account
            await account.CreateNewAccount("mypassword");
            Console.WriteLine($" New Neblio Address: {account.Address}");
            // Wait for enough confirmation on blockhain
            Console.ReadLine();
            // Create NFT Image object and load simple test data
            var nft = new VEDriversLite.NFT.ImageNFT("")
            {
                Name = "My Hello World NFT",
                Description = "Hello World NFT for testing.",
                ImageLink = "https://MyNFT.image"
            };
            // Mint NFT with account
            var res = await account.MintNFT(nft);
            // Process result
            if (res.Item1)
                Console.WriteLine($" Minted with Transaction hash: {res.Item2}");
            else
                Console.WriteLine($" Cannot mint NFT: {res.Item2}");
        }
    }
}

```

6. Spusťte kód

```shell
dotnet restore
dotnet run
```

7. Zobrazenou transakci můžete ověřit v [Neblio exploreru](https://explorer.nebl.io)
   - [`e28dcb54c5ec1d3e889a19c75d58eea5e684db6968fd2478a98e78415996760c`](https://explorer.nebl.io/tx/e28dcb54c5ec1d3e889a19c75d58eea5e684db6968fd2478a98e78415996760c)

![VEDriversLite example code output](https://ve-framework.com/ipfs/QmRS7oY66FCawfuBJpy2sM51tGzQ1GmXgc3ArExEdmSijE)

## Projekty v příkladech

- Tento repozitář obsahuje také další projekty - [příklad použití v Arduino](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/Examples/ArduinoIDE/M5StickC) - vytvořeno nad [M5StickC HW](https://m5stack.com/) (ESP32 MCU).

## Funkce

- Vytvoření účtu a posílání transakcí nebo NFT
- Server s API pro integraci existujících aplikací
- UI s integrovaným web3 prostředím
- Drivery a pomocné funkce pro mintování, prodej a obchodování s NFT
- NFT jako obrázky, příspěvky, hudba, profily, platby, atd.
- OpenAI - ChatGPT, Whisper a DALL-E jako asistenti pro tvorbu obsahu NFT jako je: 
  - převod textu z audia, 
  - text ze základní story, 
  - název, 
  - tagy, 
  - zkrácený popis, 
  - obrázky
  - Více informací najdete ve [článcích na wiki](https://github.com/fyziktom/VirtualEconomyFramework/wiki/#OpenAI)
- [IPFS](https://ipfs.io/) driver,
- Ověření vlastnictví NFT (tvorba QR kódů)
- NFT systém pro události a vstupenky
- P2P NFT systém s šifrovanými metadaty a filecontainers uložených na IPFS
- Šifrování pomocí EDCH sdíleného secret algoritmu
  - Hesla není potřeba sdílet, budou automaticky dopočítána
- RPC klient pro připojení k blockchainu
- [Neblio Blockchain](https://nebl.io/) API wrapper a pomocné třídy
- Drivery pro speciální transakce - split transakce, vícenásobný vstup/výstup tokenu, mintování více NFT, atd.

Další funkce můžete prozkoumat v readme příslušných [projektů v příkladech](#projekty-v-příkladech)

## Podpořte nás

Podpořit nás můžete několika způsoby:

1. Staňte se contributorem a podílejte se na vývoji VEFrameworku
2. Staňte se sponzorem a přispějte na vývoj VEFrameworku

### Contributing

Děkujeme, že se chcete podílet na vývoji VEFrameworku! Postupujte prosím podle naší [contributing příručky](./CONTRIBUTING.md). V příručce naleznete informace o tom
- jak se podílet na vývoji VEFrameworku
- jak vytvářet issues
- otevírat pull requesty
- a další

### Sponzoring

[Přispějte VEFrameworku](./docs/SPONSORING.md) a pomozte tak s jeho vývojem. Pokud se chcete stát oficiálním sponzorem a partnerem, kontaktujte prosím [Tomáše Svobodu - tomas.svoboda@technicinsider.com](mailto:tomas.svoboda@technicinsider.com)

[Velké díky](./docs/THANKS.md) patří našim sponzorům, mentorům a kontributorům

## License

VEFramework je možné použít pro komerční i nekomerční účely. VEFramework je vyvíjen pod [MIT licencí](./LICENSE).
