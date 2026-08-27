using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class TileEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public List<PolygonEntity> Polygons { get; set; } = new();
}