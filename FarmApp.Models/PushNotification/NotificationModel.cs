using FarmApp.Shared.Enums.PushNotification;

namespace FarmApp.Models.PushNotification;

public class NotificationModel
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Sender { get; set; }
    public string TypeUrlForRedirection { get; set; }
    public string? UrlForRedirection { get; set; }
    public string TypeOfSend { get; set; }
    public DateTime DateTimeOfSend { get; set; }
    public string Platform { get; set; }
    public string TypeOfTargetUser { get; set; }
    public string Status { get; set; }
    public NotificationKind NotificationKind { get; set; } = NotificationKind.General;
    public Dictionary<NotificationTagsType, HashSet<string>> Tags { get; set; } = new();
}

public enum NotificationTagsType
{
    User,
    VersionOwner,
    OperationSystem
}