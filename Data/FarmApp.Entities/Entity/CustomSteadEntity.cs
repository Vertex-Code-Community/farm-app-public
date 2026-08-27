using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class CustomSteadEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    
    public string UserId { get; set; }
    public UserEntity User { get; set; }
    
    public string? SteadId { get; set; }
    public SteadEntity? Stead { get; set; }
    
    public string Coordinates { get; set; }
}