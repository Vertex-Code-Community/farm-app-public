using FarmApp.ViewModels.PropertyNoteStatuses;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces
{
    public interface IPropertyNoteStatusService
    {
        Task<List<PropertyNoteStatusModel>> GetPropertyNoteStatusesAsync();
        Task<PropertyNoteStatusModel> CreateAsync(CreatePropertyNoteStatusModel model);
        Task<PropertyNoteStatusModel> UpdateAsync(int id, UpdatePropertyNoteStatusModel model);
        Task DeleteAsync(int id);
        Task<PropertyNoteStatusModel> GetStatusByIdAsync(int id);
    }
}
