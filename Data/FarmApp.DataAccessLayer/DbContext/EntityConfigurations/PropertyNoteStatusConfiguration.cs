
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations
{
    public class PropertyNoteStatusConfiguration : IEntityTypeConfiguration<PropertyNoteStatusEntity>
    {
        public void Configure(EntityTypeBuilder<PropertyNoteStatusEntity> builder)
        {
            builder.HasKey(x => x.Id);


            builder.HasData(new PropertyNoteStatusEntity
            {
                Id = 1,
                Code = "FAILED",
                Name = "Не виконано",
                TextColorHex = "#1D8B41",
                BGColorHex = "#DCFAE9",
                IsDefault = true,
                UserId = null
            },
            new PropertyNoteStatusEntity
            {
                Id = 2,
                Code = "IN_PROGRESS",
                Name = "В процессі",
                TextColorHex = "#925C00",
                BGColorHex = "#FDF2CA",
                IsDefault = true,
                UserId = null
            },
            new PropertyNoteStatusEntity
            {
                Id= 3,
                Code = "DONE",
                Name = "Виконано",
                BGColorHex = "#C42921",
                TextColorHex = "#FFDED8",
                IsDefault = true,
                UserId = null
            });
        }
    }
}
