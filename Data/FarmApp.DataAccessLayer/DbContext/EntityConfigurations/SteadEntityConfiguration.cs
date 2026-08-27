using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class SteadEntityConfiguration : IEntityTypeConfiguration<SteadEntity>
{
    public void Configure(EntityTypeBuilder<SteadEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(s => s.Category)
            .WithMany(c => c.Steads)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.Ownership)
            .WithMany(c => c.Steads)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.Purpose)
            .WithMany(c => c.Steads)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}