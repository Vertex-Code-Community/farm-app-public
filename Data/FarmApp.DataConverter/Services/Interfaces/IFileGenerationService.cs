namespace FarmApp.DataConverter.Services.Interfaces;

public interface IFileGenerationService
{
    Task GenerateMvtFilesAsync(int zoom, int zoomIndex);
}