using MaplibreMaui.Models;
using MaplibreMaui.Models.Layers;
using MaplibreMaui.Models.Sources;
using MaplibreMaui.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.Mobile.Services;

public class CustomSteadsService : ICustomSteadsService, IDisposable
{
    public IMaplibreMapService? MaplibreMapService { get; set; }
    
    private readonly ISteadEditorService _steadEditorService;
    private string? _selectedCustomSteadId;

    public List<CustomSteadModel> CustomSteads { get; set; } = new();
    
    public CustomSteadsService(ISteadEditorService steadEditorService)
    {
        _steadEditorService = steadEditorService;
        
        IMapSteadService.OnDrawCustomSteads += OnDrawCustomSteads;
        IMapSteadService.OnDrawCustomStead += OnDrawCustomStead;
        IMapSteadService.OnRemoveCustomStead += OnRemoveCustomStead;
        IMapSteadService.OnSteadEdit += OnCustomSteadEdit;
        
        IMapPropertyService.OnCreationModeSwitch += OnPropertyEditorStateChange;
        IMapSteadService.OnDrawingModeSwitch += OnSteadEditorStateChange;
        IMapCallbackService.OnClearSelection += OnClearSelection;
    }
    
    public void Dispose()
    {
        IMapSteadService.OnDrawCustomSteads -= OnDrawCustomSteads;
        IMapSteadService.OnDrawCustomStead -= OnDrawCustomStead;
        IMapSteadService.OnRemoveCustomStead -= OnRemoveCustomStead;
        IMapSteadService.OnSteadEdit -= OnCustomSteadEdit;
        
        IMapPropertyService.OnCreationModeSwitch -= OnPropertyEditorStateChange;
        IMapSteadService.OnDrawingModeSwitch -= OnSteadEditorStateChange;
        IMapCallbackService.OnClearSelection -= OnClearSelection;
    }
    
    public void OnMapReady()
    {
    }

    public void OnStyleLoaded()
    {
        var source = new GeoJsonSource(ICustomSteadsService.CustomSteadPolygonsSourceId);
        
        var fillLayer = new FillLayer(ICustomSteadsService.CustomSteadPolygonsLayerId, ICustomSteadsService.CustomSteadPolygonsSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.FillColor, "#008800" },
                { Properties.FillOpacity, 0.1f }
            },
            Filter = "['==', '$type', 'Polygon']"
        };
        
        var lineLayer = new LineLayer(ICustomSteadsService.CustomSteadLinesLayerId, ICustomSteadsService.CustomSteadPolygonsSourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.LineColor, "#008c00" },
                { Properties.LineWidth, 0.5f }
            }
        };
        
        MaplibreMapService?.AddSource(source);
        MaplibreMapService?.AddLayer(fillLayer);
        MaplibreMapService?.AddLayer(lineLayer);
        
        MaplibreMapService?.SetZoomRange(ICustomSteadsService.CustomSteadPolygonsLayerId, 11, 23);
        MaplibreMapService?.SetZoomRange(ICustomSteadsService.CustomSteadLinesLayerId, 11, 23);
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
        if (_selectedCustomSteadId is null) return;
        
        SetSelectedStateForCustomStead(null);
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
        if (IMapSteadService.IsDrawingModeOn || IMapPropertyService.IsCreationModeOn) return true;
        
        var properties = MaplibreMapService?.QueryFeaturePropertiesByPoint(ICustomSteadsService.CustomSteadPolygonsLayerId, x, y);
        
        if (properties is null)
        {
            SetSelectedStateForCustomStead(null);
            IMapCallbackService.InvokeClickOutsideOfCustomStead();
        }
        else
        {
            var steadId = properties["steadId"] as string;
            var customSteadId = properties["customSteadId"] as string;
            if (string.IsNullOrEmpty(customSteadId)) return true;
            
            var property = IPropertyEditorService.PropertiesList.FirstOrDefault(p =>
                p.PropertySteads.Any(ps => ps.CustomSteadId == customSteadId));

            if (property is not null) return true;
            
            SetSelectedStateForCustomStead(customSteadId); // TODO: Add propertyId
            IMapSteadService.InvokeCustomSteadClick(customSteadId, steadId, null, 
                x / ScreenOffsetProvider.Density, y / ScreenOffsetProvider.Density);
        }
        
        return true;
    }

    public bool OnMapMove(float x, float y)
    {
        if (!string.IsNullOrEmpty(_selectedCustomSteadId)) SetSelectedStateForCustomStead(null);
        IMapCallbackService.InvokeMove(x / ScreenOffsetProvider.Density, y / ScreenOffsetProvider.Density);
        
        return true;
    }
    
    public void OnMapRotate()
    {
    }

    private void OnDrawCustomSteads(List<CustomSteadModel> customSteads)
    {
        CustomSteads.Clear();
        CustomSteads.AddRange(customSteads);
        UpdateFilters();
        UpdateSource();
    }
    
    private void OnDrawCustomStead(CustomSteadModel customStead)
    {
        CustomSteads.Add(customStead);
        UpdateFilters();
        UpdateSource();
    }

    private void OnRemoveCustomStead(string customSteadId)
    {
        CustomSteads.RemoveAll(x => x.Id == customSteadId);
        UpdateFilters();
        UpdateSource();
    }

    private void UpdateFilters()
    {
        var steadIds = CustomSteads.Select(x => x.SteadId);
        var filter = $"['!in', 'steadId', {string.Join(", ", steadIds.Select(x => $"'{x}'"))}]";
        
        MaplibreMapService?.SetFilter(ISteadService.SteadsLayerId, filter);
        MaplibreMapService?.SetFilter(ISteadService.LinesLayerId, filter);
    }
    
    private void UpdateSource()
    {
        var featureCollection = new {
            Type = "FeatureCollection",
            Features = CustomSteads.Select((customStead, index) => new
            {
                Id = index + 1,
                Type = "Feature",
                Geometry = new
                {
                    Type = "Polygon",
                    Coordinates = new [] { JsonConvert.DeserializeObject<double[][]>(customStead.Coordinates) }
                },
                Properties = new
                {
                    CustomSteadId = customStead.Id,
                    SteadId = customStead.SteadId
                }
            }).ToList()
        };
        
        var featureCollectionStr = JsonConvert.SerializeObject(featureCollection, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        });

        MaplibreMapService?.SetGeoJsonFeature(ICustomSteadsService.CustomSteadPolygonsSourceId, featureCollectionStr);
    }
    
    private void SetSelectedStateForCustomStead(string? customSteadId)
    {
        _selectedCustomSteadId = customSteadId;
        
        MaplibreMapService?.SetProperty(ICustomSteadsService.CustomSteadPolygonsLayerId, Properties.FillOpacity,
            $"['case', ['==', ['get', 'customSteadId'], '{customSteadId ?? string.Empty}'], 0.3, 0.1]");
    }
    
    private void OnCustomSteadEdit(string? steadId, string? customSteadId)
    {
        if (string.IsNullOrEmpty(customSteadId) || !string.IsNullOrEmpty(steadId)) return;
        
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null || MaplibreMapService is null) return;
        
        mapModalService.Hide();

        var customStead = CustomSteads.FirstOrDefault(x => x.Id == customSteadId);
        if (customStead is null) return;

        try
        {
            var coordinates = JsonConvert.DeserializeObject<List<double[]>>(customStead.Coordinates);
            if (coordinates is null) return;
            
            _steadEditorService.StartDrawingWithCoordinates(coordinates, null, customSteadId);
        }
        catch
        {
            // ignored
        }
    }
    
    private void OnSteadEditorStateChange()
    {
        if (!IMapSteadService.IsDrawingModeOn) return;
        SetSelectedStateForCustomStead(null);
    }
    
    private void OnPropertyEditorStateChange()
    {
        if (!IMapPropertyService.IsCreationModeOn) return;
        SetSelectedStateForCustomStead(null);
    }
    
    private void OnClearSelection()
    {
        if (!IMapPropertyService.IsCreationModeOn) return;
        SetSelectedStateForCustomStead(null);
        
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null || MaplibreMapService is null) return;
        
        mapModalService.Hide();
    }
}