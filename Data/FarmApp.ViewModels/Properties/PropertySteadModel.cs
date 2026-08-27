namespace FarmApp.ViewModels.Properties;

public class PropertySteadModel
{
    public string Id { get; set ; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string? SteadId { get; set ; }
    public string? CustomSteadId { get; set ; }
}