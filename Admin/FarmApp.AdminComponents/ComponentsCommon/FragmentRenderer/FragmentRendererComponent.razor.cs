using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.ComponentsCommon.FragmentRenderer;

public partial class FragmentRendererComponent
{
    [Parameter] public required RenderFragment ChildContent { get; set; }
}