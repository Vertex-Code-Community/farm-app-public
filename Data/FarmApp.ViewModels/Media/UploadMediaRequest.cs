using Microsoft.AspNetCore.Http;

namespace FarmApp.ViewModels.Media
{
    public class UploadMediaRequest
    {
        public IFormFile File { get; set; } = default!;
    }
}
