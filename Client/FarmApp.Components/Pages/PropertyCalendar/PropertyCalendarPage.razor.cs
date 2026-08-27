using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.PropertyCalendar;

public partial class PropertyCalendarPage : IDisposable
{
    [Inject] private IStateService StateService { get; set; } = null!;
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IPropertyNotesAggregatorService PropertyNotesAggregatorService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    [Inject] public IGuideTourService GuideTourService { get; set; } = default!;

    [Parameter] public string PropertyId { get; set; } = string.Empty;
    [Parameter] public DateTime Date { get; set; } = DateTime.Now;
    private DateTime _selectedDay = default;
    private bool _isLoading;

    private List<PropertyNoteModel> notesForDay = new();
    private List<PropertyNoteModel> notes = new();
    private List<PropertyNoteStatusModel> _userStatuses = new();

    private bool _showCreateNoteModal = false;

    protected async override Task OnInitializedAsync()
    {
        StateService.OnPropertyNoteAdded += OnNoteAdded;
        _isLoading = true;
        await Task.WhenAll(LoadDataAsync(), Task.Delay(600));
        _isLoading = false;

        GuideTourService.StartTour(TourGroup.CalendarPage);
    }
    private async Task LoadDataAsync()
    {

        if (string.IsNullOrWhiteSpace(PropertyId))
        {
            notes = await PropertyNotesAggregatorService.LoadAllAsync();
        }
        else
        {
            var preview = StateService.GetPropertyPreview(PropertyId);
            if (preview != null)
                notes = PropertyNotesAggregatorService.GetNoteModelByPreviewModel(preview.Notes, PropertyId);
        }


    }
    private bool isDayNotesLoading = false;
    public async Task SetSelectedDay(DateTime selectedDay)
    {
        _selectedDay = selectedDay;
        isDayNotesLoading = true;

        await InvokeAsync(StateHasChanged);

        if (string.IsNullOrWhiteSpace(PropertyId))
        {
            notes = await PropertyNotesAggregatorService.LoadAllAsync();
        }
        else
        {
            var preview = StateService.GetPropertyPreview(PropertyId);
            if (preview != null)
                notes = PropertyNotesAggregatorService.GetNoteModelByPreviewModel(preview.Notes, PropertyId);
        }

        isDayNotesLoading = false;

        await InvokeAsync(StateHasChanged);
    }
    private void OnNoteAdded()
    {
        _ = InvokeAsync(async () =>
        {
            await LoadDataAsync();
            await InvokeAsync(StateHasChanged);
        });
    }
    private void GetNotesForDay()
    {
        StateService.PropertyPreviewNotes.TryGetValue(PropertyId, out var propertyNotePreviewModels);
        notesForDay = notes?.Where(x => x.StartTime.Date == _selectedDay.Date).ToList() ?? new();
    }
    private void OnNoteCreateClick()
    {
        if (_selectedDay == default)
            _selectedDay = DateTime.Today;
        _showCreateNoteModal = true;
    }


    public void Dispose()
    {
        StateService.OnPropertyNoteAdded -= OnNoteAdded;
    }
}