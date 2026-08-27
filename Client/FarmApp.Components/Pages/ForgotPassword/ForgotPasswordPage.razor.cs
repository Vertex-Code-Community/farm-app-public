using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.ForgotPassword;

public partial class ForgotPasswordPage
{
    [Inject] private INavigationService NavigationService { get; set; } = null!;
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private bool _isLoading = false;
    private string? _errorMessage;

    private readonly ForgotPasswordRequestModel _forgotPasswordRequestModel = new();

    private async Task OnSendCodeCLicked()
    {
        _isLoading = true;
        _errorMessage = null;


        var isCodeSent = await AuthenticationService.ForgotPasswordAsync(_forgotPasswordRequestModel);

        if (!isCodeSent.IsSuccess)
        {
            _errorMessage = Localizer[$"Error_{isCodeSent.ErrorMessage}"];
            _isLoading = false;
            return;
        }
        _isLoading = false;

        NavigationService.NavigateTo(Constants.ClientRoutes.EnterVerificationCodePage, new Dictionary<string, object>
        {
            ["Email"] = _forgotPasswordRequestModel.Email,
            ["VerificationPurpose"] = VerificationPurpose.ResetPassword
        });
    }

    private EditContext _editContext;
    private bool _isFormValid;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_forgotPasswordRequestModel);
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