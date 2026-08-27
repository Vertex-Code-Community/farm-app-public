using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class PolygonEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    public List<string> WktCoordinates { get; set; } = new();
    public string SteadId { get; set; }
    
    public List<TileEntity> Tiles { get; set; } = new();
}