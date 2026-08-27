using FarmApp.Shared.Enums;

namespace FarmApp.Services.Services.Interfaces;

public interface ICountryMapStorage
{
    /// <summary>Persisted map region; null if the user has never saved a choice (use app default).</summary>
    AppMapCountry? Get();

    void Set(AppMapCountry country);

    /// <summary>Fired after a new map country is saved (MAUI: synchronously; web: after localStorage write completes).</summary>
    event Action<AppMapCountry>? MapCountryChanged;
}
