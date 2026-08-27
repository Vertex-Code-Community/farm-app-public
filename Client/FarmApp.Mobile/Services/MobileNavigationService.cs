using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using FarmApp.Components.Attributes;
using FarmApp.Services.Models;
using FarmApp.Services.Providers;
using FarmApp.Services.Services;
using FarmApp.Shared.Constants;

namespace FarmApp.Mobile.Services;

public class MobileNavigationService : NavigationService
{
    private readonly AuthStateProvider _authStateProvider;
    
    public MobileNavigationService(AuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }
    
    protected override bool IsAllowedRoute(RouteDataModel route)
    {
        var allowAnonymous = Attribute.GetCustomAttribute(route.PageType, typeof(AllowAnonymousAttribute));

        if (allowAnonymous is not null)
        {
            return true;
        }
        var authAttribute = Attribute.GetCustomAttribute(route.PageType, typeof(AuthorizeAttribute));
        var notAuthAttribute = Attribute.GetCustomAttribute(route.PageType, typeof(UnauthorizedAttribute));

        var isAuthorized = _authStateProvider.IsUserLoggedIn;
        var isSessionExpired = _authStateProvider.IsSessionExpired;

        var onAuthPage = authAttribute is not null && !isAuthorized && !isSessionExpired;
        var onNotAuthPage = notAuthAttribute is not null && isAuthorized;
        
        var errorRoute = onAuthPage
            ? Constants.ClientRoutes.SignInPage
            : (onNotAuthPage ? Constants.ClientRoutes.HomePage : null);

        if (errorRoute is not null)
        {
            NavigateTo(errorRoute);
            return false;
        }
        
        var statusbarColorAttribute = Attribute.GetCustomAttribute(route.PageType, typeof(StatusBarColorAttribute));
        if (statusbarColorAttribute is StatusBarColorAttribute statusbarColor)
        {
            // CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Color.FromArgb("#ff0000"));
            // CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
        }
        
        return true;
    }
}