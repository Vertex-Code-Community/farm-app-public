using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.UploadPropertyNoteMediaFiles;
using Microsoft.AspNetCore.Components.Forms;

namespace FarmApp.Services.Services;

public class PropertyNoteService : IPropertyNoteService
{
    private readonly IHttpService _httpService;
    
    public PropertyNoteService(IHttpService httpService)
    {
        _httpService = httpService;
    }
    
    public async Task<PropertyNoteModel?> CreateAsync(CreatePropertyNoteModel model)
    {
        var propertyNoteModel = await _httpService.PostAsync<PropertyNoteModel, CreatePropertyNoteModel>("api/property-note", model);
        return propertyNoteModel;
    }

    public async Task<PropertyNoteModel?> UpdateAsync(string id, UpdatePropertyNoteModel model)
    {
        var propertyNoteModel = await _httpService.PatchAsync<PropertyNoteModel, UpdatePropertyNoteModel>($"api/property-note/{id}", model);
        return propertyNoteModel;
    }

    public async Task<PropertyNoteModel?> GetByIdAsync(string id)
    {
        var propertyNoteModel = await _httpService.GetAsync<PropertyNoteModel>($"api/property-note/{id}");
        return propertyNoteModel;
    }

    public async Task<List<PropertyNoteModel>> GetAllByDataAsync(string propertyId, DateTime day)
    {
        var propertyNotes = await _httpService.GetAsync<List<PropertyNoteModel>>($"api/property-note/{propertyId}/all/{day:dd-MM-yyyy}");
        return propertyNotes ?? new();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _httpService.DeleteAsync<object>($"api/property-note/{id}");
        return result is not null;
    }

}