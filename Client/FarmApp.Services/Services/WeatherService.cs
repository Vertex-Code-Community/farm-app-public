using System.Globalization;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Weather;
using Microsoft.AspNetCore.WebUtilities;

namespace FarmApp.Services.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IHttpService _httpService;
        public WeatherService(IHttpService httpService)
        {
            _httpService = httpService;
        }
        public async Task<WeatherResponseModel?> GetWeatherAsync(WeatherRequestModel weatherRequest)
        {
            var url = QueryHelpers.AddQueryString("api/weather",
                new Dictionary<string, string?>
                {
                    ["latitude"] = weatherRequest.Latitude.ToString(CultureInfo.InvariantCulture),
                    ["longitude"] = weatherRequest.Longitude.ToString(CultureInfo.InvariantCulture)
                });
            var result = await _httpService.GetAsync<WeatherResponseModel>(url);

            return result;
        }
    }
}
