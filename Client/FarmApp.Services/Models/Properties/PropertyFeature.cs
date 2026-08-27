namespace FarmApp.Services.Models.Properties;

public class PropertyFeature
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public PropertyGeometry Geometry { get; set; } = new();
    public PropertyProperties Properties { get; set; } = new();
}