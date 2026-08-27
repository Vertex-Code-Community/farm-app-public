using FarmApp.Shared.Helpers;

namespace FarmApp.Services.Services.Interfaces;

public interface IMapCallbackService
{
    static event Action<float, float>? OnMove;
    static ActionStorage<Task> MapIsLoaded { get; set; } = new();
    static event Action? OnClickOutsideOfStead;
    static event Action? OnClickOutsideOfCustomStead;
    static event Action<float, float, float>? OnFlyTo;
    static event Action? OnClearSelection; 
    static event Func<float, Task>? OnRotate;
    static event Func<float>? OnGetBearing;
    static float Bearing => OnGetBearing?.Invoke() ?? 0;
    static event Action? OnResetBearing;
    static event Func<bool>? OnRequestMapEventPermission;
    
    static void InvokeMove(float x, float y)
    {
        OnMove?.Invoke(x, y);
    }
    
    static void InvokeMapIsLoaded()
    {
        MapIsLoaded.Invoke();
    }
    
    static void InvokeMapRotate(float bearing)
    {
        OnRotate?.Invoke(bearing);
    }
    
    static void InvokeClickOutsideOfStead()
    {
        OnClickOutsideOfStead?.Invoke();
    }
    
    static void InvokeClickOutsideOfCustomStead()
    {
        OnClickOutsideOfCustomStead?.Invoke();
    }

    static void FlyTo(float lng, float lat, float zoom)
    {
        OnFlyTo?.Invoke(lng, lat, zoom);
    }    
    
    static void ResetBearing()
    {
        OnResetBearing?.Invoke();
    }

    static void ClearSelection()
    {
        OnClearSelection?.Invoke();
    }

    static bool MapEventsAllowed()
    {
        var invocationList = OnRequestMapEventPermission?.GetInvocationList();
        if (invocationList is null) return true;

        foreach (var funcDelegate in invocationList)
        {
            if (funcDelegate is Func<bool> func && !func()) return false;
        }

        return true;
    }
}