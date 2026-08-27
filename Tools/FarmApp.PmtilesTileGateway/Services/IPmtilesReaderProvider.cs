using PMTiles;

namespace FarmApp.PmtilesTileGateway.Services;

public interface IPmtilesReaderProvider
{
    PMTilesReader? Reader { get; }

    string? InitError { get; }

    void EnsureInitialized();
}
