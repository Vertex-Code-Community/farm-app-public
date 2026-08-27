using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Valid;

namespace FarmApp.DataConverter.Helpers;

public static class ConverterMath
{
    private const double OneDivideBy180 = 1.0 / 180.0;
    private const double OneDivideByPi = 1.0 / Math.PI;

    public static void ConvertToTileCoords(int zoom, double lng, double lat, out double tileX, out double tileY)
    {
        var lngRad = lng * OneDivideBy180; // * Math.PI
        var latRad = lat * Math.PI * OneDivideBy180;

        var xMercator = lngRad;
        var yMercator = Math.Log(Math.Tan(latRad) + (1.0 / Math.Cos(latRad)));

        var tilesNumDividedBy2 = (1 << zoom) / 2; // 1 << (zoom - 1)

        tileX = (1 + xMercator) * tilesNumDividedBy2; // xMercator / Math.PI
        tileY = (1 - yMercator / Math.PI) * tilesNumDividedBy2;
    }
    
    public static void ConvertToGeoCoords(int zoom, double tileX, double tileY, out double lng, out double lat)
    {
        double tilesNum = 1 << zoom; // Total number of tiles at this zoom level, 2^zoom

        // Convert tileX and tileY to normalized Mercator coordinates
        var xMercator = tileX / (tilesNum / 2) - 1;
        var yMercator = 1 - tileY / (tilesNum / 2);

        // Convert normalized Mercator coordinates to longitude
        lng = xMercator / OneDivideBy180;

        // Convert normalized Mercator coordinates to latitude
        var radLat = Math.Atan(Math.Sinh(yMercator * Math.PI));
        lat = radLat * OneDivideByPi / OneDivideBy180;
    }
    
    public static Polygon? FixPolygonSelfIntersectionsInLineString(Polygon polygon)
    {
        var exteriorRing = polygon.ExteriorRing as LinearRing;
        if (exteriorRing is not null && !exteriorRing.IsValid)
        {
            var validator = new IsValidOp(exteriorRing);
            if (validator.ValidationError.ErrorType == TopologyValidationErrors.RingSelfIntersection)
            {
                exteriorRing = FixLineRingSelfIntersection(exteriorRing);
            }
        }

        if (exteriorRing is null) return null;

        var holes = new List<LinearRing>();

         foreach (var hole in polygon.Holes)
         {
             if (hole.IsValid)
             {
                 holes.Add(hole);
                 continue;
             }
             
             var validator = new IsValidOp(hole);
             if (validator.ValidationError.ErrorType == TopologyValidationErrors.RingSelfIntersection)
             {
                 var newHole = FixLineRingSelfIntersection(hole);
                 if (newHole is not null) holes.Add(newHole);
             }
         }
        
        return new Polygon(exteriorRing, holes.ToArray());
    }

    private static LinearRing? FixLineRingSelfIntersection(LinearRing linearRing)
    {
        var coordinates = new List<Coordinate> ();

        var coords = linearRing.Coordinates.ToList();

        foreach (var coord in coords)
        {
            if (coordinates.Count > 1 && coordinates[^2].Equals2D(coord))
            {
                coordinates.RemoveAt(coordinates.Count - 1);
            }
            else
            {
                coordinates.Add(coord);
            }
        }

        return coordinates.Count < 3 ? null : new LinearRing(coordinates.ToArray());
    }

    // public static void ConvertToTileCoords(int zoom, double lng, double lat, out double tileX, out double tileY)
    // {
    //     var lngRad = lng * Math.PI * (1.0 / 180);
    //     var latRad = lat * Math.PI * (1.0 / 180);
    //
    //     var xMercator = lngRad;
    //     var yMercator = Math.Log(Math.Tan(latRad) + (1.0 / Math.Cos(latRad)));
    //
    //     var tilesNum = 1 << zoom;
    //
    //     tileX = (1 + (xMercator / Math.PI)) * 0.5 * tilesNum;
    //     tileY = (1 - (yMercator / Math.PI)) * 0.5 * tilesNum;
    // }
    
    // public static int long2tilex(double lon, int z)
    // {
    //     return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
    // }
    //
    // public static int lat2tiley(double lat, int z)
    // {
    //     return (int)Math.Floor((1 - Math.Log(Math.Tan(ToRadians(lat)) + 1 / Math.Cos(ToRadians(lat))) / Math.PI) / 2 * (1 << z));
    // }
    //
    // private static double ToRadians(double val)
    // {
    //     return val * Math.PI * (1.0 / 180);
    // }
    //
    // public static double tilex2long(int x, int z)
    // {
    //     return x / (double)(1 << z) * 360.0 - 180;
    // }
    //
    // public static double tiley2lat(int y, int z)
    // {
    //     double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << z);
    //     return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
    // }
}