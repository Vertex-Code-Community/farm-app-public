using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FarmApp.DataConverter.DbContext.EntityConfigurations;
using FarmApp.Entities.Entity;

namespace FarmApp.DataConverter.DbContext;

public class DataConverterDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<TileEntity> Tiles { get; set; }
    public DbSet<PolygonEntity> Polygons { get; set; }
    public DbSet<CsvRowEntity> CsvRows { get; set; }

    public DataConverterDbContext(DbContextOptions<DataConverterDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, (eventId, logLevel) => logLevel >= LogLevel.Critical);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new TileEntityConfiguration());
        builder.ApplyConfiguration(new PolygonEntityConfiguration());
        builder.ApplyConfiguration(new CsvRowEntityConfiguration());
    }
}