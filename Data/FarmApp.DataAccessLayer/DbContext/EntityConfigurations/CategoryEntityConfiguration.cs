using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasMany(p => p.Steads)
            .WithOne(s => s.Category)
            .HasForeignKey(w => w.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}