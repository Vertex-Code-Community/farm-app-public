using Microsoft.AspNetCore.Components;

namespace FarmApp.Services.Models.DialogModels
{
    public class DialogParametersModel
    {
        public string Width { get; set; } = string.Empty;
        public string Height { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public string CssStyles { get; set; } = string.Empty;
        public object? Payload { get; set; }
        public EventCallback OnOverlayClicked { get; set; }
    }
}
