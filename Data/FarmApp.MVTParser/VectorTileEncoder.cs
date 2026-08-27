using System.Numerics;
using Google.Protobuf;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Index.Bintree;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;

namespace FarmApp.MVTParser;

public class VectorTileEncoder
{
    private readonly Dictionary<String, Layer> layers = new Dictionary<String, Layer>();

    private readonly int extent;

    private readonly double minimumLength;

    private readonly double minimumArea;

    protected readonly Geometry clipGeometry;

    protected readonly Envelope clipEnvelope;

    protected readonly IPreparedGeometry clipGeometryPrepared;

    private readonly bool autoScale;

    private long autoincrement;

    private readonly bool autoincrementIds;

    private readonly double simplificationDistanceTolerance;

    private readonly GeometryFactory gf = new GeometryFactory();
    
    private int x = 0;
    private int y = 0;
    
    private sealed class Feature {
        public long Id { get; set; }
        public Geometry Geometry { get; set; }

        public List<int> Tags { get; } = new();
    }

    private class Layer {

        public List<Feature> Features { get; } = new List<VectorTileEncoder.Feature>();
        private readonly Dictionary<string, int> keys = new Dictionary<string, int>();
        private readonly Dictionary<object, int> values = new Dictionary<object, int>();

        public int Key(string key) {
            if (!keys.TryGetValue(key, out var i))
            {
                i = keys.Count;
                keys.Add(key, i);
            }
            
            return i;
        }

        public List<String> Keys()
        {
            return keys.Keys.ToList();
        }

        public int Value(object value) {
            if (!values.TryGetValue(value, out var i))
            {
                i = values.Count;
                values.Add(value, i);
            }
            
            return i;
        }

        public List<Object> Values()
        {
            return values.Keys.ToList();
        }
    }
    
    /**
     * Create a {@link VectorTileEncoder} with the default extent of 4096 and
     * clip buffer of 8.
     */
    public VectorTileEncoder() : this(4096, 8, true)
    {
    }

    /**
     * Create a {@link VectorTileEncoder} with the given extent and a clip
     * buffer of 8.
     * 
     * @param extent a int to specify vector tile extent. 4096 is a good value.
     */
    public VectorTileEncoder(int extent) : this(extent, 8, true)
    {
    }
    
    public VectorTileEncoder(int extent, int clipBuffer, bool autoScale) : this(extent, clipBuffer, autoScale, false)
    {
    }

    public VectorTileEncoder(int extent, int clipBuffer, bool autoScale, bool autoincrementIds) : this(extent, clipBuffer, autoScale, autoincrementIds, -1.0)
    {
    }

    /**
     * Create a {@link VectorTileEncoder} with the given extent value.
     * <p>
     * The extent value control how detailed the coordinates are encoded in the
     * vector tile. 4096 is a good default, 256 can be used to reduce density.
     * <p>
     * The clip buffer value control how large the clipping area is outside of the
     * tile for geometries. 0 means that the clipping is done at the tile border. 8
     * is a good default.
     *
     * @param extent
     *            a int with extent value. 4096 is a good value.
     * @param clipBuffer
     *            a int with clip buffer size for geometries. 8 is a good value.
     * @param autoScale
     *            when true, the encoder expects coordinates in the 0..255 range and
     *            will scale them automatically to the 0..extent-1 range before
     *            encoding. when false, the encoder expects coordinates in the
     *            0..extent-1 range.
     * @param autoincrementIds 
     *            when true the vector tile feature id is auto incremented when using 
     *            {@link #addFeature(String, Map, Geometry)}
     * @param simplificationDistanceTolerance
     *            a positive double representing the distance tolerance to be used
     *            for non-points before (optional) scaling and encoding. A value
     *            &lt;=0 will prevent simplifying geometry. 0.1 seems to be a good
     *            value when {@code autoScale} is turned on.
     */
    public VectorTileEncoder(int extent, int clipBuffer, bool autoScale, bool autoincrementIds, double simplificationDistanceTolerance)
    {
        this.extent = extent;
        this.autoScale = autoScale;
        this.minimumLength = autoScale ? (256.0 / extent) : 1.0;
        this.minimumArea = this.minimumLength * this.minimumLength;
        this.autoincrementIds = autoincrementIds;
        this.autoincrement = 1;
        this.simplificationDistanceTolerance = simplificationDistanceTolerance;

        int size = autoScale ? 256 : extent;

        clipGeometry = createTileEnvelope(clipBuffer, size);
        clipEnvelope = clipGeometry.EnvelopeInternal;
        clipGeometryPrepared = PreparedGeometryFactory.Prepare(clipGeometry);
    }

    private static Geometry createTileEnvelope(int buffer, int size)
    {
        Coordinate[] coords = new Coordinate[5];

        coords[0] = new Coordinate(0 - buffer, size + buffer);
        coords[1] = new Coordinate(size + buffer, size + buffer);
        coords[2] = new Coordinate(size + buffer, 0 - buffer);
        coords[3] = new Coordinate(0 - buffer, 0 - buffer);
        coords[4] = coords[0];

        return new GeometryFactory().CreatePolygon(coords);
    }
    
    public void AddFeature(String layerName, Dictionary<string, object> attributes, Geometry geometry) {
        this.AddFeature(layerName, attributes, geometry, this.autoincrementIds ? this.autoincrement++ : -1);
    }
    
    /**
     * Add a feature with layer name (typically feature type name), some attributes
     * and a Geometry. The Geometry must be in "pixel" space 0,0 upper left and
     * 256,256 lower right.
     * <p>
     * For optimization, geometries will be clipped and simplified. Features with
     * geometries outside of the tile will be skipped.
     *
     * @param layerName a {@link String} with the vector tile layer name.
     * @param attributes a {@link Map} with the vector tile feature attributes.
     * @param geometry a {@link Geometry} for the vector tile feature.
     * @param id a long with the vector tile feature id field.
     */
    public void AddFeature(String layerName, Dictionary<String, object> attributes, Geometry geometry, long id) 
    {
        // skip small Polygon/LineString.
        if (geometry is MultiPolygon && geometry.Area < minimumArea) {
            return;
        }
        if (geometry is Polygon && geometry.Area < minimumArea) {
            return;
        }
        if (geometry is LineString && geometry.Length < minimumLength) {
            return;
        }

        // special handling of GeometryCollection. subclasses are not handled here.
        // if (geometry.getClass().equals(GeometryCollection.class)) {
        if (geometry.GetType() == typeof(GeometryCollection)) {
            for (int i = 0; i < geometry.NumGeometries; i++) {
                Geometry subGeometry = geometry.GetGeometryN(i);
                // keeping the id. any better suggestion?
                AddFeature(layerName, attributes, subGeometry, id);
            }
            return;
        }
        
        // About to simplify and clip. Looks like simplification before clipping is
        // faster than clipping before simplification
        
        // simplify non-points
        if (simplificationDistanceTolerance > 0.0 && !(geometry is Point)) {
            if (geometry is LineString || geometry is MultiLineString) {
                geometry = DouglasPeuckerSimplifier.Simplify(geometry, simplificationDistanceTolerance);
            } else if (geometry is Polygon || geometry is MultiPolygon) {
                Geometry simplified = DouglasPeuckerSimplifier.Simplify(geometry, simplificationDistanceTolerance);
                // extra check to prevent polygon converted to line
                if (simplified is Polygon || simplified is MultiPolygon) {
                    geometry = simplified;
                } else {
                    geometry = TopologyPreservingSimplifier.Simplify(geometry, simplificationDistanceTolerance);
                }
            } else {
                geometry = TopologyPreservingSimplifier.Simplify(geometry, simplificationDistanceTolerance);
            }
        }
        
        // clip geometry
        if (geometry is Point) {
            if (!ClipCovers(geometry)) {
                return;
            }
        } else {
            geometry = ClipGeometry(geometry);
        }

        // no need to add empty geometry
        if (geometry == null || geometry.IsEmpty) {
            return;
        }

        // extra check for GeometryCollection after clipping as it can cause
        // GeometryCollection. Subclasses not handled here.
        // if (geometry.getClass().equals(GeometryCollection.class)) {
        if (geometry.GetType() == typeof(GeometryCollection)) {
            for (int i = 0; i < geometry.NumGeometries; i++) {
                Geometry subGeometry = geometry.GetGeometryN(i);
                // keeping the id. any better suggestion?
                AddFeature(layerName, attributes, subGeometry, id);
            }
            return;
        }

        // Tile.Types.Layer layer = layers.Get(layerName);
        // Layer layer = layers[layerName];

        if (!layers.TryGetValue(layerName, out var layer)) {
            layer = new Layer();
            //layers.put(layerName, layer);
            layers.Add(layerName, layer);
        }

        Feature feature = new Feature();
        feature.Geometry = geometry;
        feature.Id = id;
        
        this.autoincrement = Math.Max(this.autoincrement, id + 1);

        foreach (var e in attributes)
        {
            // skip attribute without value
            if (e.Value == null) {
                continue;
            }
            
            feature.Tags.Add(layer.Key(e.Key));
            feature.Tags.Add(layer.Value(e.Value));
        }

        layer.Features.Add(feature);
    }

    /**
     * A short circuit clip to the tile extent (tile boundary + buffer) for
     * points to improve performance. This method can be overridden to change
     * clipping behavior. See also {@link #clipGeometry(Geometry)}.
     * 
     * @param geom a {@link Geometry} to check for "covers"
     * @return a boolean true when the current clip geometry covers the given geom.
     */
    protected bool ClipCovers(Geometry geom) {
        if (geom is Point) {
            Point p = (Point) geom;
            return clipGeometry.EnvelopeInternal.Covers(p.Coordinate);
        }
        return clipEnvelope.Covers(geom.EnvelopeInternal);
    }
    
    /**
     * Clip geometry according to buffer given at construct time. This method
     * can be overridden to change clipping behavior. See also
     * {@link #clipCovers(Geometry)}.
     *
     * @param geometry a {@link Geometry} to check for intersection with the current clip geometry
     * @return a boolean true when current clip geometry intersects with the given geometry.
     */
    protected Geometry ClipGeometry(Geometry geometry) {
        try {
            if (clipEnvelope.Contains(geometry.EnvelopeInternal)) {
                return geometry;
            }
            
            Geometry original = geometry;
            geometry = clipGeometry.Intersection(original);

            // some times a intersection is returned as an empty geometry.
            // going via wkt fixes the problem.
            if (geometry.IsEmpty && clipGeometryPrepared.Intersects(original)) {
                Geometry originalViaWkt = new WKTReader().Read(original.ToText());
                geometry = clipGeometry.Intersection(originalViaWkt);
            }

            return geometry;
        } catch (TopologyException e) {
            // could not intersect. original geometry will be used instead.
            return geometry;
        } catch (ParseException e1) {
            // could not encode/decode WKT. original geometry will be used
            // instead.
            return geometry;
        }
    }
    
    /**
     * Validate and potentially repair the given {@link List} of commands for the
     * given {@link Geometry}. Will return a {@link List} of the validated and/or
     * repaired commands.
     * <p>
     * This can be overridden to change behavior. By returning just the incoming
     * {@link List} of commands instead, the encoding will be faster, but
     * potentially less safe.
     * 
     * @param commands
     * @param geometry
     * @return
     */
    protected List<int> ValidateAndRepairCommands(List<int> commands, Geometry geometry) {
        if (commands.Count == 0) {
            return commands;
        }

        Tile.Types.GeomType geomType = ToGeomType(geometry);
        if (simplificationDistanceTolerance > 0.0 && geomType == Tile.Types.GeomType.Polygon) {
            double scale = autoScale ? (extent / 256.0) : 1.0;
            Geometry decodedGeometry = VectorTileDecoder.DecodeGeometry(gf, geomType, commands, scale);
            if (!IsValid(decodedGeometry)) {
                // Invalid. Try more simplification and without preserving topology.
                geometry = DouglasPeuckerSimplifier.Simplify(geometry, simplificationDistanceTolerance * 2.0);
                if (geometry.IsEmpty) {
                    //Collections.emptyList();
                }
                geomType = ToGeomType(geometry);
                x = 0;
                y = 0;
                return Commands(geometry);
            }
        }

        return commands;
    }
    
    /**
     * @return a byte array with the vector tile
     */
    public byte[] Encode() 
    {
        // VectorTile.Tile.Builder tile = VectorTile.Tile.newBuilder();
        Tile tile = new Tile();

        foreach (var e in layers) {
            string layerName = e.Key;
            Layer layer = e.Value;

            // VectorTile.Tile.Layer.Builder tileLayer = VectorTile.Tile.Layer.newBuilder();
            Tile.Types.Layer tileLayer = new Tile.Types.Layer();
            
            tileLayer.Version = 2;
            tileLayer.Name = layerName;

            tileLayer.Keys.AddRange(layer.Keys());

            foreach (object value in layer.Values()) {
                
                Tile.Types.Value tileValue = new Tile.Types.Value();
                
                if (value is string s) {
                    tileValue.StringValue = s;
                } else if (value is int i) {
                    tileValue.SintValue = i;
                } else if (value is long l) {
                    tileValue.SintValue = l;
                } else if (value is float f) {
                    tileValue.FloatValue = f;
                } else if (value is double d) {
                    tileValue.DoubleValue = d;
                } else if (value is BigInteger) {
                    tileValue.StringValue = value.ToString();
                // } else if (value is Number) {
                //     tileValue.setDoubleValue(((Number) value).doubleValue());
                } else if (value is bool b) {
                    tileValue.BoolValue = b;
                } else {
                    tileValue.StringValue = value.ToString();
                }
                
                tileLayer.Values.Add(tileValue);
            }

            tileLayer.Extent = (uint) extent;

            foreach (Feature feature in layer.Features) {

                Geometry geometry = feature.Geometry;

                Tile.Types.Feature featureBuilder = new Tile.Types.Feature();

                featureBuilder.Tags.AddRange(feature.Tags.Select(x => (uint)x));
                if (feature.Id >= 0) {
                    featureBuilder.Id = (ulong) feature.Id;
                }
                
                Tile.Types.GeomType geomType = ToGeomType(geometry);
                x = 0;
                y = 0;
                
                List<int> commands = Commands(geometry);

                // Extra step to parse and check validity and try to repair.
                commands = ValidateAndRepairCommands(commands, geometry);

                // skip features with no geometry commands
                if (commands.Count == 0) {
                    continue;
                }
                
                featureBuilder.Type = geomType;
                featureBuilder.Geometry.AddRange(commands.Select(x => (uint)x));

                tileLayer.Features.Add(featureBuilder);
            }

            tile.Layers.Add(tileLayer);
        }

        return tile.ToByteArray();
    }

    private static bool IsValid(Geometry geometry) {
        try {
            return geometry.IsValid;
        } catch (Exception e) {
            return false;
        }
    }

    static Tile.Types.GeomType ToGeomType(Geometry geometry) {
        if (geometry is Point) {
            return Tile.Types.GeomType.Point;
        }
        if (geometry is MultiPoint) {
            return Tile.Types.GeomType.Point;
        }
        if (geometry is LineString) {
            return Tile.Types.GeomType.Linestring;
        }
        if (geometry is MultiLineString) {
            return Tile.Types.GeomType.Linestring;
        }
        if (geometry is Polygon) {
            return Tile.Types.GeomType.Polygon;
        }
        if (geometry is MultiPolygon) {
            return Tile.Types.GeomType.Polygon;
        }
        return Tile.Types.GeomType.Unknown;
    }

    static bool ShouldClosePath(Geometry geometry) {
        return (geometry is Polygon) || (geometry is LinearRing);
    }

    List<int> Commands(Geometry geometry) {
        
        if (geometry is MultiLineString) {
            return Commands((MultiLineString) geometry);
        }
        if (geometry is Polygon) {
            return Commands((Polygon) geometry);
        }
        if (geometry is MultiPolygon) {
            return Commands((MultiPolygon) geometry);
        }        
        
        return Commands(geometry.Coordinates, ShouldClosePath(geometry), geometry is MultiPoint);
    }
    
    List<int> Commands(MultiLineString mls) {
        List<int> commands = new List<int>();
        for (int i = 0; i < mls.NumGeometries; i++) {
            int oldX = x;
            int oldY = y;
            List<int> geomCommands =
                Commands(mls.GetGeometryN(i).Coordinates, false);
            if (geomCommands.Count > 3) {
                // if the geometry consists of all identical points (after Math.round()) commands
                // returns a single move_to command, which is not valid according to the vector tile
                // specifications.
                // (https://github.com/mapbox/vector-tile-spec/tree/master/2.1#4343-linestring-geometry-type)
                commands.AddRange(geomCommands);
            } else {
                // reset x and y to the previous value
                x = oldX;
                y = oldY;
            }
        }
        return commands;
    }
    
    List<int> Commands(MultiPolygon mp) {
        List<int> commands = new List<int>();
        for (int i = 0; i < mp.NumGeometries; i++) {
            Polygon polygon = (Polygon) mp.GetGeometryN(i);
            commands.AddRange(Commands(polygon));
        }
        return commands;
    }
    
    List<int> Commands(Polygon polygon) {
        List<int> commands = new List<int>();

        // According to the vector tile specification, the exterior ring of a polygon
        // must be in clockwise order, while the interior ring in counter-clockwise order.
        // In the tile coordinate system, Y axis is positive down.
        //
        // However, in geographic coordinate system, Y axis is positive up.
        // Therefore, we must reverse the coordinates.
        // So, the code below will make sure that exterior ring is in counter-clockwise order
        // and interior ring in clockwise order.
        LineString exteriorRing = polygon.ExteriorRing;
        if (Area.OfRingSigned(exteriorRing.Coordinates) > 0) {
            exteriorRing = (LineString) exteriorRing.Reverse(); // TODO::
        }
        commands.AddRange(Commands(exteriorRing.Coordinates, true));

        for (int i = 0; i < polygon.NumInteriorRings; i++) {
            LineString interiorRing = polygon.GetInteriorRingN(i);
            if (Area.OfRingSigned(interiorRing.Coordinates) < 0) {
                interiorRing = (LineString) interiorRing.Reverse(); // TODO::
            }
            
            commands.AddRange(Commands(interiorRing.Coordinates, true));
        }
        return commands;
    }

    /**
     * // // // Ex.: MoveTo(3, 6), LineTo(8, 12), LineTo(20, 34), ClosePath //
     * Encoded as: [ 9 3 6 18 5 6 12 22 15 ] // == command type 7 (ClosePath),
     * length 1 // ===== relative LineTo(+12, +22) == LineTo(20, 34) // ===
     * relative LineTo(+5, +6) == LineTo(8, 12) // == [00010 010] = command type
     * 2 (LineTo), length 2 // === relative MoveTo(+3, +6) // == [00001 001] =
     * command type 1 (MoveTo), length 1 // Commands are encoded as uint32
     * varints, vertex parameters are // encoded as sint32 varints (zigzag).
     * Vertex parameters are // also encoded as deltas to the previous position.
     * The original // position is (0,0)
     *
     * @param cs
     * @return
     */
    List<int> Commands(Coordinate[] cs, bool closePathAtEnd) {
        return Commands(cs, closePathAtEnd, false);
    }

    List<int> Commands(Coordinate[] cs, bool closePathAtEnd, bool multiPoint) {

        if (cs.Length == 0) {
            return new List<int>();
        }

        List<int> r = new List<int>();

        int lineToIndex = 0;
        int lineToLength = 0;

        double scale = autoScale ? (extent / 256.0) : 1.0;

        for (int i = 0; i < cs.Length; i++) {
            Coordinate c = cs[i];

            if (i == 0)
            {
                r.Add(CommandAndLength(Command.MoveTo, multiPoint ? cs.Length : 1));
            }

            int _x = (int) Math.Round(c.X * scale);
            int _y = (int) Math.Round(c.Y * scale);

            // prevent point equal to the previous
            if (i > 0 && _x == x && _y == y) {
                lineToLength--;
                continue;
            }

            // prevent double closing
            if (closePathAtEnd && cs.Length > 1 && i == (cs.Length - 1) && cs[0].Equals(c)) {
                lineToLength--;
                continue;
            }

            // delta, then zigzag
            r.Add(ZigZagEncode(_x - x));
            r.Add(ZigZagEncode(_y - y));

            x = _x;
            y = _y;

            if (i == 0 && cs.Length > 1 && !multiPoint) {
                // can length be too long?
                lineToIndex = r.Count;
                lineToLength = cs.Length - 1;
                r.Add(CommandAndLength(Command.LineTo, lineToLength));
            }

        }

        // update LineTo length
        if (lineToIndex > 0) {
            if (lineToLength == 0) {
                // remove empty LineTo
                r.RemoveAt(lineToIndex);
            } else {
                // update LineTo with new length
                r[lineToIndex] = CommandAndLength(Command.LineTo, lineToLength);
            }
        }

        if (closePathAtEnd) {
            r.Add(CommandAndLength(Command.ClosePath, 1));
        }

        return r;
    }

    static int CommandAndLength(int command, int repeat) {
        // return repeat << 3 | command;
        return (command & 0x7) | (repeat << 3);
    }

    static int ZigZagEncode(int n) {
        // https://developers.google.com/protocol-buffers/docs/encoding#types
        return (n << 1) ^ (n >> 31);
    }
}