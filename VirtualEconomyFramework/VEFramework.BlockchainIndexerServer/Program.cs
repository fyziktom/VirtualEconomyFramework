using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using BlazorPanzoom;
using IndexedDB.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using VEFramework.BlockchainIndexerServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#region VEBlazorServices
builder.Services.AddBlazoredLocalStorage();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

#if DEBUG
var baseadd = "http://localhost:5000/";
#else
var baseadd = "https://blockchainsapi.azurewebsites.net/";//"http://localhost:5000/";
#endif

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseadd) });

builder.Services.AddScoped<AppData>();
builder.Services.AddScoped<TransactionsService>();
builder.Services.AddScoped<IIndexedDbFactory, IndexedDbFactory>();
builder.Services.AddHostedService<VECoreService>();

builder.Services.AddBlazorPanzoomServices();

#endregion

builder.Services.AddSwaggerGen();

var app = builder.Build();

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
app.UseCors(c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapFallbackToPage("/_Host");
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VEFramework Blockchain Indexer Server API V1");
});


app.Run();
