using FarmApp.Services.Services;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.DisplayMedia
{
    public partial class DisplayMediaComponent : IDisposable
    {
        [Inject] public required IMediaService MediaService { get; set; }
        [Inject] public required UploadQueueService UploadQueueService { get; set; }
        [Parameter] public EventCallback OnAddClick { get; set; }
        [Parameter] public bool IsEditMode { get; set; }
        [Parameter] public string PropertyNoteId { get; set; } = string.Empty;
        [Parameter] public List<PickedMediaFile> Files { get; set; } = new List<PickedMediaFile>();
        [Parameter] public EventCallback<List<PickedMediaFile>> FilesChanged { get; set; }
        [Parameter] public EventCallback<string> OnPreviewSet { get; set; }
        [Parameter] public string? MediaViewerTitle { get; set; } = string.Empty;
        [Parameter] public string? MediaViewerSubtitle { get; set; } = string.Empty;


        private HashSet<Guid> _alreadyEnqueued = new();
        private UploadMode UploadMode => IsEditMode ? UploadMode.Temporary : UploadMode.Permanent;

        private bool _openMediaViewer = false;
        private int _startingIndex = 0;

        protected override void OnInitialized()
        {
            UploadQueueService.OnChanged += HandleUploadComplete;
        }
        protected override void OnParametersSet()
        {
            if (Files == null)
                return;

            var newLocalFiles = Files.Where(f => f.MediaSource == MediaSource.Local && 
            f.UploadState == UploadState.Pending && !_alreadyEnqueued.Contains(f.Id));

            foreach(var file in newLocalFiles)
            {
                if (UploadMode == UploadMode.Permanent)
                    file.PropertyNoteId = PropertyNoteId;

                UploadQueueService.Enqueue(file, UploadMode);
                _alreadyEnqueued.Add(file.Id);
            }
        }
        private void HandleUploadComplete(PickedMediaFile file)
        {
            if (file.isNotePreview)
            {
                OnPreviewSet.InvokeAsync(file.MediaId);
            }

            StateHasChanged();
        }
        private async Task RemoveMedia(PickedMediaFile file)
        {
            if (file.IsRemote)
            {
                var removeResult = await MediaService.DeleteUploadedMedia(PropertyNoteId, file.MediaId!);

                if (removeResult != null)
                {
                    if (removeResult.IsPreviewDeleted)
                        await OnPreviewSet.InvokeAsync(removeResult.NewPreviewMediaId);
                }
            }
            Files.Remove(file);

            await FilesChanged.InvokeAsync(Files.ToList());
        }
        private void RetryUpload(PickedMediaFile file)
        {
            if (!file.IsRemote && file.UploadState == UploadState.Failed)
                UploadQueueService.RetryUpload(file, UploadMode);

        }

        private void OpenMediaViewer(int startingIndex)
        {
            _startingIndex = startingIndex;
            _openMediaViewer = true;
        }

        public void Dispose()
        {
            UploadQueueService.OnChanged -= HandleUploadComplete;
        }
    }
}
