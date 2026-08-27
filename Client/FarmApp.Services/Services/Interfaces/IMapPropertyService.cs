using FarmApp.Services.Models.Properties;
using FarmApp.ViewModels.Map;
using FarmApp.ViewModels.Properties;

namespace FarmApp.Services.Services.Interfaces;

public interface IMapPropertyService
{
    static bool IsCreationModeOn { get; private set; }
    static int WebPropertyParcelSelectionCount { get; private set; }

    /// <summary>True while the bottom-sheet UI for naming/creating a field is open (after confirm on map).</summary>
    static bool IsFieldCreateModalOpen { get; private set; }

    static void SetFieldCreateModalOpen(bool open)
    {
        if (IsFieldCreateModalOpen == open) return;
        IsFieldCreateModalOpen = open;
        OnUpdate?.Invoke();
    }

    /// <summary>Polygon ring JSON for a drawn field boundary; no separate custom-stead record is created.</summary>
    static string? PendingDrawnCustomSteadCoordinates { get; set; }

    static bool HasPendingDrawnCustomSteadForProperty =>
        !string.IsNullOrEmpty(PendingDrawnCustomSteadCoordinates);

    static void ClearPendingDrawnCustomSteadForProperty()
    {
        PendingDrawnCustomSteadCoordinates = null;
    }

    static PropertyEditorStateModel? PropertyEditorState => OnPropertyEditorState?.Invoke();
    
    static event Func<PropertyEditorStateModel?>? OnPropertyEditorState;
    static event Func<string, string?, string?, float, float, Task>? OnPropertyClicked;
    
    static event Action? OnCreationModeSwitch;
    static event Action? OnUpdate;
    
    static event Action<List<PropertyViewModel>>? OnDrawProperties;
    static event Action<PropertyViewModel>? OnDrawProperty;
    static event Action<string>? OnRemoveProperty;
    static event Func<List<double[][]>, List<double[][]>?>? OnConvertToPixelsSpace;
    
    static void InvokeSwitchCreationMode(bool mode)
    {
        IsCreationModeOn = mode;
        if (!mode)
            WebPropertyParcelSelectionCount = 0;
        OnCreationModeSwitch?.Invoke();
        OnUpdate?.Invoke();
    }

    static void SetWebPropertyParcelSelectionCount(int count)
    {
        WebPropertyParcelSelectionCount = count;
        OnUpdate?.Invoke();
    }

    static void NotifySelectionChanged()
    {
        OnUpdate?.Invoke();
    }
    
    static void InvokeDrawProperty(PropertyViewModel property)
    {
        OnDrawProperty?.Invoke(property);
    }
    
    static void InvokeRemoveProperty(string propertyId)
    {
        OnRemoveProperty?.Invoke(propertyId);
    }
    
    static void InvokeDrawProperties(List<PropertyViewModel> properties)
    {
        OnDrawProperties?.Invoke(properties);
    }
    
    static void InvokePropertyClick(string propertyId, string? steadId, string? customSteadId, float x, float y)
    {
        OnPropertyClicked?.Invoke(propertyId, steadId, customSteadId, x, y);
    }

    static List<double[][]>? InvokeConvertToPixelsSpace(List<double[][]> polygons)
    {
        return OnConvertToPixelsSpace?.Invoke(polygons);
    }
}