namespace FarmApp.ViewModels.Users;

public class CreateUserModel
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
}
