
using FarmApp.ViewModels.PropertyNoteStatuses;

namespace FarmApp.Services.Services.Interfaces
{
    public interface IPropertyNoteStatusService
    {
        Task<List<PropertyNoteStatusModel>> GetAllAsync(bool showError, bool redirectoOnUnauthorized);
        Task<PropertyNoteStatusModel?> CreateStatus(CreatePropertyNoteStatusModel model);
        Task<PropertyNoteStatusModel?> GetByIdAsync(int id);
        Task<PropertyNoteStatusModel?> UpdateStatusAsync(int id,UpdatePropertyNoteStatusModel model);
        Task<bool> DeleteAsync(int id);
    }
}
