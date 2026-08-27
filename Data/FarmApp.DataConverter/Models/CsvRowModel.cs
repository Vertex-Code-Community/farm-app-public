using CsvHelper.Configuration.Attributes;

namespace FarmApp.DataConverter.Models;

public class CsvRowModel
{
    [Name(("landuse.cadnum"))]
    public string Cadnum { get; set; }
    
    [Name(("category"))]
    public string Category { get; set; }
    
    [Name(("purpose"))]
    public string Purpose { get; set; }
    
    [Name(("area"))]
    public string Area { get; set; }
    
    [Name(("unit_area"))]
    public string UnitArea { get; set; }
    
    [Name(("ownershipcode"))]
    public string OwnershipCode { get; set; }
    
    [Name(("ownership"))]
    public string Ownership { get; set; }
    
    [Name(("address"))]
    public string Address { get; set; }
    
    [Name(("geometry"))]
    public string Geometry { get; set; }
}