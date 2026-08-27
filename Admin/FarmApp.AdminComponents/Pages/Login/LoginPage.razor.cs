using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.Models.User;
using FarmApp.Shared.Constants;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.Login;

public partial class LoginPage : ComponentBase
{
    [Inject] public required IAuthenticationService AuthService { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    [Inject] public required IGlobalLoaderService LoaderService { get; set; }

    protected CredentialsModel Model { get; set; } = new();
    private bool _showPassword;

    protected async Task OnSubmitAsync()
    {
        using (LoaderService.SwitchOn())
        {
            await AuthService.LoginAsync(Model);
        }

        var isLogged = AuthService.IsUserLoggedIn;
        if (isLogged)
        {
            Navigation.NavigateTo(Constants.ClientRoutesAdmin.WelcomePage);
        }
    }

    private void ToggleShowPassword()
    {
        _showPassword = !_showPassword;
    }
}
