using FarmApp.Services;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.Notifications
{
    public partial class NotificationsPage
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Inject] public required INotificationDisplayPreferences NotificationPreferences { get; set; }
        [Inject] public required IUserService UserService { get; set; }

        private bool _sound = true;
        private bool _vibration;
        private readonly Dictionary<NotificationPermissionCategory, bool> _enabledByCategory = CreateDefaultPermissionState();

        private static Dictionary<NotificationPermissionCategory, bool> CreateDefaultPermissionState()
        {
            var dict = new Dictionary<NotificationPermissionCategory, bool>();
            foreach (var category in Enum.GetValues<NotificationPermissionCategory>())
            {
                dict[category] = category is not NotificationPermissionCategory.DisableAllNotifications;
            }

            return dict;
        }

        protected override void OnInitialized()
        {
            _sound = NotificationPreferences.IsSoundEnabled();
            _vibration = NotificationPreferences.IsVibrationEnabled();
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var user = await UserService.GetCurrentUserAsync();
            if (user is not null)
            {
                _enabledByCategory[NotificationPermissionCategory.DisableAllNotifications] = user.NotificationsDisabled;
                _enabledByCategory[NotificationPermissionCategory.SystemNotifications] = user.SystemNotificationsEnabled;
                _enabledByCategory[NotificationPermissionCategory.WeatherAlerts] = user.WeatherAlertsEnabled;
                _enabledByCategory[NotificationPermissionCategory.ActivityAndReminders] = user.ActivityRemindersEnabled;
                _enabledByCategory[NotificationPermissionCategory.InAppNotificationsOnly] = user.InAppNotificationsOnly;
            }
        }

        private bool IsEnabled(NotificationPermissionCategory category) =>
            _enabledByCategory[category];

        private async Task OnPermissionChanged(NotificationPermissionCategory category, bool enabled)
        {
            var previous = _enabledByCategory[category];
            _enabledByCategory[category] = enabled;

            var ok = await UserService.UpdateNotificationPreferencesAsync(BuildNotificationPreferencesModel());

            if (!ok)
            {
                _enabledByCategory[category] = previous;
                await InvokeAsync(StateHasChanged);
                return;
            }

            if (NotificationRegistrationBridge.RefreshDeviceRegistrationAsync is { } refresh)
                await refresh();
            await InvokeAsync(StateHasChanged);
        }

        private UpdateNotificationPreferencesModel BuildNotificationPreferencesModel() => new()
        {
            NotificationsDisabled = _enabledByCategory[NotificationPermissionCategory.DisableAllNotifications],
            SystemNotificationsEnabled = _enabledByCategory[NotificationPermissionCategory.SystemNotifications],
            WeatherAlertsEnabled = _enabledByCategory[NotificationPermissionCategory.WeatherAlerts],
            ActivityRemindersEnabled = _enabledByCategory[NotificationPermissionCategory.ActivityAndReminders],
            InAppNotificationsOnly = _enabledByCategory[NotificationPermissionCategory.InAppNotificationsOnly]
        };

        private Task ToggleSoundRow() => SetSoundAsync(!_sound);

        private Task ToggleVibrationRow() => SetVibrationAsync(!_vibration);

        private Task OnSoundChanged(bool value) => SetSoundAsync(value);

        private Task OnVibrationChanged(bool value) => SetVibrationAsync(value);

        private Task SetSoundAsync(bool value)
        {
            if (_sound == value)
                return Task.CompletedTask;
            _sound = value;
            NotificationPreferences.SetSoundEnabled(value);
            return InvokeAsync(StateHasChanged);
        }

        private Task SetVibrationAsync(bool value)
        {
            if (_vibration == value)
                return Task.CompletedTask;
            _vibration = value;
            NotificationPreferences.SetVibrationEnabled(value);
            return InvokeAsync(StateHasChanged);
        }
    }
}
