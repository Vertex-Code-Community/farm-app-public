using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.PropertyNoteStatuses;
using FarmApp.ViewModels.Weather;

namespace FarmApp.Services.Services.Interfaces;

public interface IStateService
{
    static List<string> SteadIds { get; } = new();
    static List<string> CustomSteadIds { get; } = new();
    Task WhenPropertiesReady { get; }
    bool ArePropertiesReady { get; }
    Task<List<PropertyViewModel>>? PropertiesTask { get; set; }
    List<PropertyViewModel> Properties { get; }
    WeatherStateModel? WeatherState { get; set; }
    Dictionary<string, List<PropertyNotePreviewModel>> PropertyPreviewNotes { get; }
    List<PropertyNoteStatusModel> PropertyNoteStatuses { get; }
    event Action? OnPropertyNoteAdded;
    event Action? OnPropertyAdded;
    /// <summary>Fired once when the initial properties list has been loaded into state (map / GetPropertiesAsync completed).</summary>
    event Action? OnPropertiesReady;
    PropertyPreviewModel? GetPropertyPreview(string propertyId);
    Task AddPropertiesAsync(List<PropertyViewModel> properties);
    Task AddPropertyAsync(PropertyViewModel property);
    void DeleteProperty(string id);
    void AddPropertyNoteStatuses(List<PropertyNoteStatusModel> statuses);
    void AddPropertyNotes(PropertyPreviewModel propertyPreview);
    void AddPropertyNote(PropertyNoteModel propertyNote);
    void UpdatePropertyNote(PropertyNoteModel propertyNote);
    void DeletePropertyNote(string propertyId, string propertyNoteId);
    void DeletePropertyNoteStatus(int statusId);
    void Clear();
}