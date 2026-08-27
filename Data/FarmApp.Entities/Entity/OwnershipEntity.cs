using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class OwnershipEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public List<SteadEntity> Steads { get; set; } = new();
}