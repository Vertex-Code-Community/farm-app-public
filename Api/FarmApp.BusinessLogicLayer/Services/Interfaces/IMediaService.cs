using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Http;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IMediaService
    {
        Task<TempUploadResult> SaveTempAsync(IFormFile file);
        Task<UploadMediaResult> SavePermanentMediaAsync(IFormFile file, string propertyNoteId);
        Task<List<CommitResult>> CommitAsync(IEnumerable<TempUploadResult> temps, string PropertyNoteId);
        Task<string> GetMediaPathById(string mediaId, bool thumbnail);
        Task<List<UploadedMediaFile>> GetMediaByPropertyNoteId(string propertyNoteId);
        Task<DeleteMediaResult> DeleteMedia(string propertyNoteId,string mediaId);
    }
}
