namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IVideoThumbnailGenerator
    {
        Task CreateThumbnailAsync(
            Stream input,
            Stream output,
            int size,
            string sourceFileName,
            CancellationToken ct = default);
    }
}
