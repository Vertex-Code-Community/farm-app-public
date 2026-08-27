namespace FarmApp.ViewModels.Accounts;

public class ResetPasswordRequestModel
{
    public string ResetToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}