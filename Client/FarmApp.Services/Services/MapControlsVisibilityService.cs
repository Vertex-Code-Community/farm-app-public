using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Services;

public sealed class MapControlsVisibilityService : IMapControlsVisibilityService
{
    private readonly Dictionary<MapControlButton, bool> _state = new();

    public bool IsVisible(MapControlButton button) =>
        !_state.TryGetValue(button, out var visible) || visible;

    public void SetVisibility(MapControlButton button, bool visible)
    {
        if (IsVisible(button) == visible) return;
        _state[button] = visible;
        Changed?.Invoke();
    }

    public event Action? Changed;
}
