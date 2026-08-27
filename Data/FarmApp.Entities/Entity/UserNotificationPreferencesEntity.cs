using System.ComponentModel.DataAnnotations.Schema;

namespace FarmApp.Entities.Entity;

[Table("user_notification_preferences")]
public class UserNotificationPreferencesEntity
{
    public string UserId { get; set; } = null!;

    public bool NotificationsDisabled { get; set; }

    public bool SystemNotificationsEnabled { get; set; } = true;
    public bool WeatherAlertsEnabled { get; set; } = true;
    public bool ActivityRemindersEnabled { get; set; } = true;
    public bool InAppNotificationsOnly { get; set; }

    public UserEntity User { get; set; } = null!;
}
