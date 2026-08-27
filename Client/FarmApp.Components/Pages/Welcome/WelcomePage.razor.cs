using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.Welcome;

public partial class WelcomePage
{
    [Inject] private INavigationService NavigationService { get; set; } = null!;
    [Inject] public required IMobilePlatformService MobilePlatformService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private string _marginTop = String.Empty;

    protected override void OnInitialized()
    {
        var cssScreenHeight = ScreenOffsetProvider.ScreenHeight / ScreenOffsetProvider.Density;
        if (OperatingSystem.IsAndroid())
        {
            cssScreenHeight -= (ScreenOffsetProvider.Bottom + ScreenOffsetProvider.Top);
        }
        // 220 is content height, 16 is parent container padding and distance from bottom edge
        var _marginTopFloat = cssScreenHeight - ScreenOffsetProvider.Bottom - (MobilePlatformService.IsIos ? 288 : 220) - 16 - 16;
        _marginTop = _marginTopFloat.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
    }

    private void OnClickSignIn()
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.SignInPage);
    }
    
    private void OnClickSignUp()
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.SignUpPage);
    }
}
