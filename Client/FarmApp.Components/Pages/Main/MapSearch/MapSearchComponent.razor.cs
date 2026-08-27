using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Properties;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.Main.MapSearch;

public partial class MapSearchComponent : IDisposable
{
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required IMapSearchStateService SearchState { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    [Inject] public IGuideTourService GuideTourService { get; set; } = default!;

    private string _searchTerm = "";

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                OnSearchChanged();
            }
        }
    }

    private List<PropertyViewModel> _properties = [];
    private List<PropertyViewModel> _matchedProperties = [];
    private bool ShouldShowMatches => _searchTerm.Trim().Length > 2;
    private int _resultsContHeight = 0;

    protected override async Task OnInitializedAsync()
    {
        IMapPropertyService.OnUpdate += OnMapPropertyUiChanged;
        await StateService.WhenPropertiesReady;

        _properties = StateService.Properties.ToList();
    }

    public void Dispose()
    {
        IMapPropertyService.OnUpdate -= OnMapPropertyUiChanged;
    }

    private void OnMapPropertyUiChanged()
    {
        InvokeAsync(StateHasChanged);
    }
    private void OnSearchChanged()
    {
        if (string.IsNullOrEmpty(_searchTerm))
        {
            _matchedProperties = _properties.ToList();

            _resultsContHeight = 0;
            SearchState.IsOpen = false;
            SearchState.CurrentResultsHeight = 0;
            return;
        }

        if (!ShouldShowMatches)
        {
            _resultsContHeight = 0;
            SearchState.IsOpen = false;
            SearchState.CurrentResultsHeight = 0;
            return;
        }

        var term = _searchTerm.Trim();
        _matchedProperties = _properties
            .Where(p => !string.IsNullOrEmpty(p.Name) &&
                        p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var matches = _matchedProperties.Count;
        _resultsContHeight = matches <= 0 ? 52 : (matches * 53 - 1);
        _resultsContHeight = _resultsContHeight > 300 ? 300 : _resultsContHeight;

        SearchState.IsOpen = true;
        SearchState.CurrentResultsHeight = _resultsContHeight;
    }
    private async Task OnMatchedPropertyClicked(PropertyViewModel matchedProperty)
    {
        _resultsContHeight = 0;
        SearchState.CurrentResultsHeight = _resultsContHeight;
        _searchTerm = string.Empty;

        if (matchedProperty.Centroid is not null)
        {
            IMapCallbackService.FlyTo(
                (float)matchedProperty.Centroid.X,
                (float)matchedProperty.Centroid.Y,
                matchedProperty.Zoom);
        }

        await Task.Delay(300);

        _matchedProperties.Clear();

        await InvokeAsync(StateHasChanged);
    }

    private void StartGuide()
    {
        if (IMapSteadService.IsDrawingModeOn != true)
        {
            IMapSteadService.InvokeSwitchDrawingMode(true);
        }

        GuideTourService.StartTour(TourGroup.HowToUse);
    }

}
