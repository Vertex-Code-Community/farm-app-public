using FarmApp.Models.User;

namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IAuthenticationService
{
    bool IsUserLoggedIn { get; }

    Task<UserTokenModel?> LoginAsync(CredentialsModel model);

    Task LogoutAsync();

    Task<string?> GetToken();
}