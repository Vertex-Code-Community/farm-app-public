using FarmApp.AdminComponents.Services.Interfaces;

namespace FarmApp.AdminComponents.ViewModels;

public class GlobalLoaderModel : IDisposable
{
    private readonly IGlobalLoaderService _globalLoaderService;

    public GlobalLoaderModel(IGlobalLoaderService globalLoaderService)
    {
        _globalLoaderService = globalLoaderService;
    }

    public void Dispose()
    {
        _globalLoaderService.SwitchOff();
    }
}

public class ContentAreaLoaderModel : IDisposable
{
    private readonly IGlobalLoaderService _globalLoaderService;

    public ContentAreaLoaderModel(IGlobalLoaderService globalLoaderService)
    {
        _globalLoaderService = globalLoaderService;
    }

    public void Dispose()
    {
        _globalLoaderService.SwitchOffContent();
    }
}