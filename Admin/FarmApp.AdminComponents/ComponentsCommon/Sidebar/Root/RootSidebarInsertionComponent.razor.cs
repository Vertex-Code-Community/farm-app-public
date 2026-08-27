using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.Sidebar.Root;

public partial class RootSidebarInsertionComponent : IDisposable
{
    [Inject] public required IHeaderInsertionService HeaderInsertionService { get; set; }
    [Inject] public required IPanelService PanelService { get; set; }

    protected override void OnInitialized()
    {
        HeaderInsertionService.OnUpdate += StateHasChanged;
        PanelService.OnUpdate += StateHasChanged;
    }

    private void HandleContainerClick()
    {
        if (!PanelService.IsShown)
        {
            PanelService.Show(true);
        }
    }

    public void Dispose()
    {
        HeaderInsertionService.OnUpdate -= StateHasChanged;
        PanelService.OnUpdate -= StateHasChanged;
    }
}
