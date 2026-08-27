namespace FarmApp.ViewModels.Media
{
    public class UploadMediaResult
    {
        public string MediaId { get; set; }
        public string MediaUrl { get; set; }
        public string? MediaThumbnailUrl { get; set; }
        public bool IsPreview { get; set; }
    }
}
