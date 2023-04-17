using Microsoft.AspNetCore.Components.WebView.Maui;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazored.LocalStorage;
using BlazorPanzoom;
using VEFramework.VEBlazor;
using IndexedDB.Blazor;
using VEBlazor.Demo.MAUI.VENFTApp.Services;

namespace VEBlazor.Demo.MAUI.VENFTApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif
            builder.Services.AddHttpClient();
            builder.Services.AddTransient<ApiService>();
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

            return builder.Build();
        }
    }
}