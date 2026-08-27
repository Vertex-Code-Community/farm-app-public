using FarmApp.Models.PushNotification;
using FarmApp.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.PushNotification.EditNotificationModal;

public partial class EditNotificationModalComponent
{
    [Parameter] public required NotificationModel SelectedNotification { get; set; }
    [Parameter] public EventCallback OnCancelClicked { get; set; }
    [Parameter] public EventCallback OnEditClicked { get; set; }
    [Parameter] public EventCallback OnCloseClicked { get; set; }

    private bool _showCancelConfirmationModal;

    private string _typeOfUser = RecipientType.All;
    private string _targetUserSelected = TargetUserType.OldVersionOwner;
    // private StoreModel? _selectedNotificationStore;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        _typeOfUser = SelectedNotification.TypeOfTargetUser is TargetUserType.All ? RecipientType.All : RecipientType.Target;
        _targetUserSelected = SelectedNotification.TypeOfTargetUser;
        // _selectedNotificationStore = LocationService.SelectedRetailer?.Stores.FirstOrDefault(x => x.StoreId == SelectedNotification.StoreId);

        StateHasChanged();
    }

    private Task SaveNotificationAsync()
    {
        SelectedNotification.TypeOfTargetUser = _typeOfUser == RecipientType.All ? TargetUserType.All : _targetUserSelected;
        // SelectedNotification.StoreId = _selectedNotificationStore is not null ? _selectedNotificationStore.StoreId : "";

        return OnEditClicked.InvokeAsync();
    }
}
