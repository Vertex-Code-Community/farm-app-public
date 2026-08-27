using Microsoft.AspNetCore.Http;

namespace FarmApp.ViewModels.UploadPropertyNoteMediaFiles
{
    public class ApiRequestFileUploadModel : RequestFileUploadModel
    {
        public required List<IFormFile> MediaFiles { get; set; }
    }
}
