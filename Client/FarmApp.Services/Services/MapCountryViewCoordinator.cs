using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Enums;

namespace FarmApp.Services.Services;

/// <summary>
/// Applies the saved country view when the map first finishes loading. Later style reloads
/// (e.g. theme) do not reset the camera.
/// </summary>
public sealed class MapCountryViewCoordinator : IDisposable
{
    private readonly ICountryMapStorage _storage;
    private bool _initialCountryViewApplied;

    public MapCountryViewCoordinator(ICountryMapStorage storage)
    {
        _storage = storage;
        IMapCallbackService.MapIsLoaded.Subscribe(OnMapIsLoaded);
    }

    private Task OnMapIsLoaded()
    {
        if (_initialCountryViewApplied)
            return Task.CompletedTask;

        _initialCountryViewApplied = true;
        var country = _storage.Get() ?? AppMapCountry.Ukraine;
        var (lng, lat, z) = CountryMapViewDefaults.Get(country);
        IMapCallbackService.FlyTo(lng, lat, z);
        return Task.CompletedTask;
    }

    public void Dispose() =>
        IMapCallbackService.MapIsLoaded.Unsubscribe(OnMapIsLoaded);
}
