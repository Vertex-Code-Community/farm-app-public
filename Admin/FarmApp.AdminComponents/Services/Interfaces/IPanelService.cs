namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IPanelService
{
    bool IsShown { get; }
    event Action? OnUpdate;
    void Show(bool isShown);
}
