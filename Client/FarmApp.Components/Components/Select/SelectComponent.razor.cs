using Bch.Modules.DomInterop.Services;
using Bch.Modules.GlobalEvents.Events;
using Bch.Modules.GlobalEvents.Services;
using Bch.Modules.Maths.Models;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace FarmApp.Components.Components.Select
{
    public partial class SelectComponent<TItem> : IAsyncDisposable
    {
        [Inject] public required IDomInteropService DomInteropService { get; set; }
        [Inject] public required IGlobalEventsService GlobalEventsService { get; set; }

        [Parameter] public required string Width { get; set; } = "200px;";
        [Parameter] public required string Height { get; set; } = "50px;";
        [Parameter] public required int DropDownHeight { get; set; } = 180;
        [Parameter] public required int DropDownItemHeight { get; set; } = 40;

        [Parameter, EditorRequired] public List<TItem> Items { get; set; } = new();

        [Parameter, EditorRequired] public required RenderFragment<TItem> ItemTemplate { get; set; }
        [Parameter, EditorRequired] public required RenderFragment<TItem?> HeaderTemplate { get; set; }

        [Parameter] public EventCallback<bool> IsOpenedChanged { get; set; }
        [Parameter] public bool IsOpened { get => _isOpened; set { } }

        [Parameter] public EventCallback<TItem?> SelectedChanged { get; set; }
        [Parameter]
        public TItem? Selected
        {
            get => _selectedValue;
            set
            {
                if ((_selectedValue != null && _selectedValue.Equals(value)) || (_selectedValue == null && value == null)) return;
                _selectedValue = value;
                SelectedChanged.InvokeAsync(value);
            }
        }

        private bool _isOpened = false;
        private TItem? _selectedValue;

        private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
        private readonly string _containerId = $"_id_{Guid.NewGuid()}";
        private readonly string _contentContainerId = $"_id_{Guid.NewGuid()}";
        private readonly string _subscriptionKey = $"_key_{Guid.NewGuid()}";

        private Vec2 _ddlContentPos = new();

        protected override Task OnInitializedAsync()
        {
            return GlobalEventsService.AddDocumentListenerAsync<BchMouseEventArgs>("mousedown", _subscriptionKey,
                OnDocumentMouseDownAsync);
        }

        public async ValueTask DisposeAsync()
        {
            await GlobalEventsService.RemoveDocumentListenerAsync<BchMouseEventArgs>("mousedown", _subscriptionKey);
        }

        private async Task OnDocumentMouseDownAsync(BchMouseEventArgs e)
        {
            var container = e.PathCoordinates
                .FirstOrDefault(x => x.Id == _containerId || x.Id == _contentContainerId);

            if (container is not null) return; // inside of select
            _isOpened = false;
            await IsOpenedChanged.InvokeAsync(_isOpened);
            StateHasChanged();
        }

        private async Task OnSelectClickedAsync()
        {
            if (!_isOpened)
            {
                var containerRect = await DomInteropService.GetBoundingClientRectAsync(_containerId);
                if (containerRect is null) return;
                _ddlContentPos.Set(containerRect.X, containerRect.Y + containerRect.Height);
            }

            _isOpened = true;
            await IsOpenedChanged.InvokeAsync(_isOpened);
            StateHasChanged();
        }

        private Task OnOptionClickedAsync(TItem? item)
        {
            Selected = item;
            _isOpened = false;
            return IsOpenedChanged.InvokeAsync(_isOpened);
        }
    }
}
