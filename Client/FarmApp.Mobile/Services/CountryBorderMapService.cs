using System.IO;
using FarmApp.Mobile.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Enums;
using MaplibreMaui.Models;
using MaplibreMaui.Models.Layers;
using MaplibreMaui.Models.Sources;
using MaplibreMaui.Services;
using Microsoft.Maui.Storage;

namespace FarmApp.Mobile.Services;

/// <summary>Draws the selected app country's outline from embedded GeoJSON (Resources/Raw/CountryBorders).</summary>
public sealed class CountryBorderMapService : IMapService, IDisposable
{
    public const string SourceId = "farmapp-country-border-source";
    public const string LineLayerId = "farmapp-country-border-line";

    public IMaplibreMapService? MaplibreMapService { get; set; }

    private readonly ICountryMapStorage _storage;

    public CountryBorderMapService(ICountryMapStorage storage)
    {
        _storage = storage;
        _storage.MapCountryChanged += OnMapCountryChanged;
    }

    public void Dispose() =>
        _storage.MapCountryChanged -= OnMapCountryChanged;

    private void OnMapCountryChanged(AppMapCountry country) =>
        _ = ApplyBorderAsync(country);

    public void OnStyleLoaded()
    {
        var source = new GeoJsonSource(SourceId);
        var lineLayer = new LineLayer(LineLayerId, SourceId)
        {
            Properties = new Dictionary<string, object?>
            {
                { Properties.LineColor, "#0d4a4d" },
                { Properties.LineWidth, 2f }
            }
        };

        MaplibreMapService?.AddSource(source);
        MaplibreMapService?.AddLayer(lineLayer);

        var country = _storage.Get() ?? AppMapCountry.Ukraine;
        _ = ApplyBorderAsync(country);
    }

    public void OnBlazorServicesLoaded() { }

    public bool OnDown(float x, float y) => true;
    public bool OnUp(float x, float y) => true;
    public bool OnClick(float x, float y) => true;
    public bool OnMapMove(float x, float y) => true;
    public void OnMapRotate() { }

    private static string? BorderAssetFile(AppMapCountry country) => country switch
    {
        AppMapCountry.Ukraine => "CountryBorders/UKR.geo.json",
        AppMapCountry.UnitedStates => "CountryBorders/USA.geo.json",
        AppMapCountry.Italy => "CountryBorders/ITA.geo.json",
        AppMapCountry.Poland => "CountryBorders/POL.geo.json",
        AppMapCountry.Germany => "CountryBorders/DEU.geo.json",
        _ => null
    };

    private async Task ApplyBorderAsync(AppMapCountry country)
    {
        if (MaplibreMapService is null) return;

        var file = BorderAssetFile(country);
        if (file is null) return;

        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(file);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            MaplibreMapService.SetGeoJsonFeature(SourceId, json);
        }
        catch
        {
            // Asset missing or map source not yet added
        }
    }
}
