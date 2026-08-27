using FarmApp.AdminComponents.Services.Interfaces;

namespace FarmApp.AdminComponents.Services;

public class PanelService : IPanelService
{
    public bool IsShown { get; private set; } = true;
    public event Action? OnUpdate;

    public void Show(bool isShown)
    {
        IsShown = isShown;
        OnUpdate?.Invoke();
    }
}
