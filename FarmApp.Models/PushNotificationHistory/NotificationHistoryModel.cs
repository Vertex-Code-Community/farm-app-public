namespace FarmApp.Models.PushNotificationHistory;

public class NotificationHistoryModel
{
    public int TotalCount { get; set; }
    public List<NotificationHistoryItemModel> Items { get; set; }
}

public class NotificationHistoryItemModel
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string Sender { get; set; }
    public DateTime DateTimeOfSend { get; set; }
}
