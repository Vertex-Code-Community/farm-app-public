using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.Models.Options;
using FarmApp.ViewModels.Weather;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;

namespace FarmApp.BusinessLogicLayer.Services
{
    public class WeatherHttpClient : IWeatherHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<WeatherOptions> _options;

        public WeatherHttpClient(HttpClient httpClient, IOptions<WeatherOptions> options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<WeatherApiResponseModel?> SendWeatherRequest(double lat, double lon, CancellationToken cancellationToken = default)
        {
            var url = QueryHelpers.AddQueryString("forecast",
                new Dictionary<string, string?>
                {
                    ["lat"] = lat.ToString(CultureInfo.InvariantCulture),
                    ["lon"] = lon.ToString(CultureInfo.InvariantCulture),
                    ["appid"] = _options.Value.ApiKey,
                    ["units"] = "metric"
                });

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // maybe we should log unseccessful response here
                return null;
            }

            return await response.Content.ReadFromJsonAsync<WeatherApiResponseModel>(cancellationToken);
        }
    }
}
