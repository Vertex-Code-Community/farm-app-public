
namespace FarmApp.ViewModels.Media
{
    public class PickedMediaFile 
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string FileName { get; init; } = default!;
        public string PropertyNoteId { get; set; } 
        public string ContentType { get; init; } = default!;
        public string? RemoteUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? MediaId { get; set; }
        public bool isNotePreview { get; set; }
        public MediaSource MediaSource { get; set; }
        public UploadState UploadState { get; set; } = UploadState.Pending;
        public CancellationTokenSource? Cancellation { get; set; }
        public TempUploadResult? TempUploadResult { get; set; }
        public bool IsRemote => MediaSource == MediaSource.Server;
        public Func<Task<Stream>> OpenReadStreamAsync { get; set; } = default!;
        public void CancelUpload()
        {
            if (Cancellation == null)
                return;

            Cancellation.Cancel();
            Cancellation.Dispose();
            Cancellation = null;
        }
    }
}
