using CommunityToolkit.Maui;
using FarmApp.Components.Extensions;
using FarmApp.Mobile.Bridges;
using FarmApp.Mobile.Controls;
using FarmApp.Mobile.Services;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Services;
using FarmApp.Services;
using FarmApp.Services.Services.Interfaces;
using MaplibreMaui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using FarmApp.Shared.Options;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Components.Services;


#if ANDROID
using FarmApp.Mobile.Platforms.Android;
#endif
#if IOS
using FarmApp.Mobile.Platforms.iOS;
#endif
namespace FarmApp.Mobile;


public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
            
        var builder = MauiApp.CreateBuilder();
        builder
                .UseMauiApp<App>()
                .UseMaplibre()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

#if IOS
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<HitRoutingAbsoluteLayout, HitRoutingAbsoluteLayoutHandler>();
        });
#endif


#if DEBUG
        const string apsettingsName = "FarmApp.Mobile.appsettings.Development.json";
#else
        const string apsettingsName = "FarmApp.Mobile.appsettings.json";
#endif

        


        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(apsettingsName);

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

        builder.Configuration.AddConfiguration(config);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddLocalization();

        services.AddFarmAppDependencies(configuration);
        services.AddSingleton<INotificationDisplayPreferences, MauiNotificationDisplayPreferences>();
        services.AddScoped<IAppStoreService, LocalStorageService>();
        services.AddScoped<INavigationService, MobileNavigationService>();
        services.AddScoped<IAdService, AdService>();
        services.AddScoped<ILocationService, MobileLocationService>();

        services.AddTransient<MainPage>();
        services.AddSingleton<ISteadService, SteadService>();
        services.AddSingleton<ISteadEditorService, SteadEditorService>();
        services.AddSingleton<ICustomSteadsService, CustomSteadsService>();
        services.AddSingleton<CountryBorderMapService>();
        services.AddSingleton<IPropertyEditorService, PropertyEditorService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMediaPickerService, MobileMediaPickerService>();
        services.AddScoped<IThemeDetector, ThemeDetector>();
        services.AddScoped<IThemeStorage, ThemeStorage>();
        services.AddScoped<HeightAnimatorService>();
        services.AddScoped<IGuideTourService, GuideTourService>();
        services.AddSingleton<IFirstTimeSetupService, FirstTimeSetupService>();

        services.AddSingleton<IMapService, SteadService>(p => (SteadService) p.GetRequiredService<ISteadService>());
        services.AddSingleton<IMapService, SteadEditorService>(p => (SteadEditorService) p.GetRequiredService<ISteadEditorService>());
        services.AddSingleton<IMapService, CustomSteadsService>(p => (CustomSteadsService) p.GetRequiredService<ICustomSteadsService>());
        services.AddSingleton<IMapService, CountryBorderMapService>(p => p.GetRequiredService<CountryBorderMapService>());
        services.AddSingleton<IMapService, PropertyEditorService>(p => (PropertyEditorService) p.GetRequiredService<IPropertyEditorService>());
        services.AddSingleton<IPlatformContext, MauiPlatformContext>();
        services.AddSingleton<IThemeDetector, ThemeDetector>();
        services.AddSingleton<IThemeStorage, ThemeStorage>();
        services.AddSingleton<IMobilePlatformService, MobilePlatformService>();

        services.AddSingleton<IBackButtonService, BackButtonService>();
        services.AddSingleton<IMapSearchStateService, MapSearchStateService>();
        
        services.AddSingleton<IPushNotificationPermissionService, PushNotificationPermissionService>();
        services.AddSingleton<PushNotificationsRegistrationService>();
        services.AddSingleton<IPushNotificationsHttpService, PushNotificationsHttpService>();
        services.AddSingleton<IPushNotificationsMessageService, PushNotificationsMessageService>();

        services.AddSingleton<IGuideTourService, GuideTourService>();

        services.Configure<PushNotificationOptions>(configuration.GetSection(nameof(PushNotificationOptions)));
        services.Configure<MapTilesOptions>(configuration.GetSection(MapTilesOptions.SectionName));
        
#if ANDROID
        services.AddSingleton<IDeviceInstallationService, DeviceInstallationAndroidService>();
        services.AddSingleton<IDeviceInfoService, DeviceInfoAndroidService>();
        services.AddSingleton<IMediaCaptureService, MediaCaptureService>();
        services.AddSingleton<IGalleryPickerService, AndroidGalleryPickerService>();
#endif
#if IOS
        services.AddSingleton<IDeviceInstallationService, DeviceInstallationIOSService>();
        services.AddSingleton<IDeviceInfoService, DeviceInfoIOSService>();
        services.AddSingleton<IMediaCaptureService, iOSAvCaptureService>();
        services.AddSingleton<IGalleryPickerService, iOSGalleryPickerService>();
        services.AddSingleton<IPhotoPermissionService, iOSPhotoPermissionService>();
        services.AddSingleton<IMobilePermissionService, iOSPermissionService>();
#endif
        builder.Services.AddSingleton<MapStyleState>();
        services.AddSingleton<ICountryMapStorage, CountryMapStorage>();

#if IOS
#else
#endif

        services.AddMauiBlazorWebView();

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        _ = app.Services.GetRequiredService<MapCountryViewCoordinator>();

        var loc = app.Services.GetRequiredService<ILocalizationService>();
        loc.Initialize();

        var pushRegistration = app.Services.GetRequiredService<PushNotificationsRegistrationService>();
        NotificationRegistrationBridge.RefreshDeviceRegistrationAsync = () => pushRegistration.RegisterDeviceAsync();

        return app;
    }
}