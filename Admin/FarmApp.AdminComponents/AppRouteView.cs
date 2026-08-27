using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FarmApp.AdminComponents;

public class AppRouteView : RouteView
{
    [Inject] public required NavigationManager NavigationManager { get; set; }

    protected override void Render(RenderTreeBuilder builder)
    {
        base.Render(builder);
    }
}
