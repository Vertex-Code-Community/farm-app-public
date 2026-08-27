using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataConverter.DbContext.EntityConfigurations;

public class TileEntityConfiguration : IEntityTypeConfiguration<TileEntity>
{
    public void Configure(EntityTypeBuilder<TileEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasMany(t => t.Polygons)
            .WithMany(p => p.Tiles);
    }
}