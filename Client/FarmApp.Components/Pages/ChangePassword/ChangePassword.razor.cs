using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FarmApp.Components.Pages.ChangePassword;

public partial class ChangePassword
{
    [Inject] public required IAuthenticationService AuthenticationService { get; set; }
    private EditContext _editContext;
    private ValidationMessageStore _validationMessageStore;

    private ChangePasswordViewModel _viewModel = new();

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_viewModel);
        _validationMessageStore = new ValidationMessageStore(_editContext);
    }

    private string ReqRowClass(Func<string, bool> check)
    {
        var pwd = _viewModel.NewPassword;
        if (string.IsNullOrEmpty(pwd)) return "req-row req-neutral";
        return check(pwd) ? "req-row req-pass" : "req-row req-fail";
    }

    private async Task SaveNewPassword()
    {
        
    }

}
