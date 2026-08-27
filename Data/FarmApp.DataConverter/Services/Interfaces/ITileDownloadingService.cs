using FarmApp.ViewModels.Options;

namespace FarmApp.DataConverter.Services.Interfaces;

public interface ITileDownloadingService
{
    Task DownloadTilesForRegionAsync(TileGenerationBoundariesOptions options, int zoom);
}