using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Notifications;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.AllNotifications
{
    public partial class AllNotificationsPage
    {
        [Inject] public required INavigationService NavigationService { get; set; }
        [Inject] public required INotificationHistoryApiService NotificationHistoryApiService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

        private readonly List<UserNotificationViewModel> _mockNotifications = new()
        {
            new UserNotificationViewModel {Id="-1", 
                Title="TestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTest",
            Content="TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest" +
                "TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest"},

            new UserNotificationViewModel {Id="0", Title="Strong wind alert",
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="1", Title="Frost warning",
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="2", Title="Rain expected today", IsRead=true, CreatedAt=DateTime.Now.AddDays(-1),
            Content = "Rain is expected today and may affect your field activities. " +
            "Consider adjusting your schedule, protecting crops where necessary, " +
            "and planning your work accordingly to avoid delays or damage." },

            new UserNotificationViewModel {Id="3",Title="High humidity", IsRead=true, CreatedAt=DateTime.Now.AddDays(-1),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="4",Title="Heat stress alert", IsRead=true, CreatedAt = new DateTime(DateTime.Now.Year, 2, 22),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="5",Title="Extreme low temperature", CreatedAt = new DateTime(DateTime.Now.Year, 2, 22),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="6",Title="Perfect weather for field work today",
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="7",Title="Drought warning",
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id="8",Title="Update available", IsRead=true, CreatedAt=DateTime.Now.AddDays(-1),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "9", Title="Other", IsRead=true, CreatedAt=DateTime.Now.AddDays(-1),
            Content = "Rain is expected today and may affect your field activities. " +
            "Consider adjusting your schedule, protecting crops where necessary, " +
            "and planning your work accordingly to avoid delays or damage." },

            new UserNotificationViewModel {Id = "10", Title="Temporary service interruption", IsRead=true, CreatedAt = new DateTime(DateTime.Now.Year, 2, 22),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "11", Title="Scheduled maintenance", CreatedAt = new DateTime(DateTime.Now.Year, 2, 22),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "12", Title="Issue resolved", CreatedAt = DateTime.Now.AddYears(-1),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "13", Title="Status changed", CreatedAt = DateTime.Now.AddYears(-2),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "14", Title="Event starts in 5 minutes", CreatedAt = DateTime.Now.AddYears(-2),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "15", Title="3 events planned for today", CreatedAt = DateTime.Now.AddYears(-2),
            Content="Update available Update available Update availableUpdate available"},

            new UserNotificationViewModel {Id = "16", Title="No activity on this field for X days", CreatedAt = DateTime.Now.AddYears(-2),
            Content="Update available Update available Update availableUpdate available"},
        }; 

        private List<UserNotificationViewModel> _apiNotifications = new();

        private List<UserNotificationViewModel> _filteredNotifications = new();

        private NotificationFilter _currentFilter = NotificationFilter.All;
        public NotificationFilter CurrentFilter
        {
            get => _currentFilter;
            set
            {
                if (_currentFilter != value)
                {
                    _currentFilter = value;
                    ApplyFilter();
                }
            }
        }

        private int UnreadCount => _apiNotifications.Count(n => !n.IsRead);

        protected override void OnInitialized()
        {
            ApplyFilter();
        }

        protected override async Task OnInitializedAsync()
        {
            var fromApi = await NotificationHistoryApiService.GetMyNotificationsAsync();
            _apiNotifications = fromApi.OrderByDescending(n => n.CreatedAt).ToList();
            ApplyFilter();
            await InvokeAsync(StateHasChanged);
        }

        private void ApplyFilter()
        {
            _filteredNotifications = _currentFilter switch
            {
                NotificationFilter.Mock => _mockNotifications.OrderByDescending(n => n.CreatedAt).ToList(),
                NotificationFilter.Read => _apiNotifications.Where(n => n.IsRead).ToList(),
                NotificationFilter.Unread => _apiNotifications.Where(n => !n.IsRead).ToList(),
                NotificationFilter.All => _apiNotifications.ToList(),
                _ => _apiNotifications.ToList()
            };
        }

        private string GetNotificationDateString(DateTime date)
        {
            DateTime today = DateTime.Today;

            if (date.Date == today)
            {
                return "Today";
            }
            else if (date.Date == today.AddDays(-1))
            {
                return "Yesterday";
            }
            else if (date.Year == today.Year)
            {
                return date.ToString("dd MMM");
            }
            else
            {
                return date.ToString("dd MMM yyyy");
            }
        }

        private void NavigateToSettings()
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.NotificationsPage);
        }
    }
}

public enum NotificationFilter
{
    All,
    Read,
    Unread,
    Mock
}
