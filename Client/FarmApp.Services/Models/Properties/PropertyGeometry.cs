namespace FarmApp.Services.Models.Properties;

public class PropertyGeometry
{
    public string Type { get; set; } = string.Empty;
    public List<double[][]> Coordinates { get; set; } = new();
}