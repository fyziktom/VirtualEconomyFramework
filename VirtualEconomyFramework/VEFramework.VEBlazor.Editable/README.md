# VEBlazor.Editable

[![VEBlazor.Editable](https://img.shields.io/nuget/v/VEFramework.VEBlazor.Editable?label=VEBlazor.Editable)](https://www.nuget.org/packages/VEFramework.VEBlazor.Editable/)

Komponenta `Editable` je dodatečným (extension) balíčkem [`VEBlazor`](../VEBlazor) knihovny. Vznikla kvůli potřebě častého zobrazování parametrů tříd v UI a jejich editace. 

![Editable text property](https://ve-framework.com/ipfs/QmbFKFFe4mrtpxKGdrsFGsutLHxgj7qdwpMYJB7XB9AKo9)

![Editable bool property](https://ve-framework.com/ipfs/QmNrCtU8etzng5kL7hs37hXwfCL5fsRNNZeA4CL6FXKPZm)

C# v kódu umožňuje pracovat s abstraktní vrstvou tříd. Příkladem využití v praxi je komponenta `Editable` resp. její vnitřní logika. Příklad demonstrace komponenty na projektu [VEBlazor.Editable.Demo](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor.Editable.Demo).

Demo vychází ze standardního dema pro Blazor WASM aplikaci. Navíc je přidána reference na projekt/balíček `VEFramework.VEBlazor.Editable`. Definice modelu třídy `Piano`, komponenta `Piano` a její přidání do `Index.razor`. Definice třídy obsahuje několik základních parametrů piána:

```csharp
namespace VEBlazor.Editable.Demo.Models
{
    public class Piano
    {
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int NumberOfKeys { get; set; } = 88;
        public double Volume { get; set; } = 0.5;
        public bool IsOn { get; set; } = true;
    }
}
```

Parametry jsou různých typů. Pro jejich zobrazení a editaci, by byla potřeba samostatná logika. A tu řeší právě komponenta `Editable`. Pokud je potřeba zobrazit parametr a umožnit jeho editaci, lze to udělat tak jako v komponentě [PianoComponent.razor](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor.Editable.Demo/Shared/PianoComponent.razor)


```csharp
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.Name))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.Brand))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.NumberOfKeys))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.Volume))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.IsOn))" TItem="Piano" ItemChanged="@OnPianoChanged" />
```

Pro všechny parametry je společný handler pro detekci jejich změny - `OnPianoChanged`, který propaguje i aktualizovanou verzi objektu:

```csharp
    [Parameter]
    public Piano PianoModel { get; set; } = new Piano() { Name = "S82", Brand = "Yamaha" };

    private void OnPianoChanged(Piano e)
    {
        PianoModel = e;
        StateHasChanged();
    }
```

Blazor umožňuje definovat, že komponenta bude přebírat generický typ objektu. To se provede tak, že se na začátek `.razor` souboru napíše:

```csharp
@typeparam TItem
```

Tím se definuje `TItem` jako `Parametr`, ideálně s `OnChanged` EventCallbackem, aby se s objektem dalo v kódu pracovat.

```csharp
    [Parameter]
    public TItem? Item { get; set; }
    [Parameter]
    public EventCallback<TItem> ItemChanged { get; set; }
```

Aby komponenta věděla, se kterým parametrem v objektu má pracovat (jestli s `Name`, `Volume`, apod.) je potřeba zadat název parametru:


```csharp
    [Parameter]
    public string? ParameterName { get; set; }
```

Ten je zadaný jako string, nicméně se doporučuje volbu parametru nezadávat natvrdo. Spíš se doporučuje využít konstrukci s `nameof`, protože pokud se automaticky přes IDE mění název ve třídě, správně se zpropaguje/změní všude. Příklad:

```csharp
   nameof(PianoModel.Name) // takto namísto "Name"
```

C# dokáže identifikovat názvy proměných a funkcí ve třídě. Ty pak vyhledat a přistupovat k nim jako k objektům. Díky tomu lze zjistit typ a přizpůsobit čtení/editaci hodnoty. To samé platí pro zobrazení v UI. Dohledání typu lze udělat automaticky, což je řešeno funkcí [LoadParam](https://github.com/fyziktom/VirtualEconomyFramework/blob/0b815587376a3a5b16e13288b9054314c05f92e0/VirtualEconomyFramework/VEFramework.VEBlazor.Editable/Editable.razor#L105).

Na začátku se ověří, jestli v objektu typu `TItem` existuje konkrétní jméno parametru. Pokud ano, vrátí první parametr jména, pokud ne, vrátí `null`.

```csharp
var param = typeof(TItem).GetProperties().Where(p => p.Name == ParameterName).FirstOrDefault();
```

Pokud je možné parametr číst, získá se jeho hodnota a podle typu se převede do obecné proměné `ParametrValue`:

```csharp
if (param != null && param.CanRead)
        {
            var value = param.GetValue(Item);
            if (value != null)
            {
                paramType = param.PropertyType;

                if (param.PropertyType == typeof(string) || param.PropertyType == typeof(String))
                    ParameterValue = (string)value;
                else if (param.PropertyType == typeof(int))
                    ParameterValue = (int)value;
                else if (param.PropertyType == typeof(double))
                    ParameterValue = (double)value;
                else if (param.PropertyType == typeof(bool))
                    ParameterValue = (bool)value;

                await InvokeAsync(StateHasChanged);
            }
        }
```

Za zmínku ještě stojí funkce, která zajistí uložení parametru: 

```csharp
    private async Task Save()
    {
        Editing = false;

        var param = typeof(TItem).GetProperties().Where(p => p.Name == ParameterName).FirstOrDefault();
        if (param != null && param.CanWrite)
        {
            param.SetValue(Item, ParameterValue);
            await ItemChanged.InvokeAsync(Item);
        }
        await InvokeAsync(StateHasChanged);
    }
```

Při ukládání se opět zavolá parametr jako obecný objekt a pomocí jeho funkce `SetValue` se mu přiřadí nová hodnota. Musí se ale zajistit správnost zadané hodnoty. To řeší funkce [OnValueChanged](https://github.com/fyziktom/VirtualEconomyFramework/blob/0b815587376a3a5b16e13288b9054314c05f92e0/VirtualEconomyFramework/VEFramework.VEBlazor.Editable/Editable.razor#L157), která parsuje vstup od uživatele podle identifikovaného typu parametru, který se dohledal automaticky:

```csharp
paramType = param.PropertyType;
```

_Komponentu by bylo vhodné rozšířit ještě minimálně o datum. Případné další návrhy a dokonce [kontribuce](../../CONTRIBUTING.md) jsou vítány :)_
