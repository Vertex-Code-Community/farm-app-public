namespace FarmApp.ViewModels.Accounts
{
    public class SignUpCacheModel
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
    }
}
