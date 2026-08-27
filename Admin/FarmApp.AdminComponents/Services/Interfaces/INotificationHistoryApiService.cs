using FarmApp.Models.PushNotification;
using FarmApp.Models.PushNotificationHistory;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface INotificationHistoryApiService
{
    Task<List<NotificationModel>> GetAllAsync();
    Task<List<NotificationModel>> GetAllDelayedNotificationAsync();
    Task<NotificationIdResponse?> AddToHistoryAsync(NotificationModel notificationModel);
    Task CompeteAsync(long notificationId);
    Task CancelAsync(long notificationId);
    Task UpdateAsync(NotificationModel notification);
}
