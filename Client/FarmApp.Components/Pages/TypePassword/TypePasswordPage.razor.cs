using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.TypePassword;

public partial class TypePasswordPage
{
    [Inject] private INavigationService NavigationService { get; set; } = null!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }

    [Parameter] public string Email { get; set; } = string.Empty;
    [Parameter] public string ResetToken { get; set; } = string.Empty;

    private readonly ResetPasswordViewModel _resetPasswordViewModel = new();
    
    private bool _isLoading = false;

    private async Task OnResetClickedAsync()
    {
        var resetModel = new ResetPasswordRequestModel()
        {
            Email = Email,
            Password = _resetPasswordViewModel.Password,
            ConfirmPassword = _resetPasswordViewModel.ConfirmPassword,
            ResetToken = ResetToken
        };

        _isLoading = true;
        StateHasChanged();

        var requestTask = AuthenticationService.ResetPasswordAsync(resetModel);
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        
        await Task.WhenAll(requestTask, minimalTimeTask);
        var result = requestTask.Result;

        if (result.IsSuccess)
        {
            NavigationService.History.Clear();
            await AuthStateProvider.NotifyUserAuthentication();
            NavigationService.NavigateTo(Constants.ClientRoutes.HomePage);
            return;
        }

        _isLoading = false;
        StateHasChanged();
    }

    private EditContext _editContext;
    private bool _isFormValid;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_resetPasswordViewModel);
        _editContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _isFormValid = _editContext.Validate();
        StateHasChanged();
    }

    public void Dispose()
    {
        _editContext.OnFieldChanged -= HandleFieldChanged;
    }
}