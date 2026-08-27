using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Users;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Pages.ChooseLocation;

public partial class ChooseLocationPage
{
    private const double MaxDistanceDegreesToMatchPresetCity = 0.12;

    private static readonly double MaxSquaredDistanceDegreesToMatchPresetCity =
        MaxDistanceDegreesToMatchPresetCity * MaxDistanceDegreesToMatchPresetCity;

    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required ILocationService LocationService { get; set; }
    [Inject] public required IStateService StateService { get; set; }

    private string _currentLocation = string.Empty;
    private string _searchQuery = string.Empty;

    private sealed record LocationOption(string Label, double Latitude, double Longitude);

    private static readonly List<LocationOption> CityOptions =
    [
        new("Kyiv, Ukraine", 50.4501, 30.5234),
        new("Lviv, Ukraine", 49.8397, 24.0297),
        new("Odesa, Ukraine", 46.4825, 30.7233),
        new("Kharkiv, Ukraine", 49.9935, 36.2304),
        new("Dnipro, Ukraine", 48.4647, 35.0462),
        new("Ivano-Frankivsk, Ukraine", 48.9226, 24.7111),
        new("Ternopil, Ukraine", 49.5535, 25.5948),
        new("Poltava, Ukraine", 49.5883, 34.5514),
        new("Vinnytsia, Ukraine", 49.2328, 28.4809)
    ];

    private List<LocationOption> _filteredLocations => string.IsNullOrWhiteSpace(_searchQuery)
        ? []
        : CityOptions
            .Where(l => l.Label.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

    private readonly List<string> _recentLocations = new();

    protected override async Task OnInitializedAsync()
    {
        var user = await UserService.GetCurrentUserAsync();
        if (user?.SelectedLocationLatitude is not null && user.SelectedLocationLongitude is not null)
        {
            _currentLocation = SubtitleForSavedCoordinates(
                user.SelectedLocationLatitude.Value,
                user.SelectedLocationLongitude.Value);
        }
        else
        {
            _currentLocation = CityOptions[0].Label;
        }
    }


    private static string SubtitleForSavedCoordinates(double latitude, double longitude)
    {
        var nearestPreset = FindNearestPresetCity(latitude, longitude);
        var squaredDistanceToNearest = SquaredDistanceDegrees(
            latitude, longitude, nearestPreset.Latitude, nearestPreset.Longitude);

        if (squaredDistanceToNearest <= MaxSquaredDistanceDegreesToMatchPresetCity)
            return nearestPreset.Label;

        return FormatDecimalDegrees(latitude, longitude);
    }

    private static LocationOption FindNearestPresetCity(double latitude, double longitude)
    {
        return CityOptions.MinBy(c => SquaredDistanceDegrees(latitude, longitude, c.Latitude, c.Longitude));
    }

    private static double SquaredDistanceDegrees(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        var deltaLatitude = latitude2 - latitude1;
        var deltaLongitude = longitude2 - longitude1;
        return deltaLatitude * deltaLatitude + deltaLongitude * deltaLongitude;
    }

    private static string FormatDecimalDegrees(double latitude, double longitude) =>
        $"{latitude:F2}°, {longitude:F2}°";

    private async Task OnLocationSelected(LocationOption option)
    {
        var ok = await UserService.UpdateSelectedLocationAsync(new UpdateSelectedLocationModel
        {
            Latitude = option.Latitude,
            Longitude = option.Longitude
        });

        if (!ok)
            return;

        StateService.WeatherState = null;

        _currentLocation = option.Label;
        _searchQuery = string.Empty;

        if (_recentLocations.Contains(option.Label))
            _recentLocations.Remove(option.Label);
        _recentLocations.Insert(0, option.Label);

        if (_recentLocations.Count > 3)
            _recentLocations.RemoveAt(3);

        StateHasChanged();
    }

    private async Task OnLocationSelectedByLabel(string label)
    {
        var option = CityOptions.FirstOrDefault(c => c.Label == label);
        if (option is not null)
            await OnLocationSelected(option);
    }

    private async Task OnCurrentLocationClicked()
    {
        var granted = await LocationService.RequestPermissionAsync();
        if (!granted)
            return;

        var loc = await LocationService.GetUserLocationAsync();
        if (loc is null)
            return;

        var ok = await UserService.UpdateSelectedLocationAsync(new UpdateSelectedLocationModel
        {
            Latitude = loc.Y,
            Longitude = loc.X
        });

        if (!ok)
            return;

        StateService.WeatherState = null;

        _currentLocation = SubtitleForSavedCoordinates(loc.Y, loc.X);
        StateHasChanged();
    }
}
