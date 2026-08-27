using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class PurposeEntityConfiguration : IEntityTypeConfiguration<PurposeEntity>
{
    public void Configure(EntityTypeBuilder<PurposeEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasMany(p => p.Steads)
            .WithOne(s => s.Purpose)
            .HasForeignKey(w => w.PurposeId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}