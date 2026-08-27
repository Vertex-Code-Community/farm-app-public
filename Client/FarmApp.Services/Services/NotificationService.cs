using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Notifications;

namespace FarmApp.Services.Services;

public class NotificationService : INotificationService
{
    public event Action? OnUpdate;
    public IReadOnlyList<NotificationModel> Notifications => _notificationModels;
    public NotificationModel? LastNotification => _notificationModels.LastOrDefault();
    
    private readonly List<NotificationModel> _notificationModels = new List<NotificationModel>();

    public void Add(string title, string message)
    {
        var lastNotification = _notificationModels.LastOrDefault();
        if (lastNotification?.Title == title && lastNotification?.Content == message) return;
        
        _notificationModels.Add(new NotificationModel
        {
            Title = title,
            Content = message
        });

        OnUpdate?.Invoke();
    }

    public void Close(NotificationModel notification)
    {
        _notificationModels.Remove(notification);
        OnUpdate?.Invoke();
    }
}