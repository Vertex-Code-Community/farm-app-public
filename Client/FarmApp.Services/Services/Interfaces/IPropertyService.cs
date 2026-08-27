using FarmApp.ViewModels.Properties;

namespace FarmApp.Services.Services.Interfaces;

public interface IPropertyService
{
    Task<PropertyViewModel?> CreateAsync(CreatePropertyModel model);
    Task<PropertyViewModel?> GetByIdAsync(string id);
    Task<PropertyViewModel?> UpdateAsync(string id, UpdatePropertyModel model);
    Task<PropertyPreviewModel?> GetPreviewByIdAsync(string id);
    Task<bool> DeleteAsync(string id);
    Task<List<PropertyViewModel>> GetAllOfCurrentUserAsync(bool showError, bool redirectOnUnauthorized);
}