using FarmApp.Shared.Enums;

namespace FarmApp.ViewModels.PropertyNotes;

public class PropertyNoteModel
{
    public string Id { get; set; }
    public string Header { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? PreviewMediaId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PropertyId { get; set; }
    public int? StatusId { get; set; }
    public bool NotificationsEnabled { get; set; }
    public NotificationOffset? NotifyBeforeStart { get; set; }
    public NotificationOffset? NotifyBeforeEnd { get; set; }
}