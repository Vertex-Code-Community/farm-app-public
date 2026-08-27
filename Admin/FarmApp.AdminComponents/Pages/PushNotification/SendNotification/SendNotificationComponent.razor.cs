using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models.PushNotification;
using FarmApp.Shared.Enums.PushNotification;
using FarmApp.Models.User;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.PushNotification.SendNotification;

public partial class SendNotificationComponent : IDisposable
{
    [Inject] public required IUsersApiService UserApiService { get; set; }
    [Parameter] public required NotificationModel Notification { get; set; }
    [Parameter] public required EventCallback<NotificationModel> OnSendNotification { get; set; }
    
    private List<string> _operationSystemList = new()
        { Platform.All, Platform.Android, Platform.IOS };

    private List<string> _authenticationTypeList = new()
        { AuthenticationType.GiveX, AuthenticationType.AppCart, AuthenticationType.NoLoyalty };
    
    private List<string> _versionTypeList = new()
        { VersionApplication.Latest, VersionApplication.Old };
    
    private UserSearchModel? _selectedUser;
    private List<UserSearchModel> _userSearchResults = new();
    private CancellationTokenSource? _searchCancellationTokenSource;
    private Timer? _debounceTimer;

    private bool _isTagDropdownIsOpen;
    private bool _isUserTagSelected;
    private bool _isVersionApplication;
    private bool _isOperationSystemTagSelected;

    private DateTime _dateTimeOfSendInUtc = DateTime.UtcNow;
    private NotificationTagsType _selectedType = NotificationTagsType.User;
    private string _typeOfUser = RecipientType.All;
    private string _selectedAuthenticateType;
    private string _selectedVersionOwner;
    private string _userSearchTerm = string.Empty;
    

    private DateTime DateTimeOfSendInUsersGmt
    {
        get => _dateTimeOfSendInUtc + ClientDateTimeHelper.TimeZoneOffset; // GMT to Local
        set
        {
            _dateTimeOfSendInUtc = value - ClientDateTimeHelper.TimeZoneOffset; // Local to GMT
            Notification.DateTimeOfSend = _dateTimeOfSendInUtc;
        }
    }

    private async Task SendNotificationAsync()
    {
        Notification.DateTimeOfSend = Notification.TypeOfSend == TypeOfSend.Now ? DateTime.UtcNow : _dateTimeOfSendInUtc;
        Notification.TypeOfTargetUser = _typeOfUser == RecipientType.All ? TargetUserType.All : Notification.TypeOfTargetUser;
        Notification.Status = NotificationStatus.Scheduled;

        await OnSendNotification.InvokeAsync(Notification);
    }

    private async Task OnUserSearchChanged(string searchTerm)
    {
        _userSearchTerm = searchTerm;

        if (_searchCancellationTokenSource != null)
        {
            Console.WriteLine("[Search] Cancelling previous search");
            _searchCancellationTokenSource.Cancel();
        }

        _debounceTimer?.Dispose();

        if (string.IsNullOrWhiteSpace(_userSearchTerm))
        {
            Console.WriteLine("[Search] Empty search term, clearing results");
            _userSearchResults.Clear();
            StateHasChanged();
            return;
        }

        _searchCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _searchCancellationTokenSource.Token;

        _debounceTimer = new Timer(async _ =>
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    // await InvokeAsync(async () =>
                    // {
                    //     var results = await UserApiService.SearchUsersAsync(_userSearchTerm, 20, cancellationToken);
                    //     if (!cancellationToken.IsCancellationRequested)
                    //     {
                    //         _userSearchResults = results;
                    //         StateHasChanged();
                    //     }
                    // });
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await InvokeAsync(() =>
                    {
                        _userSearchResults.Clear();
                        StateHasChanged();
                    });
                }
            }
        }, null, TimeSpan.FromMilliseconds(300), Timeout.InfiniteTimeSpan);
    }

    private void OpenTheTagsDropdown()
    {
        _isTagDropdownIsOpen = true;
        CloseAllTagDropdowns();
        StateHasChanged();
    }


    private void OnSelectTheTagOption()
    {
        _isTagDropdownIsOpen = false;
        CloseAllTagDropdowns();

        switch (_selectedType)
        {
            case NotificationTagsType.VersionOwner:
                _isVersionApplication = true;
                break;
            case NotificationTagsType.User:
                _isUserTagSelected = true;
                break;

            case NotificationTagsType.OperationSystem:
                _isOperationSystemTagSelected = true;
                break;
        }

        StateHasChanged();
    }

    private void AddTag(NotificationTagsType type, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (Notification.Tags.TryGetValue(type, out var set))
        {
            set.Add(value);
        }
        else
        {
            Notification.Tags[type] = new HashSet<string> { value };
        }

        CloseAllTagDropdowns();
    }

    private void RemoveTag(NotificationTagsType type, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        if (!Notification.Tags.TryGetValue(type, out var set))
            return;

        set.Remove(value);

        if (set.Count == 0)
            Notification.Tags.Remove(type);

        StateHasChanged();
    }

    private void AddUser()
    {
        if (_selectedUser is null)
            return;

        if (!string.IsNullOrWhiteSpace(_selectedUser.Id))
            AddTag(NotificationTagsType.User, _selectedUser.Id);
        else if (!string.IsNullOrWhiteSpace(_selectedUser.Email))
            AddTag(NotificationTagsType.User, _selectedUser.Email);
    }
    private void AddVersionApplication() => AddTag(NotificationTagsType.VersionOwner, _selectedVersionOwner);
    private void AddOperationSystem() => AddTag(NotificationTagsType.OperationSystem, Notification.Platform);


    private void CloseAllTagDropdowns()
    {
        _isUserTagSelected = false;
        _isOperationSystemTagSelected = false;
        _isVersionApplication = false;
    }

    public void Dispose()
    {
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource?.Dispose();
        _debounceTimer?.Dispose();
    }
}