using FarmApp.ViewModels.Media;

namespace FarmApp.Services.Services.Interfaces
{
    public interface IMediaService
    {
        Task<TempUploadResult?> UploadTempAsync(UploadStream file, CancellationToken cancellationToken = default);
        Task<UploadMediaResult?> UploadMediaAsync(UploadStream file, string propertyNoteId, CancellationToken cancellationToken = default);
        Task<SignedUrlResult?> GetSignedMediaUrlAsync(string mediaId, bool thumbnail = false);
        Task<IReadOnlyList<UploadedMediaFile>> GetMediaByNoteIdAsync(string propertyNoteId);
        Task<DeleteMediaResult?> DeleteUploadedMedia(string propertyNoteId,string mediaId);
        string GetThumbnailUrl(string mediaId);
        string GetApiBaseForUrl(string mediaUrl);
    }
}
