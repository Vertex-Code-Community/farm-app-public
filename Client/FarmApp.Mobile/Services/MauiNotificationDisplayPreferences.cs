using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using Microsoft.Maui.Storage;

namespace FarmApp.Mobile.Services;

public sealed class MauiNotificationDisplayPreferences : INotificationDisplayPreferences
{
    public bool IsSoundEnabled() =>
        Preferences.Default.Get(NotificationPreferenceKeys.SoundEnabled, true);

    public bool IsVibrationEnabled() =>
        Preferences.Default.Get(NotificationPreferenceKeys.VibrationEnabled, false);

    public void SetSoundEnabled(bool value) =>
        Preferences.Default.Set(NotificationPreferenceKeys.SoundEnabled, value);

    public void SetVibrationEnabled(bool value) =>
        Preferences.Default.Set(NotificationPreferenceKeys.VibrationEnabled, value);
}
