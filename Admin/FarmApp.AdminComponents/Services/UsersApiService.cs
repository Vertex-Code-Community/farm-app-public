using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models;
using FarmApp.Shared.Constants;
using FarmApp.ViewModels.Users;

namespace FarmApp.AdminComponents.Services;

public class UsersApiService(
    IHttpService httpService) : IUsersApiService
{
    public async Task<List<UserViewModel>> GetUsersModelAsync(int skip, int take)
    {
        var models = await httpService.GetAsync<List<UserViewModel>>
            ($"api/user/filter-with-pagination?skip={skip}&take={take}", ApiType.ManagementAppApi);

        return models;
    }

    public async Task UpdateUserAsync(UpdateUserModel model)
    {
        var response = await httpService.PutAsync<ApiResponse, UpdateUserModel>
            ($"api/user", model, ApiType.ManagementAppApi);
    }

    public async Task DeleteUserModelAsync(string id)
    {
        var response = await httpService.DeleteAsync<UserViewModel>
            ($"api/user?id={id}", ApiType.ManagementAppApi); 
    }
}