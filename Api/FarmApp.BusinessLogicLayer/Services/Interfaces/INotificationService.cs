using FarmApp.Models.PushNotification;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface INotificationService
{
    Task ResumeDelayedNotification();
    Task<List<NotificationDeliveryOutcome[]>?> RequestSendNotification(NotificationModel notificationModel,
        CancellationToken cancellationToken = default);
    Task CancelAsync(long notificationId);
    Task UpdateAsync(NotificationModel notification);
}
