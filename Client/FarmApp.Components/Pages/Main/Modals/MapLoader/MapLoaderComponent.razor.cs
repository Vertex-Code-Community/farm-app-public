// using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Pages.Main.Modals.MapLoader;

public partial class MapLoaderComponent // : IAsyncDisposable
{
    // [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    private readonly string _id = $"_id_{Guid.NewGuid()}";
    // private readonly string _lottieAnimationPath = "_content/FarmApp.Components/lottie/tractor-1-loader.json";
    
    // protected override async Task OnAfterRenderAsync(bool firstRender)
    // {
    //     if (firstRender) await JsRuntime.InvokeVoidAsync("loadLottieAnimation", _id, 
    //         _lottieAnimationPath, 1.25f);
    // }
    //
    // public async ValueTask DisposeAsync()
    // {
    //     await JsRuntime.InvokeVoidAsync("destroyLottieAnimation", _id);
    // }
}