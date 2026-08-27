using FarmApp.Shared.Math;

namespace FarmApp.ViewModels.Properties;

public class PropertyViewModel
{
    public string Id { get; set; } = string.Empty;
    public string MultipolygonSerialized { get; set; } = string.Empty;
    public List<PropertySteadModel> PropertySteads { get; set; } = new();
    public bool HasNotes { get; set; }
    public float Area { get; set; }
    public string Name { get; set; } = string.Empty;
    public Vec2? Centroid { get; set; }
    public float Zoom { get; set; }
    public string? PictogramBase64Url { get; set; }
}