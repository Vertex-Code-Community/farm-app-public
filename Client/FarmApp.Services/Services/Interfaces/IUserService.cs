using FarmApp.ViewModels.Users;

namespace FarmApp.Services.Services.Interfaces;

public interface IUserService
{
    Task<UserModel?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    Task<bool> UpdateUserAsync(UpdateUserModel model, CancellationToken cancellationToken = default);

    Task<bool> UpdateSelectedLocationAsync(UpdateSelectedLocationModel model,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateNotificationPreferencesAsync(UpdateNotificationPreferencesModel model,
        CancellationToken cancellationToken = default);
}
