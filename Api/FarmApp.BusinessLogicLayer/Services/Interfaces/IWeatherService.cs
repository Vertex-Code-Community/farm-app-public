using FarmApp.ViewModels.Weather;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherResponseModel?> GetWeather(WeatherRequestModel request);
    }
}
