using System.Diagnostics;
using FarmApp.Mobile.Services.Interfaces;
using Photos;
using PhotosUI;
using UIKit;

namespace FarmApp.Mobile;

public class iOSGalleryPickerService : IGalleryPickerService
{
    private readonly IPhotoPermissionService _permissionService;
    public iOSGalleryPickerService(IPhotoPermissionService permissionService)
    {
        _permissionService = permissionService;
    }
    public async Task<IReadOnlyCollection<FileResult>> PickAsync()
    {
        var access = await _permissionService.EnsureAccessAsync();

        if (access == ViewModels.Media.PhotoAccessResult.Denied)
            return Array.Empty<FileResult>();

        var tcs = new TaskCompletionSource<IReadOnlyCollection<FileResult>>();

        var config = new PHPickerConfiguration(PHPhotoLibrary.SharedPhotoLibrary)
        {
            SelectionLimit = 10, // 0 == unlimited
        };
        config.Filter = null;
        
        var picker = new PHPickerViewController(config);
        picker.Delegate = new PickerDelegate(picker, tcs);
        Platform.GetCurrentUIViewController()
            .PresentViewController(picker,true,null);
        
        return await tcs.Task;
    }
}