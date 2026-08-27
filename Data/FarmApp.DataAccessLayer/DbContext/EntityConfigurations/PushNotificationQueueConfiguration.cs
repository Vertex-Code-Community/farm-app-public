using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmApp.DataAccessLayer.DbContext.EntityConfigurations
{
    public class PushNotificationQueueConfiguration : IEntityTypeConfiguration<PushNotificationQueueEntity>
    {
        public void Configure(EntityTypeBuilder<PushNotificationQueueEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.HasIndex(x => new { x.Status, x.SendAt });

            builder.HasIndex(x => x.PropertyNoteId);
        }
    }
}
