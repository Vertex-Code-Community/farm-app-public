using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using FarmApp.DataConverter.DbContext;
using FarmApp.DataConverter.Helpers;
using FarmApp.DataConverter.Models;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.MVTParser;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;

namespace FarmApp.DataConverter.Services;

public class CsvGenerationService : ICsvGenerationService
{
    private readonly string _saveDirectory = "/Users/ruslandudchenko/Documents/";
    private readonly DataConverterDbContext _dataConverterDbContext;

    public CsvGenerationService(DataConverterDbContext dataConverterDbContext)
    {
        _dataConverterDbContext = dataConverterDbContext;
    }

    public async Task GenerateCsvAsync(string tilesPath, string csvFileName, int districtCode)
    {
        await SaveTilesToDbAsync(tilesPath, districtCode);
        var csvRows = await EliminateRedundantSteadsAsync();
        await SaveToCsvFromDbAsync(csvFileName, csvRows);
    }

    private async Task SaveTilesToDbAsync(string tilesPath, int districtCode)
    {
        var tilesFilePaths = Directory.GetFiles(tilesPath)
            .Where(x => x.Contains("/14"));
        
        var counter = 0;
        
        var list = new List<CsvRowEntity>();
        
        // Iterate over each file path
        foreach (var filePath in tilesFilePaths)
        {
            var regex = new Regex(@"_(\d+)_(\d+)\.pbf$");
            var match = regex.Match(filePath);
        
            if (!match.Success) continue;
        
            var tileX = int.Parse(match.Groups[1].Value);
            var tileY = int.Parse(match.Groups[2].Value);
        
            var fileBytes = await File.ReadAllBytesAsync(filePath);
        
            var decoder = new VectorTileDecoder();
            var features = decoder.Decode(fileBytes);
        
            foreach (var feature in features)
            {
                var attributes = feature.GetAttributes();
        
                var cadNum = attributes.TryGetValue("cadnum", out var cadNumValue) ? $"{cadNumValue}" : "";
                if (!cadNum.StartsWith($"{districtCode}")) continue;
        
                var ownership = attributes.TryGetValue("ownership", out var ownershipValue) ? $"{ownershipValue}" : "";
                var purpose = attributes.TryGetValue("purpose", out var purposeValue) ? $"{purposeValue}" : "";
                var category = attributes.TryGetValue("purpose", out var categoryValue) ? $"{categoryValue}" : "";
                var address = attributes.TryGetValue("address", out var addressValue) ? $"{addressValue}" : "";
        
                var originalPolygon = feature.GetGeometry();
        
                var polygonCopy = originalPolygon.Copy() as Polygon;
                if (polygonCopy is null)
                {
                    var multipolygonCopy = originalPolygon.Copy() as MultiPolygon;
                    if (multipolygonCopy?.GetGeometryN(0) is not Polygon firstPolygon) continue;
                    
                    polygonCopy = firstPolygon;
                }
        
                var zoom = 14;
        
                ConvertMultiPolygon(zoom, tileX, tileY, polygonCopy.ExteriorRing.Coordinates);
        
                foreach (var hole in polygonCopy.Holes)
                {
                    ConvertMultiPolygon(zoom, tileX, tileY, hole.Coordinates);
                }
        
                var wktWriter = new WKTWriter();
                var wktCoordinates = wktWriter.Write(polygonCopy);
        
                var record = new CsvRowEntity
                {
                    Id = $"{++counter}",
                    Cadnum = cadNum,
                    Category = category,
                    Purpose = purpose,
                    Area = "0",
                    UnitArea = "га",
                    OwnershipCode = "",
                    Ownership = ownership,
                    Address = address,
                    Geometry = wktCoordinates
                };
        
                _dataConverterDbContext.CsvRows.Add(record);
                list.Add(record);
        
                if (counter % 1000 == 0)
                {
                    await _dataConverterDbContext.SaveChangesAsync();
                    list.ForEach(p => _dataConverterDbContext.Entry(p).State = EntityState.Detached);
                    list.Clear();
                    
                    Console.WriteLine($"Added to db {counter} items");
                }
            }
        
            await _dataConverterDbContext.SaveChangesAsync();
            list.ForEach(p => _dataConverterDbContext.Entry(p).State = EntityState.Detached);
            list.Clear();
        }
    }

    private async Task<List<CsvRowEntity>> EliminateRedundantSteadsAsync()
    {
        var counter = 0;
        var groups = await _dataConverterDbContext.CsvRows
            .AsNoTracking()
            .GroupBy(x => x.Cadnum)
            .ToListAsync();

        var resultRows = new List<CsvRowEntity>();

        foreach (var group in groups)
        {
            var csvRows = group.ToList();
            var csvRow = csvRows.First();
            
            if (csvRows.Count > 1)
            {
                var geometries = csvRows.Select(x => ConvertWktToGeometry(x.Geometry)).ToList();
                var unionGeometry = CascadedPolygonUnion.Union(geometries);
                
                MultiPolygon? resultMultipolygon = null;
                
                if (unionGeometry is Polygon mergedPolygon)
                {
                    var geometryFactory = new GeometryFactory();
                    resultMultipolygon = geometryFactory.CreateMultiPolygon(new[] { mergedPolygon });
                }
                else if (unionGeometry is MultiPolygon multiPolygon)
                {
                    resultMultipolygon = multiPolygon;
                }
                
                if (resultMultipolygon is not null)
                {
                    var wktWriter = new WKTWriter();
                    var wktCoordinates = wktWriter.Write(resultMultipolygon);
                    
                    csvRow.Geometry = wktCoordinates;
                }
            }
            else
            {
                var geometry = ConvertWktToGeometry(csvRow.Geometry);
                if (geometry is Polygon polygon)
                {
                    var geometryFactory = new GeometryFactory();
                    var resultMultipolygon = geometryFactory.CreateMultiPolygon(new[] { polygon });
                    
                    var wktWriter = new WKTWriter();
                    var wktCoordinates = wktWriter.Write(resultMultipolygon);
                    
                    csvRow.Geometry = wktCoordinates;
                }
            }
            
            resultRows.Add(csvRows.First());

            if (counter % 1000 == 0)
            {
                Console.WriteLine($"PROCESSED {counter} items");
            }
            
            counter++;
        }

        return resultRows;
    }

    private async Task SaveToCsvFromDbAsync(string csvFileName, List<CsvRowEntity> csvRowEntities)
    {
        var counter = 0;
        
        var csvFilePath = $"{_saveDirectory}{csvFileName}";
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," };

        await using var streamWriter = new StreamWriter(csvFilePath);
        await using var csvWriter = new CsvWriter(streamWriter, csvConfig);

        csvWriter.WriteHeader<CsvRowModel>();
        await csvWriter.NextRecordAsync();

        Console.WriteLine("Writing to CSV");

        foreach (var csvRow in csvRowEntities)
        {
            var record = new CsvRowModel
            {
                Cadnum = csvRow.Cadnum,
                Category = csvRow.Category,
                Purpose = csvRow.Purpose,
                Area = csvRow.Area,
                UnitArea = csvRow.UnitArea,
                OwnershipCode = csvRow.UnitArea,
                Ownership = csvRow.Ownership,
                Address = csvRow.Address,
                Geometry = csvRow.Geometry
            };

            csvWriter.WriteRecord(record);
            await csvWriter.NextRecordAsync();

            if (counter % 1000 == 0)
            {
                Console.WriteLine($"Saved to CSV {counter} items");
            }
            
            counter++;
        }

        Console.WriteLine($"CSV file created successfully, handled {counter} items.");
    }
    
    private static Geometry ConvertWktToGeometry(string wkt)
    {
        var wktReader = new WKTReader();
        return wktReader.Read(wkt);
    }

    private static void ConvertMultiPolygon(int zoom, int tileX, int tileY, Coordinate[] coordinates)
    {
        // convert to tile based space
        foreach (var coordinate in coordinates)
        {
            coordinate.X = tileX + coordinate.X / 256;
            coordinate.Y = tileY + coordinate.Y / 256;
        }

        // check by vertices
        foreach (var coordinate in coordinates)
        {
            ConverterMath.ConvertToGeoCoords(zoom, coordinate.X, coordinate.Y,
                out var lng, out var lat);

            coordinate.X = lng;
            coordinate.Y = lat;
        }
    }
}