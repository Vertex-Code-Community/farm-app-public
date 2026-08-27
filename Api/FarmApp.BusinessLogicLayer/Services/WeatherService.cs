using System.Globalization;
using System.Linq;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Weather;
using Microsoft.Extensions.Caching.Memory;

namespace FarmApp.BusinessLogicLayer.Services;

public class WeatherService : IWeatherService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IWeatherHttpClient _weatherHttpClient;

    public WeatherService(IMemoryCache memoryCache, IWeatherHttpClient weatherHttpClient)
    {
        _memoryCache = memoryCache;
        _weatherHttpClient = weatherHttpClient;
    }
    public async Task<WeatherResponseModel?> GetWeather(WeatherRequestModel request)
    {
        var cacheKey = GetWeatherCacheKey(request.Latitude, request.Longitude);

        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            var result = await _weatherHttpClient.SendWeatherRequest(request.Latitude, request.Longitude);
            var mapped = Map(result);
            entry.AbsoluteExpirationRelativeToNow = mapped is null
                ? TimeSpan.FromMinutes(2)
                : TimeSpan.FromMinutes(60);
            return mapped;
        });
    }

    private static string GetWeatherCacheKey(double lat, double lon)
    {
        var roundedLat = Math.Round(lat, 2, MidpointRounding.AwayFromZero);
        var roundedLon = Math.Round(lon, 2, MidpointRounding.AwayFromZero);

        return string.Format(CultureInfo.InvariantCulture, "weather_{0:F2}_{1:F2}", roundedLat, roundedLon);
    }

    private static WeatherResponseModel? Map(WeatherApiResponseModel? apiResponse)
    {
        if (apiResponse is null || !apiResponse.List.Any())
            return null;

        var first = apiResponse.List[0];
        if (!first.Weather.Any())
            return null;

        return new WeatherResponseModel
        {
            WeatherModel = new WeatherModel
            {
                City = apiResponse.City.Name,
                Country = apiResponse.City.Country,
                Temperature = first.Main.Temp,
                Description = first.Weather[0].Main,
                Icon = first.Weather[0].Icon,
            },
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}