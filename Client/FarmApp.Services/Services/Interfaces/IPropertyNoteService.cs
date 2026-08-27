using FarmApp.ViewModels.PropertyNotes;

namespace FarmApp.Services.Services.Interfaces;

public interface IPropertyNoteService
{
    Task<PropertyNoteModel?> CreateAsync(CreatePropertyNoteModel model);
    Task<PropertyNoteModel?> UpdateAsync(string id, UpdatePropertyNoteModel model);
    Task<PropertyNoteModel?> GetByIdAsync(string id);
    Task<List<PropertyNoteModel>> GetAllByDataAsync(string propertyId, DateTime day);
    Task<bool> DeleteAsync(string id);
}