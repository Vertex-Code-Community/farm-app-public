using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Constants.Snackbar;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Enums.PushNotification;
using FarmApp.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.PushNotification;

public partial class PushNotificationPage
{
    [Inject] public required INotificationHistoryApiService NotificationHistoryService { get; set; }
    [Inject] public required INotificationApiService NotificationService { get; set; }
    [Inject] public required IHeaderControlsService HeaderControlsService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required ISnackbarService SnackbarService { get; set; }
    
    private string _notificationMessage = string.Empty;
    private MessageSeverity _notificationMessageStatus = MessageSeverity.Success;

    private NotificationModel _notificationModel = new();
    private NotificationModel? _editingNotification;
    private readonly List<NotificationModel> _notifications = new();
    private readonly List<PushNotificationTab> _tabs = new() { PushNotificationTab.Push, PushNotificationTab.History };
    private PushNotificationTab _selectedTab = PushNotificationTab.Push;

    private bool _isPending;
    protected override async Task OnInitializedAsync()
    {
        HeaderControlsService.ShowDate = false;

        await OnReloadAsync();
    }


    private async Task SendNotificationAsync(NotificationModel notification)
    {
        if (notification.DateTimeOfSend <= DateTime.UtcNow && notification.TypeOfSend is not TypeOfSend.Now)
        {
            SnackbarService.Show("To schedule a notification, please choose a date in the future", SnackbarColors.Warning);
            return;
        }

        using (var loader = GlobalLoaderService.SwitchOnContent())
        {
            var result = await NotificationService.RequestSendNotificationAsync(notification);
        }

        var message = _notificationModel.TypeOfSend == TypeOfSend.Now
            ? "Push notification sent"
            : "Push notification scheduled";
            
        SnackbarService.Show(message, SnackbarColors.Success);


        Clear();
        await OnReloadAsync();
        StateHasChanged();
    }

    private async Task EditNotificationAsync()
    {
        if (_editingNotification is null) return;

        using (var loader = GlobalLoaderService.SwitchOnContent())
        {
            await NotificationService.UpdateAsync(_editingNotification);
        }

        StateHasChanged();
        _editingNotification = null;
        await OnReloadAsync();
    }

    private async Task CancelNotificationAsync()
    {
        if (_editingNotification is null) return;

        using (var loader = GlobalLoaderService.SwitchOnContent())
        {
            await NotificationService.CancelAsync(_editingNotification.Id);
        }

        StateHasChanged();
        _editingNotification = null;
        await OnReloadAsync();
    }

    private async Task OnReloadAsync()
    {
        _isPending = true;
        StateHasChanged();
        
        using (var loader = GlobalLoaderService.SwitchOnContent())
        {
            var notifications = await NotificationHistoryService.GetAllAsync();

            foreach (var notification in notifications)
                notification.DateTimeOfSend += ClientDateTimeHelper.TimeZoneOffset;

            notifications.Reverse();

            _notifications.Clear();
            _notifications.AddRange(notifications);
        }
        
        _isPending = false;
        StateHasChanged();
    }

    private void Clear()
    {
        _notificationModel = new();
        
        StateHasChanged();
    }
}
