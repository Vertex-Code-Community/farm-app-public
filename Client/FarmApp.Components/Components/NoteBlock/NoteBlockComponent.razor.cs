using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.NoteBlock
{
    public partial class NoteBlockComponent
    {
        [Inject] public required INavigationService NavigationService { get; set; }
        [Inject] public required IMediaService MediaService { get; set; }

        [Parameter] public required PropertyNoteModel Note { get; set; }

        [Parameter] public int? AnimationIndex { get; set; }

        private string? _imgUrl => Note.PreviewMediaId != null ? MediaService.GetThumbnailUrl(Note.PreviewMediaId) : null;

        private bool _isReadyToAnimate = false;

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && AnimationIndex.HasValue)
            {
                _isReadyToAnimate = true;
                StateHasChanged();
            }
        }

        private void NavigateToViewNote(PropertyNoteModel note)
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.ViewNotePage, new Dictionary<string, object>
            {
                { "Note", note }
            });
        }
    }
}
