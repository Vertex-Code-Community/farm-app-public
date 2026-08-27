using FarmApp.ViewModels.CustomSteads;
using FarmApp.ViewModels.Map;

namespace FarmApp.Services.Services.Interfaces;

public interface IMapSteadService
{
    static event Action? OnUpdate;
    
    static bool IsDrawingModeOn { get; private set; }
    static bool IsTranslateModeOn { get; private set; }
    static SteadEditorStateModel? SteadEditorState => OnSteadEditorState?.Invoke();
    
    static event Action? OnRemoveVertexClicked;
    static event Action<string?, string?>? OnSteadEdit; 
    static event Action? OnDrawingModeSwitch;
    static event Func<SteadEditorStateModel>? OnSteadEditorState;
    static event Action<List<CustomSteadModel>>? OnDrawCustomSteads;
    static event Action<CustomSteadModel>? OnDrawCustomStead;
    static event Action<string>? OnRemoveCustomStead;
    
    static event Func<string, string?, float, float, Task>? OnSteadClicked;
    static event Func<string, string?, string?, float, float, Task>? OnCustomSteadClicked;

    static void InvokeRemoveVertexClicked()
    {
        OnRemoveVertexClicked?.Invoke();
    }
    
    static void InvokeSteadEdit(string? steadId, string? customSteadId)
    {
        OnSteadEdit?.Invoke(steadId, customSteadId);
    }

    static void InvokeSwitchDrawingMode(bool drawingMode)
    {
        IsDrawingModeOn = drawingMode;

        if (!drawingMode) IsTranslateModeOn = false;
        
        OnDrawingModeSwitch?.Invoke();
        OnUpdate?.Invoke();
    }

    static void InvokeSwitchTranslateMode()
    {
        IsTranslateModeOn = !IsTranslateModeOn;
        OnUpdate?.Invoke();
    }

    static void NotifyEditorGeometryChanged()
    {
        OnUpdate?.Invoke();
    }

    static void InvokeDrawCustomStead(CustomSteadModel steadModel)
    {
        OnDrawCustomStead?.Invoke(steadModel);
    }
    
    static void InvokeRemoveCustomStead(string customSteadId)
    {
        OnRemoveCustomStead?.Invoke(customSteadId);
    }
    
    static void InvokeDrawCustomSteads(List<CustomSteadModel> customSteads)
    {
        OnDrawCustomSteads?.Invoke(customSteads);
    }
    
    static void InvokeSteadClick(string steadId, string? propertyId, float x, float y)
    {
        OnSteadClicked?.Invoke(steadId, propertyId, x, y);
    }
    
    static void InvokeCustomSteadClick(string customSteadId, string? steadId, string? propertyId, float x, float y)
    {
        OnCustomSteadClicked?.Invoke(customSteadId, steadId, propertyId, x, y);
    }
}
