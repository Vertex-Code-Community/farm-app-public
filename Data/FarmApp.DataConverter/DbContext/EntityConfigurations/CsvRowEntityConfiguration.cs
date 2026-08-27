using Microsoft.EntityFrameworkCore;
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataConverter.DbContext.EntityConfigurations;

public class CsvRowEntityConfiguration : IEntityTypeConfiguration<CsvRowEntity>
{
    public void Configure(EntityTypeBuilder<CsvRowEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Cadnum);
    }
}