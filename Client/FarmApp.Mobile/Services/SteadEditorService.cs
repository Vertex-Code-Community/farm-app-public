using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using MaplibreMaui.Services;
using MaplibreMaui.Models;
using MaplibreMaui.Models.Layers;
using MaplibreMaui.Models.Sources;
using MaplibreMaui.Models.Features;
using FarmApp.Mobile.Helpers;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Enums;
using FarmApp.ViewModels.Map;

namespace FarmApp.Mobile.Services;

public class SteadEditorService : ISteadEditorService, IDisposable
{
    private const string PolygonSourceId = "draw-polygon-source";
    private const string PolygonLayerId = "draw-layer-id";
    private const string LineStringLayerId = "line-string-points-layer-id";
    private const string VerticesLayerId = "vertex-points-layer-id";
    private const string VerticesLayerRenderedId = "vertex-points-layer-rendered-id";
    private const string AdditionalVerticesLayerId = "additional-vertex-points-layer-id";
    private const string AdditionalVerticesLayerRenderedId = "additional-vertex-points-layer-rendered-id";

    public IMaplibreMapService? MaplibreMapService { get; set; }

    private bool _blazorServicesLoaded = false;

    private bool _polygonDrawn = false;
    private int? _selectedPointFeatureId = null;
    private int? _movingPointFeatureId = null;
    private LatLng? _prevLatLng = null;
    private bool _polygonMoving = false;
    private string? _modifyingSteadId = null;
    private string? _modifyingCustomSteadId = null;

    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private readonly List<double[]> _polygonPoints = new();

    public SteadEditorService()
    {
        IMapSteadService.OnRemoveVertexClicked += OnRemoveVertexClicked;
        IMapSteadService.OnDrawingModeSwitch += OnEditorStateChange;
        IMapSteadService.OnSteadEditorState += GetState;
    }

    public void OnMapReady()
    {
    }

    public void OnStyleLoaded()
    {
    }

    public void OnBlazorServicesLoaded()
    {
        _blazorServicesLoaded = true;
    }

    public bool OnDown(float x, float y)
    {
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (IMapSteadService.IsDrawingModeOn != true || MaplibreMapService is null ||
            mapModalService is null) return true;

        var vertexFeature = MaplibreMapService.QueryFeatureByPoint(VerticesLayerId, x, y);
        var latLng = MaplibreMapService.LatLngFromScreenLocation(x, y);

        if (vertexFeature is not null)
        {
            OnDownVertexLayer(latLng, vertexFeature);
            return true;
        }

        var additionalVertexFeature = MaplibreMapService?.QueryFeatureByPoint(AdditionalVerticesLayerId, x, y);

        if (additionalVertexFeature is not null)
        {
            OnDownAdditionalVertexLayer(latLng, additionalVertexFeature);
            return true;
        }

        var polygonFeature = MaplibreMapService?.QueryFeatureByPoint(PolygonLayerId, x, y);

        if (polygonFeature is not null)
        {
            OnDownPolygonLayer(latLng, polygonFeature);
        }

        mapModalService.Hide();

        return true;
    }

    public bool OnUp(float x, float y)
    {
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (!IMapSteadService.IsDrawingModeOn || MaplibreMapService is null ||
            mapModalService is null) return true;

        var hadMovingPoint = _movingPointFeatureId is not null || _polygonMoving;

        _polygonMoving = false;
        _prevLatLng = null;
        _movingPointFeatureId = null;

        MaplibreMapService.ToggleActions(true);

        if (hadMovingPoint) IMapSteadService.NotifyEditorGeometryChanged();

        return true;
    }

    public bool OnClick(float x, float y)
    {
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (!IMapSteadService.IsDrawingModeOn || MaplibreMapService is null ||
            mapModalService is null) return true;

        if (_polygonDrawn)
        {
            var vertexFeature = MaplibreMapService.QueryFeatureByPoint(VerticesLayerId, x, y);
            if (vertexFeature is not null)
            {
                var coordinateElement = vertexFeature.Coordinates.FirstOrDefault();
                if (coordinateElement is not double[] vertexCoords || _polygonPoints.Count <= 4) return true;

                if (MaplibreMapService is null ||
                    !vertexFeature.Properties.TryGetValue("index", out var vertexIndexObj) ||
                    vertexIndexObj is null ||
                    !TryGetIntFromProperty(vertexIndexObj, out var selectedIndex))
                    return true;

                _selectedPointFeatureId = selectedIndex;

                var point = MaplibreMapService.ScreenLocationFromLatLng(new LatLng
                    { Lng = vertexCoords[0], Lat = vertexCoords[1] });

                mapModalService.Show(MapModalType.CloseButton, point.X / ScreenOffsetProvider.Density,
                    point.Y / ScreenOffsetProvider.Density);

                UpdateSource();

                return false;
            }

            _selectedPointFeatureId = null;
            UpdateSource();

            return true;
        }

        _polygonDrawn = true;
        var latLng = MaplibreMapService.LatLngFromScreenLocation(x, y);
        var zoom = MaplibreMapService.Zoom;

        var squareArr = MapUtils.GetSquare(new[] { latLng.Lng, latLng.Lat }, 50, zoom);

        _polygonPoints.Clear();
        _polygonPoints.AddRange(squareArr);

        UpdateSource();
        IMapSteadService.NotifyEditorGeometryChanged();

        return true;
    }

    public bool OnMapMove(float x, float y)
    {
        var mapSteadService = ServiceLocator.Resolve<IMapSteadService>();
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapSteadService is null || mapModalService is null) return true;

        if (mapModalService.IsShown)
            mapModalService.Hide();

        if (_prevLatLng is null || MaplibreMapService is null) return true;

        var latLng = MaplibreMapService.LatLngFromScreenLocation(x, y);

        var deltaX = latLng.Lng - _prevLatLng.Lng;
        var deltaY = latLng.Lat - _prevLatLng.Lat;

        if (_movingPointFeatureId is not null)
        {
            var point = _polygonPoints[_movingPointFeatureId.Value];
            _prevLatLng = latLng;

            point[0] += deltaX;
            point[1] += deltaY;

            if (_movingPointFeatureId == 0)
            {
                var lastPoint = _polygonPoints[^1];

                lastPoint[0] += deltaX;
                lastPoint[1] += deltaY;
            }
            else if (_movingPointFeatureId == _polygonPoints.Count - 1)
            {
                var firstPoint = _polygonPoints[0];

                firstPoint[0] += deltaX;
                firstPoint[1] += deltaY;
            }

            UpdateSource();
        }

        if (_polygonMoving)
        {
            _prevLatLng = latLng;

            _polygonPoints.ForEach(point =>
            {
                point[0] += deltaX;
                point[1] += deltaY;
            });

            UpdateSource();
        }

        return true;
    }

    public void OnMapRotate()
    {
    }

    public void StartDrawingWithCoordinates(List<double[]> coordinates, string? steadId, string? customSteadId)
    {
        if (IMapSteadService.IsDrawingModeOn) return;

        _modifyingSteadId = steadId;
        _modifyingCustomSteadId = customSteadId;
        _polygonDrawn = true;
        _polygonPoints.Clear();
        _polygonPoints.AddRange(coordinates);

        IMapSteadService.InvokeSwitchDrawingMode(true);

        UpdateSource();
        IMapSteadService.NotifyEditorGeometryChanged();
    }

    public void Dispose()
    {
        IMapSteadService.OnRemoveVertexClicked -= OnRemoveVertexClicked;
        IMapSteadService.OnDrawingModeSwitch -= OnEditorStateChange;
        IMapSteadService.OnSteadEditorState -= GetState;
    }

    private static bool TryGetIntFromProperty(object value, out int result)
    {
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l:
                result = (int)l;
                return true;
            case double d:
                result = (int)d;
                return true;
            case float f:
                result = (int)f;
                return true;
#if ANDROID
            case Java.Lang.Number num:
                result = num.IntValue();
                return true;
#endif
#if IOS
            case Foundation.NSNumber ns:
                result = ns.Int32Value;
                return true;
#endif
            case string s when int.TryParse(s, out var parsed):
                result = parsed;
                return true;
        }

        result = 0;
        return false;
    }

    private void OnDownVertexLayer(LatLng latLng, Feature feature)
    {
        var mapSteadService = ServiceLocator.Resolve<IMapSteadService>();
        if (mapSteadService is null || MaplibreMapService is null) return;

        if (!feature.Properties.TryGetValue("index", out var vertexIndexObj) || vertexIndexObj is null) return;
        if (!TryGetIntFromProperty(vertexIndexObj, out var vertexIndex)) return;

        _movingPointFeatureId = vertexIndex;

        _prevLatLng = latLng;
        MaplibreMapService.ToggleActions(false);

        UpdateSource();
    }

    private void OnDownAdditionalVertexLayer(LatLng latLng, Feature feature)
    {
        if (MaplibreMapService is null) return;

        if (!feature.Properties.TryGetValue("index", out var vertexIndexObj) || vertexIndexObj is null) return;
        if (!TryGetIntFromProperty(vertexIndexObj, out var value)) return;

        var coordinateElement = feature.Coordinates.FirstOrDefault();
        if (coordinateElement is not double[] vertexCoords) return;

        var midPointIndex = value;
        var newVertexIndex = midPointIndex + 1;

        _polygonPoints.Insert(newVertexIndex, vertexCoords);

        _movingPointFeatureId = newVertexIndex;

        _prevLatLng = latLng;

        MaplibreMapService.ToggleActions(false);
    }

    private void OnDownPolygonLayer(LatLng latLng, Feature _)
    {
        if (!_blazorServicesLoaded || !IMapSteadService.IsTranslateModeOn || MaplibreMapService is null) return;

        Console.WriteLine($"OnDownPolygonLayer {IMapSteadService.IsTranslateModeOn}");

        _polygonMoving = true;
        _prevLatLng = latLng;

        MaplibreMapService.ToggleActions(false);
    }

    private void OnRemoveVertexClicked()
    {
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();

        if (!IMapSteadService.IsDrawingModeOn || _selectedPointFeatureId is null ||
            mapModalService is null) return;

        if (_polygonPoints.Count <= 4) return;

        var length = _polygonPoints.Count;
        _polygonPoints.RemoveAt(_selectedPointFeatureId.Value);

        if (_selectedPointFeatureId == 0)
        {
            _polygonPoints.RemoveAt(_polygonPoints.Count - 1);
            var firstPoint = _polygonPoints[0];
            _polygonPoints.Add(new[] { firstPoint[0], firstPoint[1] });
        }

        if (_selectedPointFeatureId == length - 1)
        {
            _polygonPoints.RemoveAt(0);
            var lastPoint = _polygonPoints[^1];
            _polygonPoints.Insert(0, new[] { lastPoint[0], lastPoint[1] });
        }

        _selectedPointFeatureId = null;
        _movingPointFeatureId = null;
        _prevLatLng = null;

        mapModalService.Hide();
        UpdateSource();
        IMapSteadService.NotifyEditorGeometryChanged();
    }

    private void OnEditorStateChange()
    {
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null) return;

        mapModalService.Hide();

        if (IMapSteadService.IsDrawingModeOn)
            AddSourcesAndLayers();
        else
        {
            _polygonDrawn = false;
            _selectedPointFeatureId = null;
            _polygonMoving = false;
            _prevLatLng = null;
            _movingPointFeatureId = null;
            _modifyingSteadId = null;
            _modifyingCustomSteadId = null;
            _polygonPoints.Clear();

            RemoveSourcesAndLayers();
        }
    }

    private void AddSourcesAndLayers()
    {
        var source = new GeoJsonSource(PolygonSourceId);

        var fillLayer = new FillLayer(PolygonLayerId, PolygonSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.FillColor, "#aa0000" },
                { Properties.FillOpacity, 0.1f }
            },
            Filter = "['==', '$type', 'Polygon']"
        };

        var lineLayer = new LineLayer(LineStringLayerId, PolygonSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.LineColor, "#aa0000" },
                { Properties.LineWidth, 1.0f },
                { Properties.LineDasharray, "['literal', [2, 2]]" }
            },
            Filter = "['==', '$type', 'Polygon']"
        };

        var vertexCircleLayer = new CircleLayer(VerticesLayerId, PolygonSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.CircleRadius, 15.0f },
                { Properties.CircleColor, "#00000000" }
            },
            Filter = "['all', ['==', '$type', 'Point'], ['==', 'vertex', true]]"
        };

        var vertexCircleRenderedLayer = new CircleLayer(VerticesLayerRenderedId, PolygonSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.CircleRadius, "['case', ['boolean', ['get', 'isSelected'], false], 7, 5]" },
                { Properties.CircleColor, "#aa0000" },
                { Properties.CircleStrokeWidth, 2.0f },
                { Properties.CircleStrokeColor, "white" }
            },
            Filter = "['all', ['==', '$type', 'Point'], ['==', 'vertex', true]]"
        };

        var additionalVerticesLayer = new CircleLayer(AdditionalVerticesLayerId, PolygonSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.CircleRadius, 15.0f },
                { Properties.CircleColor, "#00000000" }
            },
            Filter = "['all', ['==', '$type', 'Point'], ['==', 'midpoint', true]]"
        };

        var additionalVerticesRenderedLayer = new CircleLayer(AdditionalVerticesLayerRenderedId, PolygonSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.CircleRadius, 3.0f },
                { Properties.CircleColor, "#aa0000" }
            },
            Filter = "['all', ['==', '$type', 'Point'], ['==', 'midpoint', true]]"
        };

        MaplibreMapService?.AddSource(source);
        MaplibreMapService?.AddLayer(fillLayer);
        MaplibreMapService?.AddLayer(lineLayer);
        MaplibreMapService?.AddLayer(vertexCircleLayer);
        MaplibreMapService?.AddLayer(vertexCircleRenderedLayer);
        MaplibreMapService?.AddLayer(additionalVerticesLayer);
        MaplibreMapService?.AddLayer(additionalVerticesRenderedLayer);
    }

    private void RemoveSourcesAndLayers()
    {
        MaplibreMapService?.RemoveLayer(PolygonLayerId);
        MaplibreMapService?.RemoveLayer(LineStringLayerId);
        MaplibreMapService?.RemoveLayer(VerticesLayerId);
        MaplibreMapService?.RemoveLayer(VerticesLayerRenderedId);
        MaplibreMapService?.RemoveLayer(AdditionalVerticesLayerId);
        MaplibreMapService?.RemoveLayer(AdditionalVerticesLayerRenderedId);
        MaplibreMapService?.RemoveSource(PolygonSourceId);
    }

    private void UpdateSource()
    {
        var medianPoints = MapUtils.CalculateMedianPoints(_polygonPoints.Take(_polygonPoints.Count - 1).ToArray());
        var stringBuilder = new StringBuilder("{ \"type\": \"FeatureCollection\", \"features\": [");

        // VERTEX POINTS
        for (var i = 0; i < _polygonPoints.Count; i++)
        {
            var point = _polygonPoints[i];

            stringBuilder.Append(
                $"{{\"type\": \"Feature\",  \"properties\": {{ \"index\": {i}, \"isSelected\": {$"{i == _selectedPointFeatureId}".ToLower()}, \"vertex\": true }}, \"geometry\": {{ \"type\": \"Point\", \"coordinates\": [{point[0].ToString(_nF)}, {point[1].ToString(_nF)}] }}}},");
        }

        // MIDPOINTS
        for (var i = 0; i < medianPoints.Length; i++)
        {
            var point = medianPoints[i];

            stringBuilder.Append(
                $"{{\"type\": \"Feature\",  \"properties\": {{ \"index\": {i}, \"midpoint\": true }}, \"geometry\": {{ \"type\": \"Point\", \"coordinates\": [{point[0].ToString(_nF)}, {point[1].ToString(_nF)}] }}}},");
        }

        stringBuilder.Append(
            $"{{\"type\": \"Feature\", \"geometry\": {{ \"type\": \"Polygon\", \"coordinates\": [[{string.Join(",", _polygonPoints.Select(x => $"[{x[0].ToString(_nF)}, {x[1].ToString(_nF)}]"))}]]}}}}");

        stringBuilder.Append("]}");
        var featureStr = stringBuilder.ToString();

        MaplibreMapService?.SetGeoJsonFeature(PolygonSourceId, featureStr);
    }

    private SteadEditorStateModel GetState()
    {
        if (!IMapSteadService.IsDrawingModeOn) return new SteadEditorStateModel();

        return new SteadEditorStateModel
        {
            CustomSteadId = _modifyingCustomSteadId,
            SteadId = _modifyingSteadId,
            Coordinates = JsonConvert.SerializeObject(_polygonPoints)
        };
    }
}