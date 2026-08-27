using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataConverter.DbContext.EntityConfigurations;

public class PolygonEntityConfiguration : IEntityTypeConfiguration<PolygonEntity>
{
    public void Configure(EntityTypeBuilder<PolygonEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasMany(p => p.Tiles)
            .WithMany(t => t.Polygons);
    }
}