using System.ComponentModel.DataAnnotations;

namespace FarmApp.ViewModels.Accounts;

public class SignUpRequestModel
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required] public string Password { get; set; }

    [Required] public string ConfirmPassword { get; set; }
    [Required] public bool IsAccepted { get; set; }
}