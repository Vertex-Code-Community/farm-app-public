using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Components.DropdownSelector
{
    public partial class DropdownSelectorComponent: IAsyncDisposable
    {
        [Parameter] public required RenderFragment SelectedItem { get; set; }
        [Parameter] public required RenderFragment SelectOptions { get; set; }
        [Parameter] public bool IsOpen { get; set; } = false;
        [Parameter] public int MaxHeight { get; set; } = 500;
        [Parameter] public int Width { get; set; } = 200;
        [Parameter] public int Right { get; set; } = 0;
        [Parameter] public int Bottom { get; set; } = 0;
        [Parameter] public int Gap { get; set; } = 0;
        [Parameter] public bool TriggerClosing { get; set; } = false;
        [Parameter] public EventCallback<bool> TriggerClosingChanged { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

        private bool _openSelector = false;
        private string _elementId = $"dropdown-{Guid.NewGuid()}";
        private DotNetObjectReference<DropdownSelectorComponent>? _objRef;
        private IJSObjectReference? _module;

        private ElementReference _input;
        private ElementReference _list;

        protected override async Task OnParametersSetAsync()
        {
            if (TriggerClosing && _openSelector)
            {
                await CloseSelectorInternal();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        { 
            if (firstRender)
            {
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                    "./_content/FarmApp.Components/Components/DropdownSelector/DropdownSelectorComponent.razor.js");

                _objRef = DotNetObjectReference.Create(this);
            }

        }

        private async Task ToggleSelector()
        {
            _openSelector = !_openSelector;

            if (_openSelector)
            {
                if (_module != null)
                {
                    await _module.InvokeVoidAsync("registerClickOutside", _elementId, _objRef);
                }
            }
            else
            {
                await CleanupRegistration();
            }
        }

        private async Task CloseSelectorInternal()
        {
            _openSelector = false;
            await CleanupRegistration();

            if (TriggerClosing)
            {
                await TriggerClosingChanged.InvokeAsync(false);
            }

            StateHasChanged();
        }

        [JSInvokable]
        public async Task CloseSelector()
        {
            await CloseSelectorInternal();
        }

        private async Task CleanupRegistration()
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("unregisterClickOutside", _elementId);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await CleanupRegistration();
            _objRef?.Dispose();
        }

    }
}
