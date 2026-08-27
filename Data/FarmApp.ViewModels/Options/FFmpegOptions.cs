namespace FarmApp.ViewModels.Options
{
    public class FFmpegOptions
    {
        public string BinaryPath { get; set; } = "ffmpeg";
        public int TimeoutSeconds { get; set; } = 30;
    }
}
