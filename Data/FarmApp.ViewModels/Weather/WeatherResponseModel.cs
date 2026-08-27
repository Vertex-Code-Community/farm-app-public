namespace FarmApp.ViewModels.Weather
{
    public class WeatherResponseModel
    {
        public WeatherModel WeatherModel { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}
