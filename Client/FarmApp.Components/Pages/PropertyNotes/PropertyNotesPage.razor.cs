using Microsoft.AspNetCore.Components;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.PropertyNoteStatuses;
using System.Threading.Tasks;

namespace FarmApp.Components.Pages.PropertyNotes;

public partial class PropertyNotesPage
{
    [Inject] public required IPropertyNoteService PropertyNoteService { get; set; }
    [Inject] public required IPropertyNoteStatusService PropertyNoteStatusService { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; } 

    [Parameter] public string PropertyId { get; set; } = string.Empty;
    [Parameter] public DateTime Date { get; set; }

    private PropertyPreviewModel _propertyPreviewModel = new();
    private List<PropertyNotePreviewModel> _notes = new();

    private bool _showRemoveNoteDialog = false;
    private PropertyNotePreviewModel? _propertyNoteToBeRemoved = null;
    private Dictionary<int, PropertyNoteStatusModel> _statuses = new();

    protected override void OnInitialized()
    {
        var statuses = StateService.PropertyNoteStatuses;
        _statuses = statuses.ToDictionary(x => x.Id);
        _propertyPreviewModel = StateService.GetPropertyPreview(PropertyId) ?? new();
        _notes = _propertyPreviewModel.Notes.Where(x => x.StartTime == Date).ToList();
    }

    private async Task OnDeleteClickedAsync()
    {
        if (_propertyNoteToBeRemoved is null) return;

        var propertyNote = _propertyNoteToBeRemoved;
        _propertyNoteToBeRemoved = null;
        _showRemoveNoteDialog = false;

        using var loader = GlobalLoaderService.SwitchOn();

        var result = await PropertyNoteService.DeleteAsync(propertyNote.Id);
        if (!result) return;
        
        StateService.DeletePropertyNote(_propertyPreviewModel.Id, propertyNote.Id);
        IMapPropertyService.InvokeDrawProperties(StateService.Properties);

        _propertyPreviewModel = StateService.GetPropertyPreview(PropertyId) ?? new();
        _notes = _propertyPreviewModel.Notes.Where(x => x.StartTime == Date).ToList();
        StateHasChanged();
    }

    private void OnCreateClicked()
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.PropertyNoteCreatePage,
            new Dictionary<string, object>
            {
                { "PropertyId", PropertyId },
                { "Date", Date }
            });
    }

    private void OnPropertyNoteClicked(PropertyNotePreviewModel propertyNote)
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.PropertyNoteCreatePage,
            new Dictionary<string, object>
            {
                { "PropertyId", PropertyId },
                { "PropertyNoteId", propertyNote.Id }
            });
    }
}