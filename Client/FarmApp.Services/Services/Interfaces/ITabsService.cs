using Microsoft.AspNetCore.Components;

namespace FarmApp.Services.Services.Interfaces;

public interface ITabsService
{
    const string TabsHeight = "64px";
    event Action OnVisibilityChanged;
    
    bool Shown { get; }
    bool TabsAreRendered { get; }
    void RenderTabs(RenderFragment renderFragment);
    void SwitchVisibility(bool show);
}