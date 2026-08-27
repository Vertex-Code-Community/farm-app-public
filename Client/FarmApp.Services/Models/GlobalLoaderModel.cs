using FarmApp.Services.Services.Interfaces;

namespace FarmApp.Services.Models;

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