using FarmApp.Components.Components.Dialogs.Base;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.MediaViewerModal
{
    public partial class MediaViewerModal : ComponentBase, IBaseDialogComponent
    {
        [Inject] public required IMediaService MediaService { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnSubmit { get; set; } 
        [Parameter] public object? Payload { get; set; }

        private MediaViewerPayload _payload = default!;
        private string? _src;
        private bool isVideo => _payload.ContentType.StartsWith("video/");

        protected override async Task OnParametersSetAsync()
        {
            if (Payload is not MediaViewerPayload payload)
                throw new InvalidOperationException("MediaViewerModal requires MediaViewerPayload");

            _payload = payload;

            var url = await MediaService.GetSignedMediaUrlAsync(_payload.MediaId);

            _src = MediaService.GetApiBaseForUrl(url?.Url!);
        }

    }
}
