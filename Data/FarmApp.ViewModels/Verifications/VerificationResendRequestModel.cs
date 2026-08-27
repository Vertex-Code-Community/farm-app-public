using FarmApp.ViewModels.Accounts;

namespace FarmApp.ViewModels.Verifications
{
    public class VerificationResendRequestModel
    {
        public string Email { get; set; } = string.Empty;
        public VerificationPurpose VerificationPurpose { get; set; }
    }
}
