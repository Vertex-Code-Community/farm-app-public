using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.DataAccessLayer.Services;
using FarmApp.Shared.Constants;

namespace FarmApp.DataAccessLayer;

public static class StartupExtension
{
    public static void DataAccessInitializer(this IServiceCollection services, IConfiguration configuration)
    {
        var farmAppSqlConnectionString = configuration.GetConnectionString(Constants.AppSettings.FARM_APP_SQL_SERVER_CCONNECTION);
        
        services.AddDbContext<FarmAppDbContext>(options =>
        {
            options.UseNpgsql(farmAppSqlConnectionString, x =>
                x.MigrationsAssembly("FarmApp.DataAccessLayer"));
        });
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISteadRepository, SteadRepository>();
        services.AddScoped<ICustomSteadRepository, CustomSteadRepository>();
        services.AddScoped<IPropertyNoteRepository, PropertyNoteRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyAndSteadRepository, PropertyAndSteadRepository>();
        services.AddScoped<IPropertyNoteMediaRepository, PropertyNoteMediaRepository>();
        services.AddScoped<IPropertyNoteStatusRepository, PropertyNoteStatusRepository>();
        services.AddScoped<IPushNotificationQueueRepository, PushNotificationQueueRepository>();

        using (var context = services.BuildServiceProvider().GetService<FarmAppDbContext>())
        {
            context?.Database.Migrate();
        }

        services.SeedDataAsync().Wait();
    }
}