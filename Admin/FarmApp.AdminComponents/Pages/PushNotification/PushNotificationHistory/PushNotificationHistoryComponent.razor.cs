using Bch.Components.Table;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.PushNotification.PushNotificationHistory;

public partial class PushNotificationHistoryComponent
{
    
    [Parameter] public required List<NotificationModel> Notifications { get; set; } = new();
    [Parameter] public required EventCallback<NotificationModel> OnEditNotification { get; set; }

    private BchTable<NotificationModel>? _notificationsTable;
    private DateTime _dateTimeOfSendInUtc = DateTime.UtcNow;
    private int _currentPage = 1;
    
    private string? _infoModalText = null;

    private DateTime DateTimeOfSendInUsersGmt
    {
        get => _dateTimeOfSendInUtc + ClientDateTimeHelper.TimeZoneOffset; // GMT to Local
        set => _dateTimeOfSendInUtc = value - ClientDateTimeHelper.TimeZoneOffset; // Local to GMT
    }

    protected override void OnParametersSet()
    {
        _currentPage = 1;
        _notificationsTable?.ReloadTable();
    }
}
