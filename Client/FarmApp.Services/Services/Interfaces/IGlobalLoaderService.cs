using FarmApp.Services.Models;

namespace FarmApp.Services.Services.Interfaces;

public interface IGlobalLoaderService
{
    event Action? OnLoaderSwitch;
    bool IsLoading { get; }
    GlobalLoaderModel SwitchOn();
    void SwitchOff();
    void SwitchOffWithoutUpdate();
}