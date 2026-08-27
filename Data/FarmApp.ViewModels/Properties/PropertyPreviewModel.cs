using FarmApp.ViewModels.PropertyNotes;

namespace FarmApp.ViewModels.Properties;

public class PropertyPreviewModel
{
    public string Id { get; set; } = string.Empty;
    public List<PropertyNotePreviewModel> Notes { get; set; } = new();
    public string Name { get; set; } = string.Empty;
}