using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class PropertyEntityConfiguration : IEntityTypeConfiguration<PropertyEntity>
{
    public void Configure(EntityTypeBuilder<PropertyEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder.HasOne(p => p.User)
            .WithMany(u => u.Properties)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
