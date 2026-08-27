
namespace FarmApp.ViewModels.PropertyNoteStatuses
{
    public class PropertyNoteStatusModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TextColorHex { get; set; } = string.Empty;
        public string BGColorHex { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
