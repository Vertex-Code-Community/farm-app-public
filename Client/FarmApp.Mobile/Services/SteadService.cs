using FarmApp.Components.Helpers;
using MaplibreMaui.Models;
using MaplibreMaui.Models.Layers;
using MaplibreMaui.Models.Sources;
using MaplibreMaui.Services;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;

namespace FarmApp.Mobile.Services;

public class SteadService : ISteadService, IDisposable
{
    public IMaplibreMapService? MaplibreMapService { get; set; }

    private readonly ISteadEditorService _steadEditorService;

    private string? _selectedSteadId = null;
    
    public SteadService(ISteadEditorService steadEditorService)
    {
        _steadEditorService = steadEditorService;
        IMapSteadService.OnSteadEdit += OnSteadEdit;
        IMapPropertyService.OnCreationModeSwitch += OnPropertyEditorStateChange;
        IMapSteadService.OnDrawingModeSwitch += OnSteadEditorStateChange;
        IMapCallbackService.OnClearSelection += OnClearSelection;
    }

    public void OnStyleLoaded()
    {
        var steadSource = new VectorSource(ISteadService.SteadsSourceId)
        {
            Tiles = new List<string> { "https://YOUR-VECTOR-TILES-HOST-HERE/farm-app-vector-tiles/{z}/tile_{z}_{x}_{y}.mvt" },
            MinZoom = 11,
            MaxZoom = 14
        };
        
        var fillLayer = new FillLayer(ISteadService.SteadsLayerId, ISteadService.SteadsSourceId)
        {
            SourceLayer = ISteadService.SteadPolygonsSourceLayer,
            Properties = new Dictionary<string, object?>
            {
                { Properties.FillColor, "#008800" },
                { Properties.FillOpacity, "['case', ['boolean', ['feature-state', 'selected'], false], 0.8, 0.1]" }
            }
        };
        
        // "['case', ['boolean', ['feature-state', 'selected'], false], 0.8, 0.1]"
        
        var lineLayer = new LineLayer(ISteadService.LinesLayerId, ISteadService.SteadsSourceId)
        {
            SourceLayer = ISteadService.SteadPolygonsSourceLayer,
            Properties = new Dictionary<string, object?>
            {
                { Properties.LineColor, "#008C00" },
                { Properties.LineWidth, 0.5f }
            }
        };
        
        MaplibreMapService?.AddSource(steadSource);
        MaplibreMapService?.AddLayer(fillLayer);
        MaplibreMapService?.AddLayer(lineLayer);
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
        if (_selectedSteadId is null) return;
        
        SetSelectedStateForStead(null);
    }

    public bool OnDown(float x, float y)
    {
        MaplibreMapService?.CancelTransitions();
        return true;
    }

    public bool OnUp(float x, float y)
    {
        return true;
    }

    public bool OnClick(float x, float y)
    {
        if (IMapSteadService.IsDrawingModeOn || IMapPropertyService.IsCreationModeOn) return true;
        
        var steadId = MaplibreMapService?.QueryFeaturePropertyByPoint(ISteadService.SteadsLayerId, "steadId", x, y);

        if (string.IsNullOrEmpty(steadId))
        {
            SetSelectedStateForStead(null);
            IMapCallbackService.InvokeClickOutsideOfStead();
        }
        else
        {
            var property = IPropertyEditorService.PropertiesList.FirstOrDefault(p =>
                    p.PropertySteads.Any(ps => ps.SteadId == steadId));

            if (property is not null) return true;
            
            SetSelectedStateForStead(steadId);
            IMapSteadService.InvokeSteadClick(steadId, null, x / ScreenOffsetProvider.Density, y / ScreenOffsetProvider.Density);
        }
        
        return true;
    }

    public bool OnMapMove(float x, float y)
    {
        if (!string.IsNullOrEmpty(_selectedSteadId)) SetSelectedStateForStead(null);
        IMapCallbackService.InvokeMove(x / ScreenOffsetProvider.Density, y / ScreenOffsetProvider.Density);

        return true;
    }

    public void OnMapRotate()
    {
        if (MaplibreMapService is null) return;
        IMapCallbackService.InvokeMapRotate((float) MaplibreMapService.Bearing);
    }

    public void Dispose()
    {
        IMapSteadService.OnSteadEdit -= OnSteadEdit;
        IMapPropertyService.OnCreationModeSwitch -= OnPropertyEditorStateChange;
        IMapSteadService.OnDrawingModeSwitch -= OnSteadEditorStateChange;
        IMapCallbackService.OnClearSelection -= OnClearSelection;
    }
    
    private void OnSteadEdit(string? steadId, string? customSteadId)
    {
        if (!string.IsNullOrEmpty(customSteadId)) return;
        
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null || MaplibreMapService is null) return;
        
        mapModalService.Hide();

        var steadFeatures = MaplibreMapService
            .QuerySourceFeaturesAsJson(ISteadService.SteadsSourceId, ISteadService.SteadPolygonsSourceLayer, $"['==', 'steadId', '{steadId}']");
        // TODO: check for exceptions for CombineFeatures
        var combinedGeometry = NetTopologySuiteUtils.CombineFeatures(steadFeatures);
        if (combinedGeometry is null) return;
        
        var coordinates = combinedGeometry.Coordinates;
        var coords = coordinates.Select(c => new [] { c.X, c.Y }).ToList();
        
        _steadEditorService.StartDrawingWithCoordinates(coords, steadId, null);
    }

    private void SetSelectedStateForStead(string? steadId)
    {
        _selectedSteadId = steadId;
        
        MaplibreMapService?.SetProperty(ISteadService.SteadsLayerId, Properties.FillOpacity,
            $"['case', ['==', ['get', 'steadId'], '{steadId ?? string.Empty}'], 0.3, 0.1]");
    }

    private void OnSteadEditorStateChange()
    {
        if (!IMapSteadService.IsDrawingModeOn) return;
        SetSelectedStateForStead(null);
    }
    
    private void OnPropertyEditorStateChange()
    {
        if (!IMapPropertyService.IsCreationModeOn) return;
        SetSelectedStateForStead(null);
    }

    private void OnClearSelection()
    {
        if (!IMapPropertyService.IsCreationModeOn) return;
        SetSelectedStateForStead(null);
        
        var mapModalService = ServiceLocator.Resolve<IMapModalService>();
        if (mapModalService is null || MaplibreMapService is null) return;
        
        mapModalService.Hide();
    }
}