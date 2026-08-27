using FarmApp.AdminComponents.ViewModels;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IGlobalLoaderService
{
    event Action? OnLoaderSwitch;

    bool IsLoading { get; }
    GlobalLoaderModel SwitchOn();
    void SwitchOff();
    void SwitchOffWithoutUpdate();
    bool IsContentLoading { get; }
    ContentAreaLoaderModel SwitchOnContent();
    void SwitchOffContent();
    void SwitchOffContentWithoutUpdate();
}