namespace FarmApp.ViewModels.Weather
{
    public class WeatherApiResponseModel
    {
        public CityDto City { get; set; } = new();
        public List<ForecastDto> List { get; set; } = [];
    }
    public class CityDto
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
    public class ForecastDto
    {
        public MainDto Main { get; set; } = new();
        public List<WeatherDto> Weather { get; set; } = [];
    }
    public class MainDto
    {
        public double Temp { get; set; }
    }
    public class WeatherDto
    {
        public string Main { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
