#if ANDROID
using FarmApp.Components.Components.MediaChoiceDialog;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Models.DialogModels;
using FarmApp.ViewModels.Media;
namespace FarmApp.Mobile.Platforms.Android;

public class MediaCaptureService : IMediaCaptureService
{
    private readonly IDialogService _dialogservice;

    public MediaCaptureService(IDialogService dialogService)
    {
        _dialogservice = dialogService;
    }

    public async Task<FileResult?> CapturePhotoOrVideoAsync()
    {
        var mediaChoice =
            await _dialogservice.RequestAsync<MediaChoice?, MediaChoiceDialogComponent>(
                new DialogParametersModel());

        if (mediaChoice == null)
            return null;

        try
        {
            return mediaChoice == MediaChoice.Photo
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await MediaPicker.Default.CaptureVideoAsync();
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
#endif