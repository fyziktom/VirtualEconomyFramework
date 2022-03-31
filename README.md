
<p align="center">
  <a href="https://veframework.com/" target="_blank"><img width="250" src="https://ipfs.infura.io/ipfs/QmfLC2seWMzKT7PyTzjhMC2iZKLFktBwHFCfknodwFEx4u"></a>
</p>


Virtual Economy framework will help you to create applications related to Web3, Blockchains, IoT, NFTs, Cryptocurrencies and other useful technologies.

This project is OpenSource for personal, educational and commercial use also :)

Main framework is **.NET Core 3.1** (VEDriversLite). You can use it now with **.NET 6.0 Blazor WebAssembly application** :)
Code is written in C#, HTML, CSS and JavaScript.
Solution is for [Visual Studio 2022](https://visualstudio.microsoft.com/vs/).

Repository now contains new version of drivers for Neblio and Doge: **VEDriversLite**. It is also available as [Nuget package](https://www.nuget.org/packages/VEFramework.VEDriversLite/). Please check this drivers and [documentation](https://docs.veframework.com/VEDriversLite/index.html). 

We recommend to use VEDriversLite if you need to build app which uses just Neblio or Doge!

## Online Demo VENFT App

<p align="center">
  <img width="200" src="https://ipfs.infura.io/ipfs/QmT8wYFsDR55wCKKUwRwXYfxu67uGWpbVCKfg4kxiCM7pJ">
<p>

Please explore [VE NFT App](https://ve-nft.com/) - C# [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) Neblio NFT Web Wallet which uses VEDriversLite.

Web Wallet with integrated NFT minting and marketplace.

## Aidrop of tokens and Neblio for testing

Do you need the tokens and some Neblio for testing and development? Please visit [VENFT App Project Page](https://about.ve-nft.com/#airdrop).

If you need more tokens or Neblio plese contact the author of the project on [email](mailto:tomas.svoboda@technicinsider.com)

## VEFramework components

The VEFramework offers lots of different libraries, drivers, components which will reduce code in your application a lot.

![VEFramewoents](https://ipfs.infura.io/ipfs/QmNrjnNjk7dQKikCo8qZjsKK5J9bw7m8BrJuFXojhwBFJA)

## Development with VEFramework

The most compatible languages are C#, VisualBasic or F#. For lots of applications you can use also different languages and connect backend with use of API or MQTT. This allows to use this framework with more languages such as JavaScript, HTML, Python and others.

![Development with VEFramework](https://ipfs.infura.io/ipfs/QmVYGi34Qpt69GMDjLLoAQtF48GUs6yN5uabE9nPkw3GLX)

## Hello World with VEDriversLite

[Install the .NET Core 3.1 SDK.](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)

#### Create project

Open console line and create empty folder and ConsoleApplication project

```code
mkdir CreateAccountExample
cd CreateAccountExample
dotnet new console
dotnet add package VEFramework.VEDriversLite
```
#### Write simple code to mint NFT

Please remember that when you create address you need to load the Neblio and tokens to the address!
It is good practice to create [VENFT Web wallet](https://ve-nft.com/) for the testing and development.
You can request the [Airdrop](https://about.ve-nft.com/#airdrop) or you can send the Neblio from exchange, [Orion wallet](orion.nebl.io/) or [staking desktop wallet](https://nebl.io/wallets/).

```csharp
using System;
using VEDriversLite;

namespace CreateAccountExample
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine(" Hello World With VEDriversLite!");
            var account = new NeblioAccount(); // create NeblioAccount object
            await account.CreateNewAccount("mypassword"); // create new account
            Console.WriteLine($" New Neblio Address: {account.Address}");
            Console.WriteLine(" Please load Neblio and tokens to Address and then continue.");
            Console.ReadLine(); // wait for enough confirmation on blockhain
            // create NFT Image object and load simple test data
            var nft = new VEDriversLite.NFT.ImageNFT("")
            {
                Name = "My Hello World NFT",
                Description = "Hello World NFT for testing.",
                ImageLink = "https://MyNFT.image"
            };
            var res = await account.MintNFT(nft); // mint NFT with account
            // process result
            if (res.Item1)
                Console.WriteLine($" Minted with Transaction hash: {res.Item2}");
            else
                Console.WriteLine($" Cannot mint NFT: {res.Item2}");
        }
    }
}

```

Then you can run the code

```code
dotnet restore
dotnet run
```
  
Here is the output from this program:

![VEDriversLite example code output](https://ipfs.infura.io/ipfs/QmRS7oY66FCawfuBJpy2sM51tGzQ1GmXgc3ArExEdmSijE)
  
Here you can see the details of the transaction in the [Neblio Blockchain Explorer](https://explorer.nebl.io/tx/e28dcb54c5ec1d3e889a19c75d58eea5e684db6968fd2478a98e78415996760c).

# Project and Code Documentation

## First Steps

[First Steps with VEDrivers Lite](https://veframework.com/first-steps/)

[First Steps with API of VENFT App Server](https://veframework.com/first-steps-api/)

## Project Wiki

[Wiki](https://github.com/fyziktom/VirtualEconomyFramework/wiki)

## VEDriversLite Docfx documentation

!This is new Light version of drivers!

[VEDriversLite Documentation](https://docs.veframework.com/VEDriversLite/index.html)
  
## VENFT App Server
  
This is actual main application which provides API for most of the commands from the VEDriversLite library. You can run it as app service or in **Docker**. [On Wiki you can find article about how to use Docker and VENFT App Server](https://github.com/fyziktom/VirtualEconomyFramework/wiki/Dockerizing-VENFT-APP-Server).

## VEconomy Application

[Application has own Readme here](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEconomy#veconomy-application)


# Supported Platforms

Project is based on .NET Core 5.0 so it can run on:
-	Windows
-	Windows 10 IoT
-	Linux
-	MacOS
-	iOS
-	Android
-	x86, x64, AnyCPU, ARM
And other platforms which .NET Core supports.

# Projects in the solution

-	VEDriversLite - Light version and Actual Recommended! version of Neblio drivers, includes NFT drivers
-	TestVEDriversLite - testing utility wih integration tests/examples for VEDriversLite
-   VEFrameworkUnitTest - Unit tests project.
-	VENFTApp-Blazor - Example of use of VEDriversLite in Blazor WebAssembly App. Contains lots of Blazor components!!!
-   VENFTApp-Server - Example of use of VEDriversLite in Blazor Server App. It offers API for lots of VEDriversLite commands.
-	VEOnePage - Example of the simple webpage which works as presentation page for some address NFTs. Example was created for [Coruzant.com](https://coruzant.com/)
-   VECryptographyUtilities - encrypt and decrypt keys example

Older projects. Most of them needs testing and update based on refactoring of VEDriversLite

-	NeblioRestApi - .NET Core 5.0 wrapper for Neblio API.
-	TestNeblio – testing utility for integration tests.
-	VEconomy – ASP.NET application to be shaped to your app or used as it is. (needs refactoring)
-	VEDrivers – digital twins and other drivers for economy-based applications. (needs refactoring)
-	VEGameDrivers - drivers for connecting games and blockchain (obsolete, will be replaced with VEDriversLite)
-	VENodeExecutor – runs node-red as a service.
-	VEQTWalletExecutor – runs QT Wallet as a service.
-	VEUsersUtility – utility to create first admin user in Db


# Other Projects

This repository contains also project examples for Arduino IDE. It is developed on M5StickC HW. It is based on ESP32 MCU (official page: [M5Stack](https://m5stack.com/)). These projects you can find in folder [Examples-ArduinoIDE-M5StickC](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/Examples/ArduinoIDE/M5StickC).

For compilation you need to install Arduino IDE and M5Stack libraries and ESP32 libraries into IDE ([Guidlines](https://docs.m5stack.com/#/en/arduino/arduino_development)).
 

# Main Features

-   Create Blockchain Account and send transactions or NFTs with just few lines of the code
-   VEFramework works with multiple blockchains. Now it has support of Neblio and Dogecoin
-   Blazor Webassebmly example [VE NFT App](https://ve-nft.com/)
-   Server App with prepared API for integration existing app or UI with Web3 environments
-   Set of drivers/helpers for minting, sell and trade NFTs. Already with Images, Post, Music, Profile, Payment, etc. NFTs.
-	Integration of [IPFS](https://ipfs.io/) API which uses [Infura.io](https://infura.io/) as IPFS node
-   Create blockchain application without need of running own node
-   NFT Ownership verification system with creating QR codes/messages
-   NFT Events and Tickets system.
-   System of P2P NFT encrypted metadata and encrypted filecontainers stored on ipfs
-   Encryption with EDCH shared secret algorithm. Dont need to share password between peers, they will calculate it!
-   RPC Client for connection the blockchain node
-   [Neblio Blockchain](https://nebl.io/) API wrapper and helper classes
-   Drivers for special transactions such as split transactions, multiple token input/output, minting multiple NFTs, etc.
-   Dogecoin API wrapper and helper classes.

Other features you can explore in the readme of the specifics projects in the solution.


# Roadmap

## 1-2Q 2022
-   Unit Tests for most of the functions
-   Refactoring of UX/UI Blazor components and VENFT App
-   Automated deployment of apps
-   Dockers for VENFT App Server and IPFS node and Neblio node
-   Simple web page demo for display specific NFTs of some address
-   Add common account interface in the VEDriversLite
-   VEDriversLite core refactoring -> version 0.2.0

## 3-4Q 2020
-   Integration of some parts of the PLFramework into the VEDriversLite ;)
-   VE Blazor Component Library
-   Integration of few new blockchains
-   Raspberry PI preinstalled image
-   Integration with SAP

# Reporting Issues
  
Feel free to open Issue if you will find some bug or if you are looking for some new feature. [On Wiki you can find the information about creating the issues](https://github.com/fyziktom/VirtualEconomyFramework/wiki/Guidelines---Issue-Creation).

# Contributors and Development Partners

Tomas Svoboda - [LinkedIn](https://www.linkedin.com/in/fyziktom), [Twitter](https://twitter.com/fyziktom)

RoundSqr - [Company website](https://www.roundsqr.com/)

Löwenware - [Company website](https://lowenware.com/)

PureCrew - [Company website](https://purecrew.cz/)

Francis Karuri - [LinkedIn](https://www.linkedin.com/in/francis-karuri/)
  
We are looking for new contributors. Please feel free to [contact me on tomas.svoboda@technicinsider.com](mailto:tomas.svoboda@technicinsider.com).

# Thanks

Main Thanks goes to Mr. Jan Kuznik. He taught me lots of great knowledge about programming.

“TestNeblio.exe” utility is Mr.Kuznik design and he agreed to publish it with this project. Many thanks for this great tool.

Many thanks for Blazor developers. It is absolutelly amazing tool!

This project uses some other opensource libraries or other tools. Many thanks to all authors of these projects and other opensource projects.

-	Neblio – Blockchain solution for Enterprises – https://github.com/NeblioTeam/neblio 
-	Microsoft - .NET Core, C#, Entity Framework Core - https://docs.microsoft.com/en-us/dotnet/core/introduction 
-	Blazor - Web Apps with C# - https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor
-	Newtonsoft.Json – JSON parsing library - https://github.com/JamesNK/Newtonsoft.Json 
-	NBitcoin - .NET C# Library for Bitcoin based cryptocurrencies - https://github.com/MetacoSA/NBitcoin
-	Ant Desing Blazor - Blazor component library - https://antblazor.com/en-US/
-	BlazorFileReader - File Reader component for Blazor - https://github.com/Tewr/BlazorFileReader
-	IPFS Http Client C# - Http Api wrapper for IPFS - https://github.com/richardschneider/net-ipfs-http-client
-	DocFx – API documentation generator - https://github.com/dotnet/docfx 
-	Swagger – OpenAPI description of REST API - https://swagger.io/ 
-	MQTTNet – library for MQTT connection - https://github.com/chkr1011/MQTTnet
-	Log4net – library for logging - https://github.com/apache/logging-log4net
-	Binance.Net – library for connecting to Binance Exchange - https://github.com/JKorf/Binance.Net
-	Jint – library for run JavaScript in C# - https://github.com/sebastienros/jint
-	Npgsql – EFC provider for PostgreSQL - https://github.com/npgsql/efcore.pg
-	Node.js – JavaScript runtime - https://nodejs.org/en/  
-	Node-RED – IoT tool for event driven connections - 
-	Aedes Node-Red node – MQTT Broker - https://github.com/moscajs/aedes 
-	Paho MQTT – JavaScript library for MQTT client - https://github.com/eclipse/paho.mqtt.javascript 
-	Chart JS - JavaScript library for charts - https://github.com/chartjs 
-	CodeJar - Simple JavaScript editor - https://github.com/antonmedv/codejar
-	Prism - Code Syntax Highlight library - https://prismjs.com/
-	Crypto JS - JS library of crypto standards - https://github.com/brix/crypto-js
-	Chessboard JS - JS library for chess game - https://chessboardjs.com/
-	Bootstrap Studio – tool for simplify web-based UI - https://bootstrapstudio.io/ 

# License 

This framework can be used for any use even for commercial use. License is BSD 2 with additional conditions. 

Please read it here: 

https://github.com/fyziktom/VirtualEconomyFramework/blob/main/License/license.txt


# Donation

If you like this project, please donate the team with some Nebl, Dogecoin, Bitcoin or ReddCoin. 

You can also help as official sponsor. For this option and why to do that please [contact me on tomas.svoboda@technicinsider.com](mailto:tomas.svoboda@technicinsider.com).

Thanks to donations we can focus our energy to this opensource project.

Project Neblio Address - NUhbMPqKYaGe8irb4kXECb8KN79YbD6ZyX

Project Doge Address - D9g337oMmTVUnyMdpYDdSS5ob58CD7Hq7y

Project BTC Address - 34cuGjGbdVBHvwS3dha8pMv63jbxsF96MP

Project ReddCoin Address - RiPAe5nGNvtyPfxCC3nQoXes6EjgduQct2

