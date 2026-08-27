using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using FarmApp.DataConverter.DbContext;
using FarmApp.DataConverter.Helpers;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.MVTParser;
using FarmApp.ViewModels.Options;

namespace FarmApp.DataConverter.Services;

public class FileGenerationService : IFileGenerationService
{
    private readonly DataConverterDbContext _dataConverterDbContext;
    private readonly TileMvtFileOptions _tileMvtFileOptions;

    private const int PageSize = 200;
    // private ulong _featureIdCounter = 1; // make atomic if multithreading is used

    private long _failCount = 0;
    private long _writeCount = 0;
    
    public FileGenerationService(DataConverterDbContext dataConverterDbContext,
        IOptions<TileMvtFileOptions> tileMvtFileOptions)
    {
        _dataConverterDbContext = dataConverterDbContext;
        _tileMvtFileOptions = tileMvtFileOptions.Value;
    }

    public async Task GenerateMvtFilesAsync(int zoom, int zoomIndex)
    {
        if (zoom < 1) return;
        
        _failCount = 0;
        _writeCount = 0;
        
        var totalRecords = await _dataConverterDbContext.Tiles
            .AsNoTracking()
            .Where(x => x.Z == zoom).CountAsync(); // Get the total count
        
        var totalFiles = await _dataConverterDbContext.Tiles
            // .Include(x => x.Polygons)
            .Where(x => x.Z == zoom).CountAsync(); // Get the total count
            // .Where(x => x.Z == zoom && x.Polygons.Count > 0).CountAsync(); // Get the total count
        
        var totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
        
        Console.WriteLine($"Zoom = {zoom}");
        Console.WriteLine($"Total files = {totalFiles}");
        Console.WriteLine($"Total tiles = {totalRecords}");
        Console.WriteLine($"Total pages = {totalPages}");
        
        for (var pageNumber = 1; pageNumber <= totalPages; pageNumber++)
        {
            var tilesChunk = await _dataConverterDbContext.Tiles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Include(x => x.Polygons)
                .Where(x => x.Z == zoom)
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        
            var taskList = new List<Task>();
            
            foreach (var tile in tilesChunk)
            {
                if (tile.Polygons.Count == 0) continue;
        
                taskList.Add(GenerateTileFileAsync(tile, zoomIndex));
            }
        
            await Task.WhenAll(taskList);
            taskList.Clear();
            GC.Collect();
        
            if ((pageNumber - 1) % PageSize == 0)
            {
                Console.WriteLine($"ThreadPool.ThreadCount {ThreadPool.ThreadCount}");
                Console.WriteLine($"pageNumber = {pageNumber - 1}");
            }
        }
        
        Console.WriteLine($"Fail count = {_failCount}");
        Console.WriteLine($"Write count = {_writeCount}");
    }

    private async Task GenerateTileFileAsync(TileEntity tile, int zoomIndex)
    {
        var encoder = new VectorTileEncoder();

        foreach (var polygonEntity in tile.Polygons)
        {
            try
            {
                var wktReader = new WKTReader();
                if (wktReader.Read(polygonEntity.WktCoordinates[zoomIndex]) is not MultiPolygon multiPolygon) continue;

                var skipPolygon = false;
            
                for (var i = 0; i < multiPolygon.Geometries.Length; i ++) // var polygon in multiPolygon.Geometries
                {
                    var geometry = multiPolygon.Geometries[i];

                    if (geometry is Polygon { IsValid: false } polygon)
                    {
                        var newPolygon = ConverterMath.FixPolygonSelfIntersectionsInLineString(polygon);
                        if (newPolygon is null)
                        {
                            Interlocked.Increment(ref _failCount);
                            skipPolygon = true;
                            break;
                        }
                        
                        multiPolygon.Geometries[i] = newPolygon;
                        geometry = multiPolygon.Geometries[i];
                    }

                    ConvertMultiPolygon(geometry.Coordinates, tile);
                }

                if (skipPolygon) continue;
            
                var attributes = new Dictionary<string, object>
                {
                    { "steadId", polygonEntity.SteadId }
                };
            
                // var featureId = Interlocked.Increment(ref _featureIdCounter);
                encoder.AddFeature(_tileMvtFileOptions.MapboxLayerName, attributes, multiPolygon, (long) ulong.Parse(polygonEntity.SteadId));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        var mvtFileData = encoder.Encode();

        var filePath = $"{_tileMvtFileOptions.BaseFilePath}/{tile.Z}/tile_{tile.Z}_{tile.X}_{tile.Y}.mvt";
        await File.WriteAllBytesAsync(filePath, mvtFileData);
        
        Interlocked.Increment(ref _writeCount);
    }
    
    private static void ConvertMultiPolygon(Coordinate[] coordinates, TileEntity tile)
    {
        foreach (var coordinate in coordinates)
        {
            var dX = coordinate.X - tile.X;
            var dY = coordinate.Y - tile.Y;

            var x = dX * 256;
            var y = dY * 256;

            coordinate.X = x;
            coordinate.Y = y;
        }
    }
}