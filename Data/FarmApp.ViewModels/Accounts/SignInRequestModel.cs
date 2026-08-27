using FarmApp.Shared.Resources.Localization;
using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.Accounts;

public class SignInRequestModel
{
    [Required(ErrorMessageResourceName = "Log_in_Validation_Required_Email",
        ErrorMessageResourceType = typeof(AppRecources)),
    RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z0-9]{2,}$", ErrorMessageResourceName = "Log_in_Validation_Required_Email",
        ErrorMessageResourceType = typeof(AppRecources))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = "Log_in_Validation_Required_Password",
        ErrorMessageResourceType = typeof(AppRecources)),
    RegularExpression(@"^.{8,}$", ErrorMessageResourceName = "Log_in_Validation_Required_Password",
        ErrorMessageResourceType = typeof(AppRecources))] 
    public string Password { get; set; } = string.Empty;
}
