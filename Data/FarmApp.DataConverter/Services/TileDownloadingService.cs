using FarmApp.DataConverter.Helpers;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.ViewModels.Options;

namespace FarmApp.DataConverter.Services;

public class TileDownloadingService : ITileDownloadingService
{
    private readonly string _saveDirectory = "/Users/ruslandudchenko/Documents/missing_tiles/";
    
    public async Task DownloadTilesForRegionAsync(TileGenerationBoundariesOptions boundaries, int zoom)
    {
        ConverterMath.ConvertToTileCoords(zoom, boundaries.UpperLeftLongitude, boundaries.UpperLeftLatitude, 
            out var upperLeftTileXd, out var upperLeftTileYd);
        
        ConverterMath.ConvertToTileCoords(zoom, boundaries.LowerRightLongitude, boundaries.LowerRightLatitude, 
            out var lowerRightTileXd, out var lowerRightTileYd);

        var upperLeftTileX = (int)upperLeftTileXd;
        var upperLeftTileY = (int)upperLeftTileYd;
        
        var lowerRightTileX = (int)lowerRightTileXd;
        var lowerRightTileY = (int)lowerRightTileYd;

        var x = lowerRightTileX - upperLeftTileX;
        var y = lowerRightTileY - upperLeftTileY;

        var httpClient = new HttpClient { BaseAddress = new Uri("https://cdn.kadastr.live") };
        var downloadTasks = new List<Task>();

        for (var lng = upperLeftTileX; lng <= lowerRightTileX; lng++)
        {
            Console.WriteLine($"Processing lng = {lng}");
            
            for (var lat = upperLeftTileY; lat <= lowerRightTileY; lat++)
            {
                downloadTasks.Add(DownloadAndSaveTileAsync(httpClient, zoom, lng, lat));
            }
            
            Console.WriteLine($"Processed lng = {lng}");

            await Task.WhenAll(downloadTasks);
            downloadTasks.Clear();
        }
        
        Console.WriteLine($"COMPLETED");
    }

    private async Task DownloadAndSaveTileAsync(HttpClient httpClient, int zoom, int lng, int lat)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"tiles/maps/kadastr/{zoom}/{lng}/{lat}.pbf");

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) return;

            await using var stream = await response.Content.ReadAsStreamAsync();
            
            var filePath = $"{_saveDirectory}{zoom}_{lng}_{lat}.pbf";
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}