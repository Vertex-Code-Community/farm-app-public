using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Enums;

namespace FarmApp.Mobile.Services;

public sealed class CountryMapStorage : ICountryMapStorage
{
    private const string Key = "app_map_country";

    public event Action<AppMapCountry>? MapCountryChanged;

    public AppMapCountry? Get()
    {
        if (!Preferences.ContainsKey(Key))
            return null;

        var value = Preferences.Get(Key, string.Empty);
        return Enum.TryParse<AppMapCountry>(value, out var c) ? c : null;
    }

    public void Set(AppMapCountry country)
    {
        Preferences.Set(Key, country.ToString());
        MapCountryChanged?.Invoke(country);
    }
}
