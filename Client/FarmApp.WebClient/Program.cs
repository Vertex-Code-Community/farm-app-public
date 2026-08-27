using Bch.Components.Modal.Root;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FarmApp.Components.Extensions;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.WebClient;
using FarmApp.WebClient.Services;
using FarmApp.Services.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.Add<BchRootModal>("body::after");

var services = builder.Services;
var configuration = builder.Configuration;

services.AddFarmAppDependencies(configuration);
services.AddScoped<IAppStoreService, WebAppStoreService>();
services.AddScoped<INotificationDisplayPreferences, WebNotificationDisplayPreferences>();
services.AddScoped<INavigationService, BrowserNavigationService>();
services.AddScoped<IAdService, BrowserAdService>();
services.AddScoped<ILocationService, BrowserLocationService>();
services.AddScoped<IMessageService, MessageService>();
services.AddScoped<IThemeDetector, WebThemeDetector>();
services.AddScoped<IThemeStorage, WebThemeStorage>();
services.AddSingleton<IPlatformContext, WebPlatformContext>();
services.AddSingleton<ICountryMapStorage, WebCountryMapStorage>();

ScreenOffsetProvider.Bottom = 20;

var host = builder.Build();
_ = host.Services.GetRequiredService<MapCountryViewCoordinator>();
await host.RunAsync();
