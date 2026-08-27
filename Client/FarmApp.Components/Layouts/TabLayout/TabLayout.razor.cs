using Microsoft.AspNetCore.Components;
using FarmApp.Components.Layouts.TabLayout.TabPanel;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;

namespace FarmApp.Components.Layouts.TabLayout;

public partial class TabLayout : IDisposable
{
    [Inject] private ITabsService TabsService { get; set; } = null!;
    [Inject] private INavigationService NavigationService { get; set; } = null!;

    /// <summary>MAUI hybrid: native MapLibre sits under Blazor; page chrome must stay transparent so the map is visible.</summary>
    private bool TransparentNativeMapUnderlay =>
        !OperatingSystem.IsBrowser()
        && NavigationService.CurrentPage?.Route == Constants.ClientRoutes.MainPage;

    protected override void OnInitialized()
    {
        NavigationService.LocationChanged += OnNavigationLocationChanged;
        IMapPropertyService.OnUpdate += OnMapPropertyServiceUpdate;
        TabsService.SwitchVisibility(true);

        if (TabsService.TabsAreRendered) return;
        TabsService.RenderTabs(builder =>
        {
            builder.OpenComponent(0, typeof(TabPanelComponent));
            builder.CloseComponent();
        });
    }

    private void OnMapPropertyServiceUpdate()
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnNavigationLocationChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        NavigationService.LocationChanged -= OnNavigationLocationChanged;
        IMapPropertyService.OnUpdate -= OnMapPropertyServiceUpdate;
        TabsService.SwitchVisibility(false);
    }
}
