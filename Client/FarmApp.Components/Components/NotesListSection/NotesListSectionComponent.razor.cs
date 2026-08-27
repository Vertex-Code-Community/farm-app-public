using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.NotesListSection
{
    public partial class NotesListSectionComponent
    {
        [Parameter] public string? SectionTitle { get; set; }

        [Parameter] public required List<PropertyNoteModel> NotesList { get; set; }

        [Parameter] public int StartingAnimationIndex { get; set; } = 1;

        [Parameter] public bool IsLoading { get; set; } = false;

        [Parameter] public string? PropertyId { get; set; }

        [Parameter] public bool IsNested { get; set; }

        private bool _showCreateModal = false;

        public void OpenCreateNoteModal()
        {
            if (string.IsNullOrEmpty(PropertyId))
                return;
            _showCreateModal = true;
            StateHasChanged();
        }
    }
}
