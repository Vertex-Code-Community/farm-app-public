using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.NoteStatuses;

public partial class NoteStatuses
{
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private List<PropertyNoteStatusModel> _noteDefaultStatuses = new List<PropertyNoteStatusModel>();
    private List<PropertyNoteStatusModel> _noteCustomStatuses = new List<PropertyNoteStatusModel>();

    private bool _showCreateStatusModal = false;
    private int _editingStatusId = -1;

    protected override void OnInitialized()
    {
        RefreshStatuses();
    }
    private void RefreshStatuses()
    {
        _noteDefaultStatuses = StateService.PropertyNoteStatuses
            .Where(x => x.IsDefault)
            .ToList();

        _noteCustomStatuses = StateService.PropertyNoteStatuses
            .Where(x => !x.IsDefault)
            .ToList();
    }

    private void CloseModal()
    {
        _showCreateStatusModal = false;
        _editingStatusId = -1;
        RefreshStatuses();
        StateHasChanged();
    }

    private void CreateCallback()
    {
        _showCreateStatusModal = true;
    }
}
        