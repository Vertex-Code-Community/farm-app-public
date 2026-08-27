using FarmApp.Components.Helpers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.ViewNote
{
    public partial class ViewNotePage
    {
        [Inject] public required INavigationService NavigationService { get; set; }
        [Inject] public required IPropertyNoteService PropertyNoteService { get; set; }
        [Inject] public required IStateService StateService { get; set; }
        [Inject] public required IMediaService MediaService { get; set; }
        [Inject] public required IStringLocalizer <AppRecources> Localizer { get; set; }
        [Parameter] public required PropertyNoteModel Note { get; set; }

        private TimeOptionModel _remindAtStart = new();
        private TimeOptionModel _remindAtEnd = new();

        private List<PickedMediaFile> _files = [];

        private string PropertyName => StateService.Properties.FirstOrDefault(x => x.Id == Note.PropertyId)!.Name;

        private bool _closing = false;
        private bool _showDeleteModal = false;
        private bool _isLoading;
        private bool _showEditModal = false;
        private bool _showMediaPickerModal = false;

        private bool _isNotificationsEnabled;


        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;

            await Task.WhenAll(LoadData(), Task.Delay(600));

            _isLoading = false;
        }
        private async Task LoadData()
        {
            var uploadedMedias = await MediaService.GetMediaByNoteIdAsync(Note.Id);
            _files = uploadedMedias.Select(m => new PickedMediaFile
            {
                FileName = m.Id,
                RemoteUrl = m.Url,
                ThumbnailUrl = m.ThumbnailUrl,
                MediaId = m.Id,
                ContentType = m.ContentType,
                MediaSource = MediaSource.Server,
                UploadState = UploadState.Uploaded
            }).ToList();

            SetPropertyNoteRemindings();
        }
        private void EditNote()
        {
            _showEditModal = true;
        }
        private void SetPropertyNoteRemindings()
        {
            if (Note.NotificationsEnabled &&
                Note.NotifyBeforeStart != null && Note.NotifyBeforeEnd != null)
            {
                _isNotificationsEnabled = Note.NotificationsEnabled;
                _remindAtStart = TimeOptionHelper.GetByDuration(Note.NotifyBeforeStart!.Value, Localizer)!;
                _remindAtEnd = TimeOptionHelper.GetByDuration(Note.NotifyBeforeEnd!.Value, Localizer)!;
            }
        }
        private void OpenMediaPickerModal()
        {
            _showMediaPickerModal = true;
        }
        private void OnPreviewNoteSet(string? mediaId)
        {
            var propertyPreviewModel = StateService.GetPropertyPreview(Note.PropertyId);
            var propertyNote = propertyPreviewModel?.Notes
                .FirstOrDefault(x => x.Id == Note.Id);

            propertyNote!.PreviewMediaId = mediaId;
            
        }
        private void OnFilesPicked(List<PickedMediaFile> files)
        {
            _files = _files.Concat(files).ToList();
        }
        private async Task DeleteNote()
        {
            var success = await PropertyNoteService.DeleteAsync(Note.Id);

            if (success)
                StateService.DeletePropertyNote(Note.PropertyId, Note.Id);

            await NavigateBack(false);
        }
        private async Task OnPropertyNoteUpdated(PropertyNoteModel updatedPropertyNote)
        {
            _isLoading = true;
            await Task.Delay(600);
            Note = updatedPropertyNote;

            SetPropertyNoteRemindings();

            _isLoading = false;
        }
        private void OnRemindAtStartSelected(TimeOptionModel time)
        {
            _remindAtStart = time;
            StateHasChanged();
        }

        private void OnRemindAtEndSelected(TimeOptionModel time)
        {
            _remindAtEnd = time;
            StateHasChanged();
        }
        private async Task UpdatePropertyNoteRemindings()
        {
            if (_isNotificationsEnabled != Note.NotificationsEnabled
                || _remindAtStart.Duration != Note.NotifyBeforeStart 
                || _remindAtEnd.Duration != Note.NotifyBeforeEnd)
            {
                var result = await PropertyNoteService.UpdateAsync(Note.Id, new UpdatePropertyNoteModel
                {
                    Header = Note.Header,
                    Text = Note.Text,
                    StartTime = Note.StartTime,
                    EndTime = Note.EndTime,
                    NotificationsEnabled = _isNotificationsEnabled,
                    NotifyBeforeStart = _remindAtStart.Duration != Shared.Enums.NotificationOffset.None && _isNotificationsEnabled
                    ? _remindAtStart.Duration : null,
                    NotifyBeforeEnd = _remindAtEnd.Duration != Shared.Enums.NotificationOffset.None && _isNotificationsEnabled
                    ? _remindAtEnd.Duration : null,
                    StatusId = Note.StatusId
                });
                if (result != null)
                    StateService.UpdatePropertyNote(result);
            }
        }

        private async Task NavigateBack(bool updateRemindings = true)
        {
            _closing = true;

            if (updateRemindings)
                await UpdatePropertyNoteRemindings();

            NavigationService.Back();
        }
    }
}
