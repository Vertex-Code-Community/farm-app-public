using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class PropertyAndSteadEntity : IBaseEntity<string>
{
    public string Id { get; set ; }

    public string? SteadId { get; set ; }
    public SteadEntity? Stead { get; set ; }
    
    public string? CustomSteadId { get; set ; }
    public CustomSteadEntity? CustomStead { get; set ; }

    public string PropertyId { get; set ; }
    public PropertyEntity Property { get; set ; }
}
