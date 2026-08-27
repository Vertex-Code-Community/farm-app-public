

namespace FarmApp.ViewModels.UploadPropertyNoteMediaFiles
{
    public class RequestFileUploadModel : IMultipartFormRequest
    {
        public string PropertyId { get; set; }
        public string PropertyNoteId { get; set; }

        public void AddTo(MultipartFormDataContent content)
        {
            content.Add(
                new StringContent(PropertyId),
                nameof(PropertyId)
            );

            content.Add(
                new StringContent(PropertyNoteId),
                nameof(PropertyNoteId)
            );
        }
    }
}
