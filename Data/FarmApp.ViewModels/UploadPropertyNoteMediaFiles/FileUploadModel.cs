using Microsoft.AspNetCore.Components.Forms;
namespace FarmApp.ViewModels.UploadPropertyNoteMediaFiles
{
    public class FileUploadModel
    {
        public string PropertyId { get; set; }
        public string PropertyNoteId { get; set; }
        public required IBrowserFile MyProperty { get; set; }
    }
}
