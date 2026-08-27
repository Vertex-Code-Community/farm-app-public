using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using FarmApp.Components.Attributes;
using FarmApp.Components.Pages.Home;
using FarmApp.Components.Pages.SignIn;
using FarmApp.Services.Providers;
using Constants = FarmApp.Shared.Constants.Constants;
using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace FarmApp.WebClient;

public class AppRouteViewWebClient : RouteView
{
    [Inject] private INavigationService NavigationService { get; set; }
    [Inject] private AuthStateProvider AuthStateProvider { get; set; }

    protected override void Render(RenderTreeBuilder builder)
    {
        var authAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute));
        var notAuthAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(UnauthorizedAttribute));

        var isAuthorized = AuthStateProvider.IsUserLoggedIn;

        if (authAttribute is not null)
        {
            if (!isAuthorized)
            {
                RenderPageWithoutNavigation(builder, typeof(SignInPage));
                NavigationService.NavigateTo(Constants.ClientRoutes.SignInPage);
                
                return;
            }
        }

        if (notAuthAttribute is not null)
        {
            if (isAuthorized)
            {
                RenderPageWithoutNavigation(builder, typeof(HomePage));
                NavigationService.NavigateTo(Constants.ClientRoutes.HomePage);
                return;
            }
        }

        base.Render(builder);
    }

    private void RenderPageWithoutNavigation(RenderTreeBuilder builder, Type pageType)
    {
        RenderFragment renderPageDelegate = pageBuilder =>
        {
            pageBuilder.OpenComponent(0, pageType);
            pageBuilder.CloseComponent();
        };
        
        var pageLayoutType = pageType.GetCustomAttribute<LayoutAttribute>()?.LayoutType ?? DefaultLayout;
                
        builder.OpenComponent<LayoutView>(0);
        builder.AddAttribute(1, nameof(LayoutView.Layout), pageLayoutType);
        builder.AddAttribute(2, nameof(LayoutView.ChildContent), renderPageDelegate);
        builder.CloseComponent();
    }
}

