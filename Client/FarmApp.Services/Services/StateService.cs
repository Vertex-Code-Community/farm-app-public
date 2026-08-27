using System.Linq;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Math;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.PropertyNoteStatuses;
using FarmApp.ViewModels.Weather;
using Microsoft.JSInterop;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace FarmApp.Services.Services;

public class StateService : IStateService
{
    public Task<List<PropertyViewModel>>? PropertiesTask { get; set; }
    public List<PropertyViewModel> Properties { get; } = new();
    public List<PropertyNoteStatusModel> PropertyNoteStatuses { get; } = new();
    public Dictionary<string, List<PropertyNotePreviewModel>> PropertyPreviewNotes { get; } = new();
    public WeatherStateModel? WeatherState { get; set; }

    private readonly IJSRuntime _jsRuntime;
    private TaskCompletionSource _propertiesReadyTcs =
        new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    public Task WhenPropertiesReady => _propertiesReadyTcs.Task;
    public bool ArePropertiesReady => _propertiesReadyTcs.Task.IsCompleted;
    public event Action? OnPropertyNoteAdded;
    public event Action? OnPropertyAdded;
    public event Action? OnPropertiesReady;

    public StateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public PropertyPreviewModel? GetPropertyPreview(string propertyId)
    {
        if (PropertyPreviewNotes.TryGetValue(propertyId, out var notes))
            return new PropertyPreviewModel
            {
                Id = propertyId,
                Notes = notes.ToList()
            };
        
        return null;
    }

    public async Task AddPropertiesAsync(List<PropertyViewModel> properties)
    {
        Properties.Clear();
        PropertyPreviewNotes.Clear();

        properties = properties
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .ToList();

        var features = new List<NetTopologySuite.Features.Feature?>();

        foreach (var property in properties)
        {
            await Task.Run(() =>
            {
                try
                {
                    using var stringReader = new StringReader(property.MultipolygonSerialized);
                    using var jsonReader = new JsonTextReader(stringReader);
            
                    var serializer = GeoJsonSerializer.Create();
                    var feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
                    
                    features.Add(feature);
                    if (feature is null) return;
                    
                    var point = feature.Geometry.Centroid;
                    property.Centroid = new Vec2 { X = point.X, Y = point.Y };

                    GetPolygonDimensionsInKilometers(feature.Geometry, out var width, out var height);
                    var useWidth = width > height;
            
                    var zoom = CalculateZoomForLength(useWidth ? width : height, 
                        useWidth ? ScreenOffsetProvider.ScreenWidth : ScreenOffsetProvider.ScreenHeight);

                    property.Zoom = (float) zoom;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
                
            });
        }

        foreach (var feature in features)
        {
            if (feature is null) continue;
            
            var polygonsCoordinates = new List<double[][]>();
            var geometry = feature.Geometry;

            switch (geometry)
            {
                case Polygon polygon:
                    polygonsCoordinates.Add(
                        polygon.Coordinates.Select(c => new [] { c.X, c.Y }).ToArray());
                    break;
                case MultiPolygon multiPolygon:
                    polygonsCoordinates.AddRange(
                        multiPolygon.Geometries.Select(g => 
                            g.Coordinates.Select(c => new [] { c.X, c.Y }).ToArray()));
                    break;
                default:
                    continue;
            }
            
            var pixelPolygonsCoordinates = IMapPropertyService.InvokeConvertToPixelsSpace(polygonsCoordinates);
            var pictogram = await _jsRuntime.InvokeAsync<string?>("getMultiPolygonPictogram", pixelPolygonsCoordinates);

            var index = features.IndexOf(feature);
            properties[index].PictogramBase64Url = pictogram;
        }
        
        Properties.AddRange(properties);
        EnsureUniquePropertiesById();

        if (_propertiesReadyTcs.TrySetResult())
            OnPropertiesReady?.Invoke();
    }

    public async Task AddPropertyAsync(PropertyViewModel property)
    {
        try
        {
            using var stringReader = new StringReader(property.MultipolygonSerialized);
            await using var jsonReader = new JsonTextReader(stringReader);
            
            var serializer = GeoJsonSerializer.Create();
            var feature = serializer.Deserialize<NetTopologySuite.Features.Feature>(jsonReader);
                    
            if (feature is null) return;
            
            var point = feature.Geometry.Centroid;
            property.Centroid = new Vec2 { X = point.X, Y = point.Y };

            GetPolygonDimensionsInKilometers(feature.Geometry, out var width, out var height);
            var useWidth = width > height;
            
            var zoom = CalculateZoomForLength(useWidth ? width : height, 
                useWidth ? ScreenOffsetProvider.ScreenWidth : ScreenOffsetProvider.ScreenHeight);

            property.Zoom = (float) zoom;
            
            var polygonsCoordinates = new List<double[][]>();
            var geometry = feature.Geometry;

            switch (geometry)
            {
                case Polygon polygon:
                    polygonsCoordinates.Add(
                        polygon.Coordinates.Select(c => new [] { c.X, c.Y }).ToArray());
                    break;
                case MultiPolygon multiPolygon:
                    polygonsCoordinates.AddRange(
                        multiPolygon.Geometries.Select(g => 
                            g.Coordinates.Select(c => new [] { c.X, c.Y }).ToArray()));
                    break;
            }

            if (polygonsCoordinates.Count > 0)
            {
                var pixelPolygonsCoordinates = IMapPropertyService.InvokeConvertToPixelsSpace(polygonsCoordinates);
                var pictogram = await _jsRuntime.InvokeAsync<string?>("getMultiPolygonPictogram", pixelPolygonsCoordinates);

                property.PictogramBase64Url = pictogram;
            }

            Properties.RemoveAll(x => x.Id == property.Id);
            Properties.Add(property);
            EnsureUniquePropertiesById();
            OnPropertyAdded?.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    private void EnsureUniquePropertiesById()
    {
        if (Properties.Count <= 1)
            return;

        var unique = Properties.GroupBy(p => p.Id).Select(g => g.First()).ToList();
        if (unique.Count == Properties.Count)
            return;

        Properties.Clear();
        Properties.AddRange(unique);
    }

    public void DeleteProperty(string id)
    {
        Properties.RemoveAll(x => x.Id == id);
        PropertyPreviewNotes.Remove(id);
    }
    public void AddPropertyNoteStatuses(List<PropertyNoteStatusModel> statuses)
    {
        PropertyNoteStatuses.Clear();
        PropertyNoteStatuses.AddRange(statuses);
    }
    public void AddPropertyNotes(PropertyPreviewModel propertyPreview)
    {
        var uniqueNotes = propertyPreview.Notes
            .GroupBy(n => n.Id)
            .Select(g => g.First())
            .ToList();

        PropertyPreviewNotes.Remove(propertyPreview.Id);
        PropertyPreviewNotes.Add(propertyPreview.Id, uniqueNotes);

        var property = Properties.FirstOrDefault(x => x.Id == propertyPreview.Id);
        if (property is null) return;

        property.HasNotes = uniqueNotes.Count > 0;
    }

    public void AddPropertyNote(PropertyNoteModel propertyNote)
    {
        List<PropertyNotePreviewModel>? notesList = null;
        if (!PropertyPreviewNotes.TryGetValue(propertyNote.PropertyId, out notesList))
        {
            notesList = new List<PropertyNotePreviewModel>();
            if (!PropertyPreviewNotes.TryAdd(propertyNote.PropertyId, notesList))
                return;
        }

        notesList.RemoveAll(x => x.Id == propertyNote.Id);

        notesList.Add(new PropertyNotePreviewModel
        {
            Id = propertyNote.Id,
            Header = propertyNote.Header,
            Text = propertyNote.Text,
            CreatedAt = propertyNote.CreatedAt,
            PreviewMediaId = propertyNote.PreviewMediaId,
            StartTime = propertyNote.StartTime,
            EndTime = propertyNote.EndTime,
            StatusId = propertyNote.StatusId
        });

        var property = Properties.FirstOrDefault(x => x.Id == propertyNote.PropertyId);
        if (property is null) return;

        property.HasNotes = notesList.Count > 0;

        OnPropertyNoteAdded?.Invoke();
    }

    public void UpdatePropertyNote(PropertyNoteModel propertyNote)
    {
        if (!PropertyPreviewNotes.TryGetValue(propertyNote.PropertyId, out var notesList)) return;
        var note = notesList.FirstOrDefault(x => x.Id == propertyNote.Id);

        if (note is null) return;
        note.Header = propertyNote.Header;
        note.Text = propertyNote.Text;
        note.CreatedAt = propertyNote.CreatedAt;
        note.StartTime = propertyNote.StartTime;
        note.EndTime = propertyNote.EndTime;
        note.StatusId = propertyNote.StatusId;
        note.PreviewMediaId = propertyNote.PreviewMediaId;
        note.NotificationsEnabled = propertyNote.NotificationsEnabled;
        note.NotifyBeforeStart = propertyNote.NotifyBeforeStart;
        note.NotifyBeforeEnd = propertyNote.NotifyBeforeEnd;
    }

    public void DeletePropertyNote(string propertyId, string propertyNoteId)
    {
        if (!PropertyPreviewNotes.TryGetValue(propertyId, out var notesList)) return;
        notesList.RemoveAll(x => x.Id == propertyNoteId);

        var property = Properties.FirstOrDefault(x => x.Id == propertyId);
        if (property is null) return;

        property.HasNotes = notesList.Count > 0;
    }
    public void DeletePropertyNoteStatus(int StatusId)
    {
        if (StatusId <= 0)
            return;

        var status = PropertyNoteStatuses.FirstOrDefault(st => st.Id == StatusId);
        if (status == null || status.IsDefault)
            return;

        PropertyNoteStatuses.Remove(status);

        UpdatePropertyNotesOnStatusDelete(StatusId);
    }
    private void UpdatePropertyNotesOnStatusDelete(int StatusId)
    {
        foreach (var notes in PropertyPreviewNotes.Values)
        {
            foreach (var note in notes)
            {
                if (note.StatusId == StatusId)
                {
                    note.StatusId = 0;
                }
            }
        }
    }
    private void ResetPropertiesReady()
    {
        _propertiesReadyTcs =
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    public void Clear()
    {
        PropertiesTask = null;
        Properties.Clear();
        PropertyPreviewNotes.Clear();

        ResetPropertiesReady();
    }

    private static void GetPolygonDimensionsInKilometers(Geometry polygon, out double width, out double height)
    {
        var envelope = polygon.EnvelopeInternal;
        var midLatitude = (envelope.MinY + envelope.MaxY) / 2.0;
        var midLongitude = (envelope.MinX + envelope.MaxX) / 2.0;

        var geographicCoordinateSystem = GeographicCoordinateSystem.WGS84;
        var projectedCoordinateSystem = ProjectedCoordinateSystem.WebMercator;
        var coordinateTransformationFactory = new CoordinateTransformationFactory();
        var transformToProjected = coordinateTransformationFactory.CreateFromCoordinateSystems(geographicCoordinateSystem, projectedCoordinateSystem);

        var westPointProjected = transformToProjected.MathTransform.Transform(new [] { envelope.MinX, midLatitude });
        var eastPointProjected = transformToProjected.MathTransform.Transform(new [] { envelope.MaxX, midLatitude });

        var widthInMeters = Math.Sqrt(Math.Pow(eastPointProjected[0] - westPointProjected[0], 2) + Math.Pow(eastPointProjected[1] - westPointProjected[1], 2));
        
        var northPointProjected = transformToProjected.MathTransform.Transform(new [] { midLongitude, envelope.MinY });
        var southPointProjected = transformToProjected.MathTransform.Transform(new [] { midLongitude, envelope.MaxY });

        var heightInMeters = Math.Sqrt(Math.Pow(southPointProjected[0] - northPointProjected[0], 2) + Math.Pow(southPointProjected[1] - northPointProjected[1], 2));

        width = widthInMeters / 1000.0;
        height = heightInMeters / 1000.0;
    }
    
    private static double CalculateZoomForLength(double lengthInKm, double screenDimension)
    {
        const int earthCircumferenceKm = 40075;
        var initialScale = screenDimension / earthCircumferenceKm;
        var desiredPolygonScreenWidthPx = screenDimension / 2;

        var scaleFactor = desiredPolygonScreenWidthPx / (lengthInKm * initialScale);
        
        return Math.Log(scaleFactor, 2);
    }
}