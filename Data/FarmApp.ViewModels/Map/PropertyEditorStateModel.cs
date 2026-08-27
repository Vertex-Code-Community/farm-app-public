using System.Text.Json.Serialization;

namespace FarmApp.ViewModels.Map;

public class PropertyEditorStateModel
{
    [JsonPropertyName("steadIds")]
    public List<string> SteadIds { get; set; } = new();
    [JsonPropertyName("customSteadIds")]
    public List<string> CustomSteadIds { get; set; } = new();
    [JsonPropertyName("features")]
    public List<string> Features { get; set; } = new();
    [JsonPropertyName("area")]
    public float Area { get; set; }
   [JsonPropertyName("multipolygonSerialized")]
    public string? MultipolygonSerialized { get; set; }
}