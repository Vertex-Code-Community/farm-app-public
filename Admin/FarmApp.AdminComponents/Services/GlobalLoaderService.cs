using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.AdminComponents.ViewModels;

namespace FarmApp.AdminComponents.Services;

public class GlobalLoaderService : IGlobalLoaderService
{
    public event Action? OnLoaderSwitch;
    private int _counter = 0;
    private int _contentCounter = 0;

    public bool IsLoading { get; private set; }
    public bool IsContentLoading { get; private set; }

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

    public ContentAreaLoaderModel SwitchOnContent()
    {
        if (_contentCounter == 0 && !IsContentLoading)
        {
            IsContentLoading = true;
            OnLoaderSwitch?.Invoke();
        }

        _contentCounter++;

        return new ContentAreaLoaderModel(this);
    }

    public void SwitchOffContent()
    {
        _contentCounter--;
        _contentCounter = _contentCounter < 0 ? 0 : _contentCounter;

        if (_contentCounter == 0 && IsContentLoading)
        {
            IsContentLoading = false;
            OnLoaderSwitch?.Invoke();
        }
    }

    public void SwitchOffContentWithoutUpdate()
    {
        _contentCounter--;
        _contentCounter = _contentCounter < 0 ? 0 : _contentCounter;
    }
}