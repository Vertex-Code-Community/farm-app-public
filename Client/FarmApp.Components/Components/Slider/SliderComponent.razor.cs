using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.Slider;

public partial class SliderComponent<TItem> : ComponentBase, IAsyncDisposable
{
    [Inject] public required IJSRuntime JsRuntime { get; set; }
    [Parameter] public IEnumerable<TItem> Items { get; set; } = [];
    [Parameter] public RenderFragment<TItem> ItemTemplate { get; set; } = default!;

    [Parameter] public int StartingIndex { get; set; } = 0;

    [Parameter] public int ScrollIndex { get; set; }
    [Parameter] public EventCallback<int> ScrollIndexChanged { get; set; }

    private ElementReference sliderRef;
    private IJSObjectReference? _module;
    private DotNetObjectReference<SliderComponent<TItem>>? _objRef;
    private IJSObjectReference? _sliderInstance;

    private int _currentInternalIndex = -1;

    private int _lastStartingIndex = -1;
    private bool _isDisposed = false;
    protected override async Task OnParametersSetAsync()
    {
        if (_module == null) return;
        Console.WriteLine($"SCROLL INDEX: {ScrollIndex}");

        if (ScrollIndex != _currentInternalIndex)
        {
            await ScrollTo(ScrollIndex);
        }

        if (StartingIndex != _lastStartingIndex)
        {
            _lastStartingIndex = StartingIndex;

            await OnScrollIndexChanged(StartingIndex);

            await _module.InvokeVoidAsync("scrollToIndex", sliderRef, StartingIndex, "instant");
            return;
        }



    }
    public async Task ScrollTo(int index)
    {
        if (_module != null)
        {
            _currentInternalIndex = index;
            await _module.InvokeVoidAsync("scrollToIndex", sliderRef, index);
            await ScrollIndexChanged.InvokeAsync(index);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _isDisposed)
            return;

        _objRef = DotNetObjectReference.Create(this);
        _module = await JsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/FarmApp.Components/Components/Slider/SliderComponent.razor.js");

        if (_isDisposed)
            return;

        _sliderInstance = await _module.InvokeAsync<IJSObjectReference>("initializeSlider", sliderRef, _objRef);
    }

    [JSInvokable]
    public async Task OnScrollIndexChanged(int index)
    {
        if (_isDisposed)
            return;
        if (_currentInternalIndex != index)
        {
            _currentInternalIndex = index;
            ScrollIndex = index;
            await ScrollIndexChanged.InvokeAsync(index);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        try
        {
            if (_sliderInstance != null)
            {
                await _sliderInstance.InvokeVoidAsync("dispose");
                await _sliderInstance.DisposeAsync();
            }
            if (_module is not null)
            {
                await _module.DisposeAsync();
            }
            _objRef?.Dispose();
        }
        catch (JSDisconnectedException) { /* Handle WebView disposal */ }
    }
}