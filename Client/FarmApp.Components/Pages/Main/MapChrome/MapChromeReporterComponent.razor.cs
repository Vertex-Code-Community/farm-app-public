using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Pages.Main.MapChrome;

public partial class MapChromeReporterComponent : IAsyncDisposable
{
    [Inject] public required IJSRuntime JS { get; set; }

    private IJSObjectReference? _module;
    private DotNetObjectReference<MapChromeReporterComponent>? _objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        
        if (OperatingSystem.IsBrowser()) return;

        try
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import",
                "./_content/FarmApp.Components/Pages/Main/MapChrome/MapChromeReporterComponent.razor.js");
            _objRef = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("init", _objRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapChromeReporter] init failed: {ex.Message}");
        }
    }

    [JSInvokable]
    public void OnChromeRects(ChromeRectDto[] rects)
    {
        var mapped = new MapControlBounds[rects.Length];
        for (var i = 0; i < rects.Length; i++)
        {
            var r = rects[i];
            mapped[i] = new MapControlBounds(r.Left, r.Top, r.Right, r.Bottom);
        }

        IMapControlsBoundsService.SetBounds(mapped);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
        }
        catch
        {
            // WebView may already be torn down; ignore.
        }

        _objRef?.Dispose();
        IMapControlsBoundsService.SetBounds(null);
    }

    public sealed class ChromeRectDto
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }
    }
}
