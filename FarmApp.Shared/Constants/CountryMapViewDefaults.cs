using FarmApp.Shared.Enums;

namespace FarmApp.Shared.Constants;

/// <summary>Default map camera per selected country (lng, lat, zoom) for planet vector tiles.</summary>
public static class CountryMapViewDefaults
{
    public static (float Lng, float Lat, float Zoom) Get(AppMapCountry country) =>
        country switch
        {
            AppMapCountry.Ukraine => (31.5f, 48.8f, 5.5f),
            AppMapCountry.UnitedStates => (-98.5f, 39.5f, 3.6f),
            AppMapCountry.Italy => (12.5f, 42.5f, 5.5f),
            AppMapCountry.Poland => (19.2f, 51.8f, 5.8f),
            AppMapCountry.Germany => (10.3f, 51.1f, 5.5f),
            _ => (31.5f, 48.8f, 5.5f)
        };
}
