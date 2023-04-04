using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazored.LocalStorage;
using BlazorPanzoom;
using VEFramework.VEBlazor;
using VEBlazor.Demo.AI.LearnLanguageWithAI;
using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
// needed to store rate limit counters and ip rules
builder.Services.AddMemoryCache();
//load general configuration from appsettings.json
builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
// inject counter and rules stores
builder.Services.AddInMemoryRateLimiting();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddServerSideBlazor();

#region VEBlazorServices
builder.Services.AddBlazoredLocalStorage();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

//var baseadd = "https://localhost:7161/";
var baseadd = "https://learnlanguagewithai.azurewebsites.net";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseadd) });

builder.Services.AddScoped<AppData>();
builder.Services.AddSingleton<TransactionsService>();
builder.Services.AddHostedService<VEDriversLiteCore>();

builder.Services.AddBlazorPanzoomServices();
builder.Services.AddSwaggerGen();

// configuration (resolvers, counter key builders)
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

#endregion

var app = builder.Build();

app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MintFreeForAI API V1");
});

app.Run();
