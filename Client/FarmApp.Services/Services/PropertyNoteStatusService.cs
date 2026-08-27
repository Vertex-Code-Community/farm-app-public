
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.PropertyNoteStatuses;

namespace FarmApp.Services.Services
{
    public class PropertyNoteStatusService : IPropertyNoteStatusService
    {
        private readonly IHttpService _httpService;
        public PropertyNoteStatusService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<PropertyNoteStatusModel?> CreateStatus(CreatePropertyNoteStatusModel model)
        {
            var result = await _httpService.PostAsync<PropertyNoteStatusModel,CreatePropertyNoteStatusModel>("api/property-note-status", model);
            return result;
        }
        public async Task<PropertyNoteStatusModel?> GetByIdAsync(int id)
        {
            var result = await _httpService.GetAsync<PropertyNoteStatusModel>($"api/property-note-status/{id}");
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _httpService.DeleteAsync<object>($"api/property-note-status/{id}");
            return result is not null;
        }

        public async Task<List<PropertyNoteStatusModel>> GetAllAsync(bool showError, bool redirectoOnUnauthorized)
        {
            var result = await _httpService.GetAsync<List<PropertyNoteStatusModel>>("api/property-note-status",
                showError: showError, redirectOnUnauthorized : redirectoOnUnauthorized);

            return result ?? new List<PropertyNoteStatusModel>();
        }

        public async Task<PropertyNoteStatusModel?> UpdateStatusAsync(int id, UpdatePropertyNoteStatusModel model)
        {
            var result = await _httpService.PatchAsync<PropertyNoteStatusModel,UpdatePropertyNoteStatusModel>($"api/property-note-status/{id}", model);
            return result;
        }
    }
}
