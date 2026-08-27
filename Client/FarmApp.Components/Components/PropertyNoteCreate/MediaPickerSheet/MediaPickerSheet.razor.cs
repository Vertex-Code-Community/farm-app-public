using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;


namespace FarmApp.Components.Components.PropertyNoteCreate.MediaPickerSheet;

public partial class MediaPickerSheet
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public EventCallback<List<PickedMediaFile>> OnFilesPicked { get; set; }
    [Inject] public required IMediaPickerService MediaPickerService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private ElementReference _sheetRef;
    private List<PickedMediaFile> mediaFiles = new();

    private async Task OnPhotoOrVideoCaptureAsync()
    {
        await IsOpenChanged.InvokeAsync(false);
        var pickedMediaFile = await MediaPickerService.CapturePhotoOrVideoAsync();
        if (pickedMediaFile is null)
            return;
        await OnFilesPicked.InvokeAsync(new List<PickedMediaFile> { pickedMediaFile});
    }

/*    private async Task OnPhotoSelectAsync()
    {
        await CloseAsync();
        var pickedMediaFiles = await MediaPickerService.PickPhotoAsync(true);

        if (pickedMediaFiles is null || !pickedMediaFiles.Any())
            return;
        mediaFiles.AddRange(pickedMediaFiles);
        await OnFilesPicked.InvokeAsync(mediaFiles);
       
    }

    private async Task OnVideoSelectAsync()
    {
        var pickedMediaFiles = await MediaPickerService.PickVideoAsync(true);

        if (pickedMediaFiles is null || !pickedMediaFiles.Any())
            return;
        mediaFiles.AddRange(pickedMediaFiles);
        await OnFilesPicked.InvokeAsync(mediaFiles);
       // await CloseAsync();
    }*/

    private async Task OnMediaSelectAsync()
    {
        await IsOpenChanged.InvokeAsync(false);
        var pickedMediaFiles = await MediaPickerService.PickMediaAsync();

        if (pickedMediaFiles is null || !pickedMediaFiles.Any())
            return;
        await OnFilesPicked.InvokeAsync(pickedMediaFiles.ToList());
    }

/*    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //if (firstRender)
           // await JSRuntime.InvokeVoidAsync("bottomSheetCalendar.init", _sheetRef);
    }*/

}
