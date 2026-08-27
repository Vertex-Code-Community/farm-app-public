using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.Accounts;

public class ForgotPasswordRequestModel
{
    [Required(ErrorMessage = "Please enter your email"),
    RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z0-9]{2,}$", ErrorMessage = "Please enter your email")]
    public string Email { get; set; } = string.Empty;
}