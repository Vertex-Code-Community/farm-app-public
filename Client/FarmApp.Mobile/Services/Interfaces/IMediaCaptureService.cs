namespace FarmApp.Mobile.Services.Interfaces
{
    public interface IMediaCaptureService
    {
        Task<FileResult?> CapturePhotoOrVideoAsync();
    }
}
