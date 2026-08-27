using FarmApp.Components.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FarmApp.Services.Models.DialogModels;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Components.Components.ConfirmationDialog;
namespace FarmApp.Components.Components.DisplayMedia.Media
{
    public partial class MediaComponent
    {
        [Inject] public required IJSRuntime JSRuntime { get; set; }
        [Inject] public required IDialogService DialogService { get; set; }
        [Inject] public required IMediaService MediaService { get; set; }
        [Parameter,EditorRequired] public PickedMediaFile File { get; set; }
        [Parameter] public EventCallback OpenMediaVeiwer { get; set; }
        [Parameter] public string ImageFit { get; set; } = "cover";
        [Parameter] public bool isEditMode { get; set; }
        [Parameter] public bool isViewMode { get; set; } = false;
        [Parameter] public EventCallback<PickedMediaFile> RemoveFile { get; set; }
        [Parameter] public EventCallback<PickedMediaFile> RetryUploadFile { get; set; }
        private string _previewUrl = string.Empty;
        private bool _isVideo = false;
        private bool IsLoading => File.UploadState == UploadState.Pending ||
                                  File.UploadState == UploadState.Uploading;

        protected override async Task OnParametersSetAsync()
        {
            if (IsLoading || !string.IsNullOrEmpty(_previewUrl))
                return;

            if (File.IsRemote)
            {
                if (!isViewMode)
                {
                    _previewUrl = MediaService.GetThumbnailUrl(File.MediaId!);
                }
                else
                {
                    _previewUrl = MediaService.GetApiBaseForUrl(File.RemoteUrl!);
                }
                return;
            }

            if (File.UploadState == UploadState.Uploaded)
            {
                if (File.MediaId == null)
                {
                    File.UploadState = UploadState.Failed;
                    return;
                }
                SignedUrlResult? signedMediaUrl;

                bool videoContentType = File.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
                _isVideo = videoContentType;

                if (!isViewMode)
                {
                    _previewUrl = MediaService.GetThumbnailUrl(File.MediaId);
                    return;
                }

                signedMediaUrl = await MediaService.GetSignedMediaUrlAsync(File.MediaId);
                
                if (signedMediaUrl == null)
                {
                    File.UploadState = UploadState.Failed;
                    return;
                }
                _previewUrl = MediaService.GetApiBaseForUrl(signedMediaUrl.Url);
            }
        }
        private async Task OnDeleteMedia()
        {
            var result = await DialogService.RequestAsync<bool?, ConfirmationComponent>(new DialogParametersModel
            {
                Payload = new Dictionary<string, string>
                {
                    ["body"] = "Media_Remove" 
                }
            });
            if (result ==  true)
            {
                await RemoveFile.InvokeAsync(File);
            }
        }
        private async Task OnRetryUpload()
        {
            await RetryUploadFile.InvokeAsync(File);
        }

        private async Task OpenViewer()
        {
            if (File.UploadState == UploadState.Failed)
                return;
            await OpenMediaVeiwer.InvokeAsync();
        }

    }
}
