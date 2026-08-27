using AVFoundation;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Photos;
using PhotosUI;
using UIKit;

namespace FarmApp.Mobile.Platforms.iOS;

public class iOSPermissionService : IMobilePermissionService
{
    public Task<bool> EnsureCameraAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EnsureMicrophoneAsync()
    {
        if (OperatingSystem.IsIOSVersionAtLeast(17))
        {
            var status = AVAudioApplication.SharedInstance.RecordPermission;

            if (status == AVAudioApplicationRecordPermission.Undetermined)
            {
                var tcs = new TaskCompletionSource<bool>();

                AVAudioApplication.RequestRecordPermission(response =>
                {
                    tcs.TrySetResult(response);
                });

                return await tcs.Task;
            }

            if (status == AVAudioApplicationRecordPermission.Denied)
            {
                await ShowOpenSettingsAsync(
                    "Microphone Access Required",
                    "Please allow access to microphone in Settings.");

                return false;
            }

            return true;
        }
        else
        {
            var status = AVAudioSession.SharedInstance().RecordPermission;

            if (status == AVAudioSessionRecordPermission.Undetermined)
            {
                var tcs = new TaskCompletionSource<bool>();

                AVAudioSession.SharedInstance()
                    .RequestRecordPermission(response =>
                    {
                        tcs.TrySetResult(response);
                    });


                return await tcs.Task;
            }

            if (status == AVAudioSessionRecordPermission.Denied)
            {
                await ShowOpenSettingsAsync(
                    "Microphone Access Required",
                    "Please allow access to microphone in Settings.");

                return false;
            }

            return true;
        }
    }

    public async Task<PhotoAccessResult> EnsurePhotoLibraryAsync()
    {
        var status = PHPhotoLibrary.GetAuthorizationStatus(PHAccessLevel.ReadWrite);

        if (status == PHAuthorizationStatus.NotDetermined)
        {
            status = await PHPhotoLibrary.RequestAuthorizationAsync(PHAccessLevel.ReadWrite);
        }

        if (status == PHAuthorizationStatus.Restricted ||
            status == PHAuthorizationStatus.Denied)
        {
            await ShowOpenSettingsAsync("Photo Access Required",
                "Please allow access to your photo library in settings");
            return PhotoAccessResult.Denied;
        }

        if (status == PHAuthorizationStatus.Limited)
        {
            var expand = await ShowConfirmAsync("Limited Photo Access",
                "You have granted access to select photos only. Add more photos?");

            if (expand)
            {
                PHPhotoLibrary.SharedPhotoLibrary.PresentLimitedLibraryPicker(Platform.GetCurrentUIViewController());
            }
            return PhotoAccessResult.Limited;
        }

        return PhotoAccessResult.Authorized;
    }
    private Task ShowOpenSettingsAsync(string title, string message)
    => RunOnMainThreadAsync(() =>
    {
        var alert = UIAlertController.Create(
            title,
            message,
            UIAlertControllerStyle.Alert);

        alert.AddAction(UIAlertAction.Create(
            "Cancel",
            UIAlertActionStyle.Cancel,
            null));

        alert.AddAction(UIAlertAction.Create(
            "Open Settings",
            UIAlertActionStyle.Default,
            _ =>
            {
                UIApplication.SharedApplication.OpenUrl(
                    new Foundation.NSUrl(
                        UIApplication.OpenSettingsUrlString));
            }));

        Platform.GetCurrentUIViewController()?
            .PresentViewController(alert, true, null);
    });

    private Task<bool> ShowConfirmAsync(string title, string message)
    {
        var tcs = new TaskCompletionSource<bool>();

        RunOnMainThreadAsync(() =>
        {
            var alert = UIAlertController.Create(
                title,
                message,
                UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create(
                "Continue",
                UIAlertActionStyle.Cancel,
                _ => tcs.TrySetResult(false)));

            alert.AddAction(UIAlertAction.Create(
                "Add",
                UIAlertActionStyle.Default,
                _ => tcs.TrySetResult(true)));

            Platform.GetCurrentUIViewController()?
                .PresentViewController(alert, true, null);
        });

        return tcs.Task;
    }
    private static Task RunOnMainThreadAsync(Action action)
    {
        var tcs = new TaskCompletionSource();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }
}
