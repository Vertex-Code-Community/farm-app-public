using Bch.Components.Modal.Models;
using Bch.Components.Modal.Services;
using Microsoft.AspNetCore.Components;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Services;

public class TabsService : ITabsService, IDisposable
{
    public event Action? OnVisibilityChanged;

    public bool Shown { get; private set; }
    public bool TabsAreRendered { get; private set; }

    private readonly IModalService _modalService;
    
    private readonly ModalModel _tabsModal = new()
    {
        Height = ITabsService.TabsHeight,  /*  tab panel height */
        Width = "calc(100% - 40px)",
        X = "20px",
        Overlay = false,
        ZIndex = 5000,
        CssStyles = "pointer-events: none;"
    };
    
    public TabsService(IModalService modalService)
    {
        _modalService = modalService;
    }

    public void RenderTabs(RenderFragment renderFragment)
    {
        if (TabsAreRendered) return;
        TabsAreRendered = true;
        
        _tabsModal.Y = $"calc(100% - {ITabsService.TabsHeight} - {ScreenOffsetProvider.Bottom}px - 0px)";

        _tabsModal.Fragment = renderFragment;
        _modalService.Open(_tabsModal);
    }

    public void SwitchVisibility(bool show)
    {
        Shown = show;
        OnVisibilityChanged?.Invoke();
    }

    public void Dispose()
    {
        if (TabsAreRendered) _modalService.Close(_tabsModal);
    }
}