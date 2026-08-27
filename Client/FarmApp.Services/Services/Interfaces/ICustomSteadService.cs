using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.Services.Services.Interfaces;

public interface ICustomSteadService
{
    Task<CustomSteadModel?> CreateAsync(CreateCustomSteadModel createModel);
    Task<CustomSteadModel?> UpdateAsync(string id, UpdateCustomSteadModel updateModel);
    Task<bool> DeleteAsync(string id);
    Task<CustomSteadModel?> GetByIdAsync(string id);
    Task<List<CustomSteadModel>> GetAllOfCurrentUserAsync();
}