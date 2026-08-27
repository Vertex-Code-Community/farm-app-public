
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations
{
    public class PropertyNoteMediaConfiguration : IEntityTypeConfiguration<PropertyNoteMedia>
    {
        public void Configure(EntityTypeBuilder<PropertyNoteMedia> builder)
        {
            builder.HasKey(prop => prop.Id);

            builder.HasOne(prop => prop.PropertyNote)
                .WithMany(prop => prop.Medias)
                .HasForeignKey(prop => prop.PropertyNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(prop => prop.Id).ValueGeneratedOnAdd();
        }
    }
}
