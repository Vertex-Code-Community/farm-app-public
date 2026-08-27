using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;

namespace FarmApp.AdminComponents.Services;

public class NotificationApiService(IHttpService httpService) : INotificationApiService
{
    private const string Endpoint = "api/notifications";

    public Task ResumeDelayedNotificationAsync()
    {
        return httpService.PostAsync<object, object>(
            $"{Endpoint}/resume-delayed",
            null,
            ApiType.ManagementAppApi);
    }

    public async Task<List<NotificationDeliveryOutcome[]>> RequestSendNotificationAsync(NotificationModel notificationModel)
    {
        var list = await httpService.PostAsync<List<NotificationDeliveryOutcome[]>, NotificationModel>(
            $"{Endpoint}/send",
            notificationModel,
            ApiType.ManagementAppApi);

        return list ?? new();
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
