using FarmApp.Shared.Enums;
using Microsoft.AspNetCore.Http;

namespace FarmApp.Shared.Helpers
{
    public static class MediaTypeResolver
    {
        public static MediaType ResolveMediaType(IFormFile file)
        {
            if (file == null)
                return MediaType.Unknown;

            if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return MediaType.Photo;

            if (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                return MediaType.Video;

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".webp" or ".heic" => MediaType.Photo,
                ".mp4" or ".mov" or ".avi" or ".mkv" => MediaType.Video,
                _ => MediaType.Unknown
            };
        }

        public static MediaType ResolveFromExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".webp" => MediaType.Photo,
                ".mp4" or ".mov" or ".avi" => MediaType.Video,
                _ => MediaType.Unknown
            };
        }
        public static string GetMimeTypeFromExtension(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".heic" => "image/heic",

                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                ".mkv" => "video/x-matroska",

                _ => "application/octet-stream"
            };
        }
        public static string GetExtensionFromMime(string mime) => mime switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "video/mp4" => ".mp4",
            "video/quicktime" => ".mov",
            _ => ""
        };
    }
}
