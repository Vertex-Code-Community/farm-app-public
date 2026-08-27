using FarmApp.ViewModels.Pagination;
using FarmApp.ViewModels.Steads;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface ISteadService
{
    Task<SteadModel> GetByIdAsync(string id);
}
