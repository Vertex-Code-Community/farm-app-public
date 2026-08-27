namespace FarmApp.Services;

public static class NotificationRegistrationBridge
{
    public static Func<Task>? RefreshDeviceRegistrationAsync { get; set; }
}
