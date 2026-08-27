using FarmApp.Entities.Interfaces;

namespace FarmApp.Entities.Entity;

public class PropertyEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    public string MultipolygonSerialized { get; set; }
    public float Area { get; set; }
    public string Name { get; set; }

    public string UserId { get; set; }
    public UserEntity User { get; set; }

    public List<PropertyAndSteadEntity> PropertySteads { get; set; } = new();
    public List<PropertyNoteEntity> PropertyNotes { get; set; } = new();
}
