namespace FarmApp.ViewModels.Users;

public class UpdateNotificationPreferencesModel
{
    public bool NotificationsDisabled { get; set; }

    /// <summary>When null, existing server value is kept (older clients only send <see cref="NotificationsDisabled"/>).</summary>
    public bool? SystemNotificationsEnabled { get; set; }
    public bool? WeatherAlertsEnabled { get; set; }
    public bool? ActivityRemindersEnabled { get; set; }
    public bool? InAppNotificationsOnly { get; set; }
}
