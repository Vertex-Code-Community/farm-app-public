using Foundation;
using PhotosUI;

namespace FarmApp.Mobile;

public class PickerDelegate : PHPickerViewControllerDelegate
{
    private readonly PHPickerViewController _picker;
    private readonly TaskCompletionSource<IReadOnlyCollection<FileResult>> _tcs;

    public PickerDelegate(PHPickerViewController picker, TaskCompletionSource<IReadOnlyCollection<FileResult>> taskCompletionSource)
    {
        _picker = picker;
        _tcs = taskCompletionSource;
    }

    public override async void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
    {
        picker.DismissViewController(true, null);
        var files = new List<FileResult>();

        foreach (var file in results)
        {
            var provider = file.ItemProvider;

            if (provider.HasItemConformingTo("public.image"))
            {
                var newFile = await Load(provider, "public.image");
                if (newFile != null) files.Add(newFile);
            }
            else if (provider.HasItemConformingTo("public.movie"))
            {
                var newFile = await Load(provider, "public.movie");
                if (newFile != null) files.Add(newFile);
            }
        }
        _tcs.SetResult(files);
    }

    async Task<FileResult?> Load(NSItemProvider provider, string type)
    {
        var tcs = new TaskCompletionSource<FileResult?>();

        provider.LoadFileRepresentation(type, (url, error) =>
        {
            if (url == null)
            {
                tcs.SetResult(null);
                return;
            }

            var dest = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName((url.Path)));
            
            File.Copy(url.Path, dest, true);
            tcs.SetResult(new FileResult(dest));
        });
        return await tcs.Task;
    }
}