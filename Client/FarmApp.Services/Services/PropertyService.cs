using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Properties;

namespace FarmApp.Services.Services;

public class PropertyService : IPropertyService
{
    private readonly IHttpService _httpService;
    
    public PropertyService(IHttpService httpService)
    {
        _httpService = httpService;
    }
    
    public async Task<PropertyViewModel?> CreateAsync(CreatePropertyModel model)
    {
        var propertyModel = await _httpService.PostAsync<PropertyViewModel, CreatePropertyModel>("api/property", model);
        return propertyModel;
    }

    public Task<PropertyViewModel?> GetByIdAsync(string id)
    { 
        return _httpService.GetAsync<PropertyViewModel>($"api/property/{id}");
    }

    public async Task<PropertyViewModel?> UpdateAsync(string id, UpdatePropertyModel model)
    {
        var result = await _httpService.PatchAsync<PropertyViewModel, UpdatePropertyModel>($"api/property/{id}", model);
        return result;
    }

    public Task<PropertyPreviewModel?> GetPreviewByIdAsync(string id)
    {
        return _httpService.GetAsync<PropertyPreviewModel>($"api/property/{id}/preview");
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _httpService.DeleteAsync<object>($"api/property/{id}");
        return result is not null;
    }

    public async Task<List<PropertyViewModel>> GetAllOfCurrentUserAsync(bool showError, bool redirectOnUnauthorized)
    {
        return await _httpService.GetAsync<List<PropertyViewModel>>("api/property", redirectOnUnauthorized, showError) ?? new();
    }
}