using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.Sidebar;

public class SidebarInsertionComponent : ComponentBase, IDisposable
{
    [Inject] public required IHeaderInsertionService HeaderInsertionService { get; set; }

    [Parameter] public required RenderFragment ChildContent { get; set; }

    protected override void OnInitialized()
    {
        HeaderInsertionService.Add(ChildContent);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        HeaderInsertionService.Update();
    }

    public void Dispose()
    {
        HeaderInsertionService.Remove(ChildContent);
    }
}