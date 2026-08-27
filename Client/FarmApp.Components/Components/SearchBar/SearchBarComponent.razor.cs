using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.SearchBar
{
    public partial class SearchBarComponent
    {
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public string SearchQuery { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> SearchQueryChanged { get; set; }

        private bool _hasInput => SearchQuery.Length > 0;

        private bool _hasFocus = false;

        private ElementReference _searchInput;

        private async Task HandleInput(ChangeEventArgs e)
        {
            SearchQuery = e.Value?.ToString() ?? string.Empty;

            await SearchQueryChanged.InvokeAsync(SearchQuery);
        }

        private void HandleFocus()
        {
            _hasFocus = true;
        }

        private void HandleBlur()
        {
            _hasFocus = false;
        }

        private async Task HandleBack()
        {
            await ClearInput();
            _hasFocus = false;
        }

        private async Task HandleClear()
        {
            await ClearInput();
            await _searchInput.FocusAsync();
        }

        private async Task HandleAudioInput()
        {
            await _searchInput.FocusAsync();
        }

        private async Task ClearInput()
        {
            SearchQuery = string.Empty;
            await SearchQueryChanged.InvokeAsync(string.Empty);
        }
    }
}
