using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace FarmApp.AdminComponents.ComponentsCommon.Sidebar;

public partial class SidebarComponent : IDisposable
{
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required IPanelService PanelService { get; set; }

    private string _currentUrl = string.Empty;
    private bool _isTestApp = false;
    private bool _isWeeklyGroupOpen = false;

    private bool IsHomePage => _currentUrl == string.Empty;
    private bool IsPushNotificationPage => _currentUrl == "push-notification";
    private bool IsWarningSettings => _currentUrl.StartsWith("warning-settings");

    protected override async Task OnInitializedAsync()
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += LocationChanged;
        PanelService.OnUpdate += OnPanelUpdated;

        StateHasChanged();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= LocationChanged;
        PanelService.OnUpdate -= OnPanelUpdated;
    }

    private void NavigateToPage(string url)
    {
        NavigationManager.NavigateTo(url);
    }

    private string GetActive(string href)
    {
        return IsActive(href) ? "active-tab" : string.Empty;
    }

    private bool IsActive(string href)
    {
        if (string.IsNullOrEmpty(href))
        {
            return false; 
        }
        
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLower();
        return relativePath == href.Remove(0, 1).ToLower();
    }

    private void OnLogoClicked()
    {
        PanelService.Show(!PanelService.IsShown);
    }

    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _currentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        StateHasChanged();
    }
    
    private void OnPanelUpdated()
    {
        if (!PanelService.IsShown && _isWeeklyGroupOpen)
        {
            _isWeeklyGroupOpen = false;
        }
        StateHasChanged();
    }

    private void ToggleWeeklyGroup()
    {
        _isWeeklyGroupOpen = !_isWeeklyGroupOpen;
    }
}