

using FarmApp.Shared.Resources.Localization;
using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.PropertyNoteStatuses
{
    public class PropertyNoteStatusPreviewModel
    {
        [Required(ErrorMessageResourceName = "Status_Validation_Required_Name", 
            ErrorMessageResourceType = typeof(AppRecources))]
        [MinLength(3, ErrorMessageResourceName = "Status_Validation_Min_Length_Name",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Status_Validation_Required_Color",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string TextColorHex { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Status_Validation_Required_Color",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string BGColorHex { get; set; } = string.Empty;
    }
}
