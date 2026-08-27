using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class CustomSteadEntityConfiguration : IEntityTypeConfiguration<CustomSteadEntity>
{
    public void Configure(EntityTypeBuilder<CustomSteadEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder.HasOne(x => x.Stead)
            .WithMany()
            .HasForeignKey(x => x.SteadId)
            .IsRequired(false);
        
        builder.HasOne(p => p.User)
            .WithMany(u => u.CustomSteads)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}