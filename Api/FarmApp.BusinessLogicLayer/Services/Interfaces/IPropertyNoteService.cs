using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.UploadPropertyNoteMediaFiles;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface IPropertyNoteService
{
    Task<PropertyNoteModel?> CreateAsync(CreatePropertyNoteModel model);
    Task<PropertyNoteModel?> UpdateAsync(string id, UpdatePropertyNoteModel model);
    Task DeleteAsync(string id);
    Task<PropertyNoteModel?> GetByIdAsync(string propertyNoteId);
    Task<List<PropertyNoteModel>> GetAllByDateAsync(string propertyId, DateTime day);
}