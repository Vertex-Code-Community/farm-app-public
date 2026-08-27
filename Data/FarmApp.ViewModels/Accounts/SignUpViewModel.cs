using FarmApp.Shared.Resources.Localization;
using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.Accounts
{
    public class SignUpViewModel
    {
        [Required(ErrorMessageResourceName = "Sign_up_Validation_Required_Email",
            ErrorMessageResourceType = typeof(AppRecources)),
        RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z0-9]{2,}$", 
            ErrorMessageResourceName = "Sign_up_Validation_Required_Email",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(@"^(?=.{8,})(?=.*[A-Z])(?=.*[a-z])(?=.*\d)[a-zA-Z\d!@#$%^&*]*$", ErrorMessage = " ")] 
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Sign_up_Validation_Required_Password",
            ErrorMessageResourceType = typeof(AppRecources))]
        [Compare(nameof(Password), 
            ErrorMessageResourceName = "Sign_up_Validation_Compare_Password",
            ErrorMessageResourceType = typeof(AppRecources))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Range(typeof(bool), "true", "true", 
            ErrorMessageResourceName = "Sign_up_Validation_Required_Accept", 
            ErrorMessageResourceType = typeof(AppRecources))] 
        public bool IsAccepted { get; set; }
    }
}
