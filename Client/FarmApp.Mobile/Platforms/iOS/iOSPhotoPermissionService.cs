using FarmApp.Mobile.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Photos;
using PhotosUI;
using UIKit;
namespace FarmApp.Mobile.Platforms.iOS
{
    public class iOSPhotoPermissionService : IPhotoPermissionService
    {
        public async Task<PhotoAccessResult> EnsureAccessAsync()
        {
            var status = PHPhotoLibrary.GetAuthorizationStatus(PHAccessLevel.ReadWrite);

            if (status == PHAuthorizationStatus.NotDetermined)
            {
                status = await PHPhotoLibrary.RequestAuthorizationAsync(PHAccessLevel.ReadWrite);
            }
            if (status == PHAuthorizationStatus.Restricted ||
                status == PHAuthorizationStatus.Denied)
            {
                await ShowOpenSettingsAsync();
                return PhotoAccessResult.Denied;
            }

            if (status == PHAuthorizationStatus.Limited)
            {
                var expand = await ShowLimitedAccessAlert();

                if (expand)
                {
                    PHPhotoLibrary.SharedPhotoLibrary.PresentLimitedLibraryPicker(Platform.GetCurrentUIViewController());
                }
                return PhotoAccessResult.Limited;
            }

            return PhotoAccessResult.Authorized; 
        }

        private Task ShowOpenSettingsAsync()
        {
            var tcs = new TaskCompletionSource();

            var alert = UIAlertController.Create("Photo Access Required",
                "Please allow access to your photo library in Settings.",
                UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel, _ => tcs.SetResult()));

            alert.AddAction(UIAlertAction.Create("Open Settings", UIAlertActionStyle.Default,
                _ =>
                {
                    UIApplication.SharedApplication.OpenUrl(new Foundation.NSUrl(UIApplication.OpenSettingsUrlString));
                    tcs.SetResult();
                }));

            Platform.GetCurrentUIViewController()?.PresentViewController(alert, true, null);

            return tcs.Task;
        }

        private Task<bool> ShowLimitedAccessAlert()
        {
            var tcs = new TaskCompletionSource<bool>();

            var alert = UIAlertController.Create("Limited Photo Access",
                "You have granted access to select photos only. Add more photos?", UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Continue", UIAlertActionStyle.Cancel,
                _ => tcs.SetResult(false)));

            alert.AddAction(UIAlertAction.Create("Add Photos", UIAlertActionStyle.Default,
                _ => tcs.SetResult(true)));

            Platform.GetCurrentUIViewController()?.PresentViewController(alert, true, null);

            return tcs.Task;
        }
    }
}
