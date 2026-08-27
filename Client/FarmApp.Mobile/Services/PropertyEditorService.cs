using FarmApp.Components.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NetTopologySuite.Geometries;
using MaplibreMaui.Models;
using MaplibreMaui.Models.Layers;
using MaplibreMaui.Models.Sources;
using MaplibreMaui.Services;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Models.Properties;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Map;
using FarmApp.ViewModels.Properties;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Feature = MaplibreMaui.Models.Features.Feature;

namespace FarmApp.Mobile.Services;

public class PropertyEditorService : IPropertyEditorService, IDisposable
{
    private const string PropertyPolygonsSourceId = "property-polygons-source";
    private const string PropertyPolygonsLayerId = "property-polygons-id";
    private const string PropertyLinesLayerId = "property-lines-id";

    public IMaplibreMapService? MaplibreMapService { get; set; }

    // private readonly List<string> _steadIds = new();
    // private readonly List<string> _customSteadIds = new();

    private string? _selectedPropertyId;

    private readonly ICustomSteadsService _customSteadsService;

    public PropertyEditorService(ICustomSteadsService customSteadsService)
    {
        _customSteadsService = customSteadsService;

        IMapPropertyService.OnDrawProperties += OnDrawProperties;
        IMapPropertyService.OnDrawProperty += OnDrawProperty;
        IMapPropertyService.OnRemoveProperty += OnRemoveProperty;
        IMapPropertyService.OnCreationModeSwitch += OnEditorStateChange;
        IMapPropertyService.OnPropertyEditorState += GetState;
        IMapSteadService.OnDrawingModeSwitch += OnSteadEditorStateChange;
        IMapPropertyService.OnConvertToPixelsSpace += ConvertToPixelsScale;
        IMapCallbackService.OnClearSelection += OnClearSelection;
    }

    public void Dispose()
    {
        IMapPropertyService.OnDrawProperties -= OnDrawProperties;
        IMapPropertyService.OnDrawProperty -= OnDrawProperty;
        IMapPropertyService.OnRemoveProperty -= OnRemoveProperty;
        IMapPropertyService.OnCreationModeSwitch -= OnEditorStateChange;
        IMapPropertyService.OnPropertyEditorState -= GetState;
        IMapSteadService.OnDrawingModeSwitch -= OnSteadEditorStateChange;
        IMapPropertyService.OnConvertToPixelsSpace -= ConvertToPixelsScale;
        IMapCallbackService.OnClearSelection -= OnClearSelection;
    }

    public void OnMapReady()
    {
    }

    public void OnStyleLoaded()
    {
        var source = new GeoJsonSource(PropertyPolygonsSourceId);
        
        var fillLayer = new FillLayer(PropertyPolygonsLayerId, PropertyPolygonsSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.FillColor, "#ebcc95" },
                { Properties.FillOpacity, 0.6f }
            },
            Filter = "['==', '$type', 'Polygon']"
        };
        
        var lineLayer = new LineLayer(PropertyLinesLayerId, PropertyPolygonsSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.LineColor, "#7a4616" },
                { Properties.LineWidth, 0.8f }
            }
        };
        
        MaplibreMapService?.AddSource(source);
        MaplibreMapService?.AddLayer(fillLayer);
        MaplibreMapService?.AddLayer(lineLayer);
        
        MaplibreMapService?.SetZoomRange(PropertyPolygonsLayerId, 11, 23);
        MaplibreMapService?.SetZoomRange(PropertyLinesLayerId, 11, 23);
    }

    public void OnBlazorServicesLoaded()
    {
        var navigationService = ServiceLocator.Resolve<INavigationService>();
        if (navigationService is not null) navigationService.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged()
    {
        var navigationService = ServiceLocator.Resolve<INavigationService>();
        if (navigationService is null) return;

        var currentRoute = navigationService.CurrentPage?.Route;
        if (currentRoute == Constants.ClientRoutes.MainPage) return;
        if (_selectedPropertyId is null) return;

        SetSelectedStateForProperty(null);
    }

    public bool OnDown(float x, float y)
    {
        return true;
    }

    public bool OnUp(float x, float y)
    {
        return true;
    }

    public bool OnClick(float x, float y)
    {
        if (IMapPropertyService.IsCreationModeOn)
        {
            var steadFeature = MaplibreMapService?.QueryFeatureByPoint(ISteadService.SteadsLayerId, x, y);
            var customSteadFeature =
                MaplibreMapService?.QueryFeatureByPoint(ICustomSteadsService.CustomSteadPolygonsLayerId, x, y);

            if (customSteadFeature is not null) OnCustomSteadClicked(customSteadFeature);
            else if (steadFeature is not null) OnSteadClicked(steadFeature);
        }
        else
        {
            var propertyFeature = MaplibreMapService?.QueryFeatureByPoint(PropertyPolygonsLayerId, x, y);
            if (propertyFeature is null)
            {
                SetSelectedStateForProperty(null);
                return true;
            }

            var steadFeature = MaplibreMapService?.QueryFeatureByPoint(ISteadService.SteadsLayerId, x, y);
            var customSteadFeature =
                MaplibreMapService?.QueryFeatureByPoint(ICustomSteadsService.CustomSteadPolygonsLayerId, x, y);

            OnPropertyClicked(propertyFeature, steadFeature, customSteadFeature, x, y);
        }

        return true;
    }

    public bool OnMapMove(float x, float y)
    {
        if (!string.IsNullOrEmpty(_selectedPropertyId)) SetSelectedStateForProperty(null);

        return true;
    }

    public void OnMapRotate()
    {
    }

    private void OnEditorStateChange()
    {
        MaplibreMapService?.ToggleDoubleTapActions(!IMapPropertyService.IsCreationModeOn);

        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null) return;

        mapModalService.Hide();
        SetSelectedStateForProperty(null);

        if (IMapPropertyService.IsCreationModeOn) return;

        // reset filters for selected in editor steads and custom steads

        IStateService.SteadIds.Clear();
        IStateService.CustomSteadIds.Clear();

        UpdateFilters();
    }

    private void OnSteadClicked(Feature steadFeature)
    {
        var steadId = steadFeature.Properties["steadId"] as string;
        if (string.IsNullOrEmpty(steadId)) return;

        if (IPropertyEditorService.PropertiesList.Any(p => p.PropertySteads.Any(ps => ps.SteadId == steadId))) return;

        var isFeatureSelected = IStateService.SteadIds.Contains(steadId);

        if (!isFeatureSelected) IStateService.SteadIds.Add(steadId);
        else IStateService.SteadIds.RemoveAt(IStateService.SteadIds.IndexOf(steadId));

        UpdateFilters();
        IMapPropertyService.NotifySelectionChanged();
    }

    private void OnCustomSteadClicked(Feature customSteadFeature)
    {
        var customSteadId = customSteadFeature.Properties["customSteadId"] as string;
        if (string.IsNullOrEmpty(customSteadId)) return;

        if (IPropertyEditorService.PropertiesList.Any(
                p => p.PropertySteads.Any(ps => ps.CustomSteadId == customSteadId))) return;

        var isFeatureSelected = IStateService.CustomSteadIds.Contains(customSteadId);

        if (!isFeatureSelected) IStateService.CustomSteadIds.Add(customSteadId);
        else IStateService.CustomSteadIds.RemoveAt(IStateService.CustomSteadIds.IndexOf(customSteadId));

        UpdateFilters();
        IMapPropertyService.NotifySelectionChanged();
    }

    private void OnPropertyClicked(Feature propertyFeature, Feature? steadFeature, Feature? customSteadFeature, float x,
        float y)
    {
        if (propertyFeature.Properties["propertyId"] is not string propertyId) return;
        SetSelectedStateForProperty(propertyId);

        var steadId = steadFeature?.Properties["steadId"] as string;
        var customSteadId = customSteadFeature?.Properties["customSteadId"] as string;

        IMapPropertyService.InvokePropertyClick(propertyId, steadId, customSteadId,
            x / ScreenOffsetProvider.Density, y / ScreenOffsetProvider.Density);
    }

    private void OnDrawProperties(List<PropertyViewModel> properties)
    {
        IPropertyEditorService.PropertiesList.Clear();
        IPropertyEditorService.PropertiesList.AddRange(properties);
        UpdateSource();
    }

    private void OnDrawProperty(PropertyViewModel property)
    {
        IPropertyEditorService.PropertiesList.Add(property);
        UpdateSource();
    }

    private void OnRemoveProperty(string propertyId)
    {
        IPropertyEditorService.PropertiesList.RemoveAll(x => x.Id == propertyId);
        UpdateSource();
    }

    private void UpdateSource()
    {
        try
        {
            var features = IPropertyEditorService.PropertiesList.Select(propertyFeature =>
            {
                using var stringReader = new StringReader(propertyFeature.MultipolygonSerialized);
                using var jsonReader = new JsonTextReader(stringReader);

                var serializer = GeoJsonSerializer.Create();
                var feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
                feature?.Attributes?.Add("propertyId", propertyFeature.Id);

                return feature;
            }).ToList();

            var featureCollection = new FeatureCollection();

            foreach (var f in features)
                featureCollection.Add(f);

            var writer = new GeoJsonWriter();
            var json = writer.Write(featureCollection);

            MaplibreMapService?.SetGeoJsonFeature(PropertyPolygonsSourceId, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    private void UpdateFilters()
    {
        MaplibreMapService?.SetProperty(ISteadService.SteadsLayerId, Properties.FillOpacity,
            $"['case', ['in', ['get', 'steadId'], ['literal', [{string.Join(", ", IStateService.SteadIds.Select(x => $"'{x}'"))}]]], 0.3, 0.1]");

        MaplibreMapService?.SetProperty(ICustomSteadsService.CustomSteadPolygonsLayerId, Properties.FillOpacity,
            $"['case', ['in', ['get', 'customSteadId'], ['literal', [{string.Join(", ", IStateService.CustomSteadIds.Select(x => $"'{x}'"))}]]], 0.3, 0.1]");
    }

    private void SetSelectedStateForProperty(string? propertyId)
    {
        _selectedPropertyId = propertyId;

        MaplibreMapService?.SetProperty(PropertyPolygonsLayerId, Properties.FillOpacity,
            $"['case', ['==', ['get', 'propertyId'], '{propertyId ?? string.Empty}'], 0.8, 0.6]");
    }

    private PropertyEditorStateModel? GetState()
    {
        try
        {
            if (MaplibreMapService is null) return null;

            var steadCoordinates = new List<Coordinate[]>();

            foreach (var steadId in IStateService.SteadIds)
            {
                var steadFeaturesSerialized = MaplibreMapService
                    .QuerySourceFeaturesAsJson(ISteadService.SteadsSourceId, ISteadService.SteadPolygonsSourceLayer,
                        $"['==', 'steadId', '{steadId}']");

                var combinedSteadGeometry = NetTopologySuiteUtils.CombineFeatures(steadFeaturesSerialized);
                if (combinedSteadGeometry is null) continue;

                steadCoordinates.Add(combinedSteadGeometry.Coordinates);
            }

            var customSteads = _customSteadsService.CustomSteads
                .Where(c => IStateService.CustomSteadIds.Contains(c.Id))
                .ToList();

            var customSteadCoordinates = customSteads
                .Select(c => NetTopologySuiteUtils.ConvertToCoordinates(
                    JsonConvert.DeserializeObject<double[][]>(c.Coordinates) ?? new double[][] { }))
                .ToList();

            var allCoordinates = steadCoordinates.ToList();
            allCoordinates.AddRange(customSteadCoordinates);

            if (!string.IsNullOrEmpty(IMapPropertyService.PendingDrawnCustomSteadCoordinates))
            {
                try
                {
                    var pendingRing = JsonConvert.DeserializeObject<double[][]>(
                        IMapPropertyService.PendingDrawnCustomSteadCoordinates);
                    if (pendingRing is { Length: >= 4 })
                        allCoordinates.Add(NetTopologySuiteUtils.ConvertToCoordinates(pendingRing));
                }
                catch
                {
                    // ignored — invalid pending JSON
                }
            }

            var totalAreaHectares = allCoordinates
                .Sum(c => NetTopologySuiteUtils.GetAreaOfPolygon(c) / 10000);

            var geometries = allCoordinates
                .Select(coords =>
                    new Polygon(new LinearRing(coords)))
                .ToList();

            // var combinedGeometry = NetTopologySuiteUtils.CombineGeometries(geometries);
            // if (combinedGeometry is not Polygon combinedGeometryPolygon)
            // {
            //     Console.WriteLine("GetState, combinedGeometryPolygon is null");
            //     
            //     return null;
            // }

            // Console.WriteLine($"bufferedCombinedGeometry COUNT = {combinedGeometryPolygon.Coordinates.Length}");

            var featuresSerialized = geometries.Select(x => JsonConvert.SerializeObject(new PropertyFeature
            {
                Type = "Feature",
                Geometry = new PropertyGeometry
                {
                    Type = "Polygon",
                    Coordinates = new List<double[][]>
                    {
                        x.Coordinates.Select(c => new[] { c.X, c.Y }).ToArray()
                    }
                }
            }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            })).ToList();

            return new PropertyEditorStateModel
            {
                SteadIds = IStateService.SteadIds.ToList(),
                CustomSteadIds = IStateService.CustomSteadIds.ToList(),
                Features = featuresSerialized,
                // MultipolygonSerialized = JsonConvert.SerializeObject(new PropertyFeature
                // {
                //     Type = "Feature",
                //     Geometry = new PropertyGeometry
                //     {
                //         Type = "Polygon",
                //         Coordinates = new List<double[][]>
                //         {
                //             combinedGeometryPolygon.Coordinates.Select(
                //                 x => new[] { x.X, x.Y }).ToArray()
                //         }
                //     }
                // }, new JsonSerializerSettings
                // {
                //     ContractResolver = new CamelCasePropertyNamesContractResolver(),
                //     Formatting = Formatting.Indented
                // }),
                Area = (float)totalAreaHectares
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            var notificationService = ServiceLocator.Resolve<INotificationService>();
            if (notificationService is null) return null;

            notificationService.Add("Помилка", $"{e.Message}\n{e.StackTrace}");
        }

        return null;
    }

    private void OnSteadEditorStateChange()
    {
        if (!IMapSteadService.IsDrawingModeOn) return;
        SetSelectedStateForProperty(null);
    }

    private List<double[][]> ConvertToPixelsScale(List<double[][]> polygons)
    {
        var list = new List<double[][]>();
        if (MaplibreMapService is null) return list;

        foreach (var polygon in polygons)
        {
            if (polygon.Length == 0) continue;

            var pixelCoords = polygon
                .Select(MaplibreMapService.ScreenLocationFromPoint)
                .ToArray();

            list.Add(pixelCoords);
        }

        return list;
    }

    private void OnClearSelection()
    {
        if (!IMapSteadService.IsDrawingModeOn) return;
        SetSelectedStateForProperty(null);

        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null || MaplibreMapService is null) return;

        mapModalService.Hide();
    }
}