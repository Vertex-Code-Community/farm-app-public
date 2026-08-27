using FarmApp.ViewModels.Properties;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface IPropertyService
{
    Task<PropertyModel?> CreateAsync(CreatePropertyModel model);
    Task<PropertyModel?> GetByIdAsync(string id);
    Task<PropertyModel?> UpdateAsync(string id, UpdatePropertyModel model);
    Task<PropertyPreviewModel?> GetPreviewByIdAsync(string id);
    Task DeleteAsync(string id);
    Task<List<PropertyModel>> GetAllOfUserAsync();
}
