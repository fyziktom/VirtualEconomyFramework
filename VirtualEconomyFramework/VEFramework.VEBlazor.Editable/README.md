Komponenta Editable je dodatečným (extension) balíčkem k VEBlazor knihovně. Vznikla kvůli tomu, že jsem potřeboval často zobrazit nějaký parametr třídy v UI a zároveň umožnit i jeho editaci. V praxi to každý zná z aplikací, kde vidí nějaký parametr a vedle něj ikonu tužky. Když na ni klikne, tak se změní text na editační pole, tam změní hodnotu a entrem (či klikem mimo pole) potvrdí, aby se aplikovala změna. 

![Editable text property](https://ve-framework.com/ipfs/QmbFKFFe4mrtpxKGdrsFGsutLHxgj7qdwpMYJB7XB9AKo9)

![Editable bool property](https://ve-framework.com/ipfs/QmNrCtU8etzng5kL7hs37hXwfCL5fsRNNZeA4CL6FXKPZm)

C# je managovaný jazyk. Umožňuje v kódu pracovat s abstraktní vrstvou tříd. Hezkým příkladem jak toto v praxi využít je například komponenta Editable resp. její vnitřní logika. Prvně popíšu příklad, na kterém je funkce komponenty demonstrována. Jedná se o projekt [VEBlazor.Editable.Demo](https://github.com/fyziktom/VirtualEconomyFramework/tree/main/VirtualEconomyFramework/VEBlazor.Editable.Demo).

Demo vychází ze standardního dema pro Blazor WASM aplikaci. Navíc je přidaná reference na projekt/balíček VEFramework.VEBlazor.Editable, definice modelu třídy Piano, komponenta Piano a její přidání do Index.razor. Definice třídy obsahuje několik základních parametrů piána:

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

Parametry jsou různých typů. Pro jejich zobrazení a editaci, by tedy byla potřeba samostatná logika. Nicméně to řeší právě komponenta Editable. Pokud chci zobrazit parametr a umožnit jeho editaci lze to udělat tak jako v komponentě [PianoComponent.razor](https://github.com/fyziktom/VirtualEconomyFramework/blob/main/VirtualEconomyFramework/VEBlazor.Editable.Demo/Shared/PianoComponent.razor)


```csharp
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.Name))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.Brand))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.NumberOfKeys))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.Volume))" TItem="Piano" ItemChanged="@OnPianoChanged" />
<Editable Item="@PianoModel" ParameterName="@(nameof(PianoModel.IsOn))" TItem="Piano" ItemChanged="@OnPianoChanged" />
```

Pro všechny parametry je nyní společný handler pro detekci jejich změny a to funkce OnPianoChanged, které propaguje i aktualizovanou verzi objektu:

```csharp
    [Parameter]
    public Piano PianoModel { get; set; } = new Piano() { Name = "S82", Brand = "Yamaha" };

    private void OnPianoChanged(Piano e)
    {
        PianoModel = e;
        StateHasChanged();
    }
```

Nyní trochu k tomu jak funguje vnitřní logika komponenty...

Blazor umožňuje definovat, že komponenta bude přebírat generický typ objektu. To se provede tak, že na začátek .razor filu napíšu:

```csharp
@typeparam TItem
```

A definuji TItem jako Parametr ideálně i s OnChanged EventCallbackem, abych s objektem mohl v kódu pracovat.

```csharp
    [Parameter]
    public TItem? Item { get; set; }
    [Parameter]
    public EventCallback<TItem> ItemChanged { get; set; }
```

Aby komponenta věděla s kterým parametrem v objektu má pracovat (jestli s "Name", "Volume", apod.) je potřeba zadat ještě název parametru:


```csharp
    [Parameter]
    public string? ParameterName { get; set; }
```

Ten je zadaný jako string, nicméně doporučuji volbu parametru nedávat "hardcoded", ale spíše využít konstrukci s "nameof", protože pokud pak automaticky přes IDE měním název v třídě, tak se správně zpropaguje/změní všude. Příklad:

```csharp
   nameof(PianoModel.Name) // takto namísto "Name"
```

C# dokáže identifikovat názvy proměných a funkcí v třídě, ty pak vyhledat a přistupovat k nim jako k objektům. Díky tomu lze zjistit i typ a přizpůsobit čtení/editaci hodnoty. To stejné pro zobrazení v UI. Dohledání typu lze tedy udělat automaticky, což řeší funkce [LoadParam](https://github.com/fyziktom/VirtualEconomyFramework/blob/0b815587376a3a5b16e13288b9054314c05f92e0/VirtualEconomyFramework/VEFramework.VEBlazor.Editable/Editable.razor#L105).

Na začátku se ověří jestli existuje v objektu typu TItem (to co zadal ten kdo používá komponentu jako typ třídy) konkrétní jméno parametru. Pokud ano, tak vrátí první parametr toho jména, pokud ho nenajde, vrací null.

```csharp
var param = typeof(TItem).GetProperties().Where(p => p.Name == ParameterName).FirstOrDefault();
```

Pokud je možné parametr číst, tak se získá jeho hodnota a podle typu se převede do obecné proměné ParametrValue:

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

Za zmínku stojí ještě funkce, která zajistí uložení parametru: 

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

Při ukládání si opět zavolám parametr jako obecný objekt a pomocí jeho funkce SetValue mu můžu přiřadit novou hodnotu. Musím však zajistit správnost zadané hodnoty. To řeší funkce [OnValueChanged](https://github.com/fyziktom/VirtualEconomyFramework/blob/0b815587376a3a5b16e13288b9054314c05f92e0/VirtualEconomyFramework/VEFramework.VEBlazor.Editable/Editable.razor#L157), která parsuje vstup od uživatele podle identifikovaného typu parametru. Ten se dohledal automaticky:

```csharp
paramType = param.PropertyType;
```

Komponentu by bylo fajn rozšířit ještě minimálně o datum. Přípandě další návrhy či dokonce kontribuce jsou vítány :)
