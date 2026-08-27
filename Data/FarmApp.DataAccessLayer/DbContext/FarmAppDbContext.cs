using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FarmApp.Entities.Entity;
using FarmApp.DataAccessLayer.DbContext.EntityConfigurations;

namespace FarmApp.DataAccessLayer.DbContext;

public class FarmAppDbContext(DbContextOptions<FarmAppDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    static FarmAppDbContext()
    {
        // Allow DateTime with Kind=Local on PostgreSQL timestamptz columns. Set in static cctor
        // so it also affects EF Core tools (migrations) that don't go through Program.cs.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<SteadEntity> Steads { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<OwnershipEntity> Ownerships { get; set; }
    public DbSet<PurposeEntity> Purposes { get; set; }
    public DbSet<PropertyNoteEntity> PropertyNotes { get; set; }
    public DbSet<PropertyEntity> Properties { get; set; }
    public DbSet<PushNotificationQueueEntity> PushNotificationsQueue { get; set; }
    public DbSet<PropertyAndSteadEntity> PropertyAndSteads { get; set; }
    public DbSet<CustomSteadEntity> CustomSteads { get; set; }
    public DbSet<NotificationEntity>  Notifications { get; set; }
    public DbSet<PushDeviceRegistrationEntity> PushDeviceRegistrations { get; set; }
    public DbSet<UserNotificationPreferencesEntity> UserNotificationPreferences { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, (eventId, logLevel) => logLevel >= LogLevel.Critical);
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new SteadEntityConfiguration());
        builder.ApplyConfiguration(new CustomSteadEntityConfiguration());
        builder.ApplyConfiguration(new PurposeEntityConfiguration());
        builder.ApplyConfiguration(new OwnershipEntityConfiguration());
        builder.ApplyConfiguration(new CategoryEntityConfiguration());
        builder.ApplyConfiguration(new PropertyNoteEntityConfiguration());
        builder.ApplyConfiguration(new UserEntityConfiguration());
        builder.ApplyConfiguration(new UserNotificationPreferencesEntityConfiguration());
        builder.ApplyConfiguration(new PropertyAndSteadEntityConfiguration());
        builder.ApplyConfiguration(new PropertyEntityConfiguration());
        builder.ApplyConfiguration(new PropertyNoteMediaConfiguration());
        builder.ApplyConfiguration(new PropertyNoteStatusConfiguration());
        builder.ApplyConfiguration(new PushNotificationQueueConfiguration());
        builder.ApplyConfiguration(new PushDeviceRegistrationEntityConfiguration());
    }
}