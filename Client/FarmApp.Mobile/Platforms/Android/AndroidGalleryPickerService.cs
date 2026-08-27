using Android.App;
using Android.Content;
using Android.Provider;
using Android.Webkit;
using FarmApp.Mobile.Services.Interfaces;
using Uri = Android.Net.Uri;
namespace FarmApp.Mobile.Platforms.Android;

public class AndroidGalleryPickerService : IGalleryPickerService
{
    const int PickRequestCode = 999;
    private TaskCompletionSource<IReadOnlyCollection<FileResult>>? _tcs;
    public Task<IReadOnlyCollection<FileResult>> PickAsync()
    {
        _tcs = new TaskCompletionSource<IReadOnlyCollection<FileResult>>();

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
            LaunchPhotoPicker();
        else 
            LaunchOldPhotoPicker();

        return _tcs.Task;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("android33.0")]
    private void LaunchPhotoPicker()
    {
        var intent = new Intent(MediaStore.ActionPickImages);
        intent.PutExtra(MediaStore.ExtraPickImagesMax, 10);

        Platform.CurrentActivity!.StartActivityForResult(intent, PickRequestCode);
    }


    private void LaunchOldPhotoPicker()
    {
        var intent = new Intent(Intent.ActionOpenDocument);

        intent.AddCategory(Intent.CategoryOpenable);

        intent.PutExtra(Intent.ExtraAllowMultiple, true);
        intent.SetType("*/*");
        intent.PutExtra(Intent.ExtraMimeTypes, new[]
        {
            "image/*",
            "video/*"
        });

        Platform.CurrentActivity!
            .StartActivityForResult(intent, PickRequestCode);

    }
    public void HandleActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != PickRequestCode || _tcs == null)
            return;

        if (resultCode != Result.Ok || data == null)
        {
            _tcs.SetResult(Array.Empty<FileResult>());
            return;
        }

        var files = new List<FileResult>();

        if (data.ClipData != null)
        {
            for (int i = 0; i < data.ClipData.ItemCount; i++)
            {
                var uri = data.ClipData.GetItemAt(i).Uri;
                files.Add(CopyToCache(uri));
            }
        }
        else if (data.Data != null)
        {
            files.Add(CopyToCache(data.Data));
        }

        _tcs.SetResult(files);
    }

    FileResult CopyToCache(Uri uri)
    {
        var resolver = Platform.CurrentActivity!.ContentResolver!;

        var mime = resolver.GetType(uri)
                   ?? "application/octet-stream";

        var extension = MimeTypeMap
            .Singleton?
            .GetExtensionFromMimeType(mime);

        var fileName = $"{Guid.NewGuid()}.{extension}";
        var dest = Path.Combine(FileSystem.CacheDirectory, fileName);

        using var input = resolver.OpenInputStream(uri)!;
        using var output = File.OpenWrite(dest);
        input.CopyTo(output);

        return new FileResult(dest, mime);
    }
}


