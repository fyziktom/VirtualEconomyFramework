using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VEBlazor.Demo.Publishing.Client;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazored.LocalStorage;
using BlazorPanzoom;
using VEFramework.VEBlazor;
using IndexedDB.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

#region VEBlazorServices
builder.Services.AddBlazoredLocalStorage();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

builder.Services.AddScoped<AppData>();
builder.Services.AddSingleton<TransactionsService>();
builder.Services.AddSingleton<IIndexedDbFactory, IndexedDbFactory>();
builder.Services.AddBlazorPanzoomServices();
#endregion

await builder.Build().RunAsync();
