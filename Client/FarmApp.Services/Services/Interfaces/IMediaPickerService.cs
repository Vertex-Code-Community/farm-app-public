using FarmApp.ViewModels.Media;
namespace FarmApp.Services.Services.Interfaces
{
    public interface IMediaPickerService
    {
/*        Task<IReadOnlyCollection<PickedMediaFile>> PickPhotoAsync(bool multiple);
        Task<IReadOnlyCollection<PickedMediaFile>> PickVideoAsync(bool multiple);*/
        Task<PickedMediaFile?> CapturePhotoOrVideoAsync();
        Task<IReadOnlyCollection<PickedMediaFile>> PickMediaAsync();
    }
}
