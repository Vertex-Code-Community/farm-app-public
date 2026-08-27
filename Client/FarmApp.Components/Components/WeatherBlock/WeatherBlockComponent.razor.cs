using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Math;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Weather;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.WeatherBlock;

public partial class WeatherBlockComponent
{
    /// <summary>Value passed to <see cref="LocationChanged"/> when GPS is unavailable and the user must grant access.</summary>
    private string LocationSelectionRequiredPlaceholder = string.Empty;

    private const string DefaultWeatherIconUrl =
        "_content/FarmApp.Components/img/weather/weather/sunny.svg";

    private const string WeatherIconAssetsDirectory =
        "_content/FarmApp.Components/img/weather/weather";

    /// <summary>
    /// OpenWeatherMap returns icon ids such as "01d"; we snap coordinates so cache keys match the API request grid.
    /// </summary>
    private const int WeatherCacheCoordinateDecimalPlaces = 2;

    [Inject] public required ILocationService LocationService { get; set; }
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required IWeatherService WeatherService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    [Parameter] public required string Location { get; set; }
    [Parameter] public EventCallback<string> LocationChanged { get; set; }
   [Parameter] public bool IsLoading { get; set; }

    private bool _isUserLocationUnknown;
    private bool _weatherRequestFailed;
    private bool _isOpenWeatherRequestInFlight;

    private string _weatherIconUrl = DefaultWeatherIconUrl;
    private int _temperatureCelsius;
    private string _weatherConditionLabel = string.Empty;

    /// <summary>Skeletons and fade-in loaders while we wait on the parent or OpenWeatherMap.</summary>
    private bool ShowWeatherSkeleton => IsLoading || _isOpenWeatherRequestInFlight;

    protected override async Task OnInitializedAsync()
    {
        LocationSelectionRequiredPlaceholder = Localizer["Weather_Enable-Location_Title"];
        var coordinates = await ResolveUserCoordinatesAsync();
        _isUserLocationUnknown = coordinates is null;

        if (!TryUseCachedWeatherState(coordinates, out var cachedModel))
        {
            if (_isUserLocationUnknown)
            {
                await LocationChanged.InvokeAsync(LocationSelectionRequiredPlaceholder);
                return;
            }

            await FetchAndDisplayWeatherAsync(coordinates!);
            return;
        }

        await ApplyWeatherModelToDisplayAsync(cachedModel);
    }

    /// <summary>GPS first; otherwise the profile location chosen in settings.</summary>
    private async Task<Vec2?> ResolveUserCoordinatesAsync()
    {
        var fromGps = await LocationService.GetUserLocationAsync();
        if (fromGps is not null)
            return fromGps;

        var user = await UserService.GetCurrentUserAsync();
        if (user?.SelectedLocationLatitude is null || user.SelectedLocationLongitude is null)
            return null;

        return new Vec2
        {
            X = user.SelectedLocationLongitude.Value,
            Y = user.SelectedLocationLatitude.Value
        };
    }

    private bool TryUseCachedWeatherState(Vec2? coordinates, out WeatherModel model)
    {
        model = null!;

        var state = StateService.WeatherState;
        if (state is null || state.IsExpired || coordinates is null)
            return false;

        if (state.CachedLatitude is null || state.CachedLongitude is null)
            return false;

        var sameCell = WeatherGridCellsMatch(
            state.CachedLatitude.Value,
            state.CachedLongitude.Value,
            coordinates.Y,
            coordinates.X);

        if (!sameCell)
            return false;

        model = state.WeatherModel;
        return true;
    }

    private static bool WeatherGridCellsMatch(double cachedLat, double cachedLon, double lat, double lon)
    {
        var a = SnapToWeatherGrid(cachedLat, cachedLon);
        var b = SnapToWeatherGrid(lat, lon);
        return a.Latitude == b.Latitude && a.Longitude == b.Longitude;
    }

    private static (double Latitude, double Longitude) SnapToWeatherGrid(double latitude, double longitude)
    {
        return (
            Math.Round(latitude, WeatherCacheCoordinateDecimalPlaces, MidpointRounding.AwayFromZero),
            Math.Round(longitude, WeatherCacheCoordinateDecimalPlaces, MidpointRounding.AwayFromZero));
    }

    private static Vec2 SnapLocation(Vec2 location)
    {
        var (lat, lon) = SnapToWeatherGrid(location.Y, location.X);
        return new Vec2 { Y = lat, X = lon };
    }

    private async Task FetchAndDisplayWeatherAsync(Vec2 location)
    {
        _isOpenWeatherRequestInFlight = true;
        _weatherRequestFailed = false;

        try
        {
            var snapped = SnapLocation(location);
            var response = await WeatherService.GetWeatherAsync(new WeatherRequestModel
            {
                Longitude = snapped.X,
                Latitude = snapped.Y
            });

            if (response is null)
            {
                _weatherRequestFailed = true;
                return;
            }

            StateService.WeatherState = new WeatherStateModel
            {
                ExpiresAt = response.ExpiresAt,
                WeatherModel = response.WeatherModel,
                CachedLatitude = snapped.Y,
                CachedLongitude = snapped.X
            };

            await ApplyWeatherModelToDisplayAsync(response.WeatherModel);
        }
        finally
        {
            _isOpenWeatherRequestInFlight = false;
        }
    }

    public async Task RequestLocationPermissionAndReloadWeatherAsync()
    {
        var granted = await LocationService.RequestPermissionAsync();
        if (!granted)
            return;

        var coordinates = await LocationService.GetUserLocationAsync();
        if (coordinates is null)
            return;

        _isUserLocationUnknown = false;
        await FetchAndDisplayWeatherAsync(coordinates);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ApplyWeatherModelToDisplayAsync(WeatherModel weatherModel)
    {
        var presentation = MapOpenWeatherIconToPresentation(weatherModel.Icon);
        _weatherIconUrl = presentation.IconUrl;
        _weatherConditionLabel = presentation.ConditionLabel;
        _temperatureCelsius = (int)weatherModel.Temperature;

        await LocationChanged.InvokeAsync($"{weatherModel.City}, {weatherModel.Country}");
    }

    private WeatherIconPresentation MapOpenWeatherIconToPresentation(string openWeatherMapIconId) =>
        openWeatherMapIconId switch
        {
            OpenWeatherMapIconId.ClearSkyDay => new(Localizer["Weather_Sunny"], IconFile("sunny.svg")),
            OpenWeatherMapIconId.ClearSkyNight => new(Localizer["Weather_Clear-Night"], IconFile("clear-night.svg")),
            OpenWeatherMapIconId.FewCloudsDay => new(Localizer["Weather_Partly-Cloudy"], IconFile("cloudy.svg")),
            OpenWeatherMapIconId.FewCloudsNight => new(Localizer["Weather_Cloudy-Night"], IconFile("cloudy-night.svg")),
            OpenWeatherMapIconId.ScatteredCloudsDay or OpenWeatherMapIconId.ScatteredCloudsNight
                or OpenWeatherMapIconId.BrokenCloudsDay or OpenWeatherMapIconId.BrokenCloudsNight =>
                new(Localizer["Weather_Cloudy"], IconFile("cloudy.svg")),
            OpenWeatherMapIconId.ShowerRainDay or OpenWeatherMapIconId.ShowerRainNight =>
                new(Localizer["Weather_Rain"], IconFile("rain.svg")),
            OpenWeatherMapIconId.RainDay or OpenWeatherMapIconId.RainNight =>
                new(Localizer["Weather_Heavy-Rain"], IconFile("heavy-rain.svg")),
            OpenWeatherMapIconId.ThunderstormDay or OpenWeatherMapIconId.ThunderstormNight =>
                new(Localizer["Weather_Thunderstorm"], IconFile("thunderstorm.svg")),
            OpenWeatherMapIconId.SnowDay or OpenWeatherMapIconId.SnowNight =>
                new(Localizer["Weather_Snow"], IconFile("snow.svg")),
            OpenWeatherMapIconId.MistDay or OpenWeatherMapIconId.MistNight =>
                new(Localizer["Weather_Mist"], IconFile("cloudy.svg")),
            _ => new(Localizer["Weather_Sunny"], IconFile("sunny.svg"))
        };

    private string IconFile(string fileName) => $"{WeatherIconAssetsDirectory}/{fileName}";

    private readonly record struct WeatherIconPresentation(string ConditionLabel, string IconUrl);

    private static class OpenWeatherMapIconId
    {
        public const string ClearSkyDay = "01d";
        public const string ClearSkyNight = "01n";
        public const string FewCloudsDay = "02d";
        public const string FewCloudsNight = "02n";
        public const string ScatteredCloudsDay = "03d";
        public const string ScatteredCloudsNight = "03n";
        public const string BrokenCloudsDay = "04d";
        public const string BrokenCloudsNight = "04n";
        public const string ShowerRainDay = "09d";
        public const string ShowerRainNight = "09n";
        public const string RainDay = "10d";
        public const string RainNight = "10n";
        public const string ThunderstormDay = "11d";
        public const string ThunderstormNight = "11n";
        public const string SnowDay = "13d";
        public const string SnowNight = "13n";
        public const string MistDay = "50d";
        public const string MistNight = "50n";
    }
}
