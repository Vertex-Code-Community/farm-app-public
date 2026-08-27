using FarmApp.ViewModels.Notifications;

namespace FarmApp.Services.Services.Interfaces;

public interface INotificationHistoryApiService
{
    Task<IReadOnlyList<UserNotificationViewModel>> GetMyNotificationsAsync(CancellationToken cancellationToken = default);
}
