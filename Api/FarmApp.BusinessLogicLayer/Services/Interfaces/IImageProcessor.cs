namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IImageProcessor
    {
        Task CreateThumbnailAsync(Stream input, Stream output, int size);
    }
}
