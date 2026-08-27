using FarmApp.Shared.Constants;

namespace FarmApp.Services.Providers;

public class PageResolveProvider
{
    private readonly AuthStateProvider _authStateProvider;

    public PageResolveProvider(AuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public string GetInitialPageRoute()
    {
        var isAuthorized = _authStateProvider.IsUserLoggedIn;

        return isAuthorized ? Constants.ClientRoutes.HomePage : Constants.ClientRoutes.WelcomePage;
    }
}