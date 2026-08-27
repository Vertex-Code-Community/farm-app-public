using Bch.Components.Modal.Extensions;
using Bch.Modules.DomInterop.Extensions;
using Bch.Modules.GlobalEvents.Extensions;
using Bch.Modules.Storage.Extensions;
using Bch.Modules.Storage.Services;
using FarmApp.AdminComponents.Services;
using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FarmApp.AdminComponents.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAdminAppDependencies(this IServiceCollection services)
    {
        services.AddHttpClient();
        
        services.AddScoped<IGlobalLoaderService, GlobalLoaderService>();
        services.AddScoped<IHeaderControlsService, HeaderControlsService>();
        services.AddScoped<IHeaderInsertionService, HeaderInsertionService>();
        services.AddScoped<IPanelService, PanelService>();
        services.AddScoped<IThemeInterface, ThemeService>();
        services.AddScoped<IGlobalErrorService, GlobalErrorService>();
        services.AddScoped<IApiOperationCompletionService, ApiOperationCompletionService>();
        services.AddScoped<INotificationApiService, NotificationApiService>();
        services.AddScoped<INotificationHistoryApiService, NotificationHistoryApiService>();
        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<ISnackbarService, SnackbarService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUsersApiService, UsersApiService>();
        
        services.AddBchStorage();
        services.AddBchModal();
        services.AddBchGlobalEvents();
        services.AddBchDomInterop();
    }
}