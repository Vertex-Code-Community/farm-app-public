namespace FarmApp.ViewModels.Properties;

public class CreatePropertyModel
{
    public List<string> SteadIds { get; set; } = new();
    public List<string> CustomSteadIds { get; set; } = new();
    public string MultipolygonSerialized { get; set; } = string.Empty;
    public float Area { get; set; }
    public string Name { get; set; } = string.Empty;
}
