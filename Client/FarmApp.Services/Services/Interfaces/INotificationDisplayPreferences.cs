namespace FarmApp.Services.Services.Interfaces;

public interface INotificationDisplayPreferences
{
    bool IsSoundEnabled();
    bool IsVibrationEnabled();

    void SetSoundEnabled(bool value);
    void SetVibrationEnabled(bool value);
}
