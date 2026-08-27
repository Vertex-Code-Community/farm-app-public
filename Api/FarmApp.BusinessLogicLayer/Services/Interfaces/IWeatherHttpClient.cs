using FarmApp.ViewModels.Weather;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IWeatherHttpClient
    {
        Task<WeatherApiResponseModel?> SendWeatherRequest(double lat, double lon, CancellationToken cancellationToken = default);
    }
}
