using FarmApp.ViewModels.Weather;

namespace FarmApp.Services.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponseModel?> GetWeatherAsync(WeatherRequestModel weatherRequest);
    }
}
