using System.ComponentModel.DataAnnotations;

namespace FarmApp.Models.User;

public class CredentialsModel
{
    [Required]
    [EmailAddress(ErrorMessage = "Incorrect email format")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Password { get; set; } = string.Empty;
}
