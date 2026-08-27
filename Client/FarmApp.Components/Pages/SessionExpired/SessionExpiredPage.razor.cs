using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.SessionExpired
{
    public partial class SessionExpiredPage
    {
        [Inject] public required INavigationService NavigationService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

        private void OnSignInPage()
        {
            NavigationService.NavigateTo(Shared.Constants.Constants.ClientRoutes.WelcomePage);
        }
    }
}
