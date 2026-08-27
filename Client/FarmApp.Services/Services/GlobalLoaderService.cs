using FarmApp.Services.Models;
using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Services;

public class GlobalLoaderService : IGlobalLoaderService, IDisposable
{
    public event Action? OnLoaderSwitch;
    private int _counter = 0;

    public bool IsLoading { get; private set; }
    
    public GlobalLoaderService()
    {
        IMapCallbackService.OnRequestMapEventPermission += AllowMapEvents;
    }
    
    public void Dispose()
    {
        IMapCallbackService.OnRequestMapEventPermission -= AllowMapEvents;
    }

    private bool AllowMapEvents() => !IsLoading;

    public GlobalLoaderModel SwitchOn()
    {
        if (_counter == 0 && !IsLoading)
        {
            IsLoading = true;
            OnLoaderSwitch?.Invoke();
        }
        
        _counter++;

        return new GlobalLoaderModel(this);
    }

    public void SwitchOff()
    {
        _counter--;
        _counter = _counter < 0 ? 0 : _counter;

        if (_counter == 0 && IsLoading)
        {
            IsLoading = false;
            OnLoaderSwitch?.Invoke();
        }
    }

    public void SwitchOffWithoutUpdate()
    {
        _counter--;
        _counter = _counter < 0 ? 0 : _counter;
    }
}