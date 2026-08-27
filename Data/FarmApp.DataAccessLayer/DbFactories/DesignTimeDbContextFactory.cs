using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.Shared.Constants;

namespace FarmApp.DataAccessLayer.DbFactories;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FarmAppDbContext>
{
    public FarmAppDbContext CreateDbContext(string[] args)
    {
        var appsettingsPath = FindConverterAppsettingsPath();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile(appsettingsPath, optional: false)
            .Build();

        var builder = new DbContextOptionsBuilder<FarmAppDbContext>();
        var connectionString = configuration.GetConnectionString(Constants.AppSettings.CONVERTER_SQL_SERVER_CONNECTION);
        
        builder.UseNpgsql(connectionString);

        return new FarmAppDbContext(builder.Options);
    }

    private static string FindConverterAppsettingsPath()
    {
        string[] relativePaths =
        [
            Path.Combine("Data", "FarmApp.DataConverter", "appsettings.json"),
            Path.Combine("FarmApp.DataConverter", "appsettings.json")
        ];

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            foreach (var relative in relativePaths)
            {
                var candidate = Path.Combine(dir.FullName, relative);
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        throw new FileNotFoundException(
            "FarmApp.DataConverter/appsettings.json not found under any ancestor of AppContext.BaseDirectory.");
    }
}