using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Users;

namespace FarmApp.Mobile.Services;

public class PushNotificationsRegistrationService
{
    private readonly IDeviceInstallationService _deviceInstallationService;
    private readonly IPushNotificationsHttpService _pushNotificationsHttpService;

    public PushNotificationsRegistrationService(IPushNotificationsHttpService pushNotificationsHttpService,
        IDeviceInstallationService deviceInstallationService)
    {
        _deviceInstallationService = deviceInstallationService;
        _pushNotificationsHttpService = pushNotificationsHttpService;
    }

    public Task DeregisterDeviceAsync()
    {
        return Task.CompletedTask;
    }

    public async Task RegisterDeviceAsync()
    {
        var tags = await GetLatestTagsAsync();
        var deviceInstallation = await _deviceInstallationService.GetDeviceInstallationAsync(tags);
        if (deviceInstallation is null) return;

        await _pushNotificationsHttpService
            .PutAsync<DeviceInstallation, DeviceInstallation>(
                $"api/Notifications/installations", deviceInstallation)
            .ConfigureAwait(false);
    }

    private async Task<List<string>> GetLatestTagsAsync()
    {
        var isActualVersion = false;

        var tags = new List<string>
        {
            $"version:{(isActualVersion ? "latest" : "old")}",
        };

        try
        {
            var user = await _pushNotificationsHttpService
                .GetAsync<UserModel>("api/user/get-current-user", handleAuthError: false)
                .ConfigureAwait(false);

            if (user is not null && !string.IsNullOrWhiteSpace(user.Id))
            {
                tags.Add($"user_shopper_id:{user.Id}");
                if (user.NotificationsDisabled)
                    tags.Add(PushDeviceTags.NotificationDisable);
                tags.Add(PushDeviceTags.SystemNotifications(user.SystemNotificationsEnabled));
                    tags.Add(PushDeviceTags.WeatherAlerts(user.WeatherAlertsEnabled));
                tags.Add(PushDeviceTags.ActivityReminders(user.ActivityRemindersEnabled));
                tags.Add(PushDeviceTags.InAppOnly(user.InAppNotificationsOnly));
            }
        }
        catch
        {
        }

        return tags;
    }

}