namespace FarmApp.ViewModels.Weather;

public class WeatherStateModel
{
    public WeatherModel WeatherModel { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public double? CachedLatitude { get; set; }
    public double? CachedLongitude { get; set; }
}
