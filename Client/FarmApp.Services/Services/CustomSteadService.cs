using Microsoft.AspNetCore.Components.Authorization;
using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.Services.Services;

public class CustomSteadService : ICustomSteadService
{
    private readonly IHttpService _httpService;
    private readonly AuthStateProvider _authStateProvider;
    
    public CustomSteadService(IHttpService httpService, AuthStateProvider authStateProvider)
    {
        _httpService = httpService;
        _authStateProvider = authStateProvider;
    }
    
    public async Task<CustomSteadModel?> CreateAsync(CreateCustomSteadModel createModel)
    {
        var result = await _httpService.PostAsync<CustomSteadModel, CreateCustomSteadModel>("api/custom-stead", createModel);
        return result;
    }

    public async Task<CustomSteadModel?> UpdateAsync(string id, UpdateCustomSteadModel updateModel)
    {
        var result = await _httpService.PatchAsync<CustomSteadModel, UpdateCustomSteadModel>($"api/custom-stead/{id}", updateModel);
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _httpService.DeleteAsync<object>($"api/custom-stead/{id}");
        return result is not null;
    }

    public Task<CustomSteadModel?> GetByIdAsync(string id)
    {
        return _httpService.GetAsync<CustomSteadModel>($"api/custom-stead/{id}");
    }

    public async Task<List<CustomSteadModel>> GetAllOfCurrentUserAsync()
    {
        var isAuthorized = _authStateProvider.IsUserLoggedIn;
        
        var customSteads = await _httpService.GetAsync<List<CustomSteadModel>>("api/custom-stead", showError: isAuthorized, redirectOnUnauthorized: isAuthorized);
        return customSteads ?? new List<CustomSteadModel>();
    }
}