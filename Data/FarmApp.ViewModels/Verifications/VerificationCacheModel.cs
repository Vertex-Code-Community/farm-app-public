namespace FarmApp.ViewModels.Verifications
{
    public class VerificationCacheModel
    {
        public string Email { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public object? Payload { get; set; }
    }
}
