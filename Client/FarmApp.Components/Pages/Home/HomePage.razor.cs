using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace FarmApp.Components.Pages.Home
{
    public partial class HomePage : IDisposable
    {
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Inject] public required IStateService StateService { get; set; }
        [Inject] public required INavigationService NavigationService { get; set; }
        [Inject] public required IPropertyService PropertyService { get; set; }
        [Inject] public required IPropertyNotesAggregatorService PropertyNotesAggregatorService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Inject] public required INotificationHistoryApiService NotificationHistoryApiService { get; set; }
        [Inject] private IAppStoreService StorageService { get; set; } = default!;

        private List<PropertyNoteModel> _userNotes = [];
        private List<PropertyNoteModel> _lastNotes = [];
        private List<PropertyViewModel> _properties = [];
        private List<PropertyViewModel> _lastFields = [];

        private string _location = string.Empty;

        private bool _hasNotifications = false;
        private bool _isLoading;
        private bool _isRefreshing;
        private DotNetObjectReference<HomePage>? _dotNetRef;

        private List<PropertyViewModel> _loaderFields = [new(), new(), new()];

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await JSRuntime.InvokeVoidAsync("attachPullListener", _dotNetRef);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            //StorageService.SetItem("has_finished_onboarding", false);
            //StorageService.SetItem<List<TourGroup>>("completed_tour_groups", []);

            bool completedOnboarding = StorageService.GetItem<bool>("has_finished_onboarding");

            if (!completedOnboarding)
            {
                NavigationService.NavigateTo(Constants.ClientRoutes.OnboardingPage);
                return;
            }

            _isLoading = true;
            StateHasChanged();
            var notifications = await NotificationHistoryApiService.GetMyNotificationsAsync();
            _hasNotifications = notifications.Any();

            StateService.OnPropertiesReady += OnStatePropertiesReady;

            await LoadDataAsync();

            if (StateService.ArePropertiesReady)
                StateService.OnPropertiesReady -= OnStatePropertiesReady;

            _isLoading = false;
            StateHasChanged();
        }

        private void OnStatePropertiesReady()
        {
            StateService.OnPropertiesReady -= OnStatePropertiesReady;
            _ = InvokeAsync(async () =>
            {
                await LoadDataAsync();
                StateHasChanged();
            });
        }

        private async Task LoadDataAsync()
        {
            // LoadAllAsync needs ArePropertiesReady (and PropertyPreviewNotes from previews). If we call it
            // before the map pipeline finishes GetPropertiesAsync/AddPropertiesAsync, it returns [] and the
            // list is never refreshed unless we load notes *after* properties are in state.
            if (StateService.PropertiesTask is not null)
                await StateService.PropertiesTask;

            if (!StateService.ArePropertiesReady)
                return;

            _userNotes = await PropertyNotesAggregatorService.LoadAllAsync();
            _lastNotes = _userNotes.OrderByDescending(x => x.CreatedAt).Take(10).ToList();

            _properties = StateService.Properties.ToList();
            _lastFields = _properties.OrderBy(x => x.Id).Take(4).ToList();
            StateHasChanged();
        }

        [JSInvokable]
        public Task OnPullProgress(double distance)
        {
            if (_isRefreshing)
                return Task.CompletedTask;
            return InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public async Task OnPullTriggered()
        {
            if (_isRefreshing) return;

            _isRefreshing = true;
            StateHasChanged();

            await LoadDataAsync();
            await Task.Delay(800);

            _isRefreshing = false;
            StateHasChanged();
        }

        private void NavigateToNotificaitons() =>
            NavigationService.NavigateTo(Constants.ClientRoutes.AllNotificationsPage);

        public void Dispose()
        {
            StateService.OnPropertiesReady -= OnStatePropertiesReady;
            _ = JSRuntime.InvokeVoidAsync("removePullListener");
            _dotNetRef?.Dispose();
        }

        private void OnFieldCreateClick()
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.MainPage, new Dictionary<string, object>
            {
                { "ShowCreateFieldHint", true }
            });
        }
    }
}