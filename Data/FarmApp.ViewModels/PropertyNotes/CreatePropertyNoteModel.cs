using FarmApp.Shared.Enums;
using FarmApp.ViewModels.Media;

namespace FarmApp.ViewModels.PropertyNotes;

public class CreatePropertyNoteModel
{
    public string Header { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PropertyId { get; set; }
    public IEnumerable<TempUploadResult>? TempUploadResults { get; set; }
    public int? StatusId { get; set; }
    public bool NotificationsEnabled { get; set; }
    public NotificationOffset? NotifyBeforeStart { get; set; }
    public NotificationOffset? NotifyBeforeEnd { get;set; }
}