using FarmApp.ViewModels.Media;

namespace FarmApp.Mobile.Services.Interfaces
{
    public interface IPhotoPermissionService
    {
        Task<PhotoAccessResult> EnsureAccessAsync();
    }
}
