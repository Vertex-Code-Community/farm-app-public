using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class PushDeviceRegistrationEntityConfiguration : IEntityTypeConfiguration<PushDeviceRegistrationEntity>
{
    public void Configure(EntityTypeBuilder<PushDeviceRegistrationEntity> builder)
    {
        builder.HasKey(x => x.DeviceId);
        builder.Property(x => x.DeviceToken).IsRequired();
        builder.Property(x => x.Platform).IsRequired();
        builder.Property(x => x.TagsJson).IsRequired();
    }
}
