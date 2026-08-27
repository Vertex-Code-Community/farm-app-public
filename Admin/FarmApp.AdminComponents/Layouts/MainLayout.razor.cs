using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace FarmApp.AdminComponents.Layouts;

public partial class MainLayout : IAsyncDisposable
{
    [Inject] public required IPanelService PanelService { get; set; } = null!;
    [Inject] public required NavigationManager NavigationManager { get; set; } = null!;

    [Inject] public required IThemeInterface ThemeService { get; set; } = null!;

    private Action<string>? _onThemeChangedHandler;

    protected override async Task OnInitializedAsync()
    {
        PanelService.OnUpdate += StateHasChanged;
        NavigationManager.LocationChanged += LocationChanged!;
        _onThemeChangedHandler = _ => InvokeAsync(StateHasChanged);
        ThemeService.OnThemeChanged += _onThemeChangedHandler;
        await ThemeService.InitializeAsync();
    }

    public ValueTask DisposeAsync()
    {
        PanelService.OnUpdate -= StateHasChanged;
        NavigationManager.LocationChanged -= LocationChanged!;
        if (_onThemeChangedHandler is not null)
            ThemeService.OnThemeChanged -= _onThemeChangedHandler;

        return ValueTask.CompletedTask;
    }

    private void LocationChanged(object sender, LocationChangedEventArgs e)
    {
    }
}
