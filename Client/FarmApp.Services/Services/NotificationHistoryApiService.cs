using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Notifications;

namespace FarmApp.Services.Services;

public class NotificationHistoryApiService : INotificationHistoryApiService
{
    private readonly IHttpService _httpService;

    public NotificationHistoryApiService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<IReadOnlyList<UserNotificationViewModel>> GetMyNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await _httpService.GetAsync<List<NotificationHistoryApiItem>>(
            "api/notification-history/my",
            showError: false,
            cancellationToken: cancellationToken);

        if (items is null || items.Count == 0)
            return Array.Empty<UserNotificationViewModel>();

        return items.Select(Map).ToList();
    }

    private static UserNotificationViewModel Map(NotificationHistoryApiItem x)
    {
        var vm = new UserNotificationViewModel
        {
            Id = $"push-{x.Id}",
            Title = string.IsNullOrWhiteSpace(x.Title) ? "Notification" : x.Title,
            Content = x.Message ?? string.Empty,
            CreatedAt = x.DateTimeOfSend,
            IsRead = false,
            NotificationKind = x.NotificationKind
        };

        if (!string.IsNullOrWhiteSpace(x.ImageUrl))
            vm.IconSrc = x.ImageUrl.Trim();

        return vm;
    }
}
