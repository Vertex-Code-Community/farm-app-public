using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;

namespace FarmApp.WebClient.Services;

public sealed class WebNotificationDisplayPreferences : INotificationDisplayPreferences
{
    private readonly IAppStoreService _store;

    public WebNotificationDisplayPreferences(IAppStoreService store) => _store = store;

    public bool IsSoundEnabled() =>
        _store.GetItem<object>(NotificationPreferenceKeys.SoundEnabled) is bool b ? b : true;

    public bool IsVibrationEnabled() =>
        _store.GetItem<object>(NotificationPreferenceKeys.VibrationEnabled) is bool b ? b : false;

    public void SetSoundEnabled(bool value) =>
        _store.SetItem(NotificationPreferenceKeys.SoundEnabled, value);

    public void SetVibrationEnabled(bool value) =>
        _store.SetItem(NotificationPreferenceKeys.VibrationEnabled, value);
}
