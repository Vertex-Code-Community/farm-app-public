namespace FarmApp.Shared.Enums;

public enum NotificationPermissionCategory
{
    SystemNotifications = 0,
    WeatherAlerts = 1,
    ActivityAndReminders = 2,
    DisableAllNotifications = 3,
    InAppNotificationsOnly = 4,
}

public static class NotificationPermissionMetadata
{
    public static IReadOnlyList<NotificationPermissionItem> Items { get; } =
    [
        new(
            NotificationPermissionCategory.SystemNotifications,
            "Notifications_System",
            "Notifications_System_Desc"),
        new(
            NotificationPermissionCategory.WeatherAlerts,
            "Notifications_Weather",
            "Notifications_Weather_Desc"),
        new(
            NotificationPermissionCategory.ActivityAndReminders,
            "Notifications_Activity",
            "Notifications_Activity_Desc"),
        new(
            NotificationPermissionCategory.DisableAllNotifications,
            "Notifications_Turn-Off",
            "Notifications_Turn-Off_Desc"),
        new(
            NotificationPermissionCategory.InAppNotificationsOnly,
            "Notifications_App-Only",
            "Notifications_App-Only_Desc"),
    ];
}

public readonly record struct NotificationPermissionItem(
    NotificationPermissionCategory Category,
    string Title,
    string Description);
