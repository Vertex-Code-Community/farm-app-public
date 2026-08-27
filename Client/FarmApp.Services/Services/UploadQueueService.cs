using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Media;

namespace FarmApp.Services.Services
{
    public class UploadQueueService
    {
        private readonly SemaphoreSlim _semaphoreSlim = new(3);
        private readonly IMediaService _mediaUploadService;
        public event Action<PickedMediaFile>? OnChanged;
        
        public UploadQueueService(IMediaService mediaUploadService)
        {
            _mediaUploadService = mediaUploadService;
        }
        private void NotifyChanged(PickedMediaFile mediaFile)
        {
            OnChanged?.Invoke(mediaFile);
        }

        public void Enqueue(PickedMediaFile file, UploadMode uploadMode)
        {
            _ = EnqueueAsync(file, uploadMode);
        }
        public void RetryUpload(PickedMediaFile file, UploadMode uploadMode)
        {
            if (file.UploadState != UploadState.Failed)
                return;

            file.UploadState = UploadState.Pending;
            file.TempUploadResult = null;

            file.CancelUpload();

            Enqueue(file, uploadMode);
        }
        private async Task EnqueueAsync(PickedMediaFile file, UploadMode uploadMode)
        {
            file.Cancellation = new();

            await _semaphoreSlim.WaitAsync(file.Cancellation.Token);

            try
            {
                file.UploadState = UploadState.Uploading;

                var uploadStream = new UploadStream
                {
                    ContentType = file.ContentType,
                    FileName = file.FileName,
                    OpenStream = file.OpenReadStreamAsync,
                };

                if (uploadMode == UploadMode.Temporary)
                {
                    var result = await _mediaUploadService.UploadTempAsync(uploadStream, file.Cancellation.Token);

                    if (result is null)
                    {
                        file.UploadState = UploadState.Failed;
                        NotifyChanged(file);
                        return;
                    }
                    file.TempUploadResult = result;
                    file.MediaId = result.TempMediaId;
                }
                else
                {
                    var result = await _mediaUploadService.UploadMediaAsync(uploadStream, file.PropertyNoteId, file.Cancellation.Token);

                    if (result is null)
                    {
                        file.UploadState = UploadState.Failed;
                        NotifyChanged(file);
                        return;
                    }
                    file.MediaSource = MediaSource.Server;
                    file.RemoteUrl = result.MediaUrl;
                    file.MediaId = result.MediaId;

                    if (result.IsPreview)
                        file.isNotePreview = true;
                }


                file.UploadState = UploadState.Uploaded;
                NotifyChanged(file);
            }

            catch (OperationCanceledException)
            {
                file.UploadState = UploadState.Pending;
            }

            catch
            {
                file.UploadState = UploadState.Failed;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
         
        }
        
    }
}
