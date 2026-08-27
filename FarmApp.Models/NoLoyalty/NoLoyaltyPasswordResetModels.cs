namespace FarmApp.Models.NoLoyalty;

public class NoLoyaltyRequestPasswordResetModel
{
    public string Email { get; set; }
}

public class NoLoyaltyConfirmPasswordResetRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
