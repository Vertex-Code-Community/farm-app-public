namespace FarmApp.ViewModels.Media
{
    public class TempUploadResult
    {
        public string TempMediaId { get; set; } = string.Empty;
        public string TempExtension { get;set; } = string.Empty;
        public bool HasThumbnail { get; set; }
    }
}
