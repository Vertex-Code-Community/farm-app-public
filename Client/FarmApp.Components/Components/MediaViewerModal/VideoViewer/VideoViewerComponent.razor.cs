using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.MediaViewerModal.VideoViewer
{
    public partial class VideoViewerComponent : IAsyncDisposable
    {
        [Inject] IJSRuntime JS { get; set; } = default!;
        [Parameter] public string Src { get; set; } = default!;

        private ElementReference _video;

        private bool _isLoading = true;

        private bool IsPlaying;
        private double Duration;
        private double CurrentTime;
        private double Volume = 1;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Duration = await JS.InvokeAsync<double>(
                    "videoViewer.getDuration", _video);

                await JS.InvokeVoidAsync(
                    "videoViewer.setVolume", _video, Volume);
            }
        }
        private void OnMetadataLoaded()
        {
            _isLoading = false;
        }
        private async Task TogglePlay()
        {
            if (IsPlaying)
                await JS.InvokeVoidAsync("videoViewer.pause", _video);
            else
                await JS.InvokeVoidAsync("videoViewer.play", _video);

            IsPlaying = !IsPlaying;
        }

        private async Task OnSeek(ChangeEventArgs e)
        {
            CurrentTime = Convert.ToDouble(e.Value);
            await JS.InvokeVoidAsync(
                "videoViewer.seek", _video, CurrentTime);
        }

        private async Task OnVolumeChanged(ChangeEventArgs e)
        {
            Volume = Convert.ToDouble(e.Value);
            await JS.InvokeVoidAsync(
                "videoViewer.setVolume", _video, Volume);
        }

        public async ValueTask DisposeAsync()
        {
            await JS.InvokeVoidAsync("videoViewer.stopVideo", _video);
        }
    }
}
