namespace FarmApp.Shared.Constants;

public static class Platform
{
    public const string IOS = "IOS";
    public const string Android = "Android";
    public const string All = "All";
}

public static class AuthenticationType
{
    public const string GiveX = "GiveX";
    public const string AppCart = "AppCart";
    public const string NoLoyalty = "NoLoyalty";
}

public static class TypeOfSend
{
    public const string Now = "Now";
    public const string SpecificDate = "SpecificDate";
}
public static class VersionApplication
{
    public const string Latest = "latest";
    public const string Old = "old";
}

public static class TypeOfRedirection
{
    public const string None = "None";
    public const string WithUrl = "Url";
}

public static class RecipientType
{
    public const string All = "All";
    public const string Target = "Target";
}

public static class TargetUserType
{
    public const string All = "All";
    public const string OldVersionOwner = "OldVersionOwner";
    public const string SpecificUser = "SpecificUser";
    public const string SpecificStoreLocation = "SpecificStoreLocation";
}

public static class NotificationStatus
{
    public const string Sent = "Sent";
    public const string Scheduled = "Scheduled";
    public const string Canceled = "Canceled";
}

/// <summary>Well-known tag strings stored on device registrations for push targeting.</summary>
public static class PushDeviceTags
{
    public const string NotificationDisable = "notification_disable:true";

    public static string SystemNotifications(bool enabled) => Tag("system_notifications", enabled);
    public static string WeatherAlerts(bool enabled) => Tag("weather_alerts", enabled);
    public static string ActivityReminders(bool enabled) => Tag("activity_reminders", enabled);
    public static string InAppOnly(bool enabled) => Tag("in_app_only", enabled);

    private static string Tag(string name, bool enabled) => $"{name}:{(enabled ? "true" : "false")}";
}