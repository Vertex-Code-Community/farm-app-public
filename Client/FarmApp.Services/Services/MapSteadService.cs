using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.Services.Services;

public class MapSteadService : IMapSteadService, IDisposable
{
    private readonly ICustomSteadService _customSteadService;
    private readonly AuthStateProvider _authenticationStateProvider;
    private readonly IGlobalLoaderService _globalLoader;
    public MapSteadService(ICustomSteadService customSteadService,
        AuthStateProvider authenticationStateProvider,
        IGlobalLoaderService globalLoader)
    {
        _customSteadService = customSteadService;
        _authenticationStateProvider = authenticationStateProvider;

        IMapCallbackService.MapIsLoaded.Subscribe(OnMapLoadedAsync);
        _authenticationStateProvider.OnSignInAsync += OnUserSignInAsync;
        _globalLoader = globalLoader;
    }

    public void Dispose()
    {
        IMapCallbackService.MapIsLoaded.Unsubscribe(OnMapLoadedAsync);
        _authenticationStateProvider.OnSignInAsync -= OnUserSignInAsync;
    }

    private async Task OnMapLoadedAsync()
    {
        var customSteads = await _customSteadService.GetAllOfCurrentUserAsync();
        IMapSteadService.InvokeDrawCustomSteads(customSteads);
    }
    
    private async Task OnUserSignInAsync(bool signIn)
    {
        if (signIn) await OnMapLoadedAsync();
        else IMapSteadService.InvokeDrawCustomSteads(new List<CustomSteadModel>());
    }
}
