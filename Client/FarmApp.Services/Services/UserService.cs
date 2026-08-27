using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Users;

namespace FarmApp.Services.Services;

public class UserService : IUserService
{
    private readonly IHttpService _httpService;

    public UserService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<UserModel?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await _httpService.GetAsync<UserModel>("api/user/get-current-user", showError: false, cancellationToken: cancellationToken);
        return user;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserModel model, CancellationToken cancellationToken = default)
    {
        var result = await _httpService.PutAsync<object, UpdateUserModel>("api/user", model,
            cancellationToken: cancellationToken);
        return result is not null;
    }

    public async Task<bool> UpdateSelectedLocationAsync(UpdateSelectedLocationModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpService.PutAsync<object, UpdateSelectedLocationModel>(
            "api/user/selected-location",
            model,
            cancellationToken: cancellationToken);
        return result is not null;
    }

    public async Task<bool> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpService.PutAsync<object, UpdateNotificationPreferencesModel>(
            "api/user/notification-preferences",
            model,
            cancellationToken: cancellationToken);
        return result is not null;
    }
}
