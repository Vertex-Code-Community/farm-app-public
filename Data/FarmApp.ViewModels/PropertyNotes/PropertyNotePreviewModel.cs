using FarmApp.Shared.Enums;
using FarmApp.Shared.Resources.Localization;
using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.PropertyNotes;

public class PropertyNotePreviewModel
{
    public string Id { get; set; }

    [Required(ErrorMessageResourceName = "Property_Note_Validation_Required_Name",
        ErrorMessageResourceType = typeof(AppRecources))]
    [MinLength(3, ErrorMessageResourceName = "Property_Note_Validation_Min_Length_Name",
        ErrorMessageResourceType = typeof(AppRecources))]
    public string Header { get; set; }

    [Required(ErrorMessageResourceName = "Property_Note_Validation_Required_Desc", 
        ErrorMessageResourceType = typeof(AppRecources))]
    [MinLength(10, ErrorMessageResourceName = "Property_Note_Validation_Min_Length_Desc",
        ErrorMessageResourceType = typeof(AppRecources))]
    public string Text { get; set; }
    public string? PreviewMediaId { get; set; } 
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime EndTime { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; }
    public int? StatusId { get; set; }
    public bool NotificationsEnabled { get; set; }
    public NotificationOffset? NotifyBeforeStart { get; set; }
    public NotificationOffset? NotifyBeforeEnd { get;set; }
}