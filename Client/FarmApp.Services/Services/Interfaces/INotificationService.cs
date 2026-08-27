using FarmApp.ViewModels.Notifications;

namespace FarmApp.Services.Services.Interfaces;

public interface INotificationService
{
    event Action? OnUpdate;
    IReadOnlyList<NotificationModel> Notifications { get; }
    NotificationModel? LastNotification { get; }

    void Add(string title, string message);
    // void Add(string uri, CustomException customException);
    void Close(NotificationModel notification);
}