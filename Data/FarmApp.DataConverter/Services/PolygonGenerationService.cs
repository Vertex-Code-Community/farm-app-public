using FarmApp.DataConverter.DbContext;
using FarmApp.DataConverter.Helpers;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace FarmApp.DataConverter.Services;

public class PolygonGenerationService : IPolygonGenerationService
{
    private readonly DataConverterDbContext _dataConverterDbContext;
    private readonly HashSet<string> _tileIds = new();
    private readonly List<string> _wktLines = new();
    
    public PolygonGenerationService(DataConverterDbContext dataConverterDbContext)
    {
        _dataConverterDbContext = dataConverterDbContext;
    }
    
    public async Task GeneratePolygonAsync(string steadId, string wktLine, List<int> zoomList)
    {
        _tileIds.Clear();
        _wktLines.Clear();

        var wktReader = new WKTReader();
        if (wktReader.Read(wktLine) is not MultiPolygon multiPolygon) return;

        foreach (var zoom in zoomList)
        {
            var multiPolygonCopy = multiPolygon.Copy() as MultiPolygon;
            
            foreach (var geometry in multiPolygonCopy!.Geometries)
            {
                var polygon = geometry as Polygon;
            
                ConvertMultiPolygon(zoom, polygon.ExteriorRing.Coordinates);
            
                foreach (var hole in polygon.Holes)
                {
                    ConvertMultiPolygon(zoom, hole.Coordinates);
                }
            }
            
            var wktWriter = new WKTWriter();
            var wktCoordinates = wktWriter.Write(multiPolygonCopy);

            _wktLines.Add(wktCoordinates);
        }

        var tiles = await _dataConverterDbContext.Tiles
            .Where(x => _tileIds.Contains(x.Id))
            .ToListAsync();

        var polygonEntity = new PolygonEntity
        {
            Id = Guid.NewGuid().ToString(),
            SteadId = steadId,
            WktCoordinates = _wktLines.ToList(),
            Tiles = tiles
        };
        
        await _dataConverterDbContext.AddAsync(polygonEntity);
    }

    private void ConvertMultiPolygon(int zoom, Coordinate[] coordinates)
    {
        // check by vertices
        foreach (var coordinate in coordinates)
        {
            ConverterMath.ConvertToTileCoords(zoom, coordinate.X, coordinate.Y, 
                out var tileXd, out var tileYd);

            coordinate.X = tileXd;
            coordinate.Y = tileYd;
            
            var tileX = (int)tileXd;
            var tileY = (int)tileYd;
            
            _tileIds.Add($"{tileX}_{tileY}_{zoom}");
        }
        
        // check by intersection
        for (var i = 1; i < coordinates.Length; i ++)
        {
            var p1 = coordinates[i - 1];
            var p2 = coordinates[i - 0];

            var p1X = (int) p1.X;
            var p1Y = (int) p1.Y;
            
            var p2X = (int) p2.X;
            var p2Y = (int) p2.Y;

            if (p1X == p2X && p1Y == p2Y) continue; // line in one cell

            var comparisonY = p1Y < p2Y;
            var startY = comparisonY ? p1Y : p2Y;
            var endY = !comparisonY ? p1Y : p2Y;
            
            var comparisonX = p1X < p2X;
            var startX = comparisonX ? p1X : p2X;
            var endX = !comparisonX ? p1X : p2X;

            if (p1.Y == p2.Y) // horizontal
            {
                for (var x = startX; x < endX; x ++)
                    _tileIds.Add($"{x}_{p1Y}_{zoom}");
                
                continue;
            }
            
            if (p1.X == p2.X) // vertical
            {
                for (var y = startY; y < endY; y ++)
                    _tileIds.Add($"{p1X}_{y}_{zoom}");
                
                continue;
            }

            var dX = p2.X - p1.X;
            var dY = p2.Y - p1.Y;

            var mX = dX / dY;
            var mY = dY / dX;

            const double delta = 0.0000000001;
            
            for (var y = startY; y <= endY; y ++)
            {
                var y1 = y - delta;
                var y2 = y + delta;
                var x1 = (y1 - p1.Y) * mX + p1.X;
                var x2 = (y2 - p1.Y) * mX + p1.X;
                
                TryToAddTile(x1, y1, dX, dY, p1, zoom);
                TryToAddTile(x2, y2, dX, dY, p1, zoom);
            }
            
            for (var x = startX; x <= endX; x ++)
            {
                var x1 = x - delta;
                var x2 = x + delta;
                var y1 = (x1 - p1.X) * mY + p1.Y;
                var y2 = (x2 - p1.X) * mY + p1.Y;

                TryToAddTile(x1, y1, dX, dY, p1, zoom);
                TryToAddTile(x2, y2, dX, dY, p1, zoom);
            }
        }
    }

    private void TryToAddTile(double x, double y, double abX, double abY, Coordinate p1, int zoom)
    {
        var acX = x - p1.X;
        var acY = y - p1.Y;

        var dotProduct = abX * acX + abY * acY;
        var lengthAB = Math.Sqrt(abX * abX + abY * abY);

        var projectionLength = dotProduct / lengthAB;

        if (projectionLength >= 0 && projectionLength <= lengthAB) // C in between A and B
        {
            _tileIds.Add($"{(int)x}_{(int)y}_{zoom}");
        }
    }
}