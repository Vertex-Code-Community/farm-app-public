namespace FarmApp.ViewModels.Accounts
{
    public class RefreshTokenResultModel
    {
        public string RefreshToken { get; set; } = default!;
        public DateTime RefreshTokenLifetime { get; set; }
    }
}
