using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmApp.Components.Components.VideoPlayer
{
    public partial class VideoPlayerComponent
    {
        [Inject] public required IJSRuntime JsRuntime { get; set; }
        [Parameter] public required string VideoSrc { get; set; }

        private IJSObjectReference? _module;
        private DotNetObjectReference<VideoPlayerComponent>? _objRef;

        private ElementReference _videoPlayer;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (firstRender)
            {
                _objRef = DotNetObjectReference.Create(this);
                _module = await JsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/FarmApp.Components/Components/VideoPlayer/VideoPlayerComponent.razor.js");

                await _module.InvokeVoidAsync("initializePlayer", _videoPlayer, _objRef);
            }
        }
    }
}
