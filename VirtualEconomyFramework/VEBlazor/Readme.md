# VEBlazor

VEBlazor is the UI component library for .NET Blazor framework.

It uses the [VEDriversLite library](https://www.nuget.org/packages/VEFramework.VEDriversLite/) for the most "backend" work. It is kind of the demo how you can wrap the VEDriversLite into UI components.

The library is now as "preview". We are still working hard on the refactoring. Same as this readme.

The library is based on the [great component library Blazorise](https://blazorise.com/). Thanks to [Mladen Macanovic](https://github.com/stsrki) for help with first steps with Blazorise library :)

# Live Demo

Url with live demo will be provided soon. 

If you would like to participate on development and testing please [contact us here](mailto:tomas.svoboda@technicinsider.com).

# Compatibility

- .NET 6.0

# First steps

- Create Empty Blazor Project (can be Server or WebAssembly)
- Add VEBlazor project reference or nuget package
- Add necessary css and js dependencies in MyApp/wwwroot/index.html

Here is the Example:

```html
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>VEBlazor.Demo.EventsAndTickets</title>
    <base href="/" />
    <link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />

    <link href='https://fonts.googleapis.com/css?family=Manrope' rel='stylesheet'>

    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css" integrity="sha384-zCbKRCUGaJDkqS1kPbPd7TveP5iyJE0EjAuZQTgFLD2ylzuqKfdKlfG/eSrtxUkn" crossorigin="anonymous">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.15.4/css/all.css">

    <link href="_content/Blazorise/blazorise.css" rel="stylesheet" />
    <link href="_content/Blazorise.Bootstrap/blazorise.bootstrap.css" rel="stylesheet" />
    <link href="_content/Blazorise.Snackbar/blazorise.snackbar.css" rel="stylesheet" />
    <link href="https://unpkg.com/easymde/dist/easymde.min.css" rel="stylesheet" />

    <link href="VEBlazor.Demo.EventsAndTickets.styles.css" rel="stylesheet" />
    <link href="css/app.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js"></script>

</head>

<body>

    <div id="app">
        <div style="position:absolute; top:30vh; width:100%; text-align:center">
            <img src="images/main-bw-big.png" style="max-height:250px; max-width: 250px;" />
            <p></p>
            <hr />
            <p></p>
            <p><i class="oi oi-reload"></i> The MyApp Web App is loading...</p>
            <p>First load of the App can take a while. Next time the load will be much faster.</p>
        </div>
    </div>

    <script src="_content/VEBlazor/jquery.slim.min.js"></script>
    <script src="_content/VEBlazor/bootstrap.min.js"></script>
    <script src="_content/VEBlazor/popper.min.js"></script>
    <script src="_content/VEBlazor/easymde.min.js"></script>
    <script src="_content/VEBlazor/highlight.min.js"></script>
    <script src="_content/BlazorPanzoom/panzoom.min.js"></script>
    <script src="_content/BlazorPanzoom/blazorpanzoom.js"></script>
    <script src="_content/Texnomic.Blazor.JsonViewer/scripts/JsonViewer.js"></script>
    <script src="_content/VEBlazor/veblazor.js"></script>

    <div id="blazor-error-ui">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>

</html>

```

- Add default images in the folder MyApp/wwwroot/images

There are three main images and you can override them with your logos:

- main-bw-small.png
- main-bw-big.png
- main-bw-long.png

Please check the demos for the details.


- Add the references to the _Imports.razor:

```csharp
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using VEBlazor.Demo.EventsAndTickets
@using VEBlazor
@using VEBlazor.Pages
@using VEBlazor.Layouts
@using VEBlazor.Menu
@using Blazorise
```

- Add services to Program.cs:

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VEBlazor.Demo.EventsAndTickets;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazored.LocalStorage;
using BlazorPanzoom;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Services.AddScoped<AppData>();
builder.Services.AddScoped<TransactionsService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorPanzoomServices();

await builder.Build().RunAsync();

```

- Add template setting into the App.razor:

```csharp
@using VEDriversLite.NFT
@using VEDriversLite.NFT.Dto
@inject AppData AppData

<Blazorise.ThemeProvider Theme="@theme">
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>

    <VEBlazor.Components.VEBlazorServicesComponents />
</Blazorise.ThemeProvider>

@code {
    // Main app theme. You can load own theme. It includes colors, etc.
    Theme theme = VEBlazor.Models.Themes.DefaultTheme.GetDefaultTheme();

    protected override async Task OnInitializedAsync()
    {
        // Longer name of the App
        AppData.AppName = "Dogeft";
        // Nick of the App. Displayed in breadcrumbs, etc.
        AppData.AppNick = "DOGEFT";
        // main app website with help, info about project, etc.
        // This is link in menu "About" under the login item
        AppData.AppHomeWebsiteUrl = "https://dogeft.com/";
        // App url for sharing NFTs
        AppData.AppShareNFTUrl = "https://dogeft.ve-nft.com/";
        // Allow Work tabs with NFT image editor
        AppData.AllowWorkTabs = false;
        // Allow sending of NFT in your app
        AppData.AllowSend = true;
        // Allow Destroy (or recycle is better for this) of NFT
        AppData.AllowDestroy = true;
        // If you use the Markdown getting started page set this
        AppData.DisplayGettingStartedMenuItem = true;
        // If you use the Markdown getting started page define location
        AppData.GettingStartedPageName = "gettingstarted";
        // Define ID of your Token. 
        // You can use some defaults or create own on Neblio blockchain
        AppData.AppTokenId = NFTHelpers.BDPTokenId;

        // Define minting tabs
        // Each of them must have also page added in Pages Foler
        if ( AppData.MintingTabsData.TryGetValue( "default", out var tab ) )
        {
            tab.MintingNFTType = NFTTypes.Ticket;
            tab.HeaderLabel = "Add New Ticket";
            tab.MenuButonLabel = "New Ticket";
            tab.TabPageLocationName = "addnew";
            tab.NFT = new TicketNFT( "" );
        }
        AppData.MintingTabsData.TryAdd( "eventminting", new MintingTabData()
            {
                MintingNFTType = NFTTypes.Event,
                HeaderLabel = "Add New Event",
                MenuButonLabel = "New Event",
                TabPageLocationName = "addnewevent",
                NFT = new EventNFT( "" ),
            } );
        AppData.MintingTabsData.TryAdd( "postminting", new MintingTabData()
            {
                MintingNFTType = NFTTypes.Post,
                HeaderLabel = "Add New Post",
                MenuButonLabel = "New Post",
                TabPageLocationName = "addnewpost",
                NFT = new PostNFT( "" ),
            } );

        // you can allow just specific types of the NFTs to display
        AppData.AllowedNFTTypes = new List<NFTTypes>()
        {
            VEDriversLite.NFT.NFTTypes.Profile,
            VEDriversLite.NFT.NFTTypes.Image,
            VEDriversLite.NFT.NFTTypes.Post,
            VEDriversLite.NFT.NFTTypes.Event,
            VEDriversLite.NFT.NFTTypes.Ticket
        };

        // Define custom tags which will be displayed as Fast option
        AppData.DefaultTags.Add("MyAppTag", new VEDriversLite.NFT.Tags.Tag()
        {
            Name = "MyAppTag",
            Color = "success",
            UseAsFastButton = true
        });
        AppData.DefaultTags.Add("MySecondTag", new VEDriversLite.NFT.Tags.Tag()
        {
            Name = "MySecondTag",
            Color = "secondary",
            UseAsFastButton = true
        });
    }
}
```

- Add Pages

If you use the basic layout you should add at least pages:

- My Account
- Gallery
- Add New

Example of gallery page with default template:

```csharp
@inject AppData AppData
@page "/gallery"

@*Add Default Title and Header*@
<PageTitle>@AppData.AppName - Gallery</PageTitle>
<VEBlazor.Components.PageHeader Title="Tickets Gallery" BreadcrumbItems="@(new string[] { AppData.AppName, "Gallery" })" />

@*Add Default Theme*@
<VEBlazor.Pages.Gallery @ref="galleryComponent" />

@code {
    VEBlazor.Pages.Gallery? galleryComponent;

    // Load the parameters from URL
    [Parameter]
    [SupplyParameterFromQuery( Name = "utxo" )]
    public string[]? InUtxos { get; set; }
    // Load the parameters from URL
    [Parameter]
    [SupplyParameterFromQuery( Name = "addr" )]
    public string[]? InAddresses { get; set; }

    protected override async Task OnAfterRenderAsync( bool firstRender )
    {
        await base.OnAfterRenderAsync( firstRender );
        // Load the parameters from URL
        if ( galleryComponent == null )
            return;
        if ( InUtxos != null && InUtxos.Length > 0 )
            await galleryComponent.OpenInUtxoInTab( InUtxos );
        if ( InAddresses != null && InAddresses.Length > 0 )
            await galleryComponent.OpenInAddressesInTab( InAddresses );
    }
}
```

Other pages demo you can find in the Demo projects.

# Demos

We have created multiple demos. Please search these projects:

- BDP-WASM
- VEBlazor.Demos.EventsAndTickets
- VEBlazor.Demos.VENFTApp.Client

# Main Components

Most of the components inherits from the NFTComponentBase or similar base classes in this file.

## Services

- App Data
- Transaction Service

## Layout Related

### Layout 

- Main Layout

### Models - Themes

In the Themes you can define your own color scheme. We have one default scheme. Soon we will add Dark mode, etc.

- Default Theme

### Pages

All of these pages templates must be placed as component in page in the MyApp/Pages folder. As it was showed upper in "First Steps" on example Gallery page.

- My Account
- Gallery
- Add New
- BuyNFT
- Readme

Readme page loads the Readme.md file placed in the MyApp/wwwroot/

### Menu

Main Layout automatically switches the menus from the Side (on Tablet and biggers) to Top Menu (on phones).

- Side Menu
- Top Menu

### Tabs

- Gallery Tab Content
- Work Tab Content


## Account

- Account Balance
- Accont Sider
- Create Account
- Import Account
- Unlock Account
- Neblio Address Input

Neblio Address Input loads bookmarks and allow to use already saved address. It load part of the Unstoppable Domains. We will need to do some updates in API, but part of the addresses works fine. You will type UD name and it will change it to the Neblio Address.

## Transaction 

- Transaction List
- Transaction Details
- SendNeblioTransaction

Send Neblio Transaction component offers multiple different transactions:

- Classic Neblio or Token transactions - send to some address coins or tokens 
  - Example: 10 NEBL or 100 VENFT tokens, etc.
- Split Neblio or Token transaction - send to yourself or some address coins or tokens changed in smaller multiple lots
  - Example: 10 x 1 NEBL or 5 x 20 VENFT tokens, etc.
- Airdrop transaction
  - Example: 1 NEBL and 100 VENFT tokens in one transaction


## Bookmarks

- Bookmark Icon
- Bookmark Item
- Bookmark Manager List

## Display

- Copy Button
- Copy Icon
- Labeled Progress Bookmark
- PanzoomImage

## NFTs

### Data Upload

- Upload Data
- File Upload
- IPFS Upload

Upload of the data allows to upload single or multiple files. You can setup this option in UploadData component.

UploadData component is the main which contains two steps: FileUpload (to the browser environment) and then IPFSUpload which upload file to the IPFS node and return the IPFS Hash of the file.

The UploadData component display the preview of the files, allow to remove them and allow to select which one will be the main.

You can upload different files now:

- Images: *.bmp, *.png, *.jpg/jpeg, *.gif, *.jfif
- AVMedia: *.mp3, *.wav, *.ogg, *.mp4, *.avi, *.mpeg, *.mov
- JSON: *.json
- HTML: *.html
- PDF: *.pdf

### Minting

Minting forms contains lots of usefull components for the input. 

For example Tags input with Autocomplete, Markdown editor, etc. 
Most of these components are wrapped components from the [Blazorise library](https://blazorise.com).
I recommend to browse all [Blazorise components in their demo here](https://bootstrapdemo.blazorise.com/) to check all possibilities.

- Data Info from
- Fast Number Input
- Load And Display NFT
- Load NFT From Template
- Minting Toolbar
- Toolbar Button
- Multimint Form
- Precise Slider
- Space Tags

### NFT Actions

NFT Actions are used in the NFT Details. You can use them separately on any page.
It takes the NFT refference and then send it and inform with EventCallback about sent.

- NFT Action Control
- Destroy NFT
- Save NFT
- Send NFT
- Set NFT Price
- Share NFT
- Storage Info and Actions

### NFT Display

#### Common

The NFT Details can be injected with your specific NFT Details Form. Check the OpenNFTDetailsFactory as example how to build some simple logic to feed the NFT Details with own specific form.

- NFT Basic Info
- NFT Card
- NFT Data Item
- NFT Details
- NFT Text
- Open NFT Details Factory

#### Lists

- NFT Card List
- NFT Payment List
- NFT Receipts List

#### Verification

- This code is unique for 60 seconds. It is refreshed after this time with new digital signature.

- Ownership QR Code

#### Events And Tickets

- Event Details
- Event Specific Minting Form
- Ticket Details
- Ticket Specific Minting Form
- Use Ticket

#### Imaging - X-Ray

- X-Ray Image Exposure Params Form
- X-Ray Params Values
- X-Ray Image Details
- X-Ray Image Editor Toolbar
- X-Ray Image Editor

#### Payments

- NFT Payment Details
- NFT Receipt Details

#### Profile

Profile in the MyAccount is NFT also. These components display it and allows to create/edit it.

- Account Profile NFT
- Edit Profile NFT

### Tags

Tags displayed on the NFT Card and in NFT Details Basic Info

- Tags 

#### Blockchains

- Neblio Tags
- Dogecoin Tags
- Dogepalooza Tags

#### NFTs

- NFT Card Badge
