using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.BottomSheet
{
    public partial class BottomSheetComponent : IAsyncDisposable
    {
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Inject] private IBackButtonService BackButtonService { get; set; } = null!;

        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public string MinHeight { get; set; } = "0px";
        [Parameter] public bool AllowDrag { get; set; } = true;
        [Parameter] public bool IsOpen { get; set; } = false;
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        private DotNetObjectReference<BottomSheetComponent>? _dotNetRef;
        private IJSObjectReference? _module;
        private ElementReference _sheetRef;
        private ElementReference _contentRef;

        private bool _atMinHeight = true;
        private bool _isDisposed = false;

        protected override void OnInitialized()
        {
            BackButtonService.OnBackButtonPressed += HandleCloseSheet;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_module == null) return;

            if (IsOpen == true)
            {
                await _module.InvokeVoidAsync("triggerOpenAnimation", _sheetRef);
            }

            if (IsOpen == false)
            {
                await _module.InvokeVoidAsync("triggerCloseAnimation", _sheetRef);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender || _isDisposed)
                return;
            try
            {
                if (firstRender)
                {
                    _dotNetRef = DotNetObjectReference.Create(this);
                    _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                        "./_content/FarmApp.Components/Components/BottomSheet/BottomSheetComponent.razor.js");

                    if (_isDisposed) return;

                    await _module.InvokeVoidAsync("init", _sheetRef, _dotNetRef, MinHeight, AllowDrag);

                    if (_isDisposed) return;
                    await _module.InvokeVoidAsync("initSheetObserver", _sheetRef, _contentRef);

                    if (_isDisposed) return;

                    if (IsOpen)
                    {
                        await _module.InvokeVoidAsync("triggerOpenAnimation", _sheetRef);
                    }
                    else
                    {
                        await _module.InvokeVoidAsync("triggerCloseAnimation", _sheetRef);
                    }
                }
            }
            catch (ObjectDisposedException)
            {

            }
            catch (InvalidOperationException)
            {

            }
        }

        public async ValueTask DisposeAsync()
        {
            _isDisposed = true;

            BackButtonService.OnBackButtonPressed -= HandleCloseSheet;

            _dotNetRef?.Dispose();

            if (_module is not null)
            {
                try
                {
                    await _module.DisposeAsync();
                }
                catch
                {
                    // ignore
                }
            }
        }

        [JSInvokable]
        public async Task<bool> HandleCloseSheet()
        {
            if (MinHeight == "0px")
            {
                if (!IsOpen) return false;

                IsOpen = false;
                await IsOpenChanged.InvokeAsync(false);

                if (_module != null)
                    await _module.InvokeVoidAsync("triggerCloseAnimation", _sheetRef);

                return true;
            }
            else
            {
                if (_atMinHeight) return false;

                if (_module != null)
                    await _module.InvokeVoidAsync("triggerCloseAnimation", _sheetRef);

                _atMinHeight = true;
                StateHasChanged();
                return true;
            }
        }

        [JSInvokable]
        public async Task NotifySheetClosed()
        {
            if (IsOpen)
            {
                IsOpen = false;
                await IsOpenChanged.InvokeAsync(false);

                await OnClose.InvokeAsync();
                StateHasChanged();
            }
        }

        [JSInvokable]
        public void SetAtMinHeight(bool atMinHeight)
        {
            _atMinHeight = atMinHeight;
            StateHasChanged();
        }
    }
}