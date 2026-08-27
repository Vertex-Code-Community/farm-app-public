using FarmApp.Services.Providers;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.SignIn;

public partial class SignInPage
{
    [Inject] public required AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    
    private readonly SignInRequestModel _signInRequestModel = new ();

    private bool _isLoading = false;

    private async Task SignInAsync()
    {
        _validationMessageStore.Clear();

        if (!_editContext.Validate())
            return;

        _isLoading = true;
        StateHasChanged();
        
        var signInTask = AuthenticationService.SignInAsync(_signInRequestModel);
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        
        await Task.WhenAll(signInTask, minimalTimeTask);

        var result = signInTask.Result;

        if (result.IsSuccess)
        {
            NavigationService.History.Clear();
            await AuthStateProvider.NotifyUserAuthentication();
            NavigationService.NavigateTo(Constants.ClientRoutes.HomePage);
            return;
        }

        _validationMessageStore.Add(new FieldIdentifier(_signInRequestModel, 
            nameof(_signInRequestModel.Email)), Localizer[$"Error_{result.ErrorMessage!}"]);

        _isLoading = false;
        StateHasChanged();
    }

    private void OnClickWelcomePage()
    {
        NavigationService.History.Clear();
        NavigationService.NavigateTo(Constants.ClientRoutes.WelcomePage);
    }

    private EditContext _editContext;
    private ValidationMessageStore _validationMessageStore;
    private bool _isFormValid;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_signInRequestModel);
        _validationMessageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += HandleFieldChanged;

    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _isFormValid = _editContext.Validate();
        _validationMessageStore.Clear(e.FieldIdentifier);
        
        if (e.FieldIdentifier.FieldName == nameof(_signInRequestModel.Password))
        {
            _validationMessageStore.Clear(
                new FieldIdentifier(_signInRequestModel, nameof(_signInRequestModel.Email))
            );
        }
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();
    }

    public void Dispose()
    {
        _editContext.OnFieldChanged -= HandleFieldChanged;
    }
}
