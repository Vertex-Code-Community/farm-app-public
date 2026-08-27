using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.SignUp;

public partial class SignUpPage
{
    [Inject] private INavigationService NavigationService { get; set; } = null!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = null!;
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private readonly SignUpViewModel _signUpViewModel = new ();

    private readonly SignUpRequestModel _signUpModel = new();

    private bool _isLoading = false;

    private bool _showPrivacyPolicyModal = false;
    private bool _showTermsOfUseModal = false;

    private EditContext _editContext;
    private ValidationMessageStore _validationMessageStore;
    private bool _isFormValid;

    private async Task SignUpAsync()
    {
        _validationMessageStore.Clear();

        if (!_editContext.Validate())
            return;
        if (!_signUpViewModel.IsAccepted) return;

        _signUpModel.Email = _signUpViewModel.Email;
        _signUpModel.IsAccepted = true;
        _signUpModel.Password = _signUpViewModel.Password;
        _signUpModel.ConfirmPassword = _signUpViewModel.ConfirmPassword;

        _isLoading = true;
        StateHasChanged();
        
        var signUpTask = AuthenticationService.SignUpAsync(_signUpModel);
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        
        await Task.WhenAll(signUpTask, minimalTimeTask);
        var result = signUpTask.Result;
        
        
        if (result.IsSuccess)
        {
            NavigationService.NavigateTo(Constants.ClientRoutes.EnterVerificationCodePage, new Dictionary<string, object>
            {
                ["Email"] = _signUpModel.Email,
                ["VerificationPurpose"] = VerificationPurpose.SignUp,
            });
        }

        _validationMessageStore.Add(new FieldIdentifier(_signUpViewModel, nameof(_signUpViewModel.Email)), Localizer[$"Error_{result?.ErrorMessage!}"]);

        _isLoading = false;
        StateHasChanged();
    }

    private void OnClickWelcomePage()
    {
        NavigationService.History.Clear();
        NavigationService.NavigateTo(Constants.ClientRoutes.WelcomePage);
    }

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_signUpViewModel);
        _validationMessageStore = new(_editContext);
        _editContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _isFormValid = _editContext.Validate();
        _validationMessageStore.Clear(e.FieldIdentifier);

        if (e.FieldIdentifier.FieldName == nameof(_signUpViewModel.Password))
        {
            _validationMessageStore.Clear(
                new FieldIdentifier(_signUpViewModel, nameof(_signUpViewModel.Email))
            );
            _editContext.NotifyValidationStateChanged();
        }
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();
    }

    public void Dispose()
    {
        _editContext.OnFieldChanged -= HandleFieldChanged;
    }
}