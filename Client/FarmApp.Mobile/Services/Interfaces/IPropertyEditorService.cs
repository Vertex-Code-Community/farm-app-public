using FarmApp.ViewModels.Properties;

namespace FarmApp.Mobile.Services.Interfaces;

public interface IPropertyEditorService : IMapService
{
    static List<PropertyViewModel> PropertiesList { get; set; } = new();
}