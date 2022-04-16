using VENFTApp_Blazor;
using System;
using System.Net.Http;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Tewr.Blazor.FileReader;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAntDesign();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddFileReaderService(options => options.UseWasmSharedBuffer = true);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ClipboardService>();
builder.Services.AddSingleton<AppData>();
builder.Services.AddSingleton<TransactionsService>();

await builder.Build().RunAsync();

