using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Media;

namespace FarmApp.Mobile.Services
{
    public class MobileMediaPickerService : IMediaPickerService
    {
        private readonly IMediaCaptureService _mediaCaptureService;
        private readonly IGalleryPickerService _galleryPickerService;
        public MobileMediaPickerService(IMediaCaptureService mediaCaptureService,
            IGalleryPickerService galleryPickerService)
        {
            _mediaCaptureService = mediaCaptureService;
            _galleryPickerService = galleryPickerService;
        }
/*        public async Task<IReadOnlyCollection<PickedMediaFile>> PickPhotoAsync(bool multiple)
        {
            if (!multiple)
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                return photo is null ? Array.Empty<PickedMediaFile>()
                    : await Map([photo]);

            }
            return await PickMultiple(FilePickerFileType.Images);
        }

        public async Task<IReadOnlyCollection<PickedMediaFile>> PickVideoAsync(bool multiple)
        {
            if (!multiple)
            {
                var video = await MediaPicker.Default.PickVideoAsync();
                return video is null ? Array.Empty<PickedMediaFile>()
                    : await Map([video]);
            }
            return await PickMultiple(FilePickerFileType.Videos);
        }*/

/*        private static async Task<IReadOnlyCollection<PickedMediaFile>> PickMultiple(FilePickerFileType type)
        {
            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                FileTypes = type
            });
            return await Map(results);
        }*/

        public async Task<PickedMediaFile?> CapturePhotoOrVideoAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                    return null;
            }

            var capture = await _mediaCaptureService.CapturePhotoOrVideoAsync();
            return capture is null ? null
                    : await Map(capture);

        }

        public async Task<IReadOnlyCollection<PickedMediaFile>> PickMediaAsync()
        {
            var mediaFiles = await _galleryPickerService.PickAsync();
            var mapped = await Map(mediaFiles);
            return mapped;
        }

        private async Task<PickedMediaFile> Map(FileResult file)
        {
            var mapped = await Map([file]);
            return mapped.First();

        }

        private async Task<IReadOnlyCollection<PickedMediaFile>> Map(IEnumerable<FileResult> files)
        {
            var list = new List<PickedMediaFile>();
            if (files is null)
                return list;


            foreach (var file in files)
            {
                var originalFileName = file.FileName;
                var contentType = file.ContentType;

                if (string.IsNullOrEmpty(contentType) || contentType == "application/octet-stream")
                {
                    contentType = MediaTypeResolver.GetMimeTypeFromExtension(originalFileName);
                }

                var extension = Path.GetExtension(originalFileName);

                var isValidExtension =
                    !string.IsNullOrWhiteSpace(extension) &&
                    extension.Length > 1; 

                if (!isValidExtension)
                {
                    extension = MediaTypeResolver.GetExtensionFromMime(contentType);
                }

                var fileName = isValidExtension
                    ? originalFileName
                    : $"{Guid.NewGuid()}{extension}";

                list.Add(new PickedMediaFile
                {
                    ContentType = contentType,
                    FileName = fileName,
                    MediaSource = MediaSource.Local,
                    OpenReadStreamAsync = () => file.OpenReadAsync()
                });

            }

            return list;
        }
    }
}