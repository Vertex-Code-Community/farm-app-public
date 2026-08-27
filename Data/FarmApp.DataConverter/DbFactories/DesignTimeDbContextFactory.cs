using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using FarmApp.DataConverter.DbContext;
using FarmApp.Shared.Constants;

namespace FarmApp.DataConverter.DbFactories;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataConverterDbContext>
{
    public DataConverterDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(@Directory.GetParent(@Directory.GetCurrentDirectory())
            + "/FarmApp.DataConverter/appsettings.json").Build();

        var builder = new DbContextOptionsBuilder<DataConverterDbContext>();
        var connectionString = configuration.GetConnectionString(Constants.AppSettings.CONVERTER_SQL_SERVER_CONNECTION);
        
        builder.UseNpgsql(connectionString);

        return new DataConverterDbContext(builder.Options);
    }
}