namespace FarmApp.ViewModels.Properties;

public class PropertyModel
{
    public string Id { get; set; } = string.Empty;
    public string MultipolygonSerialized { get; set; } = string.Empty;
    public List<PropertySteadModel> PropertySteads { get; set; } = new();
    public bool HasNotes { get; set; }
    public float Area { get; set; }
    public string Name { get; set; } = string.Empty;
}
