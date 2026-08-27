using System.Globalization;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Shared.Math;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.HeaderComponent;

public partial class HeaderComponent : IDisposable
{
    [Inject] public required IPanelService PanelService { get; set; }
    [Inject] public required NavigationManager NavigationManager { get; set; }
    [Inject] public required IThemeInterface ThemeService { get; set; }
    [Inject] public required IHeaderControlsService HeaderControlsService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
 
    private bool _showProfileOptions = false;
    private readonly Vec2 _profileOptionsPos = new();
    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };

    private readonly string _profileOptionsId = $"_id_{Guid.NewGuid()}";
    private readonly string _subscriptionKey = $"_key_{Guid.NewGuid()}";

    private readonly string [] _themeOptions = ["system", "light", "dark"];
    private string? _selectedTheme;

    private bool _showSettings = false;
    
    private bool _isLoadingRetailers = false;

    protected override void OnInitialized()
    {
        PanelService.OnUpdate += StateHasChanged;
        ThemeService.OnThemeChanged += OnThemeChanged;
        _selectedTheme = ThemeService.CurrentTheme;
        HeaderControlsService.OnChanged += StateHasChanged;

        GlobalLoaderService.OnLoaderSwitch += StateHasChanged;
    }

    private async Task OnLightThemeClick() => await ThemeService.SetThemeAsync("light");
    private async Task OnDarkThemeClick() => await ThemeService.SetThemeAsync("dark");
    
    public void Dispose()
    {
        PanelService.OnUpdate -= StateHasChanged;
        ThemeService.OnThemeChanged -= OnThemeChanged;
        HeaderControlsService.OnChanged -= StateHasChanged;
        
        GlobalLoaderService.OnLoaderSwitch -= StateHasChanged;
    }

    private void OnResize()
    {
        if (!_showProfileOptions) return;

        _showProfileOptions = false;
        StateHasChanged();

        return;
    }

    // [JSInvokable]
    // public void OnDocumentMouseDown(ExtMouseEventArgs e)
    // {
    //     var coordsHolder = e.PathCoordinates.FirstOrDefault(x => x.Id == _profileOptionsId);
    //     if (coordsHolder != null) return;
    //
    //     var showState = _showProfileOptions;
    //     _showProfileOptions = false;
    //
    //     if (showState) StateHasChanged();
    // }

    private void OnThemeChanged(string theme)
    {
        _selectedTheme = theme;
        StateHasChanged();
    }

    private async Task OnThemeChangedAsync(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme)) return;
        await ThemeService.SetThemeAsync(theme);
    }

    protected override async Task OnInitializedAsync()
    {
        StateHasChanged();
    }

    private string Capitalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input [0]) + input [1..].ToLower();
    }

    private void OnSettingsClick()
    {
        _showSettings = true;
    }

    private async Task OnLogoutClick()
    {
        await AuthenticationService.LogoutAsync();
    }
}
