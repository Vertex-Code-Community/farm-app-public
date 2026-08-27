namespace FarmApp.ViewModels.Accounts
{
    public class ChangePasswordRequestModel
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
