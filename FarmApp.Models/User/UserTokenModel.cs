namespace FarmApp.Models.User;

public class UserTokenModel
{
    public string AccessToken { get; set; } = string.Empty;

    public UserTokenModel() { }

    public UserTokenModel(string accessToken)
    {
        AccessToken = accessToken;
    }
}