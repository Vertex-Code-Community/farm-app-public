using Bch.Modules.Themes.Models;
using FarmApp.AdminComponents.Services.Interfaces;
using Microsoft.JSInterop;

namespace FarmApp.AdminComponents.Services;

public class ThemeService(IJSRuntime jsRuntime) : IThemeInterface, IAsyncDisposable
{
    private DotNetObjectReference<ThemeService>? _objRef;

    public event Action<string>? OnThemeChanged;

    public string CurrentTheme { get; private set; }

    public BchTheme CurrentBchTheme => IsDark ? BchTheme.Dark : BchTheme.LightGreen;

    public bool IsDark { get; private set; }

    public async Task InitializeAsync()
    {
        _objRef = DotNetObjectReference.Create(this);
        await jsRuntime.InvokeVoidAsync("themeService.initialize", _objRef);

        CurrentTheme = await jsRuntime.InvokeAsync<string>("themeService.getCurrentTheme");
        IsDark = await jsRuntime.InvokeAsync<bool>("themeService.getIsDark");

        OnThemeChanged?.Invoke(CurrentTheme);
    }

    public async Task SetThemeAsync(string theme)
    {
        IsDark = await jsRuntime.InvokeAsync<bool>("themeService.setTheme", theme);
        CurrentTheme = await jsRuntime.InvokeAsync<string>("themeService.getCurrentTheme");
        OnThemeChanged?.Invoke(CurrentTheme);
    }

    public async Task ToggleThemeAsync()
    {
        IsDark = await jsRuntime.InvokeAsync<bool>("themeService.toggleTheme");
        CurrentTheme = await jsRuntime.InvokeAsync<string>("themeService.getCurrentTheme");
        OnThemeChanged?.Invoke(CurrentTheme);
    }

    [JSInvokable]
    public void OnSystemSchemeChanged(bool isDark)
    {
        if (!string.Equals(CurrentTheme, "system", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        IsDark = isDark;
        OnThemeChanged?.Invoke(CurrentTheme);
    }

    public ValueTask DisposeAsync()
    {
        _objRef?.Dispose();
        return ValueTask.CompletedTask;
    }
}
