using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class OwnershipEntityConfiguration : IEntityTypeConfiguration<OwnershipEntity>
{
    public void Configure(EntityTypeBuilder<OwnershipEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasMany(p => p.Steads)
            .WithOne(s => s.Ownership)
            .HasForeignKey(w => w.OwnershipId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.HasIndex(e => e.Name); 

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}