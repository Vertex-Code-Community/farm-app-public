using FarmApp.Models.PushNotification;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface INotificationHistoryService
{
    Task<List<NotificationModel>> GetAllAsync();
    Task<List<NotificationModel>> GetMyAsync(string? appVersionSegment, CancellationToken cancellationToken = default);
    Task<List<NotificationModel>> GetAllDelayedNotificationAsync();
    Task<long> AddToHistory(NotificationModel notificationModel);
    Task CompeteAsync(long notificationId);
    Task CancelAsync(long notificationId);
    Task UpdateAsync(NotificationModel notification);
}
