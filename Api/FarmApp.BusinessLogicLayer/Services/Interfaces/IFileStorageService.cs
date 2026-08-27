using FarmApp.ViewModels.Media;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IFileStorageService
    {

        Task<TempUploadResult> SaveTempAsync(
            Stream fileStream,
            Stream thumbnailStream,
            string fileName,
            CancellationToken ct = default);
        Task<CommitResult> SaveMediaPermanentlyAsync(
            Stream fileStream, 
            Stream thumbnailStream, 
            string propertyNoteId,
            string fileName, 
            CancellationToken ct = default);

        Task<List<CommitResult>> CommitAsync(IEnumerable<TempUploadResult> temps, string propertyNoteId);

        Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default);

        Task<string?> GetMediaPathById (string mediaId, string relativePath, bool thumbnail = false);
        Task CleanTempFiles();
    }
}
