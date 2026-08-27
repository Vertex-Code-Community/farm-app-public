using System.Collections;
using System.Collections.ObjectModel;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace FarmApp.MVTParser;

public class VectorTileDecoder
{
    private bool autoScale = true;

    /**
     * Get the autoScale setting.
     *
     * @return autoScale
     */
    public bool isAutoScale()
    {
        return autoScale;
    }

    /**
     * Set the autoScale setting.
     *
     * @param autoScale
     *            when true, the encoder automatically scale and return all coordinates in the 0..255 range.
     *            when false, the encoder returns all coordinates in the 0..extent-1 range as they are encoded.
     *
     */
    public void SetAutoScale(bool autoScale)
    {
        this.autoScale = autoScale;
    }

    public FeatureIterable Decode(byte[] data)
    {
        return Decode(data, Filter.ALL);
    }

    public FeatureIterable Decode(byte[] data, String layerName)
    {
        return Decode(data, new Filter.Single(layerName));
    }

    public FeatureIterable Decode(byte[] data, HashSet<String> layerNames)
    {
        return Decode(data, new Filter.Any(layerNames));
    }

    public FeatureIterable Decode(byte[] data, Filter filter)
    {
        Tile tile = Tile.Parser.ParseFrom(data);
        return new FeatureIterable(tile, filter, autoScale);
    }

    static int ZigZagDecode(int n)
    {
        return ((n >> 1) ^ (-(n & 1)));
    }

    public static Geometry DecodeGeometry(GeometryFactory gf, Tile.Types.GeomType geomType, List<int> commands, double scale)
    {
        int x = 0;
        int y = 0;

        List<List<Coordinate>> coordsList = new List<List<Coordinate>>();
        List<Coordinate> coords = null;

        int geometryCount = commands.Count();
        int length = 0;
        int command = 0;
        int i = 0;
        while (i < geometryCount)
        {

            if (geomType == Tile.Types.GeomType.Polygon)
            {
                
            }
            
            if (length <= 0)
            {
                length = (int) commands[i++];
                command = length & ((1 << 3) - 1);
                length = length >> 3;

                if (command != Command.ClosePath && command != Command.LineTo && command != Command.MoveTo)
                {
                    throw new Exception($"Unknown command {command}");
                }
            }

            if (length > 0)
            {
                if (command == Command.MoveTo)
                {
                    coords = new List<Coordinate>();
                    coordsList.Add(coords);
                }

                if (command == Command.ClosePath)
                {
                    if (geomType != Tile.Types.GeomType.Point && coords.Count != 0)
                    {
                        coords.Add(new Coordinate(coords.First()));
                    }

                    length--;
                    continue;
                }

                int dx = commands[i++];
                int dy = commands[i++];

                length--;

                dx = ZigZagDecode(dx);
                dy = ZigZagDecode(dy);

                x = x + dx;
                y = y + dy;

                Coordinate coord = new Coordinate(x / scale, y / scale);
                coords.Add(coord);
            }
        }

        Geometry geometry = null;

        switch (geomType)
        {
            case Tile.Types.GeomType.Linestring:
                List<LineString> lineStrings = new List<LineString>();
                foreach (List<Coordinate> cs in coordsList)
                {
                    if (cs.Count <= 1)
                    {
                        continue;
                    }

                    lineStrings.Add(gf.CreateLineString(cs.ToArray()));
                }

                if (lineStrings.Count == 1)
                {
                    geometry = lineStrings[0];
                }
                else if (lineStrings.Count > 1)
                {
                    geometry = gf.CreateMultiLineString(lineStrings.ToArray());
                }

                break;
            case Tile.Types.GeomType.Point:
                List<Coordinate> allCoords = new List<Coordinate>();
                foreach (List<Coordinate> cs in coordsList)
                {
                    allCoords.AddRange(cs);
                }

                if (allCoords.Count == 1)
                {
                    geometry = gf.CreatePoint(allCoords[0]);
                }
                else if (allCoords.Count > 1)
                {
                    geometry = gf.CreateMultiPointFromCoords(allCoords.ToArray());
                }

                break;
            case Tile.Types.GeomType.Polygon:
                List<List<LinearRing>> polygonRings = new List<List<LinearRing>>();
                List<LinearRing> ringsForCurrentPolygon = null;
                bool? ccw = null;

                foreach (List<Coordinate> cs in coordsList)
                {
                    Coordinate[] ringCoords = cs.ToArray();
                    double area = Area.OfRingSigned(ringCoords);
                    if (area == 0)
                    {
                        continue;
                    }

                    bool thisCcw = area < 0;
                    if (ccw == null)
                    {
                        ccw = thisCcw;
                    }

                    LinearRing ring = gf.CreateLinearRing(ringCoords);
                    if (ccw == thisCcw)
                    {
                        if (ringsForCurrentPolygon != null)
                        {
                            polygonRings.Add(ringsForCurrentPolygon);
                        }

                        ringsForCurrentPolygon = new List<LinearRing>();
                    }

                    ringsForCurrentPolygon.Add(ring);
                }

                if (ringsForCurrentPolygon != null)
                {
                    polygonRings.Add(ringsForCurrentPolygon);
                }

                List<Polygon> polygons = new List<Polygon>();
                foreach (List<LinearRing> rings in polygonRings)
                {
                    LinearRing shell = rings[0];
                    LinearRing[] holes = rings.GetRange(1, rings.Count - 1).ToArray();
                    polygons.Add(gf.CreatePolygon(shell, holes));
                }

                if (polygons.Count == 1)
                {
                    geometry = polygons[0];
                }

                if (polygons.Count > 1)
                {
                    geometry = gf.CreateMultiPolygon(GeometryFactory.ToPolygonArray(polygons));
                }

                break;
            case Tile.Types.GeomType.Unknown:
                break;
        }

        if (geometry == null)
        {
            geometry = gf.CreateGeometryCollection(new Geometry[0]);
        }

        return geometry;
    }

    public class FeatureIterable : IEnumerable<Feature>
    {
        private readonly Tile _tile;
        private readonly Filter _filter;
        private bool _autoScale;

        public FeatureIterable(Tile tile, Filter filter, bool autoScale)
        {
            _tile = tile;
            _filter = filter;
            _autoScale = autoScale;
        }

        public IEnumerator<Feature> GetEnumerator()
        {
            return new FeatureIterator(_tile, _filter, _autoScale);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<Feature> AsList()
        {
            List<Feature> features = new List<Feature>();
            foreach (Feature feature in this)
            {
                features.Add(feature);
            }

            return features;
        }

        public IReadOnlyCollection<string> GetLayerNames()
        {
            HashSet<string> layerNames = new HashSet<string>();
            foreach (Tile.Types.Layer layer in _tile.Layers)
            {
                layerNames.Add(layer.Name);
            }

            return layerNames.ToList().AsReadOnly();
        }
    }

    class FeatureIterator : IEnumerator<Feature>
    {
        private GeometryFactory _gf = new GeometryFactory();

        private Filter _filter;

        private IEnumerator<Tile.Types.Layer> _layerIterator;
        private IEnumerator<Tile.Types.Feature> _featureIterator;

        private uint _extent;
        private String _layerName;
        private double _scale;
        private bool _autoScale;

        private List<string> _keys = new List<string>();
        private List<object> _values = new List<object>();

        private Feature _next;

        public FeatureIterator(Tile tile, Filter filter, bool autoScale)
        {
            _layerIterator = tile.Layers.GetEnumerator();
            this._filter = filter;
            this._autoScale = autoScale;
        }

        public Feature Current
        {
            get
            {
                Feature n = _next;
                _next = null;

                return n;
            }
        }

        object IEnumerator.Current => Current;

        // public bool HasNext() {
        //     FindNext();
        //     return next != null;
        // }
        //
        // public Feature Next() {
        //     FindNext();
        //     if (next == null) {
        //         throw new Exception("NoSuchElementException");
        //     }
        //     
        //     Feature n = _next;
        //     _next = null;
        //     return n;
        // }

        public bool MoveNext()
        {
            FindNext();
            return _next != null;
        }

        public void Reset()
        {
            // throw new NotImplementedException();
        }

        public void Dispose()
        {
            // Dispose resources if needed
        }

        private void FindNext()
        {
            if (_next != null)
            {
                return;
            }

            while (true)
            {
                if (_featureIterator == null || !_featureIterator.MoveNext())
                {
                    if (!_layerIterator.MoveNext())
                    {
                        _next = null;
                        break;
                    }

                    Tile.Types.Layer layer = _layerIterator.Current;
                    if (!_filter.Include(layer.Name))
                    {
                        continue;
                    }

                    ParseLayer(layer);
                    continue;
                }

                _next = ParseFeature(_featureIterator.Current);
                break;
            }
        }

        private void ParseLayer(Tile.Types.Layer layer)
        {
            _layerName = layer.Name;
            _extent = layer.Extent;
            _scale = _autoScale ? _extent / 256.0 : 1.0;

            _keys.Clear();
            _keys.AddRange(layer.Keys);
            _values.Clear();

            foreach (Tile.Types.Value value in layer.Values)
            {
                if (value.HasBoolValue)
                {
                    _values.Add(value.BoolValue);
                }
                else if (value.HasDoubleValue)
                {
                    _values.Add(value.DoubleValue);
                }
                else if (value.HasFloatValue)
                {
                    _values.Add(value.FloatValue);
                }
                else if (value.HasIntValue)
                {
                    _values.Add(value.IntValue);
                }
                else if (value.HasSintValue)
                {
                    _values.Add(value.SintValue);
                }
                else if (value.HasUintValue)
                {
                    _values.Add(value.UintValue);
                }
                else if (value.HasStringValue)
                {
                    _values.Add(value.StringValue);
                }
                else
                {
                    _values.Add(null);
                }
            }

            _featureIterator = layer.Features.GetEnumerator();
        }

        private Feature ParseFeature(Tile.Types.Feature feature)
        {
            int tagsCount = feature.Tags.Count;
            Dictionary<string, object> attributes = new Dictionary<string, object>(tagsCount / 2);
            int tagIdx = 0;

            while (tagIdx < feature.Tags.Count)
            {
                string key = _keys[(int)feature.Tags[tagIdx++]];
                object value = _values[(int)feature.Tags[tagIdx++]];
                attributes.Add(key, value);
            }
            
            Geometry geometry = DecodeGeometry(_gf, feature.Type, 
                feature.Geometry.Select(x => (int) x).ToList(), _scale);
            
            if (geometry == null)
            {
                geometry = _gf.CreateGeometryCollection(new Geometry[0]);
            }

            return new Feature(_layerName, _extent, geometry, new Dictionary<string, object>(attributes), feature.Id);
        }
    }

    public class Feature
    {
        private string layerName;
        private uint extent;
        private ulong id;
        private Geometry geometry;
        private Dictionary<string, object> attributes;

        public Feature(string layerName, uint extent, Geometry geometry, Dictionary<string, object> attributes,
            ulong id)
        {
            this.layerName = layerName;
            this.extent = extent;
            this.geometry = geometry;
            this.attributes = attributes;
            this.id = id;
        }

        public String GetLayerName()
        {
            return layerName;
        }

        public ulong GetId()
        {
            return id;
        }

        public uint GetExtent()
        {
            return extent;
        }

        public Geometry GetGeometry()
        {
            return geometry;
        }

        public Dictionary<string, object> GetAttributes()
        {
            return attributes;
        }
    }
}