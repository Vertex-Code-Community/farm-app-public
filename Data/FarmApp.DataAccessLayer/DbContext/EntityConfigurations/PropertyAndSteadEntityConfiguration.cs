using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class PropertyAndSteadEntityConfiguration : IEntityTypeConfiguration<PropertyAndSteadEntity>
{
    public void Configure(EntityTypeBuilder<PropertyAndSteadEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId);

        builder.HasOne(x => x.Stead)
            .WithMany()
            .HasForeignKey(x => x.SteadId)
            .IsRequired(false);
        
        builder.HasOne(x => x.CustomStead)
            .WithMany()
            .HasForeignKey(x => x.CustomSteadId)
            .IsRequired(false);
        
        builder.HasOne(ps => ps.Property)
            .WithMany(p => p.PropertySteads)
            .HasForeignKey(ps => ps.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}
