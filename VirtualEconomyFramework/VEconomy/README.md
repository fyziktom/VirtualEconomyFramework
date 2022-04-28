# VEconomy application

VEconomy application is main ASP.NET application of VE Framework which provides API and MQTT data messages. 
This application can be used as it is or can be added or re-shaped for your needs.
Application runs Web server and MQTT server. It also provides connection to the database.


## Important note
This project needs refactoring based on the progress of the VEDriversLite. If you will need to use it as a template for your application. Please contact the admins of the repository for more informations. Any contribution welcomed :)

# Application usecases

- Wallet and account manager for Neblio cryptocurrency (Soon Bitcoin and ReddCoin support)
- Integration with another system on API level
- Automated operations triggered by transactions for integrations or automatic processing transactions
- Creating own dApp based on this template
- Playing games (now Chess game)
- Messaging client with encryption on blockchain

# Main Features

-	ASP.NET Core base application, VEconomy, which can be shaped for your app needs.
-	Digital Twins of objects related to blockchain such as Wallet, Account, Transaction, Token, etc. and publishing these data to MQTT.
-	Set of drivers/helpers for economy-based applications.
-	Set of drivers/helpers for using blockchain in games
-	Set of drivers/helpers for minting, sell and trade NFTs. Already with Images, Post, Music, Profile, Payment, etc. NFTs.
-	Automatic shop for selling source tokens (send nebl receive lot of tokens)
-	Simple Chess Game which store game data to blockchain as example
-	Client for encrypted messaging via Neblio Blockchain. Optional Both side asymmetric RSA encryption. With UI
-	Integrated key manager for storing keys in encrypted form. Solves export and import keys.
-	REST API with OpenAPI description in Swagger.
-	QT Wallet RPC client and controller for communication with desktop wallet or core of blockchain based on Bitcoin QT Wallet.
-	QT Wallet is optional.
-	Neblio API .NET Core wrapper (ReddCoin and Bitcoin to be soon).
-	Wrapper for NBitcoin library to simple create Neblio addresses, sign and send transactions of Nebl and Tokens without QTWallet or Nebliod (other currencies soon).
-	Integrated MQTT Broker with TCP/IP and WebSocket support
-	Connection to Binance Exchange data.
-	Testing application for integration tests.
-	Basic UI in HTML/CSS/JavaScript dependent just on Bootstrap and JQuery (and few small libs)!
-	Project for Bootstrap Studio which simplify building web-based UI of applications.
-	Functional “Nodes” which can perform some action such as HTTP API Request or MQTT Publish based on trigger from blockchain transaction (incoming and outgoing).
-	Nodes can run custom JavaScript with payload data from transaction and provide the tx result as payload for main function of Node.
-	Web UI has integrated JavaScript editor.
-	JS Script can be tested with simulated tx data or last tx real data. No need to send tx during debbuging of JS Scripts.
-	Web UI contains hash library for creating hash of any file on the client side and use it as metadata in NFT token transactions
-	Connection to Database where local data about digital twins and settings are stored.
-	Stored Last Processed Transaction and Last Confirmed Transaction for recovery after crash for each Account (works without Db too)
-	Db is connected via Entity Framework Core so it can be connected to another Db.
-	Db connection is optional and app can run without it
-	You can setup different Db providers. SQLite is default (it create default file if not exists), optional PostgreSQL and MSSQL
-	Security controller for creating users, rights, roles which limit access to API
-	Node-Red executor application – can run node.js and node-red as a service.
-	QT Wallet executor application – can run QT Wallet as a service.
-	Logging errors, infos and warrings with log4net library

# Installation and setting of framework for development

This framework needs few steps of installation to run with all features.

If you want to use just VEDriversLite you can skip this whole and just check the examples of using this drivers. It has almost no dependencies so you will just download the [Nuget package](https://www.nuget.org/packages/VEDrivers.VEDriversLite/0.1.1).

If you want to use all features in VEDrivers or VEconomy please continue.

If you do not want to use Database or QT wallet just skip these steps. In appsetings.json you can disable working with Db and QT.
You can run app without Db but with QT support and oposite too. Without QT app cannot sign transactions now!

Default setting of the app uses SQLite Db. This is automatically created after start if not exists in user AppData folder (on Win 10 C:\Users\UserName\AppData\Roaming\VEFramework). To install another database please follow instructions below. For simplified setup plese check [Wiki](https://github.com/fyziktom/VirtualEconomyFramework/wiki) please.

1.	Database - Optional
-	Download build of PostgreSQL for your platform: https://www.postgresql.org/download/
-	Install it based on instructions.
-	After installation open pgAdmin and create new database named “veframework” (you can change the name, but it must be changed in “appsettings.json” in the ConnectionStrings).
-	Create new user “veadmin” with password “veadmin” (you can change the user and pass, but it must be changed in “appsettings.json” in the ConnectionStrings).
-	Open query tool and run script “CreateTableScript.sql” (VEDrivers/DDL/CreateTableScript - PostgreSQL.sql). This will create tables and fill some sample data (in section “Accounts” please fill some your account addresses instead of default example accounts address)!!!
-	Grant privileges to “veadmin” – uncomment and run just last lines of “CreateTableScript.sql” file.
-	That should be all about preparing the Database.

2.	ASP.NET Core 5.0
-	If you do not have installed ASP.NET Core 5.0 runtime and .NET Core 5.0 runtime libraries you have to install it before you run VEFramework.
-	Go to the https://dotnet.microsoft.com/download/dotnet/5.0 and download latest release and follow common installation instruction for your platform.

3.	Creating first admin user - Optional - Just with Db
-	Run “VEUserUtility.exe” Follow the instructions in console. 
-	Type “1” for add new user. At first fill login - “admin” and then full username “John Doe” (just example). All confirm with “enter” after input.
-	Type “2” to create password. At first fill login and then password. All confirm with “enter” after input.
-	That is all. Now you can use this account to login in VEconomy UI and create other users in UI.

4.	Download and synchronize Neblio desktop wallet - Optional
-	Go to the https://nebl.io/wallets/ and download wallet for your platform. 
-	Move “neblio-qt.exe” to some folder and set the path in “appsettings.json” of “VEQTWalletExecutor.exe”
-	If you do not want to use VEQTWalletExecutor you do not need to setup this.
-	Run QT Wallet and lets it synchronize with the network.
-	In QT Wallet click to "Help" -> "Debug Window" -> "Show Data Directory". In the data directory folder create new file "neblio.conf" with this content (username and pass you can change, but remeber to change it in appsetting.json of the apps):

```
server=1
rpcuser=user
rpcpassword=password
rpcport=6326
```

This step is now optional. VEFramework now contains own MQTT Broker. If you need Node-RED for another use please install it but it is not necessary for VEFramework now. 
Please remember. If you will install MQTT Broker in Node-RED you have to set different port than in VEconomy. It cannot run on same ports!

5.	Install MQTT Broker – Optional
-	If you want to use another MQTT Broker it is no problem, just VEconomy needs WS sockets too!
-	Go to the https://nodejs.org/en/ and download and install version of “node.js”.
-	Go to the https://nodered.org/ and download and install version of node-red for your platform. [Here for Linux](https://nodered.org/docs/getting-started/local), [Here for Windows](https://nodered.org/docs/getting-started/windows)
-	If you want to run node-RED as service, you can use “VENodeExecutor.exe”. Just setup in “appsettings.json” path to node.exe (node.js main app) and node-red path (common path for windows is in file, just change the username”.
-	If you do not want to use “VENodeExecutor.exe” please run node-red with type “node-red” into command line.
-	Open node-red interface http://localhost:1880/ 
-	Open “Manage Pallet” in node-red (right top menu next to “Deploy” button). Switch to tab “Install” and install package “node-red-contrib-aedes” (node-red-contrib-aedes (node) - Node-RED).
-	After installation, close „Manage Pallet“ panel and add the node „aedes broker“ to the flow. 
-	With double-click open detail of MQTT broker. Fill MQTT Port to „1883“ and WS Port to „8083“ and on tab „Security“ fill Username „user“ and Password „userpass“ (if you want to use different ports, user and password you have to change it in all appsettings.json of applications which uses MQTT, mainly in „VEconomy“).
-	Close Broker details and click to Deploy. MQTT should run now correctly.


# Pre-Beta Pre-Build of VEconomy :)

Here you can download first pre-beta pre-build. It is preset to work with SQLite Db (created automatically after start if not exists).

Actual version now supports signing and sending transactions even without QT Wallet. It works as independent Wallet for Neblio now.

For details about setting of VEconomy application please read the VEconomy [Application Readme](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEconomy#veconomy-application). For using please visit the [Wiki](https://github.com/fyziktom/VirtualEconomyFramework/wiki)

Here you can download the app:

[VEFramework Release Folder](https://technicinsider-my.sharepoint.com/:f:/p/tomas_svoboda/EgLTmYjsqDRHvQyGvkKkO6EBl44-fFopmkSZUQH_gF__Xg?e=HKcLCg)

In the folder you can find .NET Core 5.0 and ASP.NET 5.0 installers too.

# Main application setting
Main application setitng you can find in the appsetting.json in the main folder (where the VEconomy.exe or .dll is located).

There are several settings options:

## Port and allowed hosts

```json
  "MainPort": 8080,
  "AllowedHosts": "*",
  "Urls": "http://*:8080",
```

Usually you will reconfigure just the port. Please check the MainPort and Urls match.

## MQTT Setting

```json
  "MQTT": {
    "Host": "127.0.0.1",
    "Port": 1883,
    "WSPort": 8083,
    "User": "user",
    "Pass": "userpass"
  },
```

If you run application just localy keep the default address. If you wan to access UI from remote client, please fill the address of PC where you run the VEconomy. Listening on all available addresses will be added and tested later.

For running web client it is important to fill the WSPort (web socket port). And please check the firewall setting to do not block these ports.

## RPC connection to QT Wallet

```json
  "UseRPC": false,
  "QTRPC": {
    "Host": "127.0.0.1",
    "Port": 6326,
    "User": "user",
    "Pass": "password"
  },
```

If you want to connect to the QT wallet you must set "UseRPC" to the true and fill correct connection parameters. Application now can communicate just with one QT wallet. Multiple clients will be added in next versions.

QT Wallet must have enabled RPC server. Details you can find in the [installation section](https://github.com/fyziktom/VirtualEconomyFramework#installation-and-setting)

## Accounts for load without Db

```json
  "Accounts": [
    "NXm99M9UaEFUdUGK8m7kQoM96aGqxPUyax",
    "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA",
    "NMLQiY8CQ2X5UEFxRbVAVgpbZMgFRBf8SS"
  ],
```

If you run application without database but you need to load some accounts you can add addresses here. Now it supports just Neblio addresses. 
After start (when Db is disabled) application create one wallet and add these accounts and addresses.

## Database Connection

```json
  "UseDatabase": true,

  "Provider": "SQLite",
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=veframework;Username=veadmin;Password=veadmin",
    "MSSQL": "Server=localhost\\SQLExpress;Database=veframework;User ID=veadmin;Password=veadmin",
    "SQLite": "Data Source=C:\\VEFramework\\veframeworkdb.db"
  },
```

if you use database set "UseDatabase" to true. Application runs with SQLite database in the default. It is file based database so it is just one file coppied and stored automatically in the AppData user folder located based on your operation system. On Windows it is for example "C:\Users\username\AppData\Roaming\VEFramework".
If you want to store file on the different location you can fill the path to the parametr of connection string.
Same for other Database providers. MSSQL and PostgreSQL runs the server with Db, so you have to fill host, port, Db name and login credentials.

To change provider change parameter "Provider". Now only these three providers are supported.
- PostgreSQL
- MSSQL
- SQLite

## Other settings

```json
  "StartBrowserAtStart": false,
  "NumberOfConfirmationsToAccept": 1
```

VEconomy can start browser with main page automatically after the start. For this option set "StartBrowserAtStart" to true.

"NumberOfConfirmationToAccept" sets how many confirmation in blockchain network is need to take payment as confirmed.


# Using VEconomy API

You can use VE Economy as background service which communicates just with API. The description of API is in OpenAPI form, swagger: 

[VEconomy Open API Description](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VEconomy-swagger.json)

- When you run VEconomy, you can access swagger UI on http://localhost:8080/swagger/index.html 
- Data about digital twins are available on MQTT under topics:
- VEF/Wallets – full list of wallets and their accounts
- VEF/Accounts – full list of accounts
- VEF/Nodes – full list of functional nodes
- VEF/Cryptocurrencies – full list of available cryptocurrencies (just drivers in VEF)
- Refreshing rate is now 1s (it is in MainDataContext.cs, will be connected to appsetting.json)

# Using VEconomy UI

Host and port you can setup in VEconomy “appsettings.json”. Default setting listen an all local hosts and port 8080 (http://*:8080/).

You can open UI in your browser on http://localhost:8080/.

If you connecting from different PC you have to set the address and then you can connect with specified addres (for example http://192.168.0.1:8080/ if you run VEconomy on pc with IP 192.168.0.1)

Step by step tutorial for web UI will be published soon.

# Messaging Client

VEconomy application has inbuild messaging client. This client uses MSGT tokens to transfer and store message data. Messages can be encrypted with asymmetric encryption (RSA). Each side has own key. This key is different than key for Account (Blockchain Address). Message encryption keys can be created separately with own password for their safe storage in the database. You can create for each new message new key or use the same one. Encrypted messages can be readed just with the original Private Key. So dont lose it and dont lose the passwrod for your encrypted private key.

## How This messaging works?

Alice wants to privately chat with the Bob. She will download the VEconomy, starts it, create address and buy some MSGT tokens. Bob will do the same things. Alice will start communication with Bob by sending "Init Message". This message is public and it is not encrypted. But with this message Alice can select key (or create new one) and "check" the option Encrypt Message. 

![image](https://user-images.githubusercontent.com/78320021/113488381-08f67a00-94be-11eb-85ee-3a1adbb08a43.png)

This option will take selected public key and add it into message. This will tells to the Bob client to encrypt message with this Public key before he will send message back the Alice.

Same thing can do the Bob when he is sending his message to the Alice. 
When Bob sends the message he always sends the copy of original message but he will use the Alice public key to encrypt it same as his new message. Thanks to this Alice will use just one encryption by her Private Key to read "First" message and the "Response".

![image](https://user-images.githubusercontent.com/78320021/113488520-fb8dbf80-94be-11eb-9562-da465377231f.png)

Alice will receive this message:

![image](https://user-images.githubusercontent.com/78320021/113488553-2aa43100-94bf-11eb-9d57-ab8f88cb87d8.png)

And she can decrypt it with her private key which is stored in the VEconomy database (encrypted with her password) or she can import it and then use it.

![image](https://user-images.githubusercontent.com/78320021/113488594-63dca100-94bf-11eb-8148-6fad3979a688.png)

Each "Init Message" will create new Unique ID of message stream. This stream UID can track some group communication. Also each Message contains TxId of previous message to allow track the exact stream of messages.

Here you can find the example of ecrypted message on the [Neblio Blockchain Explorer](https://explorer.nebl.io/tx/c0f3f2508e693fb5f731f3da8807300db7dd78f36dad6bd475e6ed366f1d87ea)

This is the "Init Message" already with Alice public key [Neblio Blockchain Explorer](https://explorer.nebl.io/tx/9cf9d4d897876ea36c46201861fca4099bacd07ac4f8a7fe93e1844456d5b0c9)

Alice can display all her messages. They are searched from her address transactions:

![image](https://user-images.githubusercontent.com/78320021/113488816-b4083300-94c0-11eb-89dd-4a89a30b4e8f.png)

If Alice needs new key she can easily create new one which is encrypted with her password and stored in her local database:

![image](https://user-images.githubusercontent.com/78320021/113488843-e6199500-94c0-11eb-9b18-a4b18bd29d59.png)

To use this feature Alice must have on her account [MSGT tokens](https://explorer.nebl.io/token/La3Fiunz84XRHDGb1HhQboH6x3TzhV2PMeRQNZ) (I am preparing shop for MSGT) and some Neblio for fees:

![image](https://user-images.githubusercontent.com/78320021/113488872-1a8d5100-94c1-11eb-80f9-95931ca7ea22.png)

The UI needs a lots of improvement. I expect during April there will be support of trancking/marking aswered messages, grouping of messages, filtering, sorting, addings attachenments to the messages.
If you are interested in testing please write me on the twitter @fyziktom and I can send you some MSGT tokens if you will help me with the testing.


# VEconomy Structure

You can use VEconomy application as base for your own project. This application uses ASP.NET and creates webserver with secured API. It also runs several internal services:
-	Starupt – runs web server
-	VEconomyCore – runs MQTT client
-	WalletHandlerCore – handling wallets, accounts and nodes and publish data to MQTT in the intervals.
-	ExchangeService - runs Exchange connector


You can add another service which handle your tasks. Please follow structure as VEEconomyCore.cs and you must add setup/start of the service in Program.cs

Main HTTP controller are placed in folder “Controllers”. You can add your own commands. In the controller there is examples for GET and PUT commands.

If you want to secure some command please add the rights before the function ([example](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEconomy/Controllers/MainController.cs#L62)).

UI files are in folder “wwwroot”. Project of web UI is in Bootstrap Studio “VEView.bsdesign”.

MQTT Controller. Is controller structure prepared for your own commands which can be provided via MQTT to the core. All commands must be registered in function “RegistredTopics” and it must have function which meets specification described by Dictionary “ApiFunctions”. Example is “Started” function. Same structure you can find in “QTWalletRPC.cs” which is controller for RPC commands for QT Wallets.

Main shared objects are stored in [EconoyMainContext.cs](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEDrivers/Common/EconomyMainContext.cs) static class. Most important is dictionaries:
-	Wallets
-	Accounts
-	Nodes
-	Cryptocurrencies
-	Owners (not used yet)


You can also find there ExchangeDataProvider to get info about prices from Binance exchange or MQTT and QTRPC clients.

More detailed explanation of structure of code will be added soon (especially for VEDrivers).
