using Bch.Modules.Themes.Models;
using Microsoft.AspNetCore.Components;

namespace Bch.Modules.Themes.Providers;

public partial class BchThemeProvider : ComponentBase
{
    [Parameter] public BchTheme Theme { get; set; } = BchTheme.LightGreen;
    [Parameter] public required RenderFragment ChildContent { get; set; }
}


