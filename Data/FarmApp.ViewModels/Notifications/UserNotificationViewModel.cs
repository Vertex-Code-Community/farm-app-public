using FarmApp.Shared.Enums.PushNotification;

namespace FarmApp.ViewModels.Notifications;

public class UserNotificationViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }

    public string? IconSrc { get; set; }
    public string? IconBackground { get; set; }
    public string? IconColor { get; set; }

    public NotificationKind NotificationKind { get; set; } = NotificationKind.General;
}

