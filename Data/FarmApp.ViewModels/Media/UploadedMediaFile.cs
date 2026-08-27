using FarmApp.Shared.Enums;

namespace FarmApp.ViewModels.Media
{
    public class UploadedMediaFile
    {
        public string Id { get; set; }
        public string PropertyNoteid { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public MediaType MediaType { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public long ExpiresAt { get; set; }
    }
}
