using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models.PushNotification;
using FarmApp.Models.PushNotificationHistory;
using FarmApp.Shared.Constants;

namespace FarmApp.AdminComponents.Services;

public class NotificationHistoryApiService(IHttpService httpService) : INotificationHistoryApiService
{
    private const string Endpoint = "api/notification-history";

    public async Task<List<NotificationModel>> GetAllAsync()
    {
        var list = await httpService.GetAsync<List<NotificationModel>>(
            $"{Endpoint}",
            ApiType.ManagementAppApi);

        return list ?? new();
    }

    public async Task<List<NotificationModel>> GetAllDelayedNotificationAsync()
    {
        var list = await httpService.GetAsync<List<NotificationModel>>(
            $"{Endpoint}/delayed",
            ApiType.ManagementAppApi);

        return list ?? new();
    }

    public Task<NotificationIdResponse?> AddToHistoryAsync(NotificationModel notificationModel)
    {
        return httpService.PostAsync<NotificationIdResponse, NotificationModel>(
            $"{Endpoint}",
            notificationModel,
            ApiType.ManagementAppApi);
    }

    public Task CompeteAsync(long notificationId)
    {
        return httpService.PutAsync<object, object>(
            $"{Endpoint}/{notificationId}/complete",
            null,
            ApiType.ManagementAppApi);
    }

    public Task CancelAsync(long notificationId)
    {
        return httpService.PutAsync<object, object>(
            $"{Endpoint}/{notificationId}/cancel",
            null,
            ApiType.ManagementAppApi);
    }

    public Task UpdateAsync(NotificationModel notification)
    {
        return httpService.PutAsync<object, NotificationModel>(
            $"{Endpoint}",
            notification,
            ApiType.ManagementAppApi);
    }
}
