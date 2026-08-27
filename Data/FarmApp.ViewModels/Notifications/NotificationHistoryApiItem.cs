using FarmApp.Shared.Enums.PushNotification;

namespace FarmApp.ViewModels.Notifications;

public class NotificationHistoryApiItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime DateTimeOfSend { get; set; }
    public string Status { get; set; } = string.Empty;

    public NotificationKind NotificationKind { get; set; }

    public string? ImageUrl { get; set; }
}
