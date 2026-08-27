using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface ICustomSteadService
{
    Task<CustomSteadModel?> CreateAsync(CreateCustomSteadModel createModel);
    Task<CustomSteadModel?> UpdateAsync(string id, UpdateCustomSteadModel model);
    Task DeleteAsync(string id);
    Task<CustomSteadModel> GetByIdAsync(string id);
    Task<List<CustomSteadModel>> GetAllOfCurrentUserAsync();
}