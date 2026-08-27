using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.TractorAnimation;

public partial class TractorAnimationComponent
{
    [Parameter] public string Width { get; set; } = "150px";
}