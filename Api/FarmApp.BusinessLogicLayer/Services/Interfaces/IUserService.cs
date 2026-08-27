using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Users;

namespace FarmApp.BusinessLogicLayer.Services.Interfaces;

public interface IUserService
{
    Task<UserEntity> CreateAsync(CreateUserModel model);
    Task<List<UserViewModel>> FilterByAsync(int skip, int take);
    
    Task<UserModel> GetUserByIdAsync(string id);
    Task<UserModel?> GetCurrentUserAsync();
    string? GetCurrentUserId();

    Task UpdateAsync(UpdateUserModel model);
    Task UpdateSelectedLocationAsync(UpdateSelectedLocationModel model);
    Task UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesModel model);
    Task DeleteAsync(string id);
}
