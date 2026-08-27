using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class CsvRowEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    public string Cadnum { get; set; }
    public string Category { get; set; }
    public string Purpose { get; set; }
    public string Area { get; set; }
    public string UnitArea { get; set; }
    public string OwnershipCode { get; set; }
    public string Ownership { get; set; }
    public string Address { get; set; }
    public string Geometry { get; set; }
}