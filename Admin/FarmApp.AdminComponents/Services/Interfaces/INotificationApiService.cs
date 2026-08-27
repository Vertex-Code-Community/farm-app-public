using FarmApp.Models.PushNotification;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface INotificationApiService
{
    Task ResumeDelayedNotificationAsync();
    Task<List<NotificationDeliveryOutcome[]>> RequestSendNotificationAsync(NotificationModel notificationModel);
    Task CancelAsync(long notificationId);
    Task UpdateAsync(NotificationModel notification);
}
