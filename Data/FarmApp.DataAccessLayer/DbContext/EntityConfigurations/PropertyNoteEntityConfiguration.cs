using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class PropertyNoteEntityConfiguration : IEntityTypeConfiguration<PropertyNoteEntity>
{
    public void Configure(EntityTypeBuilder<PropertyNoteEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(pn => pn.Property)
            .WithMany(p => p.PropertyNotes)
            .HasForeignKey(pn => pn.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pn => pn.Status)
            .WithMany()
            .HasForeignKey(pn => pn.StatusId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
    }
}
