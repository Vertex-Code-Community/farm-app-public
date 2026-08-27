using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using CommunityToolkit.Maui.Core;
using FarmApp.Components.Attributes;
using FarmApp.Components.Pages.SignIn;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;

namespace FarmApp.Mobile;

public class AppRouteViewMobileClient : RouteView
{
    [Inject] private NavigationManager NavigationManager { get; set; }
    [Inject] private IAuthenticationService AuthenticationService { get; set; }

    protected override void Render(RenderTreeBuilder builder)
    {
        // var authAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute));
        // var notAuthAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(UnauthorizedAttribute));
        //
        // var isAuthorized = AuthenticationService.IsUserLoggedIn;
        //
        // if (authAttribute is not null)
        // {
        //     if (!isAuthorized)
        //     {
        //         RenderPageWithoutNavigation(builder, typeof(SignInPage));
        //         NavigationManager.NavigateTo(Constants.ClientRoutes.SignInPage);
        //         
        //         return;
        //     }
        // }
        //
        // if (notAuthAttribute is not null)
        // {
        //     if (isAuthorized)
        //     {
        //         RenderPageWithoutNavigation(builder, typeof(Components.Pages.Main.MainPage));
        //         NavigationManager.NavigateTo(Constants.ClientRoutes.MainPage);
        //         return;
        //     }
        // }
        //
        // var statusbarColorAttribute = Attribute.GetCustomAttribute(RouteData.PageType, typeof(StatusBarColorAttribute));
        // if (statusbarColorAttribute is StatusBarColorAttribute statusbarColor)
        // {
        //     // CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Color.FromArgb("#ff0000"));
        //     // CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
        // }

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

