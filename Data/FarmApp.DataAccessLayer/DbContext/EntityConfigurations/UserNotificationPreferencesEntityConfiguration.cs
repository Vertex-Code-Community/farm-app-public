using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

public class UserNotificationPreferencesEntityConfiguration
    : IEntityTypeConfiguration<UserNotificationPreferencesEntity>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreferencesEntity> builder)
    {
        builder.HasKey(x => x.UserId);
        builder
            .HasOne(p => p.User)
            .WithOne(u => u.NotificationPreferences)
            .HasForeignKey<UserNotificationPreferencesEntity>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
