using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Bch.Modules.Themes.Attributes;
using Bch.Modules.Themes.Extensions;
using Bch.Modules.Themes.Models;

namespace Bch.Modules.Themes.Providers;

public class BchGlobalThemeProvider : ComponentBase, IAsyncDisposable
{
    [Inject] public required IJSRuntime JsRuntime { get; set; }
    
    [Parameter] public BchTheme Theme { get; set; } = BchTheme.LightGreen;

    private BchTheme? _previousTheme;

    protected override async Task OnParametersSetAsync()
    {
        if (_previousTheme == Theme) return;
        _previousTheme = Theme;

        var theme = Theme.GetValue<string, CssNameAttribute>(a => a.CssName) ?? string.Empty;

        await JsRuntime.InvokeVoidAsync("bchSetThemeToHtml", theme);
    }

    public async ValueTask DisposeAsync()
    {
        await JsRuntime.InvokeVoidAsync("bchClearThemeToHtml");
    }
}