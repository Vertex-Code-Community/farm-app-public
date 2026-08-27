using FarmApp.ViewModels.Media;

namespace FarmApp.Mobile.Services.Interfaces
{
    public interface IMobilePermissionService
    {
        Task<PhotoAccessResult> EnsurePhotoLibraryAsync();
        Task<bool> EnsureMicrophoneAsync();
        Task<bool> EnsureCameraAsync();
    }
}
