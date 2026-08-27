using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class PropertyNoteStatusEntity : IBaseEntity<int>
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = default!;
    public string TextColorHex { get; set; } = "#CCCCCC";
    public string BGColorHex { get; set; } = "#CCCCCC";
    public bool IsDefault { get; set; }
    public string? UserId { get; set; }
}
