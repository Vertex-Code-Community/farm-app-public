using System.ComponentModel.DataAnnotations;
using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class SteadEntity : IBaseEntity<string>
{
    [Key] public string Id { get; set; }

    public string CadNum { get; set; }
    public float Area { get; set; }
    public string AreaUnit { get; set; }
    public string Address { get; set; }
    
    public string OwnershipId { get; set; }
    public OwnershipEntity Ownership { get; set; }
    
    public string PurposeId { get; set; }
    public PurposeEntity Purpose { get; set; }
    
    public string CategoryId { get; set; }
    public CategoryEntity Category { get; set; }
}