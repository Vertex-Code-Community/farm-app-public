using FarmApp.Mobile.Services.Interfaces;

namespace FarmApp.Mobile.Services;

public class PushNotificationsMessageService : IPushNotificationsMessageService
{
    public event Func<string, string, Task>? OnNotificationCame;

    public void Invoke(string title, string messgae)
    {
        OnNotificationCame?.Invoke(title, messgae);
    }
}