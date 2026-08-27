using FarmApp.Entities.Interfaces;
using FarmApp.Shared.Enums;

namespace FarmApp.Entities.Entity;

public class PropertyNoteEntity : IBaseEntity<string>
{
    public string Id { get; set; }
    public string Header { get; set; }
    public string Text { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PropertyNoteMedia> Medias { get; set; } = new();
    public string? PreviewMediaId { get; set; }
    public int? StatusId { get; set; }
    public PropertyNoteStatusEntity? Status { get; set; }
    public string PropertyId { get; set; }
    public PropertyEntity Property { get; set; }
    public bool NotificationsEnabled { get; set; }
    public NotificationOffset? NotifyBeforeStart { get; set; }
    public NotificationOffset? NotifyBeforeEnd { get; set; }
}
