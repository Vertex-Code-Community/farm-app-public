using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.FadeInLoader
{
    public partial class FadeInLoaderComponent
    {
        [Parameter] public required RenderFragment Content { get; set; }
        [Parameter] public required RenderFragment Loader { get; set; }
        [Parameter] public string AlignItems { get; set; } = "center";
        [Parameter] public bool IsLoading { get; set; } = true;
        [Parameter] public int AnimationDuration { get; set; } = 300;
        [Parameter] public int AnimationDelay { get; set; } = 0;

        private bool _internalIsLoading = true;

        protected override void OnParametersSet()
        {
            if (AnimationDelay <= 0)
            {
                _internalIsLoading = IsLoading;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (AnimationDelay > 0 && _internalIsLoading != IsLoading)
            {
                await Task.Delay(AnimationDelay);
                _internalIsLoading = IsLoading;
                StateHasChanged();
            }
        }
    }
}
