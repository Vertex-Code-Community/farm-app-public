namespace FarmApp.DataConverter.Services.Interfaces;

public interface ITileGenerationService
{
    Task GenerateTilesAsync(int zoom);
}