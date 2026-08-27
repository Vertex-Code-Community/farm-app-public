using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.InfiniteScrollColumn
{
    public partial class InfiniteScrollColumnComponent<TItem> : IAsyncDisposable
    {
        [Inject] public IJSRuntime JS { get; set; } = default!;

        [Parameter] public List<TItem> Items { get; set; } = new();
        [Parameter] public TItem CurrentValue { get; set; } = default!;
        [Parameter] public EventCallback<TItem> OnValueChanged { get; set; }

        private ElementReference _scrollContainer;
        private IJSObjectReference? _module;

        private DotNetObjectReference<InfiniteScrollColumnComponent<TItem>>? _objRef;

        private List<TItem> _fullCycle = new();

        protected override void OnParametersSet()
        {
            _fullCycle = Items.Concat(Items).Concat(Items).ToList();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && Items.Any())
            {
                _module = await JS.InvokeAsync<IJSObjectReference>("import",
                    "./_content/FarmApp.Components/Components/InfiniteScrollColumn/InfiniteScrollColumnComponent.razor.js");

                _objRef = DotNetObjectReference.Create(this);

                int baseIndex = Items.IndexOf(CurrentValue);
                if (baseIndex == -1) baseIndex = 0;

                await _module.InvokeVoidAsync("initInfiniteScroll", _scrollContainer, baseIndex, Items.Count, _objRef);
            }
        }

        [JSInvokable]
        public async Task UpdateSelectedFromScroll(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                var newItem = Items[index];
                if (!EqualityComparer<TItem>.Default.Equals(newItem, CurrentValue))
                {
                    await OnValueChanged.InvokeAsync(newItem);
                    StateHasChanged();
                }
            }
        }
        private async Task HandleItemClick(TItem item)
        {
            int index = Items.IndexOf(item);
            if (index == -1) return;

            if (_module is not null)
            {
                await _module.InvokeVoidAsync("scrollToIndex", _scrollContainer, index);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (_module is not null)
            {
                await _module.DisposeAsync();
            }
        }
    }
}
