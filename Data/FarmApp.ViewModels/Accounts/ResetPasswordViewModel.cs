using FarmApp.Shared.Resources.Localization;
using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.Accounts
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceName = "Password_Reset_Validation_Password",
            ErrorMessageResourceType = typeof(AppRecources)), 
        RegularExpression(@"^(?=.{8,})(?=.*[A-Z])(?=.*[a-z])(?=.*\d)^[a-zA-Z\d]*$",
        ErrorMessageResourceName = "Password_Reset_Validation_Password",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Password_Reset_Validation_Confirm_Password",
            ErrorMessageResourceType = typeof(AppRecources))]
        [Compare(nameof(Password), ErrorMessageResourceName = "Sign_up_Validation_Compare_Password",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
