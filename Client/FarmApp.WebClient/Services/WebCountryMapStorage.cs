using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Enums;
using Microsoft.JSInterop;

namespace FarmApp.WebClient.Services;

public sealed class WebCountryMapStorage : ICountryMapStorage
{
    private readonly IJSRuntime _js;

    public WebCountryMapStorage(IJSRuntime js) => _js = js;

    public event Action<AppMapCountry>? MapCountryChanged;

    public AppMapCountry? Get()
    {
        var s = _js.InvokeAsync<string?>("countryMapStorage.get").Result;
        if (string.IsNullOrEmpty(s))
            return null;
        return Enum.TryParse<AppMapCountry>(s, out var c) ? c : null;
    }

    public void Set(AppMapCountry country) =>
        _ = SetAsync(country);

    private async Task SetAsync(AppMapCountry country)
    {
        await _js.InvokeVoidAsync("countryMapStorage.set", country.ToString());
        MapCountryChanged?.Invoke(country);
    }
}
