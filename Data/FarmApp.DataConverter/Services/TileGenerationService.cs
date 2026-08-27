using FarmApp.DataConverter.DbContext;
using FarmApp.DataConverter.Helpers;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Options;
using Microsoft.Extensions.Options;

namespace FarmApp.DataConverter.Services;

public class TileGenerationService : ITileGenerationService
{
    private readonly DataConverterDbContext _dataConverterDbContext;
    private readonly TileGenerationBoundariesOptions _boundaries;
    
    public TileGenerationService(DataConverterDbContext dataConverterDbContext,
        IOptions<TileGenerationBoundariesOptions> boundariesOptions)
    {
        _dataConverterDbContext = dataConverterDbContext;
        _boundaries = boundariesOptions.Value;
    }
    
    public async Task GenerateTilesAsync(int zoom)
    {
        ConverterMath.ConvertToTileCoords(zoom, _boundaries.UpperLeftLongitude, _boundaries.UpperLeftLatitude, 
            out var upperLeftTileXd, out var upperLeftTileYd);
        
        ConverterMath.ConvertToTileCoords(zoom, _boundaries.LowerRightLongitude, _boundaries.LowerRightLatitude, 
            out var lowerRightTileXd, out var lowerRightTileYd);

        var upperLeftTileX = (int)upperLeftTileXd;
        var upperLeftTileY = (int)upperLeftTileYd;
        
        var lowerRightTileX = (int)lowerRightTileXd;
        var lowerRightTileY = (int)lowerRightTileYd;

        var x = lowerRightTileX - upperLeftTileX;
        var y = lowerRightTileY - upperLeftTileY;

        var num = x * y;

        var counter = 0;

        for (var lng = upperLeftTileX; lng <= lowerRightTileX; lng++)
        {
            for (var lat = upperLeftTileY; lat <= lowerRightTileY; lat++)
            {
                var tile = new TileEntity
                {
                    Id = $"{lng}_{lat}_{zoom}",
                    X = lng,
                    Y = lat,
                    Z = zoom
                };

                _dataConverterDbContext.Tiles.Add(tile);
            }

            if (counter % 100 == 0)
            {
                Console.WriteLine($"lng = {lng}");
                await _dataConverterDbContext.SaveChangesAsync();
            }

            counter++;
        }

        Console.WriteLine($"Before Saving");
        await _dataConverterDbContext.SaveChangesAsync();
        Console.WriteLine($"After Saving");
    }
}