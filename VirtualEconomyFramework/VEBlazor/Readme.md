# VEBlazor

VEBlazor is the UI component library for .NET Blazor framework.

It uses the [VEDriversLite library](https://www.nuget.org/packages/VEFramework.VEDriversLite/) for the most "backend" work. It is kind of the demo how you can wrap the VEDriversLite into UI components.

Here you can find the [VEBlazor Nuget package](https://www.nuget.org/packages/VEFramework.VEBlazor/)

The library is now as "preview". We are still working hard on the refactoring. Same as this readme.

The library is based on the [great component library Blazorise](https://blazorise.com/). Thanks to [Mladen Macanovic](https://github.com/stsrki) for help with first steps with Blazorise library :)

# Live Demo

You can test [live demo here](https://apptest.ve-nft.com/)

<p align="center">
    <img width="400" alt="Main Page" src="https://ve-framework.com/ipfs/QmdmkGUG5nGsFUmXpNvs9APPHhqevium3HGuLjiXinR23B" />
</p>

<p align="center">
    <img width="400" alt="Image Gallery" src="https://ve-framework.com/ipfs/QmQsRizVtG2vWjr7a6Xyb6V8RwQVypkm56AbxurjK2kcUp" />
</p>

<p align="center">
    <img width="400" alt="Minting Page Images" src="https://ve-framework.com/ipfs/QmYJ1Wtdhz5W5wHXtUUaqpMrSZqtb4sVVctTmFf3o25Xs7" />
</p>

<p align="center">
    <img width="400" alt="Minting Page Data" src="https://ve-framework.com/ipfs/QmRzXJrdrBJB1XPGx8JVtEVGjddar6ijGhs4GqwS51sYqb" />
</p>

If you would like to participate on development and testing please [contact us here](mailto:tomas.svoboda@technicinsider.com).

# Compatibility

- .NET 6.0

# First steps

- Create Empty Blazor Project (can be Server or WebAssembly)
- Add VEBlazor project reference or [VEBlazor Nuget package](https://www.nuget.org/packages/VEFramework.VEBlazor/)
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

    <script src="_content/VEFramework.VEBlazor/jquery.slim.min.js"></script>
    <script src="_content/VEFramework.VEBlazor/bootstrap.min.js"></script>
    <script src="_content/VEFramework.VEBlazor/popper.min.js"></script>
    <script src="_content/VEFramework.VEBlazor/easymde.min.js"></script>
    <script src="_content/VEFramework.VEBlazor/highlight.min.js"></script>
    <script src="_content/BlazorPanzoom/panzoom.min.js"></script>
    <script src="_content/BlazorPanzoom/blazorpanzoom.js"></script>
    <script src="_content/Texnomic.Blazor.JsonViewer/scripts/JsonViewer.js"></script>
    <script src="_content/VEFramework.VEBlazor/AudioRecorder.js"></script>
    <script src="_content/VEFramework.VEBlazor/veblazor.js"></script>

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

Based on the components what you need to use please add another usings, for example VEBlazor.Components.AI

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

- [BDP-WASM](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/BDP-WASM)
- [VEBlazor.Demos.EventsAndTickets](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor.Demo.EventsAndTickets)
- [VEBlazor.Demos.VENFTApp.Client](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor.Demo.VENFTApp.Client)

# Main Components

Most of the components inherits from the [NFTComponentBase or similar base classes in this file](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Base/NFTComponentBase.cs).

## Services

- [App Data](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Services/AppData.cs)
- [Transaction Service](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Services/TransactionsService.cs)

## Layout Related

### Layout 

Do not forget to override the default one when you will create empty Blazor project. It is reffered in the App.razor file.

- [Main Layout](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Layouts/MainLayout.razor)

### Models - Themes

In the Themes you can define your own color scheme. We have one default scheme. Soon we will add Dark mode, etc.

- [Default Theme](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Models/Themes/DefaultTheme.cs)

### Pages

All of these pages templates must be placed as component in page in the MyApp/Pages folder. As it was showed upper in "First Steps" on example Gallery page.

- [My Account](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Pages/MyAccount.razor)
- [Gallery](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Pages/Gallery.razor)
- [Add New](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Pages/AddNew.razor)
- [BuyNFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Pages/BuyNFT.razor)
    - Buy NFT page will load the account without NFTs. It is fast load just for do the transaction to buy NFT.
- [Readme](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Pages/Readme.razor)
    - Readme page loads the Readme.md file placed in the MyApp/wwwroot/

### Menu

Main Layout automatically switches the menus from the Side (on Tablet and biggers) to Top Menu (on phones).

- [Side Menu](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Menu/SideMenu.razor)
- [Top Menu](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Menu/TopMenu.razor)

### Tabs

- [Gallery Tab Content](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tabs/GalleryTabContent.razor)
    - Gallery tab is opened when you are opening new address just to search it. It load the ActiveTab.cs class with (default) 40 newest NFTs.
- [Work Tab Content](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tabs/WorkTabContent.razor)
    - Work tab is used in the BDP App. It is opened for change of the properties or other functions in future. You can forbid this function [like this](https://github.com/fyziktom/VirtualEconomyFramework/blob/4ddd9550051934386f48da6260c927c62cee2b6a/VirtualEconomyFramework/VEBlazor.Demo.VENFTApp.Client/App.razor#L30). It will hide all buttons/labels in other related components around automatically.

## AI

- [AI Settings](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEBlazor/Components/AI/AISettings.razor)
- [CreateMermaidByAIButton](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEBlazor/Components/AI/CreateMermaidByAIButton.razor)
- [CreateTextByAIButton](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEBlazor/Components/AI/CreateTextByAIButton.razor)

## Audio

- [AudioRecorder](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEBlazor/Components/Audio/AudioRecorder.razor)

## Account

- [Account Balance](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/AccountBalances.razor)
- [Accont Sider](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/AccountSider.razor)
    - Main component to display account stats and other things. Sorry for the name. It should be like account page,Tab...Thanks to responsive design you can use it really as kind of sider.
- [Create Account](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/CreateAccount.razor)
    - It creates only Neblio Account now. Doge will be added soon.
- [Import Account](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/ImportAccount.razor)
    - Allow to import the standard VENFT backup file. Or you can import the raw or encrypted key.
- [Unlock Account](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/UnlockAccount.razor)
- [Neblio Address Input](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/NeblioAddressInput.razor)
- [Show Address As QR]()

Neblio Address Input loads bookmarks and allow to use already saved address. It load part of the Unstoppable Domains. We will need to do some updates in API, but part of the addresses works fine. You will type UD name and it will change it to the Neblio Address.

## Transaction 

- [Transaction List](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/Transactions/TransactionsList.razor)
- [Transaction Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/Transactions/TransactionDetails.razor)
- [SendNeblioTransaction](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Account/Transactions/SendNeblioTransaction.razor)

Send Neblio Transaction component offers multiple different transactions:

- Classic Neblio or Token transactions - send to some address coins or tokens 
  - Example: 10 NEBL or 100 VENFT tokens, etc.
- Split Neblio or Token transaction - send to yourself or some address coins or tokens changed in smaller multiple lots
  - Example: 10 x 1 NEBL or 5 x 20 VENFT tokens, etc.
- Airdrop transaction
  - Example: 1 NEBL and 100 VENFT tokens in one transaction


## Bookmarks

- [Bookmark Icon](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Bookmarks/BookmarkIcon.razor)
    - Bookmark Icon contains the way to add/remove bookmark
- [Bookmark Item](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Bookmarks/BookmarkItem.razor)
- [Bookmark Manager List](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Bookmarks/BookmarksManagerList.razor)

## Display

- [Copy Button](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Display/CopyButton.razor)
    - Button will copy the text which is passed as parameter to the component.
- [Copy Icon](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Display/CopyIcon.razor)
    - Icon will copy the text which is passed as parameter to the component.
- [Labeled Progress Bar](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Display/LabeledProgressBar.razor)
    - Wrapped [Blazorise Progress bar](https://blazorise.com/docs/components/progress) with way to setup Name before and Unit with use of the [Blazorise Field component](https://blazorise.com/docs/components/field).
- [PanzoomImage](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Display/PanzoomImage.razor)
    - Wrap for the image with [BlazorPanzoom](https://github.com/shaigem/BlazorPanzoom) component.
- [MermaidEditor](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEBlazor/Components/Display/MermaidEditor.razor)
- [PanzoomMermaid](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEBlazor/Components/Display/PanzoomMermaid.razor)

## NFTs

### Data Upload

- [Upload Data](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/DataUpload/UploadData.razor)
- [File Upload](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/DataUpload/FileUpload.razor)
- [IPFS Upload](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/DataUpload/IPFSUpload.razor)

Upload of the data allows to upload single or multiple files. You can setup this option in UploadData component.

UploadData component is the main which contains two steps: FileUpload (to the browser environment) and then IPFSUpload which upload file to the IPFS node and return the IPFS Hash of the file.

The UploadData component display the preview of the files, allow to remove them and allow to select which one will be the main.

You can upload different files now:

- Images: *.bmp, *.png, *.jpg/jpeg, *.gif, *.jfif
- AVMedia: *.mp3, *.wav, *.ogg, *.mp4, *.avi, *.mpeg, *.mov
- JSON: *.json
- HTML: *.html
- PDF: *.pdf

The upload now contains the option to create images or Mermaid with AI. Go to AI section to see the available components.

### Minting

Minting forms contains lots of usefull components for the input. 

For example Tags input with Autocomplete, Markdown editor, etc. 
Most of these components are wrapped components from the [Blazorise library](https://blazorise.com).
I recommend to browse all [Blazorise components in their demo here](https://bootstrapdemo.blazorise.com/) to check all possibilities.

- [Data Info from](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/DataInfoForm.razor)
- [Fast Number Input](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/FastNumberInput.razor)
- [Load And Display NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/LoadAndDisplayNFTModal.razor)
- [Load NFT From Template](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/LoadNFTFromTemplate.razor)
- [Minting Toolbar](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/MintingToolbar.razor)
- [Toolbar Button](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/ToolbarButton.razor)
- [Multimint Form](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/MultimintForm.razor)
- [Precise Slider](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/PreciseSlider.razor)
- [Space Tags](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Minting/SpaceTags.razor)

### NFT Actions

NFT Actions are used in the NFT Details. You can use them separately on any page.
It takes the NFT refference and then send it and inform with EventCallback about sent.

- [NFT Action Control](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/NFTActionControl.razor)
- [Destroy NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/DestroyNFT.razor)
- [Save NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/SaveNFT.razor)
- [Send NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/SendNFT.razor)
- [Set NFT Price](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/SetNFTPrice.razor)
- [Share NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/ShareNFT.razor)
- [Storage Info and Actions](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Actions/StorageInfoActions.razor)

### NFT Display

#### Common

The NFT Details can be injected with your specific NFT Details Form. Check the OpenNFTDetailsFactory as example how to build some simple logic to feed the NFT Details with own specific form.

- [NFT Basic Info](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Common/NFTBasicInfo.razor)
- [NFT Card](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Common/NFTCard.razor)
- [NFT Data Item](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Common/NFTDataItem.razor)
- [NFT Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Common/NFTDetails.razor)
- [NFT Text](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Common/NFTText.razor)
- [Open NFT Details Factory](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Common/OpenNFTDetailsFactory.razor)

#### Lists

- [NFT Card List](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Lists/NFTCardList.razor)
- [NFT Payments List](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Lists/NFTPaymentsList.razor)
- [NFT Receipts List](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Lists/NFTReceiptsList.razor)

#### Verification

- This code is unique for 60 seconds. It is refreshed after this time with new digital signature.

- [Ownership QR Code](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Verification/OwnershipQRCode.razor)

#### Events And Tickets

- [Event Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/EventsAndTickets/EventDetails.razor)
- [Event Specific Minting Form](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/EventsAndTickets/EventSpecificMintForm.razor)
- [Ticket Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/EventsAndTickets/TicketDetails.razor)
- [Ticket Specific Minting Form](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/EventsAndTickets/TicketSpecificMintForm.razor)
- [Use Ticket](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/EventsAndTickets/UseTicket.razor)

#### Imaging - X-Ray

- [X-Ray Image Exposure Params Form](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Imaging/Xray/XrayExposureParamsForm.razor)
- [X-Ray Params Values](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Imaging/Xray/XrayParamsValues.razor)
- [X-Ray Image Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Imaging/Xray/XrayImagesDetails.razor)
- [X-Ray Image Editor Toolbar](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Imaging/Xray/XrayImageEditorToolbar.razor)
- [X-Ray Image Editor](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Imaging/Xray/XrayImageNFTEditor.razor)

#### Payments

- [NFT Payment Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Payments/NFTPaymentDetails.razor)
- [NFT Receipt Details](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Payments/NFTReceiptDetails.razor)

#### Profile

Profile in the MyAccount is NFT also. These components display it and allows to create/edit it.

- [Account Profile NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Profile/AccountProfile.razor)
- [Edit Profile NFT](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/NFTs/Profile/EditProfile.razor)

### Tags

Tags displayed on the NFT Card and in NFT Details Basic Info

- [Tags](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tags/Tags.razor)

#### Blockchains

- [Neblio Tags](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tags/Blockchains/NeblioTags.razor)
- [Dogecoin Tags](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tags/Blockchains/DogecoinTags.razor)
- [Dogepalooza Tags](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tags/Blockchains/DogepaloozaTags.razor)

#### NFTs

- [NFT Card Badge](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor/Components/Tags/NFTs/NFTCardBadge.razor)
