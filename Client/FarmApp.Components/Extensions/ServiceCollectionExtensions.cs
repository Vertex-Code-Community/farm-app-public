using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Bch.Components.Modal.Extensions;
using Bch.Modules.DomInterop.Extensions;
using Bch.Modules.GlobalEvents.Extensions;
using FarmApp.Services.Providers;
using FarmApp.Services.Services;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Options;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Components.Services;
using FarmApp.Services.Auth;

namespace FarmApp.Components.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFarmAppDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuthStateProvider>();
        services.AddScoped<PageResolveProvider>();
        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<IMapSteadService, MapSteadService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICustomSteadService, CustomSteadService>();
        services.AddScoped<IMapPropertyService, MapPropertyService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPropertyNoteService, PropertyNoteService>();
        services.AddScoped<IPropertyNoteStatusService, PropertyNoteStatusService>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<ITabsService, TabsService>();
        services.AddScoped<IMapModalService, MapModalService>();
        services.AddScoped<IMapCallbackService, MapCallbackService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationHistoryApiService, NotificationHistoryApiService>();
        services.AddScoped<IGlobalLoaderService, GlobalLoaderService>();
        services.AddScoped<IWeatherService, WeatherService>();
        services.AddScoped<ILocalizationService, LocalizationService>();

        services.AddScoped<IPropertyNotesAggregatorService, PropertyNotesAggregatorService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<TokenRefreshCoordinator>();
        services.AddSingleton<UploadQueueService>();
        services.AddSingleton<IMediaService, MediaService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<MapCountryViewCoordinator>();
        services.AddSingleton<IMapControlsVisibilityService, MapControlsVisibilityService>();
        services.Configure<ApiOptions>(configuration.GetSection(Constants.AppSettings.API_CONFIGURATION));
        
        services.AddScoped(sp =>
        {
            var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
            return new HttpClient { BaseAddress = new Uri(apiOptions.BaseUri) };
        });
        
        services.AddBchModal();
        services.AddBchGlobalEvents();
        services.AddBchDomInterop();
    }
}