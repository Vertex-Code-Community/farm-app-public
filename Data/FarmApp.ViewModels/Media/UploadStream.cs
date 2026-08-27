namespace FarmApp.ViewModels.Media
{
    public class UploadStream
    {
        public Func<Task<Stream>> OpenStream { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = "application/octec-stream";
    }
}
