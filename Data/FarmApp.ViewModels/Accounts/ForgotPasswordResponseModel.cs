namespace FarmApp.ViewModels.Accounts;

public class ForgotPasswordResponseModel
{
    public DateTime CodeSentTime { get; set; }
    public DateTime CurrentTime { get; set; }
    public int Ttl { get; set; }
}