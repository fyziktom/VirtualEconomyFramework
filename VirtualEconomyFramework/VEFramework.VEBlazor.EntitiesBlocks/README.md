# VEFramework.VEBlazor.EntitiesBlocks library

Tato knihovna obsahuje komponenty ke knihovně [VEFramework.VEDriversLite.EntitiesBlocks](https://www.nuget.org/packages/VEFramework.VEDriversLite.EntitiesBlocks/).

Jako základní knihovna komponent je použita knihovna [Blazorise](https://blazorise.com/). 

## Demo

Demo použití komponent si můžete prohlédnout [zde](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor.EntitiesBlocks.Demo.Energy).

O knihovně byly také napsány články, které jsou k dispozici na [wiki](https://github.com/fyziktom/VirtualEconomyFramework/wiki#entitiesblocks).


## Použití knihovny

Vytvořte novou Blazor aplikaci (WASM nebo Server App).

### Instalace

Pro instalaci knihovny pomocí NuGet:

```bash

Install-Package VEFramework.VEBlazor.EntitiesBlocks

```

### Přidání dependencí

Pokud chcete používat všechny komponenty, je potřeba přidat následující dependenci do souboru `_Imports.razor`:
```csharp
@using VEBlazor.EntitiesBlocks.Demo.Energy
@using VEBlazor.EntitiesBlocks.Demo.Energy.Shared
@using VEFramework.VEBlazor.EntitiesBlocks
@using VEFramework.VEBlazor.EntitiesBlocks.Blocks
@using VEFramework.VEBlazor.EntitiesBlocks.Graphs
@using VEFramework.VEBlazor.EntitiesBlocks.Entities
@using VEFramework.VEBlazor.EntitiesBlocks.Services
@using VEFramework.VEBlazor.EntitiesBlocks.PVE
@using VEFramework.VEBlazor.EntitiesBlocks.BatteryStorage
@using VEFramework.VEBlazor.EntitiesBlocks.Simulators;

@using Blazorise
@using Blazorise.TreeView
@using Blazorise.Charts
@using Blazorise.Components
@using Blazorise.Snackbar

@using VEDriversLite.EntitiesBlocks.Handlers
@using VEDriversLite.EntitiesBlocks.Sources
@using VEDriversLite.EntitiesBlocks.Consumers
@using VEDriversLite.EntitiesBlocks.Blocks
@using VEDriversLite.EntitiesBlocks.Tree
@using VEDriversLite.EntitiesBlocks.Entities
@using VEDriversLite.EntitiesBlocks.PVECalculations
@using VEDriversLite.EntitiesBlocks.StorageCalculations
```

Poté přidejte inicializaci knihovny do souboru `Program.cs`. Zde je celý Program.cs:

```csharp
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VEBlazor.EntitiesBlocks.Demo.Energy;
using VEFramework.VEBlazor.EntitiesBlocks.Analytics;
using VEFramework.VEBlazor.EntitiesBlocks.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AppData>();

builder.Services.AddScoped<ICalculationService, CalculationService>();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

await builder.Build().RunAsync();

```

App.razor v základní formě s jednoduchou úpravou barevného schéma vypadá takto:

```csharp
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
</Blazorise.ThemeProvider>

<MessageProvider />
<PageProgressProvider />
<NotificationProvider />

@code {
    string PrimaryColor = "#0c345c";
    string SecondaryColor = "#43ab74";


    private Theme theme = new Theme
        {
            ColorOptions = new ThemeColorOptions
            {
                Primary = "#0c345c",
                Secondary = "#43ab74",
            },

            TextColorOptions = new ThemeTextColorOptions
            {
                Primary = "#0c345c",
                Secondary = "#43ab74",
            },
            BackgroundOptions = new ThemeBackgroundOptions
            {
                Primary = "#0c345c",
                Secondary = "#43ab74",
            },
            ButtonOptions = new ThemeButtonOptions
            {
                Padding = ".5rem 1rem",
            },
            BodyOptions = new ThemeBodyOptions
            {
                TextColor = "#000000"
            },
            InputOptions = new ThemeInputOptions
            {
                Color = "#000000"
            },

        };

}
```

Posledním krokem je přidání odkazů na zdroje pro css a externí JavaScrip skripty. Ta se provede v index.html, který pak vypadá následovně:

```csharp
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    <title>VE Energy</title>
    <base href="/" />

    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css" integrity="sha384-zCbKRCUGaJDkqS1kPbPd7TveP5iyJE0EjAuZQTgFLD2ylzuqKfdKlfG/eSrtxUkn" crossorigin="anonymous">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.15.4/css/all.css">

    <link href="_content/Blazorise/blazorise.css" rel="stylesheet" />
    <link href="_content/Blazorise.Bootstrap/blazorise.bootstrap.css" rel="stylesheet" />
    <link href="_content/Blazorise.Snackbar/blazorise.snackbar.css" rel="stylesheet" />
    <link href="_content/Blazorise.TreeView/blazorise.treeview.css" rel="stylesheet" />

    <link href="https://unpkg.com/easymde/dist/easymde.min.css" rel="stylesheet" />

    <link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="css/app.css" rel="stylesheet" />
    <link href="VEBlazor.EntitiesBlocks.Demo.Energy.styles.css" rel="stylesheet" />

    <script src="js/main.js"></script>
</head>

<body>
    <div id="app">
        <div style="position:absolute; top:30vh; width:100%; text-align:center">
            <h2>VE Energy App</h2>
            <p></p>
            <hr />
            <p></p>
            <p><i class="oi oi-reload"></i> The VE Energy Demo Web App is loading...</p>
            <p>First load of the App can take a while. Next time the load will be much faster.</p>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/jquery@3.5.1/dist/jquery.slim.min.js" integrity="sha384-DfXdz2htPH0lsSSs5nCTpuj/zy4C+OGpamoFVy38MVBnE+IbbVYUew+OrCXaRkfj" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.1/dist/umd/popper.min.js" integrity="sha384-9/reFTGAW83EW2RDu2S0VKaIzap3H66lZH81PoYlFhbGU+6BZp6G7niu735Sk7lN" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/js/bootstrap.min.js" integrity="sha384-VHvPCCyXqtD5DqJeNxl2dtTyhF78xXNXdkwX1CZeRusQfRKp+tA7hAShOK/B/fQ2" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/3.7.1/chart.min.js"></script>
    <script src="_content/VEFramework.VEBlazor.EntitiesBlocks/JsInterop.js"></script>
    <script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>

    <div id="blazor-error-ui">
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>
    <script src="_framework/blazor.webassembly.js"></script>

</body>

</html>

```

Následně je možné použít komponenty například hned na stránce Index.razor, nebo si pro ně vytvořit vlastní stránky.

Detaily použití komponent si prosím prohlédněte ve zdrojovém kódu [demo aplikace](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor.EntitiesBlocks.Demo.Energy/Pages)

Doporučuji primárně stránku Index a PVESimulation. Ty ostatní jsou spíše experimentální.

## Services

Hlavní je služba [AppData](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Services/AppData.cs), která obsahuje [EntitiesHandler](https://github.com/fyziktom/VirtualEconomyFramework/blob/b45f80e7db34f403d60e6697ba6a5f352a86a8a1/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Services/AppData.cs#L17) a [PVE Handler](https://github.com/fyziktom/VirtualEconomyFramework/blob/b45f80e7db34f403d60e6697ba6a5f352a86a8a1/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Services/AppData.cs#L18) a [BatteryBlocksHandler](https://github.com/fyziktom/VirtualEconomyFramework/blob/b45f80e7db34f403d60e6697ba6a5f352a86a8a1/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Services/AppData.cs#L19).


## Komponenty

### Base

Pro komponenty je vytvořena base, která obsahuje některé základní vlastnosti a metody. Doporučuji si prostudovat třídu [zde](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Base/EntitiesBlocksComponentBase.cs).

### Entities

- [EntitiesTree](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/EntitiesTree.razor)
- [AddEntity](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/AddEntity.razor)
- [RemoveEntity](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/RemoveEntity.razor)
- [SetEntityType](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/SetEntityType.razor)
- [SetSourceType](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/SetSourceType.razor)
- [SetConsumerType](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/SetConsumerType.razor)
- [EntityTotalBalance](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Entities/EntityTotalBalance.razor)

### Blocks

- [AddBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/AddBlock.razor)
- [AddSimulatedBlocks](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/AddSimulatedBlocks.razor)
- [ClearAllBlocks](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/ClearAllBlocks.razor)
- [RemoveBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/RemoveBlock.razor)
- [SetBlockAmount](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/SetBlockAmount.razor)
- [SetBlockDirection](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/SetBlockDirection.razor)
- [SetBlockType](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/SetBlockType.razor)
- [SetRepetitiveBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/SetRepetitiveBlock.razor)
- [StartEndDaySelector](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Blocks/StartEndDaySelector.razor)

### Simulators

- [AddSimulator](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Simulators/AddSimulator.razor)
- [RemoveSimulator](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Simulators/RemoveSimulator.razor)
- [SimulatorDayProfileDataBarGraph](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Simulators/SimulatorDayProfileDataBarGraph.razor)
- [SimulatorsGroupCard](https://github.com/fyziktom/VirtualEconomyFramework/blob/Development/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Simulators/SimulatorsGroupCard.razor)

### PVE Simulator

- [AddPVPanel](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/AddPVPanel.razor)
- [PVEBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/PVEBlock.razor)
- [PVEPanelCard](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/PVPanelCard.razor)
- [PVEPanelsGroupCard](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/PVPanelsGroupCard.razor)
- [RemovePanel](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/RemovePanel.razor)
- [PVESimulationGraph](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/PVESimulationGraph.razor)
- [PVEYearGraph](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/PVE/PVEYearGraph.razor)

### Battery Storage

- [AddBatteryBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/BatteryStorage/AddBatteryBlock.razor)
- [RemoveBatteryBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/BatteryStorage/RemoveBatteryBlock.razor)
- [BatteryBlocksGroupCard](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/BatteryStorage/BatteryBlocksGroupCard.razor)

### Graphs

- [ConsumptionGraph](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Graphs/ConsumptionGraph.razor)

- [BlocksTimeGraph](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Graphs/BlocksTimeGraph.razor)
- [BlockGraphBlock](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Graphs/BlockGraphBlock.razor)
- [BlockGraphVerticalLines](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Graphs/BlockGraphVerticalLines.razor)
- [RemoveBlockGraphIcon](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEFramework.VEBlazor.EntitiesBlocks/Graphs/RemoveBlockGraphIcon.razor)
