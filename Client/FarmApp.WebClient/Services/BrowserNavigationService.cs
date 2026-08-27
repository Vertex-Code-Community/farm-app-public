using Microsoft.AspNetCore.Authorization;
using FarmApp.Components.Attributes;
using FarmApp.Services.Models;
using FarmApp.Services.Providers;
using FarmApp.Services.Services;
using FarmApp.Shared.Constants;
using Microsoft.AspNetCore.Components.Authorization;

namespace FarmApp.WebClient.Services;

public class BrowserNavigationService : NavigationService
{
    private readonly AuthStateProvider _authStateProvider;
    
    public BrowserNavigationService(AuthStateProvider authenticationStateProvider)
    {
        _authStateProvider = authenticationStateProvider;
    }
    
    protected override bool IsAllowedRoute(RouteDataModel route)
    {
        var authAttribute = Attribute.GetCustomAttribute(route.PageType, typeof(AuthorizeAttribute));
        var notAuthAttribute = Attribute.GetCustomAttribute(route.PageType, typeof(UnauthorizedAttribute));

        var isAuthorized = _authStateProvider.IsUserLoggedIn;

        var onAuthPage = authAttribute is not null && !isAuthorized;
        var onNotAuthPage = notAuthAttribute is not null && isAuthorized;
        
        var errorRoute = onAuthPage
            ? Constants.ClientRoutes.SignInPage
            : (onNotAuthPage ? Constants.ClientRoutes.HomePage : null);

        if (errorRoute is null) return true;
        
        NavigateTo(errorRoute);
        return false;
    }
}