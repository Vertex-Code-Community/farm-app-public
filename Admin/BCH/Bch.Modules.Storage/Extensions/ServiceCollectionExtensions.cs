using Bch.Modules.Storage.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bch.Modules.Storage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBchStorage(this IServiceCollection services)
    {
        services.AddScoped<ILocalStorageService, LocalStorageService>();
        return services;
    }
}