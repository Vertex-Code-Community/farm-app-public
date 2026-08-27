using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace FarmApp.Components.Components.SnapSlider;

public partial class SnapSliderComponent<TItem> : ComponentBase, IAsyncDisposable
{
    [Inject] public required IJSRuntime JsRuntime { get; set; }

    [Parameter] public required RenderFragment<TItem> ItemTemplate { get; set; }
    [Parameter] public Func<int, TItem> OnItem { get; set; } = _ => default!;

    [Parameter]
    public int ScrollIndex
    {
        get => _scrollIndex;
        set
        {
            if (value == _scrollIndex) return;
            _scrollIndex = value;
            ScrollIndexChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback<int> ScrollIndexChanged { get; set; }
    [Parameter] public int DefaultLeftShift { get; set; } = 2000;

    private readonly string _id = $"_id_{Guid.NewGuid()}";
    private readonly string _scrollerId = $"_id_{Guid.NewGuid()}";

    private ElementReference _scrollerRef;
    private IJSObjectReference? _module;
    private int _scrollIndex = 0;
    private bool _firstRendered = false;
    private bool _isDisposed = false;
    private readonly NumberFormatInfo _nfWithDot = new() { NumberDecimalSeparator = ".", NumberDecimalDigits = 14 };

    private DotNetObjectReference<SnapSliderComponent<TItem>>? _dotNetObjectReference;

    protected override Task OnInitializedAsync()
    {
        _dotNetObjectReference = DotNetObjectReference.Create(this);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;

        try
        {
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("releaseSnapFeedbackRef", _scrollerId);
                await _module.DisposeAsync();
            }

            _dotNetObjectReference?.Dispose();
        }
        catch (JSDisconnectedException) { /* WebView disposed */ }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/FarmApp.Components/Components/SnapSlider/SnapSliderComponent.razor.js");

            if (_isDisposed || _module is null || _dotNetObjectReference is null)
                return;

            await _module.InvokeVoidAsync(
                "initializeSnapSlider",
                _scrollerRef,
                _dotNetObjectReference,
                _scrollerId,
                DefaultLeftShift);
        }

        if (!firstRender)
            return;

        await Task.Yield();
        _firstRendered = true;
        StateHasChanged();
    }

    [JSInvokable]
    public Task OnNextCalledFromScrollListenerAsync(int index)
    {
        OnNextClicked(index);
        return Task.CompletedTask;
    }

    private void OnNextClicked(int k)
    {
        ScrollIndex += k;
        StateHasChanged();
    }
    public async Task ScrollAsync(bool toRight)
    {
        if (_module is null)
            return;

        OnNextClicked(toRight ? 1 : -1);

        await _module.InvokeVoidAsync("snapSliderScrollTo", _scrollerId, toRight);
    }
}
