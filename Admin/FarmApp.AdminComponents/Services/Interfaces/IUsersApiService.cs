using FarmApp.ViewModels.Users;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IUsersApiService
{
    Task<List<UserViewModel>> GetUsersModelAsync(int skip, int take);
    Task UpdateUserAsync(UpdateUserModel model);
    Task DeleteUserModelAsync(string id);
}