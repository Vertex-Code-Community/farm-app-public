using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Union;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace FarmApp.Components.Helpers;

public static class NetTopologySuiteUtils
{
    public static Geometry? CombineFeatures(List<string> featuresSerialized)
    {
        if (featuresSerialized.Count is 0) return null;
        
        var serializer = GeoJsonSerializer.Create();
        var geometries = new List<Geometry>();
        
        foreach(var featureJson in featuresSerialized)
        {
            using var stringReader = new StringReader(featureJson);
            using var jsonReader = new JsonTextReader(stringReader);
            
            var feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
            if (feature is not null) geometries.Add(feature.Geometry);
        }

        return CombineGeometries(geometries);
    }
    
    public static Geometry? CombineGeometries(List<Geometry> geometries)
    {
        if (geometries.Count is 0) return null;

        var geometriesWithoutRings = geometries
            .Select(x => GetPolygonWithoutRings(x)!)
            .ToArray();
        
        // var geometryCollection = new GeometryCollection(geometriesWithoutRings, new GeometryFactory());

        Console.WriteLine($"geometriesWithoutRings.Length = {geometriesWithoutRings.Length}");
        
        var unitedGeometry = CascadedPolygonUnion.Union(geometriesWithoutRings);
        
        // var firstGeometry = geometries.First();
        // Geometry? combinedGeometry = GetPolygonWithoutRings(firstGeometry);
        //
        // if (combinedGeometry is null) return null;
        //
        // for (var i = 1; i < geometries.Count; i++)
        // {
        //     var polygon = GetPolygonWithoutRings(geometries[i]);
        //     if (polygon is null) continue;
        //     combinedGeometry = combinedGeometry.Union(polygon);
        // }

        return unitedGeometry;
    }
    
    private static Polygon? GetPolygonWithoutRings(Geometry geometry)
    {
        if (geometry is not Polygon polygon) return null;
        if (polygon.ExteriorRing is not LinearRing exteriorRing) return null;
        
        return new Polygon(exteriorRing);
    }
    
    public static double GetAreaOfPolygon(Coordinate[] coordinates)
    {
        if (coordinates.Length is 0) return 0;
        var polygon = TransformToUTMZone(new Polygon(new LinearRing(coordinates)));
        
        return polygon.Area;
    }

    public static Polygon TransformToUTMZone(Polygon polygon)
    {
        var coordinate = polygon.Coordinates.First();
        
        var zone = (int)((coordinate[0] + 180) / 6) + 1;
        var hemisphere = coordinate[1] >= 0 ? 'N' : 'S';
        
        var transformFactory = new CoordinateTransformationFactory();
        var transformation = transformFactory.CreateFromCoordinateSystems(
            GeographicCoordinateSystem.WGS84, 
            ProjectedCoordinateSystem.WGS84_UTM(zone, hemisphere == 'N'));
        
        return TransformPolygon(polygon, transformation.MathTransform);
    }
    
    public static Polygon TransformFromUTMToWGS84Zone(Polygon polygon)
    {
        var coordinate = polygon.Coordinates.First();
        
        var zone = (int)((coordinate[0] + 180) / 6) + 1;
        var hemisphere = coordinate[1] >= 0 ? 'N' : 'S';
        
        var transformFactory = new CoordinateTransformationFactory();

        var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, hemisphere == 'N');
        var wgs84 = GeographicCoordinateSystem.WGS84;

        var transformToWgs84 = transformFactory.CreateFromCoordinateSystems(utm, wgs84);
        
        return TransformPolygon(polygon, transformToWgs84.MathTransform);
    }
    
    private static Polygon TransformPolygon(Polygon polygon, GeoAPI.CoordinateSystems.Transformations.IMathTransform transform)
    {
        var transformedCoordinates = polygon.Coordinates.Select(coord =>
        {
            var newCoord = transform.Transform(new double[] { coord.X, coord.Y });
            return new Coordinate(newCoord[0], newCoord[1]);
        }).ToArray();

        return new Polygon(new LinearRing(transformedCoordinates));
    }

    public static Coordinate[] ConvertToCoordinates(double[][] values)
    {
        return values.Select(v => new Coordinate(v[0], v[1])).ToArray();
    }
}