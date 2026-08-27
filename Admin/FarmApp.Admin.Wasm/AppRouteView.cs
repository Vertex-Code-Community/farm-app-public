using FarmApp.AdminComponents.Attributes;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FarmApp.Admin.Wasm;


public class AppRouteView : RouteView
{
    [Inject] private NavigationManager NavManager { get; set; } = null!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;

    protected override void Render(RenderTreeBuilder builder)
    {
        var authAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute));
        var notAuthAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(UnauthorizedAttribute));

        var isAuthorized = AuthenticationService.IsUserLoggedIn;

        if (authAttribute is not null)
        { 
            if (!isAuthorized)
            {
                NavManager.NavigateTo(Constants.ClientRoutesAdmin.Login);
                return;
            }
        }

        if (notAuthAttribute is not null)
        {
            if (isAuthorized)
            {
                NavManager.NavigateTo(Constants.ClientRoutesAdmin.WelcomePage);
                return;
            }
        }

        base.Render(builder);
    }
}